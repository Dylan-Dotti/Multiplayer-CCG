using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public bool Interactable { get; set; } = true;

    public List<Card> Cards { get; private set; }
    public Card SelectedCard { get; private set; }
    public int HandLimit { get; set; } = 6;
    public int CurrNumCards { get => Cards.Count; }

    private const float CARD_DEFAULT_Y = 0;
    private const float CARD_MOUSEOVER_Y_DELTA = 0.5f;

    [SerializeField] private GameBoard playerBoard;

    private Duelist owner;
    private PhotonView pView;

    private void Awake()
    {
        Cards = new List<Card>();
        owner = GetComponentInParent<Duelist>();
        pView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        // move selected card to mouse position, 
        // or deselect if right click
        if (SelectedCard != null)
        {
            if (Input.GetMouseButtonUp(1))
            {
                DeselectCard();
            }
            else
            {
                Vector3 mousePos = Camera.main.
                    ScreenToWorldPoint(Input.mousePosition);
                SelectedCard.transform.position = new Vector3(
                    mousePos.x, mousePos.y, -2);
            }
        }
    }

    public void AddCardToHand(Card card)
    {
        Cards.Add(card);
        card.transform.parent = transform;
        if (owner == DuelMetaData.Instance.MyDuelist)
        {
            card.CardMouseEnterEvent += OnCardMouseEnter;
            card.CardMouseExitEvent += OnCardMouseExit;
            card.CardMouseClickEvent += OnCardMouseClick;
        }
        card.GetComponent<Collider>().enabled = true;
        MoveCardsToHandPositions(true);
    }

    public void RemoveCardFromHand(Card card)
    {
        Debug.Log("removing card from hand");
        Cards.Remove(card);
        card.transform.parent = null;
        if (owner.IsLocalDuelist())
        {
            card.CardMouseEnterEvent -= OnCardMouseEnter;
            card.CardMouseExitEvent -= OnCardMouseExit;
            card.CardMouseClickEvent -= OnCardMouseClick;
        }
        card.GetComponent<Collider>().enabled = false;
        MoveCardsToHandPositions(true);
    }

    public int GetCardIndex(Card handCard)
    {
        int index = Cards.IndexOf(handCard);
        if (index == -1) throw new System.ArgumentException("Card not in hand");
        return index;
    }

    // selects given card, allowing it to follow mouse and be played to board
    private void SelectCard(Card card)
    {
        SelectedCard = card;
        card.OnSelectedFromHand();
        foreach (CardZone playableZone in card.GetPlayableZones())
        {
            playableZone.ZoneMouseClickEvent +=
                card.OnPlayableZoneSelected;
        }
        SelectedCard.GetComponent<Collider>().enabled = false;
    }

    // deselects selected card and returns to position if still in hand
    private void DeselectCard()
    {
        MoveCardsToHandPositions(true);
        SelectedCard.GetComponent<Collider>().enabled = Cards.Contains(SelectedCard);
        foreach (CardZone playableZone in SelectedCard.GetPlayableZones())
        {
            playableZone.ZoneMouseClickEvent -=
                SelectedCard.OnPlayableZoneSelected;
        }
        SelectedCard.OnDeselectedFromHand();
        SelectedCard = null;

    }

    public void PlayCard(Card card, CardZone targetZone, PlayType pType)
    {
        if (card != SelectedCard)
        {
            SelectCard(card);
        }
        PlaySelectedCard(targetZone, pType);
    }

    public void PlayCard(int handIndex, CardZone targetZone, PlayType pType)
    {
        PlayCard(Cards[handIndex], targetZone, pType);
    }

    // remove card from hand, deselect, move and add to targetZone
    public void PlaySelectedCard(CardZone targetZone, PlayType pType)
    {
        Card card = SelectedCard;
        int cardIndex = GetCardIndex(card);
        RemoveCardFromHand(card);
        DeselectCard();
        card.transform.localPosition = Vector3.zero;
        card.MoveToZone(targetZone);
        if (owner.IsLocalDuelist())
        {
            pView.RPC("RPC_PlayEnemyCard", RpcTarget.OthersBuffered,
                cardIndex, targetZone.name, pType);
        }
        card.OnPlayedFromHand(pType, targetZone);
    }

    public void SummonMonster(MonsterCard monster, CardZone summonZone,
        PlayType pType)
    {
        switch (pType)
        {
            case PlayType.NormalSummon:
                PlayCard(monster, summonZone, pType);
                break;
            case PlayType.NormalSet:
                PlayCard(monster, summonZone, pType);
                owner.SwitchMonsterMode(monster);
                break;
            default:
                throw new System.NotImplementedException(
                    "No action specified for sType: " + pType);
        }
        monster.OnPlayedFromHand(pType, summonZone);
    }

    // Move all cards in hand to their position
    private void MoveCardsToHandPositions(bool lerp)
    {
        if (CurrNumCards == 0) return;
        int midpoint = CurrNumCards / 2;
        float shiftAmount = Card.CARD_WIDTH * 1.05f;
        for (int i = 0; i < CurrNumCards; i++)
        {
            //calculate position
            int shiftMultiplier = i - midpoint;
            Vector3 newPosition = new Vector3(
                shiftMultiplier * shiftAmount, CARD_DEFAULT_Y, -1);
            //shift cards if even number in hand
            if (CurrNumCards % 2 == 0)
            {
                newPosition += Vector3.right * Card.CARD_WIDTH / 2;
            }
            //move to position
            if (lerp)
            {
                Cards[i].LerpToPosition(transform.TransformPoint(
                    newPosition), 0.1f);
            }
            else
            {
                Cards[i].transform.position = newPosition;
            }
        }
    }

    private void MoveCardMouseover(Card card)
    {
        card.transform.localPosition = new Vector3(
            card.transform.localPosition.x,
            CARD_DEFAULT_Y + CARD_MOUSEOVER_Y_DELTA,
            card.transform.localPosition.z);
    }

    private void MoveCardDefault(Card card)
    {
        card.transform.localPosition = new Vector3(
            card.transform.localPosition.x, CARD_DEFAULT_Y,
            card.transform.localPosition.z);
    }

    /* mouse events */

    private void OnCardMouseEnter(Card card)
    {
        if (Interactable && SelectedCard == null)
        {
            MoveCardMouseover(card);
        }
    }

    private void OnCardMouseExit(Card card)
    {
        if (Interactable && SelectedCard == null)
        {
            MoveCardDefault(card);
        }
    }

    private void OnCardMouseClick(Card card)
    {
        if (Interactable && SelectedCard == null)
        {
            SelectCard(card);
        }
    }

    [PunRPC]
    private void RPC_PlayEnemyCard(int handIndex, string zoneName, 
        PlayType pType)
    {
        Duelist enemyDuelist = DuelMetaData.Instance.EnemyDuelist;
        CardZone targetZone = enemyDuelist.Board.GetZoneByName(zoneName);
        enemyDuelist.MainHand.PlayCard(handIndex, targetZone, pType);
    }

    // doesn't work with current setup
    [PunRPC]
    private void RPC_SummonEnemyMonster(int handIndex, string zoneName,
        PlayType pType)
    {
        Duelist enemyDuelist = DuelMetaData.Instance.EnemyDuelist;
        CardZone summonZone = enemyDuelist.Board.GetZoneByName(zoneName);
        MonsterCard enemyMonster = enemyDuelist.MainHand.
            Cards[handIndex] as MonsterCard;
        enemyDuelist.MainHand.SummonMonster(enemyMonster, summonZone, pType);
    }
}

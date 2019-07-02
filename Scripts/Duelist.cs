using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Duelist : MonoBehaviour, IMonsterTargettable
{
    public string DuelistName { get; private set; } = "Unknown";
    public int LifePoints { get => lifePoints; set => lifePoints = Mathf.Max(0, value); }

    public GameBoard Board { get => board; }
    public Deck MainDeck { get; private set; }
    public Hand MainHand { get; private set; }

    [SerializeField] private GameBoard board;

    private int lifePoints = 8000;
    private PhotonView pView;

    private Coroutine targetSelectionCR;

    private void Awake()
    {
        pView = GetComponent<PhotonView>();
        MainDeck = transform.Find("Deck").GetComponent<Deck>();
        MainHand = GetComponentInChildren<Hand>();
    }

    private void OnTransformParentChanged()
    {
        //init duelist
        Vector3 deckZonePos = board.DeckZone.GetHoverPosition(0.05f);
        MainDeck.transform.position = deckZonePos;
        if (IsLocalDuelist())
        {
            DuelistName = MultiplayerSettings.Instance.PlayerUsername;
            pView.RPC("RPC_InitEnemyDuelistName",
                RpcTarget.OthersBuffered, DuelistName);
            MainDeck.Shuffle();
            StartCoroutine(DrawCardsCR(5));
        }
    }

    public void ReceiveMonsterAttack(MonsterCard monster)
    {
        LifePoints -= monster.AttackStrength;
    }

    public void DrawCard()
    {
        Card nextCard = MainDeck.DrawCard(IsLocalDuelist());
        MainHand.AddCardToHand(nextCard);
        if (IsLocalDuelist())
        {
            pView.RPC("RPC_DrawEnemyCard", RpcTarget.OthersBuffered);
        }
    }

    public Coroutine DrawCards(int numCards)
    {
        return StartCoroutine(DrawCardsCR(numCards));
    }

    public void PlayCardFromHand(Card card, CardZone playZone,
        PlayType pType)
    {
        MainHand.PlayCard(card, playZone, pType);
    }

    public void SendCardToGraveyard(Card card)
    {
        string occupiedZoneName = card.OccupiedZone.name;
        SendCardToGraveyardNoRPC(card);
        pView.RPC("RPC_SendEnemyCardToGraveyard", RpcTarget.OthersBuffered, 
            !IsLocalDuelist(), occupiedZoneName);
    }

    public void TributeMonster(MonsterCard monster)
    {
        SendCardToGraveyard(monster);
    }

    public void SetCardOrientation(Card card, Card.FaceOrientation fOrient,
        Card.BodyOrientation bOrient)
    {
        //test
        card.FOrient = fOrient;
        card.BOrient = bOrient;
        if (IsLocalDuelist())
        {
            pView.RPC("RPC_SetEnemyCardOrientation", RpcTarget.OthersBuffered,
                card.OccupiedZone.name, fOrient, bOrient);
        }
    }

    public void SwitchMonsterMode(MonsterCard monster)
    {
        monster.SwitchMode();
        pView.RPC("RPC_SwitchEnemyMonsterMode", 
            RpcTarget.OthersBuffered, monster.OccupiedZone.name);
    }

    public void FlipSummonMonster(MonsterCard monster)
    {
        monster.FlipSummon();
        pView.RPC("RPC_FlipSummonEnemyMonster", RpcTarget.OthersBuffered,
            monster.OccupiedZone.name);
    }

    public void EnableTargetSelection(Card targettingCard)
    {
        List<CardZone> targettableZones = targettingCard.GetTargettableZones();
        if (targettableZones.Count == 0) return;

        MainHand.Interactable = false;
        board.DisableAllZoneEvents();
        foreach (CardZone tz in targettableZones)
        {
            CardZoneHighlighter.Instance.HighlightZone(tz,
                CardZoneHighlighter.HighlightType.Enemy);
            tz.ZoneMouseClickEvent += targettingCard.OnTargetSelected;
        }
        targetSelectionCR = StartCoroutine(WaitForTargetCR(targettingCard));
    }

    public void DisableTargetSelection(Card targettingCard)
    {
        StopCoroutine(targetSelectionCR);
        foreach (CardZone tz in targettingCard.GetTargettableZones())
        {
            CardZoneHighlighter.Instance.UnhighlightZone(tz);
            tz.ZoneMouseClickEvent -= targettingCard.OnTargetSelected;
        }
        MainHand.Interactable = true;
        board.EnableAllZoneEvents();
    }

    public void AttackWithMonster(MonsterCard monster, MonsterCard target=null)
    {
        Debug.Log("attacking with monster");
        if (target == null)
        {
            //attack enemy duelist directly
            monster.AttackTarget(DuelMetaData.Instance.EnemyDuelist);
            pView.RPC("RPC_AttackWithEnemyMonster", RpcTarget.OthersBuffered, 
                monster.OccupiedZone.name, null);
        }
        else
        {
            //attack target
            monster.AttackTarget(target);
            pView.RPC("RPC_AttackWithEnemyMonster", RpcTarget.OthersBuffered,
                monster.OccupiedZone.name, target.OccupiedZone.name);
        }
    }

    public bool IsLocalDuelist()
    {
        return this == DuelMetaData.Instance.MyDuelist;
    }

    private void SendCardToGraveyardNoRPC(Card card)
    {
        card.MoveToZone(card.Owner.Board.Graveyard, 1f);
        card.OnSentToGraveyard(card.Owner.Board.Graveyard);
    }

    private IEnumerator DrawCardsCR(int numCards)
    {
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < numCards; i++)
        {
            DrawCard();
            yield return new WaitForSeconds(0.5f);
        }
    }

    //wait for monster target selection, cancel if right click / esc
    private IEnumerator WaitForTargetCR(Card targettingCard)
    {
        while (true)
        {
            if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Escape))
            {
                DisableTargetSelection(targettingCard);
                break;
            }
            yield return null;
        }
    }

    /* RPC Calls */

    [PunRPC]
    private void RPC_InitEnemyDuelistName(string name)
    {
        DuelMetaData.Instance.EnemyDuelist.DuelistName = name;
    }

    [PunRPC]
    private void RPC_DrawEnemyCard()
    {
        DuelMetaData.Instance.EnemyDuelist.DrawCard();
    }

    [PunRPC]
    private void RPC_SetEnemyCardOrientation(string zoneName,
        Card.FaceOrientation fOrient, Card.BodyOrientation bOrient)
    {
        //test
        Duelist enemyDuelist = DuelMetaData.Instance.EnemyDuelist;
        CardZone zone = enemyDuelist.Board.GetZoneByName(zoneName);
        Card enemyCard = zone.Occupants[0];
        enemyCard.FOrient = fOrient;
        enemyCard.BOrient = bOrient;
    }

    [PunRPC]
    private void RPC_SwitchEnemyMonsterMode(string mZoneName)
    {
        Duelist enemyDuelist = DuelMetaData.Instance.EnemyDuelist;
        CardZone mZone = enemyDuelist.Board.GetZoneByName(mZoneName);
        MonsterCard enemyMonster = mZone.Occupants[0] as MonsterCard;
        enemyMonster.SwitchMode();
    }

    [PunRPC]
    private void RPC_FlipSummonEnemyMonster(string mZoneName)
    {
        Duelist enemyDuelist = DuelMetaData.Instance.EnemyDuelist;
        CardZone mZone = enemyDuelist.Board.GetZoneByName(mZoneName);
        MonsterCard enemyMonster = mZone.Occupants[0] as MonsterCard;
        enemyMonster.FlipSummon();
    }

    [PunRPC]
    private void RPC_AttackWithEnemyMonster(string monsterZoneName, 
        string targetZoneName=null)
    {
        Duelist attackingDuelist = DuelMetaData.Instance.EnemyDuelist;
        MonsterCard attackingMonster = attackingDuelist.Board.
            GetZoneByName(monsterZoneName).Occupants[0] as MonsterCard;
        Duelist targetDuelist = DuelMetaData.Instance.MyDuelist;

        if (targetZoneName == null)
        {
            // attack enemy duelist
            attackingMonster.AttackTarget(targetDuelist);
        }
        else
        {
            // attack enemy monster
            CardZone targetZone = targetDuelist.Board.
                GetZoneByName(targetZoneName);
            MonsterCard targetMonster = targetZone.Occupants[0] as MonsterCard;
            attackingMonster.AttackTarget(targetMonster);
        }
    }

    [PunRPC]
    private void RPC_SendEnemyCardToGraveyard(bool friendlyDuelist,
        string cardZoneName)
    {
        Duelist cardOwner = friendlyDuelist ? 
            DuelMetaData.Instance.MyDuelist : DuelMetaData.Instance.EnemyDuelist;
        Card destroyedCard = cardOwner.Board.
            GetZoneByName(cardZoneName).Occupants[0];
        SendCardToGraveyardNoRPC(destroyedCard);
    }
}

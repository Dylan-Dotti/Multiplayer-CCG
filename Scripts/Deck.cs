using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Deck : MonoBehaviour
{
    public UnityAction CardDrawnEvent;
    public UnityAction DeckOutEvent;

    public List<Card> Cards { get; private set; }
    public int CardsRemaining { get => Cards.Count; }

    [SerializeField] private List<Card> cardPrefabs;

    private Duelist owner;
    private PhotonView pView;

    private void Awake()
    {
        pView = GetComponent<PhotonView>();
        Cards = new List<Card>(cardPrefabs.Count);
        owner = GetComponentInParent<Duelist>();
    }

    private void Start()
    {
        cardPrefabs.ForEach(c => AddCardToTop(Instantiate(c)));
    }

    public Card DrawCard(bool flip)
    {
        if (Cards.Count > 0)
        {
            Card card = Cards[Cards.Count - 1];
            RemoveCard(card);
            if (flip)
            {
                card.FOrient = Card.FaceOrientation.FaceUp;
            }
            CardDrawnEvent?.Invoke();
            return card;
        }
        DeckOutEvent?.Invoke();
        return null;
    }

    public void AddCardToTop(Card card)
    {
        AddCardAt(card, Cards.Count);
    }

    public void AddCardAt(Card card, int index)
    {
        card.Owner = owner;
        Cards.Add(card);
        owner.Board.DeckZone.AddCard(card);
        MoveCardsToDeckPositions();
    }

    public void RemoveCard(Card card)
    {
        Cards.Remove(card);
        owner.Board.DeckZone.RemoveCard(card);
    }

    public void MoveCardsToDeckPositions()
    {
        for (int i = 0; i < Cards.Count; i++)
        {
            Cards[i].transform.rotation = Quaternion.Euler(0, 180, 0);
            Cards[i].transform.localPosition = new Vector3(0, 0,
                -(Card.CARD_THICKNESS * i) - (Card.CARD_THICKNESS / 2));
        }
    }

    public void Shuffle()
    {
        for (int i = 0; i < cardPrefabs.Count; i++)
        {
            int randIndex = Random.Range(0, Cards.Count);
            SwapCards(i, randIndex);
            pView.RPC("RPC_SwapEnemyCards",
                RpcTarget.OthersBuffered, i, randIndex);
        }
    }

    private void SwapCards(int index1, int index2)
    {
        Card temp = Cards[index1];
        Cards[index1] = Cards[index2];
        Cards[index2] = temp;
        MoveCardsToDeckPositions();

    }

    [PunRPC]
    private void RPC_SwapEnemyCards(int index1, int index2)
    {
        DuelMetaData.Instance.EnemyDuelist.
            MainDeck.SwapCards(index1, index2);
    }
}

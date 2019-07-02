using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CardZone : MonoBehaviour
{
    public UnityAction<CardZone> ZoneMouseEnterEvent;
    public UnityAction<CardZone> ZoneMouseExitEvent;
    public UnityAction<CardZone> ZoneMouseClickEvent;

    public List<Card> Occupants { get; private set; }
    public int NumOccupants { get => Occupants.Count; }
    public bool IsOccupied { get => Occupants.Count > 0; }

    public bool EventsEnabled { get; set; } = true;

    private void Awake()
    {
        Occupants = new List<Card>();
    }

    protected virtual void OnMouseEnter()
    {
        if (EventsEnabled)
        {
            ZoneMouseEnterEvent?.Invoke(this);
        }
    }

    protected virtual void OnMouseExit()
    {
        if (EventsEnabled)
        {
            ZoneMouseExitEvent?.Invoke(this);
        }
    }

    protected virtual void OnMouseUpAsButton()
    {
        if (EventsEnabled)
        {
            Debug.Log("zone mouse click event");
            ZoneMouseClickEvent?.Invoke(this);
        }
    }

    public void AddCard(Card card)
    {
        Occupants.Add(card);
        card.transform.parent = transform;
    }

    public void AddCards(IEnumerable<Card> cards)
    {
        foreach (Card c in cards)
        {
            AddCard(c);
        }
    }

    public void RemoveCard(Card card)
    {
        if (Occupants.Remove(card))
        {
            card.transform.parent = null;
        }
    }

    public int GetCardIndex(Card card)
    {
        return Occupants.IndexOf(card);
    }

    public Vector3 GetHoverPosition(float hoverHeight)
    {
        return transform.position + new Vector3(0, 0, -hoverHeight);
    }

    protected void EnableNumCardsPanel(string formattedMessage)
    {
        PlayerUI.Instance.NumCardsPanel.Activate(this, formattedMessage);
    }

    protected void DisableNumCardsPanel()
    {
        PlayerUI.Instance.NumCardsPanel.Deactivate();
    }
}

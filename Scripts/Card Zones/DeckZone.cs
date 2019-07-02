using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckZone : CardZone
{
    protected override void OnMouseEnter()
    {
        base.OnMouseEnter();
        EnableNumCardsPanel("Deck: {0} cards remaining");
    }

    protected override void OnMouseExit()
    {
        base.OnMouseExit();
        DisableNumCardsPanel();
    }
}

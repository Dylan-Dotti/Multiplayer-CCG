using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraDeckZone : CardZone
{
    protected override void OnMouseEnter()
    {
        base.OnMouseEnter();
        EnableNumCardsPanel("Extra Deck: {0} cards");
    }

    protected override void OnMouseExit()
    {
        base.OnMouseExit();
        DisableNumCardsPanel();
    }
}

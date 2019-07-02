using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graveyard : CardZone
{
    protected override void OnMouseEnter()
    {
        base.OnMouseEnter();
        EnableNumCardsPanel("Graveyard: {0} cards");
    }

    protected override void OnMouseExit()
    {
        base.OnMouseExit();
        DisableNumCardsPanel();
    }
}

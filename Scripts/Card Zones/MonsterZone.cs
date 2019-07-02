using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MonsterZone : CardZone
{
    protected override void OnMouseEnter()
    {
        base.OnMouseEnter();
    }

    protected override void OnMouseExit()
    {
        base.OnMouseExit();
    }

    protected override void OnMouseUpAsButton()
    {
        if (EventsEnabled && !PlayerUI.Instance.MActionsPanel.gameObject.activeInHierarchy)
        {
            if (IsOccupied)
            {
                Occupants[0].OnBattleZoneLeftClick();
            }
            else
            {
                Debug.Log("Zone not occupied");
            }
            base.OnMouseUpAsButton();
        }
        else
        {
            Debug.Log("Zone events blocked");
        }
    }
}

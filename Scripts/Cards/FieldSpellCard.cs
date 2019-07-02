using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldSpellCard : SpellCard
{
    public override List<CardZone> GetPlayableZones()
    {
        return new List<CardZone> { Owner.Board.FieldCardZone };
    }

    public override List<CardZone> GetTargettableZones()
    {
        return new List<CardZone>();
    }
}

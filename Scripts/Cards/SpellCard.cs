using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpellCard : Card
{
    public override bool IsPlayable { get => true; }
    public override CardType CType { get => CardType.Spell; }
}

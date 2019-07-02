using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class MonsterCard : Card, IMonsterTargettable
{
    public enum CombatMode { Attack, Defense }

    public static UnityAction<MonsterCard, CardZone> NormalSummonEvent;
    public static UnityAction<MonsterCard, CardZone> NormalSetEvent;
    public static UnityAction<MonsterCard> FlipSummonEvent;

    public override bool IsPlayable { get => Playable(); }
    public virtual bool CanDirectAttack { get => GetTargettableZones().Count == 0; }
    public virtual bool CanAttackMonster { get => GetTargettableZones().Count > 0; }

    public override CardType CType { get => CardType.Monster; }
    public CombatMode Mode { get => cMode; set { cMode = value; SetModeOrientation(); } }

    public int Level { get => level; set => level = value; }
    public int AttackStrength { get => attackStrength; set => attackStrength = value; }
    public int DefenseStrength { get => defenseStrength; set => defenseStrength = value; }
    public MonsterAttribute Attribute { get => attribute; set => attribute = value; }

    public int TributeValue { get; set; } = 1;
    public int NumTributesRequired { get; set; }

    [SerializeField] private int level;
    [SerializeField] private int attackStrength;
    [SerializeField] private int defenseStrength;
    [SerializeField] private MonsterAttribute attribute;

    private CombatMode cMode;

    protected override void Awake()
    {
        base.Awake();
        NumTributesRequired = level <= 4 ?
            0 : level <= 6 ? 1 : level <= 9 ? 2 : 3;
    }

    public void AttackTarget(IMonsterTargettable target)
    {
        target.ReceiveMonsterAttack(this);
    }

    public void ReceiveMonsterAttack(MonsterCard monster)
    {
        if (Mode == CombatMode.Attack)
        {
            if (AttackStrength > monster.AttackStrength)
            {
                // destroy enemy
                monster.Owner.LifePoints -=
                    AttackStrength - monster.AttackStrength;
                monster.DestroyCard();
            }
            else if (AttackStrength < monster.AttackStrength)
            {
                // destroy self
                Owner.LifePoints -=
                    monster.AttackStrength - AttackStrength;
                DestroyCard();
            }
            else
            {
                monster.DestroyCard();
                DestroyCard();
                //destroy each other
            }
        }
        else //defense mode
        {

        }
    }

    public void SwitchMode()
    {
        Mode = cMode == CombatMode.Attack ?
            CombatMode.Defense : CombatMode.Attack;
    }

    public void TributeMonster()
    {
        TurnStats.Instance.NumTributes += TributeValue;
    }

    public virtual void FlipSummon()
    {
        FOrient = FaceOrientation.FaceUp;
        Mode = CombatMode.Attack;
        FlipSummonEvent?.Invoke(this);
    }

    /* Callbacks */

    public override List<CardZone> GetPlayableZones()
    {
        List<CardZone> playableZones = new List<CardZone>();
        playableZones.AddRange(Owner.Board.MonsterCardZones.
            Where(mz => !mz.IsOccupied));
        return playableZones;
    }

    public override List<CardZone> GetTargettableZones()
    {
        GameBoard enemyBoard = DuelMetaData.Instance.EnemyDuelist.Board;
        return enemyBoard.MonsterCardZones.
            Where(mz => mz.IsOccupied).ToList<CardZone>();
    }

    public override void OnDeselectedFromHand()
    {
        base.OnDeselectedFromHand();
        PlayerUI.Instance.MActionsPanel.Deactivate();
    }

    public override void OnPlayableZoneSelected(CardZone playableZone)
    {
        base.OnPlayableZoneSelected(playableZone);
        PlayerUI.Instance.MActionsPanel.ActivateSummonOptions(this, playableZone);
    }

    public override void OnPlayedFromHand(PlayType pType, CardZone targetZone)
    {
        base.OnPlayedFromHand(pType, targetZone);
        switch (pType)
        {
            case PlayType.NormalSummon:
                TurnStats.Instance.NumNormalSummons += 1;
                FOrient = FaceOrientation.FaceUp;
                Mode = CombatMode.Attack;
                NormalSummonEvent?.Invoke(this, targetZone);
                break;
            case PlayType.NormalSet:
                TurnStats.Instance.NumNormalSummons += 1;
                FOrient = FaceOrientation.FaceDown;
                Mode = CombatMode.Defense;
                NormalSetEvent?.Invoke(this, targetZone);
                break;
            default:
                throw new System.NotImplementedException(
                    "No action specified for pType: " + pType);
        }
    }

    public override void OnTargetSelected(CardZone targetZone)
    {
        base.OnTargetSelected(targetZone);
        Owner.DisableTargetSelection(this);
        MonsterCard target = targetZone.Occupants[0] as MonsterCard;
        Owner.AttackWithMonster(this, target);
    }

    public override void OnBattleZoneLeftClick()
    {
        if (Owner.MainHand.SelectedCard == null &&
            Owner.IsLocalDuelist())
        {
            base.OnBattleZoneLeftClick();
            if (FOrient == FaceOrientation.FaceUp)
            {
                PlayerUI.Instance.MActionsPanel.ActivateBattleOptions(this);
            }
            else if (TurnsSincePlayed >= 1)
            {
                //change to >= 1
                PlayerUI.Instance.MActionsPanel.ActivateFlipSummonOptions(this);
            }
        }
    }

    private void SetModeOrientation()
    {
        BOrient = cMode == CombatMode.Attack ? 
            BodyOrientation.Normal : BodyOrientation.Rotated;
    }

    private bool Playable()
    {
        return TurnStats.Instance.NumTributes >=
            NumTributesRequired;
    }
}

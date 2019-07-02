using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MonsterActionsPanel : MonoBehaviour,
    IPointerExitHandler
{
    [SerializeField] private Button normalSummonButton;
    [SerializeField] private Button normalSetButton;
    [SerializeField] private Button flipSummonButton;

    [SerializeField] private Button directAttackButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button tributeButton;
    [SerializeField] private Button switchModeButton;

    private MonsterCard monster;
    private CardZone targetZone;

    public void OnPointerExit(PointerEventData eventData)
    {
        Deactivate();
    }

    public void ActivateSummonOptions(MonsterCard monster, CardZone summonZone)
    {
        ActivatePanel(monster, summonZone);
        EnableNormalSummonButton();
        EnableNormalSetButton();
    }

    public void ActivateFlipSummonOptions(MonsterCard monster)
    {
        ActivatePanel(monster);
        EnableFlipSummonButton();
    }

    public void ActivateBattleOptions(MonsterCard monster)
    {
        ActivatePanel(monster);
        if (monster.CanDirectAttack) EnableDirectAttackButton();
        if (monster.CanAttackMonster) EnableAttackButton();
        EnableTributeButton();
        EnableSwitchModeButton();
    }

    public void Deactivate()
    {
        GetAllButtons().ForEach(b => DisableButton(b));
        gameObject.SetActive(false);
    }

    private void ActivatePanel(MonsterCard monster, CardZone targetZone=null)
    {
        gameObject.SetActive(true);
        this.monster = monster;
        this.targetZone = targetZone;
        GetComponent<RectTransform>().position =
            Input.mousePosition + new Vector3(-20, -20);
    }

    private void EnableNormalSummonButton()
    {
        EnableButton(normalSummonButton, () =>
        {
            Deactivate();
            monster.Owner.PlayCardFromHand(monster, targetZone, 
                PlayType.NormalSummon);
        });
    }

    private void EnableNormalSetButton()
    {
        EnableButton(normalSetButton, () =>
        {
            Deactivate();
            monster.Owner.PlayCardFromHand(monster, targetZone,
                PlayType.NormalSet);
        });
    }

    private void EnableFlipSummonButton()
    {
        EnableButton(flipSummonButton, () =>
        {
            Deactivate();
            monster.Owner.FlipSummonMonster(monster);
        });
    }

    private void EnableDirectAttackButton()
    {
        EnableButton(directAttackButton, () =>
        {
            Deactivate();
            monster.Owner.AttackWithMonster(monster);
        });
    }

    private void EnableAttackButton()
    {
        EnableButton(attackButton, () =>
        {
            Deactivate();
            monster.Owner.EnableTargetSelection(monster);
        });
    }

    private void EnableTributeButton()
    {
        EnableButton(tributeButton, () =>
        {
            Deactivate();
            monster.Owner.TributeMonster(monster);
        });
    }

    private void EnableSwitchModeButton()
    {
        EnableButton(switchModeButton, () =>
        {
            Deactivate();
            monster.Owner.SwitchMonsterMode(monster);
        });
    }

    private void EnableButton(Button button, UnityAction onClickAction)
    {
        button.transform.parent.gameObject.SetActive(true);
        button.onClick.AddListener(onClickAction);
    }

    private void DisableButton(Button button)
    {
        button.onClick.RemoveAllListeners();
        button.transform.parent.gameObject.SetActive(false);
    }

    private List<Button> GetAllButtons()
    {
        return new List<Button>()
        {
            normalSummonButton, normalSetButton,
            flipSummonButton, directAttackButton,
            attackButton, tributeButton, switchModeButton
        };
    }
}

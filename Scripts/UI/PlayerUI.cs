using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance { get; private set; }

    public MonsterActionsPanel MActionsPanel => maPanel;
    public SpellActionsPanel SActionsPanel => saPanel;
    public TrapActionsPanel TActionsPanel => taPanel;
    public NumCardsPanel NumCardsPanel => ncPanel;

    [SerializeField] private MonsterActionsPanel maPanel;
    [SerializeField] private SpellActionsPanel saPanel;
    [SerializeField] private TrapActionsPanel taPanel;
    [SerializeField] private NumCardsPanel ncPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
}

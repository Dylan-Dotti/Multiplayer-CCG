using UnityEngine;

public class DuelMetaData : MonoBehaviour
{
    public static DuelMetaData Instance { get; private set; }

    public static Vector3 MY_DUELIST_SPAWN_POS { get; private set; } =
        new Vector3(0, -6.64f, 0);
    public static Vector3 ENEMY_DUELIST_SPAWN_POS { get; private set; } =
        new Vector3(0, 8.25f, 0);

    public Duelist MyDuelist { get; set; }
    public Duelist EnemyDuelist { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        MyDuelist = GameObject.Find("My Duelist").
            GetComponent<Duelist>();
        EnemyDuelist = GameObject.Find("Enemy Duelist").
            GetComponent<Duelist>();
    }
}

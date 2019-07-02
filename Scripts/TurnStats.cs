using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnStats : MonoBehaviour
{
    public static TurnStats Instance { get; private set; }

    public int NumNormalSummons { get; set; }
    public List<MonsterCard> TributedMonsters { get; private set; }
    public int NumTributes { get; set; }

    private void Awake()
    {
        if (Instance != this)
        {
            Instance = this;
        }
    }
}

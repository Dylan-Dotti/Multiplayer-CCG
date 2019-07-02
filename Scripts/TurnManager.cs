using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public UnityAction<int> TurnStartEvent;
    public UnityAction<int> TurnEndEvent;

    public int CurrentTurnNum { get; private set; }

    private PhaseManager phaseManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            CurrentTurnNum = 0;
            phaseManager = PhaseManager.Instance;
            phaseManager.PhaseEndEvent += OnPhaseEnd;
        }
    }

    public void TransitionNextTurn()
    {
        Debug.Log("Turn End");
        TurnEndEvent?.Invoke(CurrentTurnNum);
        CurrentTurnNum++;
        Debug.Log("Turn Start");
        TurnStartEvent?.Invoke(CurrentTurnNum);
    }

    private void OnPhaseEnd(Phase phase)
    {
        if (phase == Phase.End)
        {
            TransitionNextTurn();
        }
    }
}

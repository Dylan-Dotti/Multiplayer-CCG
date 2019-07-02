using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhaseManager : MonoBehaviour
{
    public static PhaseManager Instance { get; private set; }

    public UnityAction<Phase> PhaseStartEvent;
    public UnityAction<Phase> PhaseEndEvent;

    public Phase CurrentPhase { get; private set; }

    private TurnManager turnManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            turnManager = TurnManager.Instance;
        }
    }

    public void TransitionNextPhase()
    {
        Phase currPhase = CurrentPhase;
        PhaseEndEvent?.Invoke(CurrentPhase);
        if (currPhase == Phase.End) return;
        switch (CurrentPhase)
        {
            case Phase.Draw:
                CurrentPhase = Phase.Standby;
                break;
            case Phase.Standby:
                CurrentPhase = Phase.Main1;
                break;
            case Phase.Main2:
                CurrentPhase = Phase.End;
                break;
        }
        PhaseStartEvent?.Invoke(CurrentPhase);
    }

    public void TransitionNewTurn()
    {
        CurrentPhase = Phase.Draw;
        PhaseStartEvent?.Invoke(CurrentPhase);
    }

    public void TransitionEndPhase()
    {
        PhaseEndEvent?.Invoke(CurrentPhase);
        CurrentPhase = Phase.End;
        PhaseStartEvent?.Invoke(CurrentPhase);
    }

    private void OnTurnStart(int turnNum)
    {
        TransitionNewTurn();
    }
}

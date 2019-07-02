using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Card : MonoBehaviour
{
    public enum FaceOrientation { FaceDown, FaceUp }
    public enum BodyOrientation { Normal, Rotated }

    public const float CARD_WIDTH = 2.05f;
    public const float CARD_HEIGHT = 2.6f;
    public const float CARD_THICKNESS = 0.005f;

    public static readonly Vector3 FACE_UP_NORMAL_ROTATION = new Vector3(0, 0, 0);
    public static readonly Vector3 FACE_UP_SIDE_ROTATION = new Vector3(180, 180, -90);
    public static readonly Vector3 FACE_DOWN_NORMAL_ROTATION = new Vector3(0, 180, 0);
    public static readonly Vector3 FACE_DOWN_SIDE_ROTATION = new Vector3(0, 180, -90);

    public static UnityAction<Card> CardPlayedEvent;
    public static UnityAction<Card> CardDestroyedEvent;
    public static UnityAction<Card, Graveyard> CardSentToGraveyardEvent;

    public UnityAction<Card> CardMouseEnterEvent;
    public UnityAction<Card> CardMouseExitEvent;
    public UnityAction<Card> CardMouseClickEvent;

    public abstract bool IsPlayable { get; }
    public abstract CardType CType { get; }
    public string CardName { get => cardName; }
    public string Description { get => description; }

    public FaceOrientation FOrient { get => fOrient;
        set => LerpOrientation(value, bOrient, 0); }
    public BodyOrientation BOrient { get => bOrient;
        set => LerpOrientation(fOrient, value, 0); }
    public bool IsMoving { get; private set; }
    public bool IsRotating { get; private set; }

    public int TurnsSincePlayed { get; private set; }

    public Duelist Owner { get; set; }
    public CardZone OccupiedZone
    {
        get { return occupiedZone; }
        private set
        {
            if (occupiedZone != null)
            {
                occupiedZone.RemoveCard(this);
            }
            occupiedZone = value;
        }
    }

    protected GameObject CardBody { get; private set; }

    [SerializeField] private string cardName;
    [SerializeField] private string description;

    private FaceOrientation fOrient;
    private BodyOrientation bOrient;
    private CardZone occupiedZone;

    protected virtual void Awake()
    {
        CardBody = transform.GetChild(0).gameObject;
    }

    private void OnMouseEnter()
    {
        CardMouseEnterEvent?.Invoke(this);
    }

    private void OnMouseExit()
    {
        CardMouseExitEvent?.Invoke(this);
    }

    private void OnMouseUpAsButton()
    {
        CardMouseClickEvent?.Invoke(this);
    }

    public abstract List<CardZone> GetPlayableZones();

    public abstract List<CardZone> GetTargettableZones();

    public virtual void PlayCard()
    {
        CardPlayedEvent?.Invoke(this);
    }

    public virtual void DestroyCard()
    {
        CardDestroyedEvent?.Invoke(this);
        Owner.SendCardToGraveyard(this);
    }

    // lerp to target position over specified duration
    public Coroutine LerpToPosition(Vector3 position, float duration=0.1f)
    {
        IsMoving = true;
        return StartCoroutine(LerpToPositionCR(position, duration));
    }

    // lerp to target rotation over specified duration
    public Coroutine LerpToRotation(Vector3 rotation, float duration=0.1f)
    {
        IsRotating = true;
        return StartCoroutine(LerpToRotationCR(rotation, duration));
    }

    // move to target zone, lerping over a duration if specified
    public Coroutine MoveToZone(CardZone targetZone, 
        float duration=0, float hoverHeight=0.1f)
    {
        //reassign zone
        OccupiedZone = targetZone;
        targetZone.AddCard(this);   //move into lerping function if lerping?
        //move to zone
        if (duration > 0)
        {
            return LerpToPosition(targetZone.GetHoverPosition(hoverHeight), duration);
        }
        transform.position = targetZone.GetHoverPosition(hoverHeight);
        return null;
    }

    // lerps rotation to match new orientation over given duration
    public void LerpOrientation(FaceOrientation newFOrient,
        BodyOrientation newBOrient, float duration = 0.1f)
    {
        if (fOrient == newFOrient && bOrient == newBOrient) return;
        fOrient = newFOrient;
        bOrient = newBOrient;
        //determine new rotation
        Vector3 targetRotation;
        if (newFOrient == FaceOrientation.FaceUp)
        {
            targetRotation = newBOrient == BodyOrientation.Normal ?
                FACE_UP_NORMAL_ROTATION : FACE_UP_SIDE_ROTATION;
        }
        else  //face down
        {
            targetRotation = newBOrient == BodyOrientation.Normal ?
                FACE_DOWN_NORMAL_ROTATION : FACE_DOWN_SIDE_ROTATION;
        }
        //rotate
        if (duration > 0)
        {
            LerpToRotation(targetRotation, duration);
        }
        else
        {
            CardBody.transform.rotation = Quaternion.Euler(targetRotation);
        }
    }

    /* Callback functions */

    public virtual void OnSelectedFromHand()
    {
        GetPlayableZones().ForEach(z => CardZoneHighlighter.Instance.
            HighlightZone(z, CardZoneHighlighter.HighlightType.Friendly));
    }

    public virtual void OnDeselectedFromHand()
    {
        GetPlayableZones().ForEach(z => CardZoneHighlighter.Instance.
            UnhighlightZone(z));
    }

    public virtual void OnPlayedFromHand(PlayType pType, CardZone targetZone)
    {
        TurnsSincePlayed = 0;
        TurnManager.Instance.TurnStartEvent += OnTurnStart;
    }

    public virtual void OnPlayableZoneSelected(CardZone playableZone)
    {

    }

    public virtual void OnTargetSelected(CardZone targetZone)
    {

    }

    public virtual void OnSentToGraveyard(Graveyard gy)
    {
        TurnsSincePlayed = 0;
        TurnManager.Instance.TurnStartEvent -= OnTurnStart;
        CardSentToGraveyardEvent?.Invoke(this, gy);
    }

    public virtual void OnZoneMouseEnter()
    {

    }

    public virtual void OnZoneMouseExit()
    {

    }

    public virtual void OnBattleZoneLeftClick()
    {

    }

    protected virtual void OnTurnStart(int turnNum)
    {
        TurnsSincePlayed++;
    }

    private IEnumerator LerpToPositionCR(Vector3 position, float duration)
    {
        Vector3 startPosition = transform.position;
        float startTime = Time.time;
        for (float elapsed = 0; elapsed < duration;
             elapsed = Time.time - startTime)
        {
            float lerpPercent = elapsed / duration;
            transform.position = Vector3.Lerp(
                startPosition, position, lerpPercent);
            yield return null;
        }
        transform.position = position;
        IsMoving = false;
    }

    private IEnumerator LerpToRotationCR(Vector3 rotation, float duration)
    {
        Vector3 startRotation = CardBody.transform.rotation.eulerAngles;
        float startTime = Time.time;
        for (float elapsed = 0; elapsed < duration;
             elapsed = Time.time - startTime)
        {
            float lerpPercent = elapsed / duration;
            CardBody.transform.rotation = Quaternion.Euler(Vector3.Lerp(
                startRotation, rotation, lerpPercent));
            yield return null;
        }
        CardBody.transform.rotation = Quaternion.Euler(rotation);
        IsRotating = false;
    }
}

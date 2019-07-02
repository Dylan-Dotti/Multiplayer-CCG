using System.Collections.Generic;
using UnityEngine;

public class CardZoneHighlighter : MonoBehaviour
{
    public enum HighlightType { Friendly, Enemy }

    public static CardZoneHighlighter Instance { get; private set; }

    [SerializeField] private GameObject friendlyZoneHighlight;
    [SerializeField] private GameObject enemyZoneHighlight;

    private Dictionary<CardZone, GameObject> activeZoneHighlights;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            activeZoneHighlights = new Dictionary<CardZone, GameObject>();
        }
    }

    public void HighlightZone(CardZone zone, HighlightType hType)
    {
        if (!activeZoneHighlights.ContainsKey(zone))
        {
            GameObject highlight = null;
            switch (hType)
            {
                case HighlightType.Friendly:
                    highlight = Instantiate(friendlyZoneHighlight);
                    break;
                case HighlightType.Enemy:
                    highlight = Instantiate(enemyZoneHighlight);
                    break;
            }
            highlight.transform.parent = transform;
            highlight.transform.position = zone.GetHoverPosition(1.5f);
            activeZoneHighlights.Add(zone, highlight);
        }
        else
        {
            Debug.Log("Zone already highlighted, unhighlight first");
        }
    }

    public void UnhighlightZone(CardZone zone)
    {
        if (activeZoneHighlights.ContainsKey(zone))
        {
            Destroy(activeZoneHighlights[zone]);
            activeZoneHighlights.Remove(zone);
        }
        else
        {
            Debug.Log("Zone is not highlighted");
        }
    }

    public void UnhighlightAllZones()
    {
        foreach (CardZone highlightedZone in activeZoneHighlights.Keys)
        {
            Destroy(activeZoneHighlights[highlightedZone]);
        }
        activeZoneHighlights.Clear();
    }
}

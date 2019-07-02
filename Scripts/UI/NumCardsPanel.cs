using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumCardsPanel : MonoBehaviour
{
    private RectTransform rTransform;
    private RectTransform numberRTransform;
    private Text numberText;
    private Vector2 padding = new Vector2(20, 20);

    private CardZone monitoredZone;
    private string displayString;

    private void Awake()
    {
        rTransform = GetComponent<RectTransform>();
        numberText = GetComponentInChildren<Text>();
        numberRTransform = numberText.GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        numberText.text = string.Format(
            displayString, monitoredZone.NumOccupants);
        rTransform.sizeDelta = numberRTransform.sizeDelta + padding;
        rTransform.position = Input.mousePosition;
    }

    public void Activate(CardZone zoneToMonitor, 
        string formattedDisplayString="{0} Cards")
    {
        monitoredZone = zoneToMonitor;
        displayString = formattedDisplayString;
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}

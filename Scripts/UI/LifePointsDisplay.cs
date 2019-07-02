using UnityEngine;
using UnityEngine.UI;

public class LifePointsDisplay : MonoBehaviour
{
    [SerializeField] private Duelist duelist;

    private Text lifePointsText;
    int currentDisplayedLP;

    private void Awake()
    {
        lifePointsText = GetComponent<Text>();
    }

    private void LateUpdate()
    {
        //calculate delta
        int lpDelta = 0;
        int lpDifference = Mathf.Abs(
            duelist.LifePoints - currentDisplayedLP);
        if (lpDifference > 5000)
        {
            lpDelta = 70;
        }
        else if (lpDifference > 1000)
        {
            lpDelta = 35;
        }
        else if (lpDifference > 100)
        {
            lpDelta = 15;
        }
        else if (lpDifference > 10)
        {
            lpDelta = 5;
        }
        else
        {
            lpDelta = 1;
        }
        //update display
        if (duelist.LifePoints > currentDisplayedLP)
        {
            currentDisplayedLP += lpDelta;
        }
        else if (duelist.LifePoints < currentDisplayedLP)
        {
            currentDisplayedLP -= lpDelta;
        }
        lifePointsText.text = "LP: " + currentDisplayedLP;
    }
}

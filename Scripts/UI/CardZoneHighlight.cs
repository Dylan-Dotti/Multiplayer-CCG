using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardZoneHighlight : MonoBehaviour
{
    private Renderer rend;
    private float startAlpha;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        startAlpha = rend.material.color.a;
    }

    private void Start()
    {
        StartCoroutine(LerpMaterialPeriodic());
    }

    private IEnumerator LerpMaterialPeriodic()
    {
        float startTime = Time.time;
        while (true)
        {
            Material currMaterial = rend.material;
            float lerpPercent = Mathf.PingPong(
                Time.time - startTime, startAlpha);
            rend.material.color = new Color(currMaterial.color.r, 
                currMaterial.color.g, currMaterial.color.b, lerpPercent);
            yield return null;
        }
    }
}

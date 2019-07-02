using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DuelistNameDisplay : MonoBehaviour
{
    [SerializeField] private Duelist duelist;

    private Text duelistNameText;

    private void Awake()
    {
        duelistNameText = GetComponent<Text>();
    }

    private void LateUpdate()
    {
        duelistNameText.text = duelist.DuelistName;
    }
}

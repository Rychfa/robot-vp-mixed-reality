using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonsHandler : MonoBehaviour
{
    // public GameObject robot;
    // private Button _resetButton;

    // private void Awake()
    // {
    //     // _resetButton = GameObject.Find("Reset Button").GetComponent<Button>();
    //     // _resetButton.GetComponentInChildren<Text>().text = "la di da";
    //     // _resetButton.transform.position = new Vector3(0.1f, 0.1f);
    //     // var resetRect = _resetButton.GetComponent<RectTransform>();
    //     // resetRect.rect = 
    // }

    public void ClearRobot()
    {
        if (PlacementController.spawnedRobot != null)
        {
            Destroy(PlacementController.spawnedRobot);
        }
            
    }
}
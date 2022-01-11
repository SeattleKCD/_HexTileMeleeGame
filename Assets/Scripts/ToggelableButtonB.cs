using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggelableButtonB : MonoBehaviour
{

    private Button thisButton;
    private Image thisImage;

    // Start is called before the first frame update
    void Start()
    {
        thisButton = GetComponent<Button>();
        thisImage = GetComponent<Image>();
        MapManager.FighterPEngaged += ToggleButton;
    }

    public void ToggleButton(bool isEngaged)
    {
        thisButton.interactable = !isEngaged;
        if (isEngaged)
            thisImage.color = Color.grey;
        else
            thisImage.color = Color.white;
    }
}

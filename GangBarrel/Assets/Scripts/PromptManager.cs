using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PromptManager : MonoBehaviour
{
    // The references are in the Level1 scene and can be found following the given path, starting at the root ScreenSpaceCanvas.
    public GameObject interactionPromptBackground;  // ScreenSpaceCanvas/InteractionPromptBackground
    public TextMeshProUGUI promptText;              // ScreenSpaceCanvas/InteractionPromptBackground/InteractionPromptText
    public TextMeshProUGUI descriptionText;         // ScreenSpaceCanvas/InteractionPromptBackground/DescriptionBackground/DescriptionText

    // Chest interaction objects
    public GameObject chestContentPanelObject;      // ScreenSpaceCanvas/ChestContentPanel
    public Button xButton;                          // ScreenSpaceCanvas/ChestContentPanel/X-Button
    public GameObject chestContentParentObject;     // ScreenSpaceCanvas/ChestContentPanel/Background/ScrollArea/ItemsMask/Content

    private void Start()
    {
        interactionPromptBackground.SetActive(false);
    }
    
    public void ShowInteractionPrompt(string prompt)
    {
        promptText.text = prompt;
        interactionPromptBackground.SetActive(true);
    }

    public void StopInteractionPrompt()
    {
        interactionPromptBackground.SetActive(false);
    }

    public void SetDescriptionTextForItem(Item item)
    {
        descriptionText.text = item.description;
    }
    
}
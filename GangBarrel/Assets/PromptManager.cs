using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PromptManager : MonoBehaviour
{
    public GameObject interactionPromptBackground;
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI descriptionText;

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

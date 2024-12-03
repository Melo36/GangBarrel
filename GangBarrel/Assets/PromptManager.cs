using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PromptManager : MonoBehaviour
{
    public static GameObject interactionPromptBackground;
    private static TextMeshProUGUI promptText;

    private void Start()
    {
        interactionPromptBackground = this.gameObject;
        promptText = interactionPromptBackground.GetComponentInChildren<TextMeshProUGUI>();
        interactionPromptBackground.SetActive(false);
    }

    public static void ShowInteractionPrompt(string prompt)
    {
        promptText.text = prompt;
        interactionPromptBackground.SetActive(true);
    }

    public static void StopInteractionPrompt()
    {
        interactionPromptBackground.SetActive(false);
    }
    
}

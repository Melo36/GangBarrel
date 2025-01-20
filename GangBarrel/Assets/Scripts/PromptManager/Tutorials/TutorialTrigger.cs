using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public Tutorial tutorial;
    public PromptManager promptManager;

    private void Awake()
    {
        promptManager = FindObjectOfType<PromptManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            bool useMultipleImages = !(tutorial.explanationImage2 == null && tutorial.explanationImage3 == null);
            promptManager.OpenTutorialPanel(tutorial, useMultipleImages);
            Destroy(gameObject);   
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            promptManager.CloseTutorialPanel();   
        }
    }
}

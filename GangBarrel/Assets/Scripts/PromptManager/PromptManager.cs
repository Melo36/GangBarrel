using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PromptManager : MonoBehaviour
{
    // The references are in the Level1 scene and can be found following the given path, starting at the root ScreenSpaceCanvas.
    [Header("Prompts")]
    public GameObject interactionPromptBackground;  // ScreenSpaceCanvas/InteractionPromptBackground
    public TextMeshProUGUI promptText;              // ScreenSpaceCanvas/InteractionPromptBackground/InteractionPromptText
    public TextMeshProUGUI descriptionText;         // ScreenSpaceCanvas/InteractionPromptBackground/DescriptionBackground/DescriptionText

    // Chest interaction objects
    public GameObject chestContentPanelObject;      // ScreenSpaceCanvas/ChestContentPanel
    public Button xButton;                          // ScreenSpaceCanvas/ChestContentPanel/X-Button
    public GameObject chestContentParentObject;     // ScreenSpaceCanvas/ChestContentPanel/Background/ScrollArea/ItemsMask/Content

    [Header("Game Over")] public GameObject gameOverPanel;
    [Header("Pause")] public GameObject pausePanel;

    [Header("Tutorials")]
    [SerializeField]private GameObject tutorialPanel;
    
    [SerializeField]private GameObject oneImagePanel;
    [SerializeField]private Image singleImage;
    
    [SerializeField]private GameObject multipleImagesPanel;
    [SerializeField]private Image firstImage;
    [SerializeField]private Image secondImage;
    [SerializeField]private Image thirdImage;
    
    [SerializeField]private TextMeshProUGUI headerText;
    [SerializeField]private TextMeshProUGUI tutorialDescriptionText;

    [SerializeField] private Button xButtonTutorial;

    //audio
    [SerializeField] public AudioSource pickupBarrel;
    [SerializeField] public AudioSource placeBarrel;
    [SerializeField] public AudioSource openUI;


    [Header("References")] [SerializeField]
    private ItemUsage itemUsage;

    private void Awake()
    {
        itemUsage = FindObjectOfType<ItemUsage>();
        pickupBarrel = GameObject.FindGameObjectWithTag("PickUp").GetComponent<AudioSource>();
        placeBarrel = GameObject.FindGameObjectWithTag("Place").GetComponent<AudioSource>();
        openUI = GameObject.FindGameObjectWithTag("OpenUI").GetComponent<AudioSource>();
    }

    private void Start()
    {
        interactionPromptBackground.SetActive(false);
        xButtonTutorial.onClick.AddListener(CloseTutorialPanel);
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

    public void OpenTutorialPanel(Tutorial tutorial, bool multipleImages)
    {
        oneImagePanel.SetActive(!multipleImages);
        multipleImagesPanel.SetActive(multipleImages);

        itemUsage.CancelItemPlacement();
        
        // Pause the game
        Time.timeScale = 0;
        
        // Set the images
        singleImage.sprite = tutorial.explanationImage1;
        if (multipleImages)
        {
            firstImage.sprite = tutorial.explanationImage1;
            secondImage.sprite = tutorial.explanationImage2;
            thirdImage.sprite = tutorial.explanationImage3;
            
            // Make sure to deactivate images, that are not used
            if(tutorial.explanationImage2 == null)
                secondImage.gameObject.SetActive(false);
            if (tutorial.explanationImage3 == null)
                thirdImage.gameObject.SetActive(false);
        }
        
        // Set the texts
        headerText.text = tutorial.header;
        tutorialDescriptionText.text = tutorial.description;
        
        // Activate the panel
        tutorialPanel.SetActive(true);
        openUI.Play();
    }

    public void ShowGameOverScreen()
    {
        openUI.Play();
        gameOverPanel.SetActive(true);
    }

    public void OpenPauseScreen()
    {
        Time.timeScale = 0f;
        openUI.Play();
        pausePanel.gameObject.SetActive(true);
    }

    public void ClosePauseScreen()
    {
        Time.timeScale = 1f;
        openUI.Play();
        pausePanel.gameObject.SetActive(false);
    }
    public void CloseTutorialPanel()
    {
        // Go back to the game
        Time.timeScale = 1;
        openUI.Play();
        tutorialPanel.SetActive(false);
    }
    
}
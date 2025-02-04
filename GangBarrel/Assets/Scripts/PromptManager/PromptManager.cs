using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    //[Header("Game Over")] public GameObject gameOverPanel;
    [Header("Pause")] public GameObject pausePanel;
    public Button pauseButton;

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

    [SerializeField] private Button backToGameButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button resetLevelButton;
    [SerializeField] private Button exitGameButton;

    [SerializeField] private TextMeshProUGUI pauseGameOverText;
    
    private void Awake()
    {
        itemUsage = FindObjectOfType<ItemUsage>();
        
        // find all relevant references (TODO: could also be done more smart, but this is a quick solution to fix missing references)
        interactionPromptBackground = GameObject.Find("InteractionPromptBackground");
        promptText = GameObject.Find("InteractionPromptText").GetComponent<TextMeshProUGUI>();
        descriptionText = GameObject.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
        chestContentPanelObject = GameObject.Find("ChestContentPanel");
        xButton = GameObject.Find("XButton").GetComponent<Button>();
        chestContentParentObject = GameObject.Find("ChestContentPanelObject");
        pausePanel = GameObject.Find("PauseScreen");
        pauseButton = GameObject.Find("PauseButton").GetComponent<Button>();
        tutorialPanel = GameObject.Find("TutorialPanel");
        oneImagePanel = GameObject.Find("OneImage");
        singleImage = GameObject.Find("SingleImage").GetComponent<Image>();
        multipleImagesPanel = GameObject.Find("MoreImages");
        firstImage = GameObject.Find("MoreImages1").GetComponent<Image>();
        secondImage = GameObject.Find("MoreImages2").GetComponent<Image>();
        thirdImage = GameObject.Find("MoreImages3").GetComponent<Image>();
        headerText = GameObject.Find("HeaderTextTutorial").GetComponent<TextMeshProUGUI>();
        tutorialDescriptionText = GameObject.Find("TutorialContentDescriptionText").GetComponent<TextMeshProUGUI>();
        xButtonTutorial = GameObject.Find("XButtonTutorial").GetComponent<Button>();
        pickupBarrel = GameObject.FindGameObjectWithTag("PickUp").GetComponent<AudioSource>();
        placeBarrel = GameObject.FindGameObjectWithTag("Place").GetComponent<AudioSource>();
        openUI = GameObject.FindGameObjectWithTag("OpenUI").GetComponent<AudioSource>();
        backToGameButton = GameObject.Find("BackToGameButton").GetComponent<Button>();
        mainMenuButton = GameObject.Find("MainMenuButton").GetComponent<Button>();
        resetLevelButton = GameObject.Find("ResetLevelButton").GetComponent<Button>();
        exitGameButton = GameObject.Find("ExitGameButton").GetComponent<Button>();
        pauseGameOverText = GameObject.Find("PauseGameOverText").GetComponent<TextMeshProUGUI>();
        
        xButton.onClick.AddListener(CloseChestContentWindow);
        pauseButton.onClick.AddListener(OpenPauseScreen);
        
        backToGameButton.onClick.AddListener(ClosePauseScreen);
        mainMenuButton.onClick.AddListener(() =>
        {
            LoadScene("Main Menu");
        });
        resetLevelButton.onClick.AddListener(ResetLevel);
        exitGameButton.onClick.AddListener(ExitGame);
        
        //disable all those which are not needed
        interactionPromptBackground.SetActive(false);
        chestContentPanelObject.SetActive(false);
        pausePanel.SetActive(false);
        tutorialPanel.SetActive(false);
        oneImagePanel.SetActive(false);
        multipleImagesPanel.SetActive(false);
        
        tutorialPanel.SetActive(false);
    }
    
    public void ExitGame()
    {
        Application.Quit();
    }
    
    public void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }
    
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    private void CloseChestContentWindow()
    {
        chestContentPanelObject.SetActive(false);
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
        
        Debug.Log("Time scale 0, OpenTutorialPanel");
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
        pauseGameOverText.text = "Game Over";
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        //gameOverPanel.SetActive(true);
    }

    public void OpenPauseScreen()
    {
        Debug.Log("Time scale 0, OpenPauseScreen");
        Time.timeScale = 0f;
        pauseGameOverText.text = "Pause";
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
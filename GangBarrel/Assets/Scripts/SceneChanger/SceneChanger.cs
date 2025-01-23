using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger Instance; // Singleton instance

    private int currentLevelIndex = 0;

    private void Awake()
    {
        // Ensure this GameObject persists across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadNextLevel()
    {
        currentLevelIndex++;
        string nextLevelName = "Level " + currentLevelIndex;
        if (SceneUtility.GetBuildIndexByScenePath(nextLevelName) == -1)
        {
            SceneManager.LoadScene("Main Menu");
            return;
        }
        SceneManager.LoadScene(nextLevelName);
    }

    public void LoadCongratsScreen()
    {
        SceneManager.LoadScene("Congrats");
    }

    public void RestartGame()
    {
        currentLevelIndex = 0;
        LoadNextLevel();
    }

    public void nextLevelButtonClicked()
    {
        Debug.Log("Next level button clicked");
        SceneChanger.Instance.LoadNextLevel();
    }
}

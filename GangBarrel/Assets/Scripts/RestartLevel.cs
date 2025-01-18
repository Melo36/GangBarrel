using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartLevel : MonoBehaviour
{
    private int currentLevel = 1;
    public int amountOfLevels = 3;
    
    public void restartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void startNextLevel()
    {
        currentLevel++;
        if (currentLevel > amountOfLevels)
        {
            gameObject.SetActive(false);
            return;
        }
        SceneManager.LoadScene("Level " + currentLevel);
    }
}

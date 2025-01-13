using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtons : MonoBehaviour
{
    
    public void PressButton(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void quit()
    {
        Application.Quit();
    }

}

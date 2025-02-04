using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void loadNextLevel()
    {
        SceneChanger.Instance.LoadNextLevel();
    }

    public void loadCongratsScreen()
    {
        SceneChanger.Instance.LoadCongratsScreen();
    }
}

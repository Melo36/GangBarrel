using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveGrid : MonoBehaviour
{
    public string gridName = "Grid"; // Name of the Grid GameObject
    public TextMeshProUGUI levelName;

    public void MoveGridToNewScene()
    {
        // Find the Grid GameObject in the current scene
        GameObject grid = GameObject.Find(gridName);
        if (grid == null)
        {
            Debug.LogError($"GameObject with name '{gridName}' not found in the current scene.");
            return;
        }
        
        // Detach the Grid from its parent, if it has one
        if (grid.transform.parent != null)
        {
            grid.transform.SetParent(null); // Unparent the Grid
        }

        // Create a new scene
        Scene newScene = SceneManager.CreateScene(levelName.text);

        // Move the Grid GameObject to the new scene
        SceneManager.MoveGameObjectToScene(grid, newScene);

        // Save the scene to a folder named "Scenes" in the project's "Assets" directory
        string scenePath = "Assets/Scenes/CustomLevels" + levelName.text + ".unity";

        // Ensure the "Scenes" folder exists
        if (!System.IO.Directory.Exists("Assets/Scenes"))
        {
            System.IO.Directory.CreateDirectory("Assets/Scenes");
        }

        // Save the scene
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(newScene, scenePath);

        Debug.Log($"'{gridName}' has been moved to the new scene: {newScene.name} and saved to {scenePath}");
    }
}
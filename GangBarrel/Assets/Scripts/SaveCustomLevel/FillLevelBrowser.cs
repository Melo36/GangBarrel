using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FillLevelBrowser : MonoBehaviour
{
    private string filePath = "Assets/CustomLevels/";
    public GameObject button;
    
    void Start()
    {
        if (Directory.Exists(filePath))
        {
            string[] files = Directory.GetFiles(filePath, "*.json");

            foreach (string file in files)
            {
                GameObject newButton = Instantiate(button, transform);
                string fileName = Path.GetFileName(file);
                newButton.GetComponentInChildren<TextMeshProUGUI>().text = fileName.Split(".")[0];
            }

            foreach (string file in files)
            {
                Debug.Log("File found: " + file);
            }
        }
        else
        {
            Debug.LogError("Directory does not exist: " + filePath);
        }
    }
}

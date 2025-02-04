using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FillLevelBrowser : MonoBehaviour
{
    private string filePath;
    public GameObject button;
    
    void Start()
    {
        filePath = Application.persistentDataPath;
        if (Directory.Exists(filePath))
        {
            string[] files = Directory.GetFiles(filePath, "*.json");

            foreach (string file in files)
            {
                GameObject newButton = Instantiate(button, transform);
                string fileName = Path.GetFileNameWithoutExtension(file);
                newButton.GetComponentInChildren<TextMeshProUGUI>().text = fileName;
            }
        }
        else
        {
            Debug.LogError("Directory does not exist: " + filePath);
        }
    }
}
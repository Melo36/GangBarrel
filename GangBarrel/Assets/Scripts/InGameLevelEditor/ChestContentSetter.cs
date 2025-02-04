using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChestContentSetter : MonoBehaviour
{
    public GameObject chestContentUI;

    // Barrels, Bullets, Planks, Fuses
    private int[] objectsInChest = new int[4];

    public Button doneButton;
    public Button closeButton;

    private string filePath;

    private void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "chest_content.txt");
        doneButton.onClick.AddListener(setObjectsInsideChest);
        doneButton.onClick.AddListener(SaveArrayToFile);
        doneButton.onClick.AddListener(closeUI);
        closeButton.onClick.AddListener(closeUI);
    }

    private void setObjectsInsideChest()
    {
        TMP_InputField[] inputFields = chestContentUI.GetComponentsInChildren<TMP_InputField>(true);
        for (int i=0;i<inputFields.Length;i++)
        {
            if (inputFields[i].text.Length == 0)
            {
                inputFields[i].text = "0";
            }
            objectsInChest[i] = int.Parse(inputFields[i].text);
        }
    }

    public void openUI()
    {
        chestContentUI.SetActive(true);
    }
    private void closeUI()
    {
        chestContentUI.SetActive(false);
    }
    
    private void SaveArrayToFile()
    {
        // Convert the array to a single line with commas separating values
        string arrayContent = string.Join(",", objectsInChest) + "\n";

        // Write the string to a file
        File.AppendAllText(filePath, arrayContent);

        Debug.Log($"Array written to file at: {filePath}");
    }
}

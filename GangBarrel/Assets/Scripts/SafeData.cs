using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SafeData : MonoBehaviour
{
    public int levels;
    public Level level = new Level();

    public void SaveToJson()
    {

        string data = JsonUtility.ToJson(level);
        string filePath = Application.persistentDataPath + "/Data.json";
        Debug.Log(filePath);
        System.IO.File.WriteAllText(filePath, data);
        Debug.Log("Saved");
    }

    private void Start()
    {
        LoadFromJson();
    }

    public void LoadFromJson()
    {
        string filePath = Application.persistentDataPath + "/Data.json";
        string data = System.IO.File.ReadAllText(filePath);

        level = JsonUtility.FromJson<Level>(data);
        Debug.Log("Loaded");
    }
}

[System.Serializable]
public class Level
{
    public int level;
    private SafeData safeData;

    public void Start()
    {
        if (safeData.levels == null)
            level = 0;
        else
            level = safeData.levels + 1;
    }
}
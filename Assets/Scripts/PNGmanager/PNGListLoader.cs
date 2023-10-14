using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using TMPro;

public class PNGListLoader : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform contentPanel;
    public PixelArtEditor pixelArtEditor;

    void Start()
    {
        LoadPNGList();
    }

    public void LoadPNGList()
    {
        // Get directory
        string directoryPath = Path.Combine(Application.persistentDataPath, "ExportedPng"); // Application.dataPath -> Application.persistentDataPath

        if (!Directory.Exists(directoryPath))
        {
            Debug.LogWarning("디렉토리가 존재하지 않습니다. 불러올 파일이 없습니다.");
            return;
        }

        // Get all json files
        string[] filePaths = Directory.GetFiles(directoryPath, "*.png");
        
        // Clear all existing buttons
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Create buttons for each file
        foreach (string filePath in filePaths)
        {
            GameObject button = Instantiate(buttonPrefab, contentPanel);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            button.GetComponentInChildren<TMP_Text>().text = fileName;
            button.GetComponent<Button>().onClick.AddListener(() => pixelArtEditor.LoadPixelArt(fileName));
        }
    }

    // 리스트 리프레시 (새로 저장할때)
    public void RefreshPNGList()
    {
        LoadPNGList();
    }
}

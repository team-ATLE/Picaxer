using UnityEngine;
using UnityEngine.UI;
using TMPro;  // TextMeshPro를 위한 네임스페이스
using System.IO;
using System.Collections.Generic;

public class PNGmanager : MonoBehaviour
{
    public GameObject fileEntryPrefab; 
    public Transform fileListParent;

    private List<string> pngFiles = new List<string>();

    void Start()
    {
        RefreshFileList();
        DisplayFiles();
    }

    private void RefreshFileList()
    {
        string path = Application.dataPath + "/ExportedPng/";
        pngFiles.Clear();
        pngFiles.AddRange(Directory.GetFiles(path, "*.png"));
        Debug.Log($"Total PNG files found: {pngFiles.Count}");
    }

    private void DisplayFiles()
    {
        // Clear previous entries
        foreach (Transform child in fileListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (string pngFile in pngFiles)
        {
            GameObject entry = Instantiate(fileEntryPrefab, fileListParent);

            // Image display
            RawImage imageDisplay = entry.transform.Find("ImageDisplay").GetComponent<RawImage>();
            if (imageDisplay)
            {
                imageDisplay.texture = LoadTexture(pngFile);
            }
            else
            {
                Debug.LogWarning("RawImage component is missing in the prefab!");
            }

            // File name display
            TextMeshProUGUI fileNameText = entry.transform.Find("FileName").GetComponent<TextMeshProUGUI>();
            if (fileNameText)
            {
                fileNameText.text = Path.GetFileNameWithoutExtension(pngFile);
            }
            else
            {
                Debug.LogWarning("TextMeshProUGUI component is missing in the prefab!");
            }

            // Delete button
            Button deleteButton = entry.transform.Find("DeleteButton").GetComponent<Button>();
            if (deleteButton)
            {
                string currentFile = pngFile;  // Capture the string by value
                deleteButton.onClick.AddListener(() => 
                {
                    DeleteFile(currentFile);
                    Destroy(entry);
                });
            }
            else
            {
                Debug.LogWarning("Button component is missing in the prefab!");
            }
        }
    }

    private Texture2D LoadTexture(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }

    private void DeleteFile(string filePath)
    {
        File.Delete(filePath);
        RefreshFileList();
        DisplayFiles();
    }
}

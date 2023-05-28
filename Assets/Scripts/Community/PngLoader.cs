using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using TMPro;

public class PngLoader : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform contentPanel;
    public CreatePost createPost;

    void Start()
    {
        LoadPng();
    }

    public void LoadPng()
    {
        // Get directory
        string directoryPath = Path.Combine(Application.dataPath, "ExportedPng");

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
            Texture2D texture = LoadTextureFromFile(filePath);
            button.GetComponentInChildren<RawImage>().texture = texture; 
            button.GetComponentInChildren<Button>().onClick.AddListener(() => createPost.UpdateImgURL(fileName)); // 여기서 픽셀아트 로드(나는 value change)
        }
    }

    Texture2D LoadTextureFromFile(string filePath)
    {
        // Read the image file data
        byte[] imageData = System.IO.File.ReadAllBytes(filePath);

        // Create a new Texture2D
        Texture2D texture = new Texture2D(2, 2);
        
        // Load the image data into the texture
        bool loadSuccess = texture.LoadImage(imageData);

        if (!loadSuccess)
        {
            return null;
        }

        return texture;
    }

    // 리스트 리프레시 (새로 저장할때)
    public void RefreshPng()
    {
        LoadPng();
    }
}

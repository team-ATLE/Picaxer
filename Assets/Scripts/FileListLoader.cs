using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class FileListLoader : MonoBehaviour
{
    public GameObject buttonPrefab;
    private string fileDirectory;
    private string selectedFile;
    
    public string SelectedFile { get { return selectedFile; } }

    private void Awake()
    {
        fileDirectory = Path.Combine(Application.dataPath, "SavedPixelArts");
    }

    private void Start()
    {
        LoadFileList();
    }

    private void LoadFileList()
    {
        string[] files = Directory.GetFiles(fileDirectory, "*.json");

        foreach (string file in files)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, transform);
            Button button = buttonObj.GetComponent<Button>();


            Text buttonText = buttonObj.GetComponentInChildren<Text>();

            string fileName = Path.GetFileName(file);
            buttonText.text = fileName;

            button.onClick.AddListener(() => OnFileSelected(fileName));
        }
    }

    public PixelArtDisplay pixelArtDisplay;

    private void OnFileSelected(string fileName)
    {
        selectedFile = fileName;
        Debug.Log("Selected file: " + selectedFile);

        pixelArtDisplay.LoadPixelArt(selectedFile);

    }

}

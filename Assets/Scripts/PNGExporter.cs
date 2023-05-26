using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;

public class PNGExporter : MonoBehaviour
{
    public FileListLoader fileListLoader;
    public PixelArtDisplay pixelArtDisplay;
    public string exportDirectory = Path.Combine(Application.dataPath, "ExportedPixelArts");

    private void Start()
    {
        if (!Directory.Exists(exportDirectory))
        {
            Directory.CreateDirectory(exportDirectory);
        }
    }

    public void ExportPixelArtToPNG()
    {
        string fileName = fileListLoader.SelectedFile;
        string filePath = Path.Combine(exportDirectory, fileName.Replace(".json", ".png"));

        List<GameObject> pixelButtons = pixelArtDisplay.GetPixelButtons();

        Texture2D texture = new Texture2D(pixelArtDisplay.GridSize, pixelArtDisplay.GridSize);
        for (int i = 0; i < pixelArtDisplay.GridSize * pixelArtDisplay.GridSize; i++)
        {
            texture.SetPixel(i % pixelArtDisplay.GridSize, i / pixelArtDisplay.GridSize, pixelButtons[i].GetComponent<Image>().color);
        }
        texture.Apply();

        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(filePath, pngData);

        Debug.Log("Exported pixel art to: " + filePath);
    }
}

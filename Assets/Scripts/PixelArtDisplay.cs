using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;

public class PixelArtDisplay : MonoBehaviour
{
    [System.Serializable]
    public class PixelData
    {
        public Color color;
    }

    [System.Serializable]
    public class PixelArtData
    {
        public int size;
        public List<PixelData> pixels;
    }

    public GameObject pixelButtonPrefab;
    public Transform gridParent;
    public int gridSize;
    public int GridSize { get { return gridSize; } }

    public List<GameObject> GetPixelButtons()
    {
        return pixelButtons;
    }


    private List<GameObject> pixelButtons = new List<GameObject>();

    public void LoadPixelArt(string fileName)
    {
        ClearGrid();

        string filePath = Path.Combine(Application.dataPath, "SavedPixelArts", fileName);
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            PixelArtData pixelArtData = JsonUtility.FromJson<PixelArtData>(jsonData);

            gridSize = pixelArtData.size;
            CreateGrid(pixelArtData.pixels);
        }
    }

    private void CreateGrid(List<PixelData> pixels)
    {
        for (int i = 0; i < gridSize * gridSize; i++)
        {
            GameObject buttonObj = Instantiate(pixelButtonPrefab, gridParent);
            buttonObj.GetComponent<Image>().color = pixels[i].color;
            pixelButtons.Add(buttonObj);
        }
    }

    private void ClearGrid()
    {
        foreach (GameObject button in pixelButtons)
        {
            Destroy(button);
        }
        pixelButtons.Clear();
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class PixelArtEditor : MonoBehaviour
{
    public GameObject pixelPrefab;
    public RectTransform grid;
    public TMP_InputField widthInput;
    public TMP_InputField heightInput;
    public Button applyButton;
    public int pixelSize;

    // 사용 가능한 색상 옵션
    public Color[] colorOptions;

    // 현재 선택된 색상
    private Color selectedColor = Color.black;

    // 초기화, 유니티에서는 Awake로 초기화한다
    void Awake()
    {
        colorOptions = new Color[]
        {
            Color.black,
            Color.white,
            Color.red,
            Color.blue,
            Color.green
        };
    }

    void Start()
    {
        applyButton.onClick.AddListener(ApplyGridSize);
    }

    // 그리드 크기 적용 함수
    void ApplyGridSize()
    {
        int width = int.Parse(widthInput.text);
        int height = int.Parse(heightInput.text);
        GenerateGrid(width, height);
    }

    // 그리드 생성 함수
    void GenerateGrid(int width, int height)
    {
        // 기존 그리드 내용 삭제
        foreach (Transform child in grid)
        {
            Destroy(child.gameObject);
        }

        // 새로운 그리드 생성
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject pixel = Instantiate(pixelPrefab, grid);
                RectTransform rectTransform = pixel.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(x * pixelSize, -y * pixelSize);
                rectTransform.sizeDelta = new Vector2(pixelSize, pixelSize);

                // 클릭 이벤트 연결
                Button pixelButton = pixel.GetComponent<Button>();
                pixelButton.onClick.AddListener(() => ChangeColor(pixelButton));
            }
        }
    }

    // 픽셀 색상 변경 함수
    public void ChangeColor(Button pixelButton)
    {
        // 픽셀의 Image 컴포넌트를 가져옵니다.
        Image pixelImage = pixelButton.GetComponent<Image>();

        // 선택한 색상으로 픽셀 색상을 변경합니다.
        pixelImage.color = selectedColor;
    }

    // 색상 선택 함수
    public void SelectColor(int index)
    {
        if (index >= 0 && index < colorOptions.Length)
        {
            selectedColor = colorOptions[index];
        }
        else
        {
            Debug.LogError("Invalid color index selected.");
        }
    }

    // 픽셀 아트 데이터 저장 함수
    public void SavePixelArt()
    {
        List<Color> colors = new List<Color>();
        int width = int.Parse(widthInput.text);
        int height = int.Parse(heightInput.text);

        foreach (Transform child in grid)
        {
            Image pixelImage = child.GetComponent<Image>();
            colors.Add(pixelImage.color);
        }

        PixelArtData data = new PixelArtData(width, height, colors);
        PlayerPrefs.SetString("PixelArtWidth", width.ToString());
        PlayerPrefs.SetString("PixelArtHeight", height.ToString());
        PlayerPrefs.SetString("PixelArt", data.ToJson());
        PlayerPrefs.Save();

        Debug.Log("Pixel Art saved!");
    }



    // 픽셀 아트 데이터 불러오기 함수
    public void LoadPixelArt()
    {
        // 데이터를 불러옵니다.
        PixelArtData data = PixelArtData.Load();

        if (data == null)
        {
            Debug.LogWarning("No saved Pixel Art data found. Skipping the load.");
            return;
        }

        // 그리드 크기를 불러온 데이터에 맞게 변경합니다.
        GenerateGrid(data.width, data.height);

        int i = 0;
        int numberOfPixels = grid.childCount;
        int numberOfColors = data.colors.Count;

        if (numberOfPixels != numberOfColors)
        {
            Debug.LogError("Mismatch between the number of pixels and colors.");
            return;
        }

        // 그리드 내의 픽셀들을 순회하며 색상을 변경합니다.
        foreach (Transform child in grid)
        {
            Image pixelImage = child.GetComponent<Image>();
            pixelImage.color = data.colors[i];
            i++;
        }
    }



}

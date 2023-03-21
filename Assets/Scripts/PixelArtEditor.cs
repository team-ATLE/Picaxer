using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PixelArtEditor : MonoBehaviour
{
    public GameObject pixelPrefab;
    public RectTransform grid;
    public TMP_InputField widthInput;
    public TMP_InputField heightInput;
    public Button applyButton;
    public int pixelSize;


    // 사용 가능한 색상 옵션
    public Color[] colorOptions = {
        Color.black,
        Color.white,
        Color.red,
        Color.blue,
        Color.green
    };


    // 현재 선택된 색상
    private Color selectedColor;

    void Start()
    {
        applyButton.onClick.AddListener(ApplyGridSize);
        
    }

    void ApplyGridSize()
    {
        int width = int.Parse(widthInput.text);
        int height = int.Parse(heightInput.text);
        GenerateGrid(width, height);
    }

    void GenerateGrid(int width, int height)
    {
        // 기존 그리드 내용 삭제
        foreach (Transform child in grid)
        {
            Destroy(child.gameObject);
        }

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
                pixelButton.onClick.AddListener(() => ChangeColor(pixel));
            }
        }
    }

    public void ChangeColor(GameObject pixel)
    {
        // 픽셀의 Image 컴포넌트를 가져옵니다.
        Image pixelImage = pixel.GetComponent<Image>();

        // 선택한 색상으로 픽셀 색상을 변경합니다.
        pixelImage.color = selectedColor;
    }

    // 색상 선택 함수
    public void SelectColor(int index)
    {
        selectedColor = colorOptions[index];
    }
}

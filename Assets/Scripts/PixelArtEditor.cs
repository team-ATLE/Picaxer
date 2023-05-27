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
    public TMP_InputField nameInput;

    public Button applyButton;
    public int pixelSize;

    public Canvas canvas;

    public Button saveButton;
    public Button loadButton;
    public Button clearButton;
    

    // 사용 가능한 색상 옵션
    public Color[] colorOptions;

    // 현재 선택된 색상
    private Color selectedColor = Color.black;

    public Color defaultColor = Color.white;

    // 픽셀 아트 그리드 크기조절
    private int calculatedPixelSize;


    public ImageExporter imageExporter; // ImageExporter 참조 추가

    public Button exportButton; // Export 버튼 선언

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
        PlayerPrefs.DeleteAll(); // 이 줄을 추가하여 PlayerPrefs를 초기화합니다.
        applyButton.onClick.AddListener(ApplyGridSize);

        // 저장 로드 클리어 버튼 추가
        loadButton.onClick.AddListener(LoadColors);
        clearButton.onClick.AddListener(ClearPixelArt);
        
        saveButton.onClick.AddListener(() => SavePixelArt(nameInput.text));
        // 로드시 이름
        loadButton.onClick.AddListener(() => LoadPixelArt());


        // Export 버튼 클릭 이벤트 연결
        exportButton.onClick.AddListener(() => ExportPixelArt(nameInput.text)); 
   

    }

    // 그리드 크기 적용 함수
    void ApplyGridSize()
    {
        int width = int.Parse(widthInput.text);
        int height = int.Parse(heightInput.text);
        GenerateGrid(width, height);
    }

    // 그리드 생성 함수
    void GenerateGrid(int width, int height, List<Color> colors = null)
    {
        // 기존 그리드 내용 삭제
        foreach (Transform child in grid)
        {
            Destroy(child.gameObject);
        }

        // 그리드 크기를 조절하고 픽셀 사이즈를 계산
        ResizeGridToFitScreen(width, height);

        // 새로운 그리드 생성
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject pixel = Instantiate(pixelPrefab, grid);
                RectTransform rectTransform = pixel.GetComponent<RectTransform>();
                
                // 픽셀의 위치를 그리드의 가운데에 위치하도록 조정
                float posX = x * pixelSize - width * pixelSize / 2f + pixelSize / 2f;
                float posY = -y * pixelSize + height * pixelSize / 2f - pixelSize / 2f;
                rectTransform.anchoredPosition = new Vector2(posX, posY);

                rectTransform.sizeDelta = new Vector2(pixelSize, pixelSize);

                // 클릭 이벤트 연결
                Button pixelButton = pixel.GetComponent<Button>();
                pixelButton.onClick.AddListener(() => ChangeColor(pixelButton));

                // 기존 색상 데이터를 로드하려면 이렇게 설정
                if (colors != null)
                {
                    int index = y * width + x;
                    Image pixelImage = pixelButton.GetComponent<Image>();
                    pixelImage.color = colors[index];
                }
                else
                {
                    Image pixelImage = pixelButton.GetComponent<Image>();
                    pixelImage.color = defaultColor;
                }
            }
        }
    }

    
   private void ResizeGridToFitScreen(int width, int height, float screenPercentage = 0.8f)
    {
        // 캔버스의 사이즈
        float canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
        float canvasHeight = canvas.GetComponent<RectTransform>().rect.height;

        // 그리드를 화면의 일정 비율로 크기를 조절
        float gridSize = Mathf.Min(canvasWidth, canvasHeight) * screenPercentage;

        // 그리드의 RectTransform 컴포넌트
        RectTransform gridRectTransform = grid.GetComponent<RectTransform>();

        // 그리드의 크기를 변경합니다.
        gridRectTransform.sizeDelta = new Vector2(gridSize, gridSize);

        // 그리드를 캔버스의 중앙으로 위치시키기
        gridRectTransform.anchoredPosition = Vector2.zero;

        // 동적 픽셀 사이즈 계산
        pixelSize = Mathf.FloorToInt(gridSize / Mathf.Max(width, height));
    }



    // 드래그 
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            ColorPixelUnderMouse();
        }
    }


    // 픽셀 색상 변경 함수
    public void ChangeColor(Button pixel)
    {
        Image pixelImage = pixel.GetComponent<Image>();
        pixelImage.color = selectedColor;
    }

    // 색상 선택 함수
    public void SelectColor(int colorIndex)
    {
        if (colorIndex >= 0 && colorIndex < colorOptions.Length)
        {
            selectedColor = colorOptions[colorIndex];
        }
    }

    // 그리드의 색상 정보를 가져오는 함수
    public List<Color> GetColors()
    {
        List<Color> colors = new List<Color>();

        foreach (Transform child in grid)
        {
            Image pixelImage = child.GetComponent<Image>();
            colors.Add(pixelImage.color);
        }

        return colors;
    }

    // 그리드의 색상 정보를 저장하는 함수
    public void SaveColors()
    {
        List<Color> colors = GetColors();
        string colorData = ColorListToString(colors);
        PlayerPrefs.SetString("PixelArtColors", colorData);
        PlayerPrefs.SetInt("PixelArtWidth", int.Parse(widthInput.text));
        PlayerPrefs.SetInt("PixelArtHeight", int.Parse(heightInput.text));
        PlayerPrefs.Save();
    }

    // 그리드의 색상 정보를 불러오는 함수
    public void LoadColors()
    {
        if (PlayerPrefs.HasKey("PixelArtColors"))
        {
            string colorData = PlayerPrefs.GetString("PixelArtColors");
            List<Color> colors = StringToColorList(colorData);
            int width = PlayerPrefs.GetInt("PixelArtWidth");
            int height = PlayerPrefs.GetInt("PixelArtHeight");
            GenerateGrid(width, height, colors);
            widthInput.text = width.ToString();
            heightInput.text = height.ToString();
        }
    }

    // 색상 목록을 문자열로 변환하는 함수
    private string ColorListToString(List<Color> colors)
    {
        string[] colorStrings = new string[colors.Count];

        for (int i = 0; i < colors.Count; i++)
        {
            Color32 color32 = colors[i];
            colorStrings[i] = ColorUtility.ToHtmlStringRGBA(color32);
        }

        return string.Join(",", colorStrings);
    }

    // 문자열을 색상 목록으로 변환하는 함수
    private List<Color> StringToColorList(string colorString)
    {
        string[] colorStrings = colorString.Split(',');
        List<Color> colors = new List<Color>();

        foreach (string colorHtml in colorStrings)
        {
            if (ColorUtility.TryParseHtmlString("#" + colorHtml, out Color color))
            {
                colors.Add(color);
            }
        }

        return colors;
    }
    
    // 저장
    public void SavePixelArt(string pixelArtName)
    {
        // 이름 체크 
        string inputPixelArtName = nameInput.text;
        if (string.IsNullOrEmpty(inputPixelArtName))
        {
            Debug.LogError("No file name provided.");
            return;
        }

        List<Color> colors = new List<Color>();

        foreach (Transform child in grid)
        {
            Image pixelImage = child.GetComponent<Image>();
            colors.Add(pixelImage.color);
        }

        PixelArtData data = new PixelArtData(int.Parse(widthInput.text), int.Parse(heightInput.text), colors);
        string json = JsonUtility.ToJson(data);

        // 저장할 디렉토리 경로 생성
        string directoryPath = Path.Combine(Application.dataPath, "SavedPixelArts");

        // 디렉토리가 없는 경우 디렉토리를 생성
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // 파일 경로 생성
        string filePath = Path.Combine(directoryPath, inputPixelArtName + ".json");
        File.WriteAllText(filePath, json);

        Debug.Log("Pixel Art saved!");
    }

    
    public void ClearPixelArt()
    {
        foreach (Transform child in grid)
        {
            Image pixelImage = child.GetComponent<Image>();
            pixelImage.color = defaultColor;
        }
    }

    private void ColorPixelUnderMouse()
    {
        RectTransform canvasRect = grid.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Vector2 localMousePosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out localMousePosition))
        {
            Vector2 anchoredPosition = grid.InverseTransformPoint(canvasRect.TransformPoint(localMousePosition));
            for (int i = 0; i < grid.childCount; i++)
            {
                RectTransform pixelRect = grid.GetChild(i).GetComponent<RectTransform>();
                if (pixelRect.rect.Contains(anchoredPosition - pixelRect.anchoredPosition))
                {
                    Button pixelButton = pixelRect.GetComponent<Button>();
                    ChangeColor(pixelButton);
                    break;
                }
            }
        }
    }


    // 로드
    public void LoadPixelArt()
    {
        string inputPixelArtName = nameInput.text;

        if (string.IsNullOrEmpty(inputPixelArtName))
        {
            Debug.LogError("No file name provided.");
            return;
        }

        string filePath = Path.Combine(Application.dataPath, "SavedPixelArts", inputPixelArtName + ".json");

        if (!File.Exists(filePath))
        {
            Debug.LogWarning("No saved Pixel Art data found. Skipping the load.");
            return;
        }

        string jsonData = File.ReadAllText(filePath);
        PixelArtData data = JsonUtility.FromJson<PixelArtData>(jsonData);

        if (data == null)
        {
            Debug.LogWarning("No saved Pixel Art data found. Skipping the load.");
            return;
        }

        // 그리드 크기를 불러온 데이터에 맞게 변경
        ResizeGridToFitScreen(data.width, data.height);
        GenerateGrid(data.width, data.height, data.colors);

        // 로드된 픽셀 아트의 크기를 입력 필드에 설정
        widthInput.text = data.width.ToString();
        heightInput.text = data.height.ToString();

        int i = 0;
        int numberOfPixels = grid.childCount;
        int numberOfColors = data.colors.Count;

        if (numberOfPixels != numberOfColors)
        {
            Debug.LogError("Mismatch between the number of pixels and colors.");
            return;
        }

        // 그리드 내의 픽셀들을 순회하며 색상을 변경
        foreach (Transform child in grid)
        {
            Image pixelImage = child.GetComponent<Image>();
            pixelImage.color = data.colors[i];
            i++;
        }
    }

    
    // Pixel Art를 PNG Export하는 함수
    public void ExportPixelArt(string pixelArtName)
    {
        if (string.IsNullOrEmpty(pixelArtName))
        {
            Debug.LogError("No file name provided.");
            return;
        }

        // 그리드 캡쳐를 위한 RenderTexture 생성
        
        int width = Mathf.RoundToInt(grid.rect.width);
        int height = Mathf.RoundToInt(grid.rect.height);
        RenderTexture renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        renderTexture.antiAliasing = 8;


        // 캔버스를 RenderTexture에 렌더링
        CanvasRenderer renderer = grid.GetComponent<CanvasRenderer>();
        renderer.SetMaterial(new Material(Shader.Find("Unlit/Texture")), Texture2D.whiteTexture);
        renderer.SetTexture(renderTexture);

        // 현재 RenderTexture를 활성화
        RenderTexture currentActiveRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        // 화면을 캡쳐하여 새 Texture2D에 저장
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        // 원래의 RenderTexture를 활성화
        RenderTexture.active = currentActiveRT;

        // 캡쳐된 텍스쳐를 PNG로 변환
        byte[] bytes = texture.EncodeToPNG();

        // ExportedPng 디렉토리 생성
        string directoryPath = Path.Combine(Application.dataPath, "ExportedPng");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // PNG 파일 저장
        string filePath = Path.Combine(directoryPath, pixelArtName + ".png");
        File.WriteAllBytes(filePath, bytes);

        Debug.Log("Pixel Art exported!");
    }



}

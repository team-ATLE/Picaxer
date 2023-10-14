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

    // 색버튼 추가 선언
    public TMP_InputField rInputField;
    public TMP_InputField gInputField;
    public TMP_InputField bInputField;
    public Button addColorButton;

    public Image selectedColorImage; // 추가: 선택된 색상을 표시할 Image UI 요소
    
    //확대축소를 위한 크기 변수
    public float gridScale = 1.0f;

    //스포이드 액티브 상태 불 변수와 버튼
    private bool isSpoidActive = false;
    public Button spoidButton;
    public TMP_Text spoidButtonText;

    // 초기화, 유니티에서는 Awake로 초기화한다
    void Awake()
    {
        colorOptions = new Color[]
        {
            Color.black,
            Color.white,
            Color.red,
            Color.blue,
            Color.green,
            new Color(0,0,0,0) //투명
        };
    }

    void Start()
    {
        PlayerPrefs.DeleteAll(); // 이 줄을 추가하여 PlayerPrefs를 초기화
        applyButton.onClick.AddListener(ApplyGridSize);

        // 저장 로드 클리어 버튼 추가
        loadButton.onClick.AddListener(LoadColors);
        clearButton.onClick.AddListener(ClearPixelArt);
        
        saveButton.onClick.AddListener(() => SavePixelArt(nameInput.text));
        // 로드시 이름
        loadButton.onClick.AddListener(() => LoadPixelArt(nameInput.text));

        // Export 버튼 클릭 이벤트 연결
        exportButton.onClick.AddListener(() => ExportPixelArt(nameInput.text)); 
   
        // 색추가 버튼
        addColorButton.onClick.AddListener(AddColor);

    }
    
    // 스포이드 버튼
    public void OnSpoidButtonClicked()
    {
        isSpoidActive = !isSpoidActive; // 상태 토글
        spoidButtonText.text = isSpoidActive ? "Click color!" : "Spoid";
    }


    // 그리드 크기 적용 함수
    void ApplyGridSize()
    {
        int width = int.Parse(widthInput.text);
        int height = int.Parse(heightInput.text);
        GenerateGrid(width, height);
        ScaleGrid(gridScale);
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
        
        // 스포이드가 활성화되어 있으면, 픽셀의 색상을 선택된 색상으로 설정
        if (isSpoidActive)
        {
            selectedColor = pixelImage.color;
            spoidButtonText.text = "Spoid";
            isSpoidActive = false;
            // 색상 옵션 배열에 새로운 색상을 추가합니다.
            AddColorOption(selectedColor);
            // 새로운 색상을 현재 선택된 색상으로 설정하고 UI에 반영
            SelectColor(selectedColor);
        }
        // 그렇지 않으면, 선택된 색상을 픽셀의 색상으로 설정
        else
        {
            pixelImage.color = selectedColor;
        }

        // 색상번호 표시
        rInputField.text = $"{selectedColor.r*255:0}";
        gInputField.text = $"{selectedColor.g*255:0}";
        bInputField.text = $"{selectedColor.b*255:0}";
    }

    // 색상 선택 함수
    public void SelectColor(int colorIndex)
    {
        if (colorIndex >= 0 && colorIndex < colorOptions.Length)
        {
            selectedColor = colorOptions[colorIndex];
        }
        
        rInputField.text = $"{selectedColor.r*255:0}";
        gInputField.text = $"{selectedColor.g*255:0}";
        bInputField.text = $"{selectedColor.b*255:0}";
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
            //Debug.LogError("No file name provided.");
            ShowAndroidToastMessage("파일 이름이 없습니다.");
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
        string directoryPath = Path.Combine(Application.persistentDataPath, "SavedPixelArts"); // Application.dataPath -> Application.persistentDataPath

        // 디렉토리가 없는 경우 디렉토리를 생성
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // 파일 경로 생성
        string filePath = Path.Combine(directoryPath, inputPixelArtName + ".json");
        File.WriteAllText(filePath, json);

        //Debug.Log("Pixel Art saved!");
        ShowAndroidToastMessage("픽셀 아트 저장 완료!");
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
                
                if (isSpoidActive)
                {
                    ChangeColor(pixelButton);
                }
                else
                {
                    ChangeColor(pixelButton);
                    break;
                }
            }
        }
    }
}

    // 로드
    public void LoadPixelArt(string pixelArtName)
    {
       
       //픽셀 아트의 이름을 문자열 매개변수로 받게 수정 
        if (string.IsNullOrEmpty(pixelArtName))
        {
            //Debug.LogError("작성된 파일 이름이 없습니다.");
            return;
        }

        string filePath = Path.Combine(Application.persistentDataPath, "SavedPixelArts", pixelArtName + ".json"); // Application.dataPath -> Application.persistentDataPath

        if (!File.Exists(filePath))
        {
            //Debug.LogWarning("저장된 Pixel Art 데이터를 찾을 수 없습니다. 로드를 건너뜁니다.");
            ShowAndroidToastMessage("저장된 Pixel Art 데이터를 찾을 수 없습니다. 로드를 건너뜁니다.");
            return;
        }

        string jsonData = File.ReadAllText(filePath);
        PixelArtData data = JsonUtility.FromJson<PixelArtData>(jsonData);


        if (data == null)
        {
            //Debug.LogWarning("저장된 Pixel Art 데이터를 찾을 수 없습니다. 로드를 건너뜁니다.");
            ShowAndroidToastMessage("저장된 Pixel Art 데이터를 찾을 수 없습니다. 로드를 건너뜁니다.");
            return;
        }

        // 그리드 크기를 불러온 데이터에 맞게 변경
        ResizeGridToFitScreen(data.width, data.height);
        GenerateGrid(data.width, data.height, data.colors);

        // 로드된 픽셀 아트의 크기를 입력 필드에 설정
        widthInput.text = data.width.ToString();
        heightInput.text = data.height.ToString();

        // 로드된 픽셀 아트의 이름을 입력 필드에 설정
        nameInput.text = pixelArtName;

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
    // json에서 직접 만들자
    public void ExportPixelArt(string pixelArtName)
    {
        if (string.IsNullOrEmpty(pixelArtName))
        {
            //Debug.LogError("파일명을 입력하세요.");
            ShowAndroidToastMessage("파일명을 입력하세요.");
            return;
        }

        string filePath = Path.Combine(Application.persistentDataPath, "SavedPixelArts", pixelArtName + ".json"); // Application.dataPath -> Application.persistentDataPath

        if (!File.Exists(filePath))
        {
            //Debug.LogWarning("픽셀 아트 데이터가 존재하지 않습니다. 로드를 스킵합니다.");
            ShowAndroidToastMessage("픽셀 아트 데이터가 존재하지 않습니다. 로드를 스킵합니다.");
            return;
        }

        string jsonData = File.ReadAllText(filePath);
        PixelArtData data = JsonUtility.FromJson<PixelArtData>(jsonData);

        if (data == null)
        {
            //Debug.LogWarning("픽셀 아트 데이터가 존재하지 않습니다. 로드를 스킵합니다.");
            ShowAndroidToastMessage("픽셀 아트 데이터가 존재하지 않습니다. 로드를 스킵합니다.");
            return;
        }

        // Create a new texture with the dimensions of the PixelArtData
        Texture2D texture = new Texture2D(data.width, data.height);

        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                int colorIndex = y * data.width + x;
                //뒤집힘 문제 해결
                texture.SetPixel(x, data.height - y - 1, data.colors[colorIndex]);
            }
        }

        texture.Apply();

        byte[] bytes = texture.EncodeToPNG();

        // 디렉토리에 png 파일 세이브
        string directoryPath = Path.Combine(Application.persistentDataPath, "ExportedPng"); // Application.dataPath -> Application.persistentDataPath

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string exportPath = Path.Combine(directoryPath, pixelArtName + ".png");
        File.WriteAllBytes(exportPath, bytes);

        //Debug.Log("Pixel Art exported!");
        ShowAndroidToastMessage("PNG 내보내기 완료!");
    }

    //색 추가 기능
    private void AddColor()
    {
        if (float.TryParse(rInputField.text, out float r) &&
            float.TryParse(gInputField.text, out float g) &&
            float.TryParse(bInputField.text, out float b))
        {
            // RGB 값을 0~1 사이의 값으로 정규화합니다.
            r = Mathf.Clamp01(r / 255f);
            g = Mathf.Clamp01(g / 255f);
            b = Mathf.Clamp01(b / 255f);

            // 새로운 색상을 생성합니다.
            Color newColor = new Color(r, g, b);
            
            // 색상 옵션 배열에 새로운 색상을 추가합니다.
            AddColorOption(newColor);

            // 추가: 새로운 색상을 현재 선택된 색상으로 설정하고 UI에 반영
            SelectColor(newColor);
        }
        else
        {
            //Debug.LogWarning("Invalid RGB input.");
            ShowAndroidToastMessage("적절하지 않은 RGB값입니다. 0~255값을 입력해주세요!");
        }
    }

    private void AddColorOption(Color newColor)
    {
        // 새로운 색상 옵션 배열을 생성하고, 기존 색상들을 복사합니다.
        Color[] newColorOptions = new Color[colorOptions.Length + 1];
        for (int i = 0; i < colorOptions.Length; i++)
        {
            newColorOptions[i] = colorOptions[i];
        }

        // 새로운 색상을 추가합니다.
        newColorOptions[newColorOptions.Length - 1] = newColor;

        // 색상 옵션 배열을 갱신합니다.
        colorOptions = newColorOptions;
    }

    // 추가: 새로운 메서드. 선택된 색상을 저장하고 UI에 반영
    private void SelectColor(Color color)
    {
        selectedColor = color;
        selectedColorImage.color = color;
        
        float red = selectedColor.r;
        float green = selectedColor.g;
        float blue = selectedColor.b;
        float alpha = selectedColor.a;
        rInputField.text = $"{red:0}";
        gInputField.text = $"{green:0}";
        bInputField.text = $"{blue:0}";

    }

    //확대축소 기능
    public void ScaleGrid(float scalingFactor)
    {
        gridScale *= scalingFactor;
    }

    public void OnZoomInButtonClicked()
    {
        ScaleGrid(1.1f);  // Zoom in by 10%
    }

    public void OnZoomOutButtonClicked()
    {
        ScaleGrid(0.9f);  // Zoom out by 10%
    }

    //안드로이드 빌드용 토스트메세지 출력 함수
    public static void ShowAndroidToastMessage(string message)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                AndroidJavaObject toastInstance = toastClass.CallStatic<AndroidJavaObject>("makeText", currentActivity, message, toastClass.GetStatic<int>("LENGTH_SHORT"));
                toastInstance.Call("show");
            }));
        }
    }


}

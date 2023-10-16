using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

using static CommunityDAO;
using static HighScore;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Texture2D SelectedTexture { get; private set; } // 현재 선택된 텍스쳐를 저장

    public float initialGameSpeed = 5f;
    public float gameSpeedIncrease = 0.1f;
    public float gameSpeed { get; private set; }

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hiscoreText;
    public TextMeshProUGUI gameOverText;
    public Button retryButton;
    public RectTransform imageSelectionPanel;
    public RectTransform imageContentPanel;

    public GameObject imageButtonPrefab;

    private Player player;
    private Spawner spawner;

    FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;
    DatabaseReference reference;
    private float score;
    private string directoryPath;

    private void Awake()
    {
        directoryPath = Path.Combine(Application.persistentDataPath, "ExportedPng");

        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        reference.Child("Score").OrderByChild("email").EqualTo(user.Email).ValueChanged += ScoreValueChanged;

        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        InitializeGame();
    }
    public void InitializeGame()
    {
        player = FindObjectOfType<Player>();
        spawner = FindObjectOfType<Spawner>();

        NewGame();
        LoadImagesFromDirectory();
    }


    public void NewGame()
    {
        Obstacle[] obstacles = FindObjectsOfType<Obstacle>();
        foreach (var obstacle in obstacles)
        {
            Destroy(obstacle.gameObject);
        }

        score = 0f;
        gameSpeed = initialGameSpeed;
        enabled = true;

        player.gameObject.SetActive(true);
        spawner.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
        imageSelectionPanel.gameObject.SetActive(false);  // 시작 시 이미지 선택 패널 활성화

        UpdateHiscore();
    }

    public void GameOver()
    {
        gameSpeed = 0f;
        enabled = false;

        player.gameObject.SetActive(false);
        spawner.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(true);
        imageSelectionPanel.gameObject.SetActive(true);  // 게임오버 시 이미지 선택 패널 활성화
        UpdateHiscore();
    }

    private void Update()
    {
        gameSpeed += gameSpeedIncrease * Time.deltaTime;
        score += gameSpeed * Time.deltaTime;
        scoreText.text = Mathf.FloorToInt(score).ToString("D5");
    }

    private void UpdateHiscore()
    {
        float hiscore = PlayerPrefs.GetFloat("hiscore", 0);
        if (score > hiscore)
        {
            hiscore = score;
            PlayerPrefs.SetFloat("hiscore", hiscore);

            HighScore newhighscore = new HighScore(user.Email, Mathf.FloorToInt(score));
            string json = JsonUtility.ToJson(newhighscore);
            reference.Child("Score").Child(user.UserId).SetRawJsonValueAsync(json);
        }
    }

    void ScoreValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        DataSnapshot data = args.Snapshot;
        if (data != null)
        {
            foreach (DataSnapshot cur in data.Children)
            {
                long value = long.Parse(cur.Child("highscore").Value.ToString());
                hiscoreText.text = value.ToString("D5");
            }
        }
    }

    void LoadImagesFromDirectory()
    {
        // 지정된 경로가 존재하는지 확인
        if (Directory.Exists(directoryPath))
        {
            // 경로 내의 모든 PNG 파일 가져오기
            var filePaths = Directory.GetFiles(directoryPath, "*.png");
            
            // PNG 파일이 없으면 경고 출력
            if (filePaths.Length == 0)
            {
                Debug.LogWarning("No PNG files found in the directory!");
                return;
            }

            foreach (var filePath in filePaths)
            {
                Debug.Log("Loading image from: " + filePath);  // 로그 메시지 추가

                // 파일에서 이미지 데이터 읽기
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData); 

                // 버튼 생성하고 이미지 설정
                GameObject buttonObj = Instantiate(imageButtonPrefab, imageContentPanel);
                buttonObj.GetComponent<RawImage>().texture = texture;
                buttonObj.GetComponent<Button>().onClick.AddListener(() => SelectImage(texture));
            }
        }
        else
        {
            Debug.LogError("Directory not found: " + directoryPath);  // 로그 메시지 추가
        }
    }

    void SelectImage(Texture2D selectedTexture)
    {
        SelectedTexture = selectedTexture;
        imageSelectionPanel.gameObject.SetActive(false);  // 이미지 선택 후 패널 비활성화

        Spawner spawner = FindObjectOfType<Spawner>();
        if(spawner != null) spawner.UpdateTree2Sprite(SelectedTexture);
    }
}
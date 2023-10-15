using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public float initialGameSpeed = 5f;
    public float gameSpeedIncrease = 0.1f;
    public float gameSpeed { get; private set; }

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hiscoreText;
    public TextMeshProUGUI gameOverText;
    public Button retryButton;

    private Player player;
    private Spawner spawner;

    FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;
    DatabaseReference reference;
    private float score;

    private void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        reference.Child("Score").OrderByChild("email").EqualTo(user.Email).ValueChanged += ScoreValueChanged;
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void Start()
    {
        player = FindObjectOfType<Player>();
        spawner = FindObjectOfType<Spawner>();

        NewGame();
    }

    public void NewGame()
    {
        Obstacle[] obstacles = FindObjectsOfType<Obstacle>();

        foreach (var obstacle in obstacles) {
            Destroy(obstacle.gameObject);
        }

        score = 0f;
        gameSpeed = initialGameSpeed;
        enabled = true;

        player.gameObject.SetActive(true);
        spawner.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);

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
        // PlayerPrefs.DeleteKey("hiscore");
        float hiscore = PlayerPrefs.GetFloat("hiscore", 0);

        Debug.Log("hi!");

        if (score > hiscore)
        {
            hiscore = score;
            PlayerPrefs.SetFloat("hiscore", hiscore);

            HighScore newhighscore = new HighScore(user.Email, Mathf.FloorToInt(score));
            string json = JsonUtility.ToJson(newhighscore);
            reference.Child("Score").Child(user.UserId).SetRawJsonValueAsync(json);
            Debug.Log("Finished");
        }

        // hiscoreText.text = Mathf.FloorToInt(hiscore).ToString("D5");
    }

    void ScoreValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        DataSnapshot data = args.Snapshot;
        Debug.Log("ScoreValueChanged");
        if (data != null)
        {
            // find if the user likes ith post
            foreach (DataSnapshot cur in data.Children)
            {
                long value = long.Parse(cur.Child("highscore").Value.ToString());
                hiscoreText.text = value.ToString("D5");
            }
        }
    }

}

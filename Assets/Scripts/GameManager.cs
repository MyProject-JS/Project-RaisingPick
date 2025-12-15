using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int maxLives = 3;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton; // 메인 메뉴로 가는 버튼
    [SerializeField] private GameObject lifeImagePrefab;
    [SerializeField] private Transform livesPanel;

    [Header("Game Components")]
    [SerializeField] private PlayerController player;
    [SerializeField] private EnemySpawner spawner;
    
    private List<Image> populatedLifeImages = new List<Image>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // --- Dependency Checks ---
        if (player == null) Debug.LogError("PlayerController is not assigned in GameManager!");
        if (spawner == null) Debug.LogError("EnemySpawner is not assigned in GameManager!");
        
        // --- Event Subscriptions ---
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
        GameDataManager.Instance.OnDataChanged += UpdateUI;

        // --- Initial State Setup ---
        SetupLifeUI();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        Time.timeScale = 1f;

        // Reset game values for the first start
        ResetGameValues();
    }

    private void OnDestroy()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.OnDataChanged -= UpdateUI;
        }
    }

    private void OnApplicationQuit()
    {
        GameDataManager.Instance.ForceSave();
    }

    public void AddScore(int amount)
    {
        GameDataManager.Instance.ModifyData(data => data.playerScore += amount);
    }
    
    public void LoseLife()
    {
        int lives = GameDataManager.Instance.Data.currentLives;
        if (lives <= 0) return;

        GameDataManager.Instance.ModifyData(data => data.currentLives--);

        if (GameDataManager.Instance.Data.currentLives <= 0)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        var gameData = GameDataManager.Instance.Data;

        // Check for and save high score
        if (gameData.playerScore > gameData.highScore)
        {
            Debug.Log("New High Score: " + gameData.playerScore);
            // ModifyData를 호출하면 highScore가 업데이트되고 자동으로 저장됩니다.
            GameDataManager.Instance.ModifyData(data => data.highScore = data.playerScore);
        }
        
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // 게임 일시 정지
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // 항상 타임스케일을 원복하고 씬을 로드합니다.
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        ObjectPooler.Instance.ReturnAllToPool();
        
        if (player != null) player.ResetPlayer();
        if (spawner != null) spawner.ResetSpawner();
        
        ResetGameValues();
    }

    private void ResetGameValues()
    {
        GameDataManager.Instance.ModifyData(data => 
        {
            data.playerScore = 0;
            data.currentLives = maxLives;
        });
    }

    // --- UI Update Methods ---

    private void UpdateUI(GameData data)
    {
        UpdateScoreUI(data.playerScore);
        UpdateLifeUI(data.currentLives);
    }
    
    private void SetupLifeUI()
    {
        if (populatedLifeImages.Count > 0) return;
        if (lifeImagePrefab == null || livesPanel == null) return;
        
        for (int i = 0; i < maxLives; i++)
        {
            GameObject lifeIcon = Instantiate(lifeImagePrefab, livesPanel);
            populatedLifeImages.Add(lifeIcon.GetComponent<Image>());
        }
    }
    
    private void UpdateLifeUI(int lives)
    {
        for (int i = 0; i < populatedLifeImages.Count; i++)
        {
            if (populatedLifeImages[i] != null)
                populatedLifeImages[i].gameObject.SetActive(i < lives);
        }
    }

    private void UpdateScoreUI(int newScore)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + newScore;
    }
}

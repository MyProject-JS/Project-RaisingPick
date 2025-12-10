using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int maxLives = 3;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private GameObject lifeImagePrefab; // Life Image Prefab
    [SerializeField] private Transform livesPanel;       // Parent panel for life images

    [Header("Game Components")]
    [SerializeField] private PlayerController player;
    [SerializeField] private EnemySpawner spawner;

    private int score;
    private int currentLives;
    private List<Image> populatedLifeImages = new List<Image>(); // List to hold instantiated images

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
        // Check for required components
        if (player == null) Debug.LogError("PlayerController is not assigned in GameManager!");
        if (spawner == null) Debug.LogError("EnemySpawner is not assigned in GameManager!");
        if (restartButton == null) Debug.LogError("Restart Button is not assigned in GameManager!");
        if (lifeImagePrefab == null) Debug.LogError("Life Image Prefab is not assigned in GameManager!");
        if (livesPanel == null) Debug.LogError("Lives Panel is not assigned in GameManager!");

        // Register button listener
        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(RestartGame);

        // Setup UI and game state
        SetupLifeUI();
        gameOverPanel.SetActive(false);
        Time.timeScale = 1f;

        // Reset game values for the first start
        ResetGameValues();
    }

    private void SetupLifeUI()
    {
        // Instantiate life images based on maxLives
        for (int i = 0; i < maxLives; i++)
        {
            GameObject lifeIcon = Instantiate(lifeImagePrefab, livesPanel);
            populatedLifeImages.Add(lifeIcon.GetComponent<Image>());
        }
    }

    public void LoseLife()
    {
        if (currentLives <= 0) return;

        currentLives--;
        UpdateLifeUI();

        if (currentLives <= 0)
        {
            TriggerGameOver();
        }
    }
    
    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    private void TriggerGameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);

        ReturnAllToPool("Enemy");
        ReturnAllToPool("DestructionEffect");
        
        if (player != null) player.ResetPlayer();
        if (spawner != null) spawner.ResetSpawner();
        
        ResetGameValues();
    }

    private void ResetGameValues()
    {
        score = 0;
        currentLives = maxLives;
        UpdateScoreUI();
        UpdateLifeUI();
    }
    
    private void UpdateLifeUI()
    {
        // Use the runtime-populated list
        for (int i = 0; i < populatedLifeImages.Count; i++)
        {
            populatedLifeImages[i].gameObject.SetActive(i < currentLives);
        }
    }

    private void UpdateScoreUI()
    {
        scoreText.text = "Score: " + score;
    }
    
    private void ReturnAllToPool(string tag)
    {
        GameObject[] objectsToPool = GameObject.FindGameObjectsWithTag(tag);
        if (objectsToPool.Length > 0 && ObjectPooler.Instance != null)
        {
            foreach (GameObject obj in objectsToPool)
            {
                ObjectPooler.Instance.ReturnToPool(tag, obj);
            }
        }
    }
}

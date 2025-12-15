using System;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Refs")]
    public PlayerController player;

    [Header("Score")]
    public TMP_Text scoreText;
    public TMP_Text finalScoreText;
    public int Score { get; private set; }
    public event Action<int> OnScoreChanged;

    [Header("Timing")]
    public TMP_Text timeSurvivedText;

    [Header("Menus")]
    public GameObject mainMenuPanel;
    public GameObject descriptionPanel;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;
    public GameObject hudPanel;

    [Header("Debug/Flow")]
    public bool autoStart = true;

    private float coinBonusEndTime;
    private int coinBonusPoints;
    private bool ended;
    private bool isPaused;
    private bool isRunning;
    private float runTimer;
    private Vector3 playerStartPos;
    private Quaternion playerStartRot;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CachePlayerStart();
        UpdateScoreText();
        if (autoStart)
        {
            StartGame();
        }
        else
        {
            ShowMainMenu();
        }
    }

    void Update()
    {
        if (isRunning && !isPaused && !ended)
        {
            runTimer += Time.deltaTime;
        }
    }

    public void AddScore(int amount)
    {
        Score += amount;
        OnScoreChanged?.Invoke(Score);
        UpdateScoreText();
    }

    public void ResetScore()
    {
        Score = 0;
        OnScoreChanged?.Invoke(Score);
        UpdateScoreText();
        ended = false;
        isPaused = false;
        isRunning = false;
        runTimer = 0f;
        Time.timeScale = 1f;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
    }

    public void OnPlayerDied()
    {
        if (ended) return;
        ended = true;
        isRunning = false;
        ShowOnlyPanel(gameOverPanel);
        if (finalScoreText != null) finalScoreText.text = $"Score: {Score}";
        if (timeSurvivedText != null) timeSurvivedText.text = $"Time: {FormatTime(runTimer)}";
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {Score}";
        }
    }

    public bool IsCoinBonusActive => Time.time < coinBonusEndTime;
    public int CurrentCoinBonus => IsCoinBonusActive ? coinBonusPoints : 0;

    public void ActivateCoinBonus(int bonusAmount, float durationSeconds)
    {
        coinBonusPoints = bonusAmount;
        coinBonusEndTime = Time.time + durationSeconds;
    }

    void CachePlayerStart()
    {
        if (player != null)
        {
            playerStartPos = player.transform.position;
            playerStartRot = player.transform.rotation;
        }
    }

    void ResetPlayerState()
    {
        if (player == null) return;
        player.transform.position = playerStartPos;
        player.transform.rotation = playerStartRot;
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    string FormatTime(float seconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));
        return $"{t.Minutes:00}:{t.Seconds:00}";
    }

    // ---- UI Hooks ----
    public void ShowMainMenu()
    {
        isRunning = false;
        isPaused = false;
        ShowOnlyPanel(mainMenuPanel);
    }

    public void StartGame()
    {
        ResetScore();
        ResetPlayerState();
        runTimer = 0f;
        ended = false;
        isPaused = false;
        isRunning = true;
        ShowOnlyPanel(hudPanel);
    }

    public void ShowDescription(bool show)
    {
        if (show)
        {
            isRunning = false;
            isPaused = true;
            ShowOnlyPanel(descriptionPanel);
        }
        else
        {
            ShowMainMenu();
        }
    }

    public void PauseGame()
    {
        if (!isRunning || ended) return;
        isPaused = true;
        ShowOnlyPanel(pauseMenuPanel);
    }

    public void ResumeGame()
    {
        if (!isRunning || ended) return;
        isPaused = false;
        ShowOnlyPanel(hudPanel);
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void ReturnToMainMenu()
    {
        ShowMainMenu();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void ShowOnlyPanel(GameObject panelToShow)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(panelToShow == mainMenuPanel);
        if (descriptionPanel != null) descriptionPanel.SetActive(panelToShow == descriptionPanel);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(panelToShow == pauseMenuPanel);
        if (gameOverPanel != null) gameOverPanel.SetActive(panelToShow == gameOverPanel);
        if (hudPanel != null) hudPanel.SetActive(panelToShow == hudPanel);

        bool hudVisible = panelToShow == hudPanel;
        Time.timeScale = hudVisible ? 1f : 0f;
    }
}

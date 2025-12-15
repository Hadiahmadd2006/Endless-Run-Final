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

    [Header("Audio")]
    public AudioSource globalAudioSource;

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
        // Keep time running so death animation can play; menu can pause later if desired.
        Time.timeScale = 1f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
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
        Time.timeScale = 0f;
        isRunning = false;
        isPaused = false;
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (descriptionPanel != null) descriptionPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
    }

    public void StartGame()
    {
        ResetScore();
        ResetPlayerState();
        runTimer = 0f;
        ended = false;
        isPaused = false;
        isRunning = true;
        Time.timeScale = 1f;
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (descriptionPanel != null) descriptionPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);
    }

    public void ShowDescription(bool show)
    {
        if (descriptionPanel != null) descriptionPanel.SetActive(show);
    }

    public void PauseGame()
    {
        if (!isRunning || ended) return;
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    public void ResumeGame()
    {
        if (!isRunning || ended) return;
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
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

    public void SetVolume(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);
        if (globalAudioSource != null)
        {
            globalAudioSource.volume = Mathf.Clamp01(value);
        }
    }
}

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
    public bool autoStart = false;

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
        Time.timeScale = 0f;
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

// ---- Interaction components ----
public class Collectible : MonoBehaviour
{
    public CollectibleData data;
    public AudioClip collectSfx; // optional override
    public float rotationSpeed = 180f; // degrees per second, coins spin by default

    void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void Update()
    {
        if (Mathf.Abs(rotationSpeed) > 0.01f)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;
        HandleCollect(player);
    }

    public void HandleCollect(PlayerController player)
    {
        if (player == null) return;
        int pts = data != null ? data.points : 1;
        var gm = GameManager.Instance;
        if (gm != null && gm.IsCoinBonusActive)
        {
            pts += gm.CurrentCoinBonus;
        }
        gm?.AddScore(pts);

        if (gm != null && data != null && data.bonusChance > 0f && data.bonusDuration > 0f && data.bonusPoints != 0)
        {
            if (UnityEngine.Random.value < data.bonusChance)
            {
                gm.ActivateCoinBonus(data.bonusPoints, data.bonusDuration);
            }
        }

        AudioClip clipToPlay = collectSfx != null ? collectSfx : (data != null ? data.pickSfx : null);
        player.PlaySfx(clipToPlay);
        Destroy(gameObject);
    }
}

public class Obstacle : MonoBehaviour
{
    public ObstacleData data;

    void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player != null) HandleHit(player);
    }

    public void HandleHit(PlayerController player)
    {
        if (player == null) return;
        player.PlaySfx(data != null ? data.hitSfx : null);
        player.Die();
    }
}

public class SimpleObstacle : MonoBehaviour
{
    public AudioClip hitSfx;

    void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player != null)
        {
            player.PlaySfx(hitSfx);
            player.Die();
        }
    }
}

[DefaultExecutionOrder(-99)]
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public int Score => GameManager.Instance != null ? GameManager.Instance.Score : 0;

    public TMP_Text scoreText;

    public event Action<int> OnScoreChanged
    {
        add { if (GameManager.Instance != null) GameManager.Instance.OnScoreChanged += value; }
        remove { if (GameManager.Instance != null) GameManager.Instance.OnScoreChanged -= value; }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (GameManager.Instance == null)
        {
            gameObject.AddComponent<GameManager>();
        }

        if (GameManager.Instance.scoreText == null && scoreText != null)
        {
            GameManager.Instance.scoreText = scoreText;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void AddScore(int amount) => GameManager.Instance?.AddScore(amount);
    public void ResetScore() => GameManager.Instance?.ResetScore();
}

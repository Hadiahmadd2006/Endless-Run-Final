using UnityEngine;
using System;
using TMPro;

// Compatibility shim forwarding to GameManager. Prefer using GameManager directly.
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

        // Preserve score text if assigned here.
        if (GameManager.Instance.scoreText == null && scoreText != null)
        {
            GameManager.Instance.scoreText = scoreText;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void AddScore(int amount) => GameManager.Instance?.AddScore(amount);
    public void ResetScore() => GameManager.Instance?.ResetScore();
}

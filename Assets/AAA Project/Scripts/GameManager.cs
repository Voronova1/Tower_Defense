using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    public enum LevelType { Normal, Desert }
    public LevelType currentLevelType = LevelType.Normal;

    public static GameManager Instance { get; private set; }
    public static event Action<bool> OnGameEnded;

    public int playerHealth;
    public TMP_Text healthText;
    public int money;
    public TMP_Text moneyText;

    [Header("UI Elements")]
    public GameObject levelFailedPanel;
    public GameObject levelCompletePanel;

    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";
    public string dialogueScene = "DialogueScene";

    [System.NonSerialized]
    public List<MovementMobs> EnemyList = new List<MovementMobs>();

    private WaveSpawner waveSpawner;
    private bool isQuitting = false;
    private bool gameEnded = false;

    [Header("Input Blocking")]
    public static bool IsInputBlocked { get; private set; }

    public static void SetInputBlock(bool blocked)
    {
        IsInputBlocked = blocked;
    }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeUI();
    }

    void Start()
    {
        waveSpawner = FindObjectOfType<WaveSpawner>();
        DetermineLevelType();
    }

    private void DetermineLevelType()
    {
        // Более надежная проверка для пустынного уровня
        currentLevelType = SceneManager.GetActiveScene().name.ToLower().Contains("desert")
            ? LevelType.Desert
            : LevelType.Normal;
    }

    void InitializeUI()
    {
        healthText.text = playerHealth.ToString();
        moneyText.text = money.ToString();
        levelFailedPanel.SetActive(false);
        levelCompletePanel.SetActive(false);
    }

    public void ChangeMoney(int count)
    {
        money += count;
        moneyText.text = money.ToString();
    }

    public void ChangeHealth(int count)
    {
        playerHealth -= count;
        healthText.text = playerHealth.ToString();

        if (playerHealth <= 0)
        {
            LevelFailed();
        }
    }

    public void AddEnemyOnList(MovementMobs enemy)
    {
        if (enemy == null) return;
        if (!EnemyList.Contains(enemy))
        {
            EnemyList.Add(enemy);
        }
    }

    public void SafeRemoveEnemyFromList(MovementMobs enemy)
    {
        if (isQuitting || enemy == null) return;

        if (EnemyList.Contains(enemy))
        {
            EnemyList.Remove(enemy);
            CheckLevelCompletion();
        }
    }

    public void CheckLevelCompletion()
    {
        if (gameEnded) return;
        if (waveSpawner == null) waveSpawner = FindObjectOfType<WaveSpawner>();
        if (waveSpawner == null) return;

        bool wavesDone = waveSpawner.AllWavesCompleted;
        bool noActiveEnemies = EnemyList.Count == 0;
        bool notSpawning = !waveSpawner.IsSpawning;

        if (wavesDone && noActiveEnemies && notSpawning && playerHealth > 0)
        {
            LevelComplete();
        }
    }

    private void LevelFailed()
    {
        if (!gameEnded && levelFailedPanel != null)
        {
            gameEnded = true;
            Time.timeScale = 0f;
            levelFailedPanel.SetActive(true);
            SetInputBlock(true); // Блокируем ввод
            OnGameEnded?.Invoke(true);
        }
    }

    private void LevelComplete()
    {
        if (!gameEnded && levelCompletePanel != null)
        {
            gameEnded = true;
            Time.timeScale = 0f;
            levelCompletePanel.SetActive(true);
            SetInputBlock(true); // Блокируем ввод
            OnGameEnded?.Invoke(true);
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        gameEnded = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        gameEnded = false;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void ContinueToDialogue()
    {
        Time.timeScale = 1f;
        gameEnded = false;
        SceneManager.LoadScene(dialogueScene);
    }

    void OnApplicationQuit()
    {
        isQuitting = true;
    }
}
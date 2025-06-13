using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
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

    public List<MovementMobs> EnemyList = new List<MovementMobs>();
    private WaveSpawner waveSpawner;

    void Start()
    {
        healthText.text = playerHealth.ToString();
        moneyText.text = money.ToString();
        levelFailedPanel.SetActive(false);
        levelCompletePanel.SetActive(false);
        waveSpawner = FindObjectOfType<WaveSpawner>();
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
        EnemyList.Add(enemy);
    }

    public void RemoveEnemyFromList(MovementMobs enemy)
    {
        EnemyList.Remove(enemy);
        CheckLevelCompletion();
    }

    private void CheckLevelCompletion()
    {
        if (waveSpawner.AllWavesCompleted && EnemyList.Count == 0 && playerHealth > 0)
        {
            LevelComplete();
        }
    }

    private void LevelFailed()
    {
        Time.timeScale = 0f;
        levelFailedPanel.SetActive(true);
    }

    private void LevelComplete()
    {
        Time.timeScale = 0f;
        levelCompletePanel.SetActive(true);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void ContinueToDialogue()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(dialogueScene);
    }
}
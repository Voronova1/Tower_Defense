using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public int playerHealth;
    public TMP_Text heathText;

    public int money;
    public TMP_Text moneyText;

    public List<MovementMobs> EnemyList = new List<MovementMobs>();
    void Start()
    {
        heathText.text = playerHealth.ToString();
        moneyText.text = money.ToString();
    }

    public void ChangeMoney(int count)
    {
        money += count;
        moneyText.text = money.ToString();
    }

    public void ChangeHealth(int count)
    {
        playerHealth -= count;
        heathText.text = playerHealth.ToString();
        if (playerHealth <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void AddEnemyOnList (MovementMobs enemy)
    {
        EnemyList.Add(enemy);
    }

    public void RemoveEnemyFromList (MovementMobs enemy)
    {
        EnemyList.Remove(enemy);
    }
}

using UnityEngine;
using TMPro;

public class LevelTowerInfo : MonoBehaviour
{
    [Header("Настройки для этого уровня")]
    public bool showPanel = true; // Галочка "Показать панель"
    [TextArea(3, 10)]
    public string infoText = "Тут описание башни..."; // Текст для панели

    [Header("Ссылки (перетащите вручную)")]
    public GameObject towerInfoPanel; // Сюда перетащите TowerInfoPanel из канваса
    public TMP_Text textUI; // Сюда перетащите InfoText

    void Start()
    {
        if (showPanel)
        {
            textUI.text = infoText;
            towerInfoPanel.SetActive(true);
            Time.timeScale = 0f; // Пауза игры
        }
    }

    // Вызовите этот метод при нажатии на кнопку "Закрыть"
    public void ClosePanel()
    {
        towerInfoPanel.SetActive(false);
        Time.timeScale = 1f; // Возобновление игры
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private GameObject[] levelButtons;
    [SerializeField] private GameObject activeButtonPrefab;
    [SerializeField] private GameObject inactiveButtonPrefab;

    [Header("Settings UI")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject resetConfirmationPanel;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button resetProgressButton;
    [SerializeField] private Button confirmResetButton;
    [SerializeField] private Button cancelResetButton;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private void Start()
    {
        if (debugMode) Debug.Log("LevelManager initialization started");

        if (!ValidateDependencies())
        {
            if (debugMode) Debug.LogError("Initialization failed - dependencies not met");
            return;
        }

        SetupUIElements();
        UpdateLevelButtons();
    }

    private void SetupUIElements()
    {
        // Устанавливаем максимальный порядок в отрисовке для панелей
        settingsPanel.transform.SetAsLastSibling();
        resetConfirmationPanel.transform.SetAsLastSibling();

        // Настройка кнопок
        settingsButton.onClick.AddListener(OpenSettings);
        closeSettingsButton.onClick.AddListener(CloseSettings);
        quitButton.onClick.AddListener(QuitGame);
        resetProgressButton.onClick.AddListener(ShowResetConfirmation);
        confirmResetButton.onClick.AddListener(ConfirmResetProgress);
        cancelResetButton.onClick.AddListener(CancelResetProgress);

        // Скрываем панели при старте
        settingsPanel.SetActive(false);
        resetConfirmationPanel.SetActive(false);
    }

    private void OpenSettings()
    {
        if (debugMode) Debug.Log("Opening settings panel");

        // Делаем панель последним дочерним элементом (поверх всего)
        settingsPanel.transform.SetAsLastSibling();
        settingsPanel.SetActive(true);

        // Блокируем взаимодействие с кнопками уровней
        SetLevelButtonsInteractable(false);
    }

    private void CloseSettings()
    {
        if (debugMode) Debug.Log("Closing settings panel");
        settingsPanel.SetActive(false);

        // Восстанавливаем взаимодействие с кнопками уровней
        SetLevelButtonsInteractable(true);
    }

    private void ShowResetConfirmation()
    {
        if (debugMode) Debug.Log("Showing reset confirmation");

        // Делаем панель подтверждения последним дочерним элементом
        resetConfirmationPanel.transform.SetAsLastSibling();
        resetConfirmationPanel.SetActive(true);
    }

    private void ConfirmResetProgress()
    {
        if (debugMode) Debug.Log("Resetting all progress");
        ResetAllProgress();
        resetConfirmationPanel.SetActive(false);
        settingsPanel.SetActive(false);

        // Восстанавливаем взаимодействие с кнопками уровней
        SetLevelButtonsInteractable(true);
    }

    private void CancelResetProgress()
    {
        if (debugMode) Debug.Log("Cancel reset progress");
        resetConfirmationPanel.SetActive(false);
    }

    private void SetLevelButtonsInteractable(bool interactable)
    {
        foreach (var button in levelButtons)
        {
            if (button != null)
            {
                var btnComponent = button.GetComponent<Button>();
                if (btnComponent != null)
                {
                    btnComponent.interactable = interactable;
                }
            }
        }
    }

    private bool ValidateDependencies()
    {
        // Проверка LevelProgressManager
        if (LevelProgressManager.Instance == null)
        {
            Debug.LogError("LevelProgressManager instance is null!");
            return false;
        }

        // Проверка кнопок уровней
        if (levelButtons == null || levelButtons.Length == 0)
        {
            Debug.LogError("Level buttons array is not set or empty!");
            return false;
        }

        // Проверка префабов
        if (activeButtonPrefab == null || inactiveButtonPrefab == null)
        {
            Debug.LogError("Button prefabs are not assigned!");
            return false;
        }

        // Проверка UI элементов
        if (settingsPanel == null || resetConfirmationPanel == null ||
            settingsButton == null || closeSettingsButton == null ||
            quitButton == null || resetProgressButton == null ||
            confirmResetButton == null || cancelResetButton == null)
        {
            Debug.LogError("Some UI elements are not assigned!");
            return false;
        }

        if (debugMode) Debug.Log("All dependencies validated successfully");
        return true;
    }

    public void UpdateLevelButtons()
    {
        if (debugMode) Debug.Log($"Updating {levelButtons.Length} level buttons");

        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] == null)
            {
                Debug.LogWarning($"Button at index {i} is null, skipping");
                continue;
            }

            Transform buttonParent = levelButtons[i].transform.parent;
            if (buttonParent == null)
            {
                Debug.LogWarning($"Button at index {i} has no parent, skipping");
                continue;
            }

            if (debugMode) Debug.Log($"Processing button {i} with parent {buttonParent.name}");

            // Сохраняем параметры
            Vector3 position = levelButtons[i].transform.localPosition;
            Vector3 scale = levelButtons[i].transform.localScale;
            string name = levelButtons[i].name;

            // Уничтожаем старую кнопку
            Destroy(levelButtons[i]);

            // Создаем новую кнопку
            levelButtons[i] = CreateLevelButton(i + 1, buttonParent);

            // Восстанавливаем параметры
            levelButtons[i].transform.localPosition = position;
            levelButtons[i].transform.localScale = scale;
            levelButtons[i].name = name;
        }
    }

    private GameObject CreateLevelButton(int levelNumber, Transform parent)
    {
        if (debugMode) Debug.Log($"Creating button for level {levelNumber}");

        // Проверка доступности уровня
        bool isUnlocked = LevelProgressManager.Instance.IsLevelUnlocked(levelNumber);
        GameObject prefabToUse = isUnlocked ? activeButtonPrefab : inactiveButtonPrefab;

        if (prefabToUse == null)
        {
            Debug.LogError($"Prefab for {(isUnlocked ? "active" : "inactive")} button is null!");
            return null;
        }

        if (parent == null)
        {
            Debug.LogError("Parent transform is null!");
            return null;
        }

        // Создаем кнопку
        GameObject newButton = Instantiate(prefabToUse, parent);

        if (isUnlocked)
        {
            SetupActiveButton(newButton, levelNumber);
        }

        return newButton;
    }

    private void SetupActiveButton(GameObject button, int levelNumber)
    {
        // Установка номера уровня
        TMP_Text textComponent = button.GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = levelNumber.ToString();
        }
        else
        {
            Debug.LogWarning("No TMP_Text component found on active button");
        }

        // Настройка обработчика клика
        Button buttonComponent = button.GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.onClick.RemoveAllListeners();
            buttonComponent.onClick.AddListener(() => LoadLevel(levelNumber));
        }
        else
        {
            Debug.LogWarning("No Button component found on active button");
        }
    }

    public void LoadLevel(int levelIndex)
    {
        if (LevelProgressManager.Instance.IsLevelUnlocked(levelIndex))
        {
            if (debugMode) Debug.Log($"Loading intro dialogue for level {levelIndex}");
            SceneManager.LoadScene($"IntroDialogue{levelIndex}_1"); // Изменено на вашу структуру
        }
        else
        {
            Debug.LogWarning($"Attempted to load locked level {levelIndex}");
        }
    }

    public void ResetAllProgress()
    {
        // 1. Сбрасываем прогресс через LevelProgressManager
        LevelProgressManager.Instance.ResetProgress();

        // 2. Обновляем все кнопки
        UpdateLevelButtons();

        // 3. Визуальная обратная связь (опционально)
        if (debugMode) Debug.Log("All progress has been reset");

        // 4. Можно добавить анимацию (пример с LeanTween)
        foreach (var button in levelButtons)
        {
            if (button != null)
            {
                button.transform.localScale = Vector3.zero;
                LeanTween.scale(button, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);
            }
        }
    }

    public void QuitGame()
    {
        if (Application.isMobilePlatform)
        {
            // Дополнительные действия перед выходом (опционально)
            SaveBeforeQuit();

            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call<bool>("moveTaskToBack", true);
        }
        else // Для редактора и PC/Mac
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }

    private void SaveBeforeQuit()
    {
        // Сохранение данных перед выходом
        LevelProgressManager.Instance.SaveProgress();
        Debug.Log("Game data saved before quit");
    }

}
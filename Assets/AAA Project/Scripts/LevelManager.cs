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

        UpdateLevelButtons();
    }

    private bool ValidateDependencies()
    {
        // �������� LevelProgressManager
        if (LevelProgressManager.Instance == null)
        {
            Debug.LogError("LevelProgressManager instance is null!");
            return false;
        }

        // �������� ������
        if (levelButtons == null || levelButtons.Length == 0)
        {
            Debug.LogError("Level buttons array is not set or empty!");
            return false;
        }

        // �������� ��������
        if (activeButtonPrefab == null)
        {
            Debug.LogError("Active button prefab is not assigned!");
            return false;
        }

        if (inactiveButtonPrefab == null)
        {
            Debug.LogError("Inactive button prefab is not assigned!");
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

            // ��������� ���������
            Vector3 position = levelButtons[i].transform.localPosition;
            Vector3 scale = levelButtons[i].transform.localScale;
            string name = levelButtons[i].name;

            // ���������� ������ ������
            Destroy(levelButtons[i]);

            // ������� ����� ������
            levelButtons[i] = CreateLevelButton(i + 1, buttonParent);

            // ��������������� ���������
            levelButtons[i].transform.localPosition = position;
            levelButtons[i].transform.localScale = scale;
            levelButtons[i].name = name;
        }
    }

    private GameObject CreateLevelButton(int levelNumber, Transform parent)
    {
        if (debugMode) Debug.Log($"Creating button for level {levelNumber}");

        // �������� ����������� ������
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

        // ������� ������
        GameObject newButton = Instantiate(prefabToUse, parent);

        if (isUnlocked)
        {
            SetupActiveButton(newButton, levelNumber);
        }

        return newButton;
    }

    private void SetupActiveButton(GameObject button, int levelNumber)
    {
        // ��������� ������ ������
        TMP_Text textComponent = button.GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = levelNumber.ToString();
        }
        else
        {
            Debug.LogWarning("No TMP_Text component found on active button");
        }

        // ��������� ����������� �����
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
            SceneManager.LoadScene($"IntroDialogue{levelIndex}_1"); // �������� �� ���� ���������
        }
        else
        {
            Debug.LogWarning($"Attempted to load locked level {levelIndex}");
        }
    }

    public void ResetAllProgress()
    {
        // 1. ���������� �������� ����� LevelProgressManager
        LevelProgressManager.Instance.ResetProgress();

        // 2. ��������� ��� ������
        UpdateLevelButtons();

        // 3. ���������� �������� ����� (�����������)
        if (debugMode) Debug.Log("All progress has been reset");

        // 4. ����� �������� �������� (������ � LeanTween)
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
            // �������������� �������� ����� ������� (�����������)
            SaveBeforeQuit();

            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call<bool>("moveTaskToBack", true);
        }
        else // ��� ��������� � PC/Mac
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
        // ���������� ������ ����� �������
        LevelProgressManager.Instance.SaveProgress();
        Debug.Log("Game data saved before quit");
    }

}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 5)]
        public string text;
        public Sprite characterSprite;
    }

    public DialogueLine[] dialogueLines;
    public TMP_Text dialogueText; // Изменили на TMP_Text
    public Image characterImage;
    public GameObject tapPrompt;
    public string nextSceneName = "level_1";

    private int currentLine = 0;
    private float timeSinceLastTap = 0f;
    private bool promptActive = false;

    void Start()
    {
        // Добавляем CanvasGroup если отсутствует
        if (dialogueText.GetComponent<CanvasGroup>() == null)
        {
            dialogueText.gameObject.AddComponent<CanvasGroup>();
        }

        ShowLine(0);
        if (tapPrompt != null) tapPrompt.SetActive(false);
    }

    void Update()
    {
        timeSinceLastTap += Time.deltaTime;

        if (timeSinceLastTap > 5f && !promptActive)
        {
            ShowTapPrompt();
        }

        // Обработка ввода через новую систему
        if (Mouse.current.leftButton.wasPressedThisFrame ||
           (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame))
        {
            HandleTap();
        }
    }

    void ShowLine(int index)
    {
        if (index >= dialogueLines.Length) return;

        currentLine = index;
        dialogueText.text = dialogueLines[index].text;
        characterImage.sprite = dialogueLines[index].characterSprite;

        timeSinceLastTap = 0f;
        if (promptActive) HideTapPrompt();

        // Анимация с проверкой CanvasGroup
        CanvasGroup cg = dialogueText.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0;
            LeanTween.alphaCanvas(cg, 1f, 0.3f);
        }
    }

    void ShowTapPrompt()
    {
        if (tapPrompt == null) return;

        promptActive = true;
        tapPrompt.SetActive(true);

        CanvasGroup cg = tapPrompt.GetComponent<CanvasGroup>();
        if (cg == null) cg = tapPrompt.AddComponent<CanvasGroup>();

        LeanTween.alphaCanvas(cg, 0.3f, 0.5f)
            .setLoopPingPong();
    }

    void HideTapPrompt()
    {
        if (tapPrompt == null) return;

        LeanTween.cancel(tapPrompt);
        promptActive = false;
        tapPrompt.SetActive(false);
    }

    void HandleTap()
    {
        timeSinceLastTap = 0f;

        if (tapPrompt != null)
            tapPrompt.SetActive(false);

        if (currentLine < dialogueLines.Length - 1)
        {
            ShowLine(currentLine + 1);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
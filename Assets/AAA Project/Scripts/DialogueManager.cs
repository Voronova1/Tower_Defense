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
        public AudioClip soundEffect; // ���� ��� ���������� �������
    }

    public DialogueLine[] dialogueLines;
    public TMP_Text dialogueText;
    public Image characterImage;
    public GameObject tapPrompt;
    public string nextSceneName = "level_1";
    public AudioSource audioSource; // �������� �����
    public AudioClip defaultSlideSound; // ���� �� ���������

    private int currentLine = 0;
    private float timeSinceLastTap = 0f;
    private bool promptActive = false;

    private PlayerInput playerInput;
    private InputAction touchPressAction;

    void Start()
    {
        // ������������� ������� �����
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            playerInput = gameObject.AddComponent<PlayerInput>();
        }

        touchPressAction = playerInput.actions.FindAction("Fire", true);
        if (touchPressAction == null)
        {
            Debug.LogError("�� ������� �������� Fire � Input System");
        }

        // ������������� AudioSource ���� �����������
        if (audioSource == null)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        // ������������� UI
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

        // ��������� �����
        if (touchPressAction != null && touchPressAction.WasPressedThisFrame())
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

        // ��������������� ����� ��� ������� �������
        PlaySlideSound(dialogueLines[index].soundEffect);

        timeSinceLastTap = 0f;
        if (promptActive) HideTapPrompt();

        CanvasGroup cg = dialogueText.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0;
            LeanTween.alphaCanvas(cg, 1f, 0.3f);
        }
    }

    void PlaySlideSound(AudioClip clip)
    {
        if (audioSource == null) return;

        // ���������� ���������� ���� ��� ������� ��� ���� �� ���������
        AudioClip soundToPlay = clip != null ? clip : defaultSlideSound;

        if (soundToPlay != null)
        {
            audioSource.PlayOneShot(soundToPlay);
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
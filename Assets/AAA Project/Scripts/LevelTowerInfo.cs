using UnityEngine;
using TMPro;

public class LevelTowerInfo : MonoBehaviour
{
    [Header("��������� ��� ����� ������")]
    public bool showPanel = true; // ������� "�������� ������"
    [TextArea(3, 10)]
    public string infoText = "��� �������� �����..."; // ����� ��� ������

    [Header("������ (���������� �������)")]
    public GameObject towerInfoPanel; // ���� ���������� TowerInfoPanel �� �������
    public TMP_Text textUI; // ���� ���������� InfoText

    void Start()
    {
        if (showPanel)
        {
            textUI.text = infoText;
            towerInfoPanel.SetActive(true);
            Time.timeScale = 0f; // ����� ����
        }
    }

    // �������� ���� ����� ��� ������� �� ������ "�������"
    public void ClosePanel()
    {
        towerInfoPanel.SetActive(false);
        Time.timeScale = 1f; // ������������� ����
    }
}
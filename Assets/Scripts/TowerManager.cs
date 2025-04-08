using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))] // Добавляем автоматически
public class TowerManager : MonoBehaviour
{
    public GameObject contextMenu;
    public GameObject contextMenuUp;
    public GameObject contextMenuMaxUp;
    public RectTransform menuParent;
    private GameObject selectedBuildPoint;
    private GameObject selectedTower;
    private Camera mainCamera;

    private PlayerInput playerInput;
    private InputAction rightClickAction;

    private GameManager gameManager;

    void Start()
    {
        // Автоматически находим GameManager
        gameManager = FindObjectOfType<GameManager>();

        mainCamera = Camera.main;
        contextMenu.SetActive(false);
        contextMenuUp.SetActive(false);
        contextMenuMaxUp.SetActive(false);

        // Настройка Input System
        playerInput = GetComponent<PlayerInput>();
        rightClickAction = playerInput.actions["RightClick"];
    }

    void Update()
    {
        // Обработка правого клика через Input System
        if (rightClickAction.WasPressedThisFrame())
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("BuildPoint"))
                {
                    selectedBuildPoint = hit.collider.gameObject;
                    ShowContextMenu(mousePosition);
                }
                if (hit.collider.CompareTag("Tower"))
                {
                    selectedTower = hit.collider.gameObject;
                    ShowContextMenuUp(mousePosition);
                }
                if (hit.collider.CompareTag("TowerMaxUp"))
                {
                    selectedTower = hit.collider.gameObject;
                    ShowContextMenuMaxUp(mousePosition);
                }
            }
        }

        // Закрытие меню по левому клику
        if (Mouse.current.leftButton.wasPressedThisFrame && contextMenu.activeSelf)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(contextMenu.GetComponent<RectTransform>(), Mouse.current.position.ReadValue()))
            {
                contextMenu.SetActive(false);
            }
        }
        if (Mouse.current.leftButton.wasPressedThisFrame && contextMenuUp.activeSelf)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(contextMenuUp.GetComponent<RectTransform>(), Mouse.current.position.ReadValue()))
            {
                contextMenuUp.SetActive(false);
            }
        }
        if (Mouse.current.leftButton.wasPressedThisFrame && contextMenuMaxUp.activeSelf)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(contextMenuMaxUp.GetComponent<RectTransform>(), Mouse.current.position.ReadValue()))
            {
                contextMenuMaxUp.SetActive(false);
            }
        }
    }

    void ShowContextMenu(Vector2 screenPosition)
    {
        contextMenu.SetActive(true);

        // Конвертируем позицию для UI
        RectTransformUtility.ScreenPointToLocalPointInRectangle(menuParent, screenPosition, null, out Vector2 localPoint);
        contextMenu.GetComponent<RectTransform>().localPosition = localPoint;
    }
    void ShowContextMenuUp(Vector2 screenPosition)
    {
        contextMenuUp.transform.GetChild(2).GetComponent<TMP_Text>().text = ((int)Mathf.Round(selectedTower.GetComponent<Tower>().cost * 1.6f)).ToString();
        contextMenuUp.transform.GetChild(3).GetComponent<TMP_Text>().text = ((int)Mathf.Round(selectedTower.GetComponent<Tower>().cost * 0.6f)).ToString();
        contextMenuUp.SetActive(true);

        // Конвертируем позицию для UI
        RectTransformUtility.ScreenPointToLocalPointInRectangle(menuParent, screenPosition, null, out Vector2 localPoint);
        contextMenuUp.GetComponent<RectTransform>().localPosition = localPoint;
    }
    void ShowContextMenuMaxUp(Vector2 screenPosition)
    {
        contextMenuMaxUp.transform.GetChild(1).GetComponent<TMP_Text>().text = ((int)Mathf.Round(selectedTower.GetComponent<Tower>().cost * 0.6f)).ToString();
        contextMenuMaxUp.SetActive(true);

        // Конвертируем позицию для UI
        RectTransformUtility.ScreenPointToLocalPointInRectangle(menuParent, screenPosition, null, out Vector2 localPoint);
        contextMenuMaxUp.GetComponent<RectTransform>().localPosition = localPoint;
    }

    public void BuildTower(GameObject towerPrefab)
    {
        if (towerPrefab != null && selectedBuildPoint != null && gameManager.money - towerPrefab.GetComponent<Tower>().cost >= 0)
        {
            gameManager.ChangeMoney(-towerPrefab.GetComponent<Tower>().cost);

            Destroy(selectedBuildPoint);
            Instantiate(towerPrefab, selectedBuildPoint.transform.position, Quaternion.identity);

            contextMenu.SetActive(false);
        }
    }

    public void SellTower(GameObject BuildPoint)
    {
        if (selectedTower != null)
        {
            gameManager.ChangeMoney((int)Mathf.Round(selectedTower.GetComponent<Tower>().cost*0.6f));
            contextMenuUp.SetActive(false);
            contextMenuMaxUp.SetActive(false);
            Destroy(selectedTower);
            Instantiate(BuildPoint, selectedTower.transform.position, Quaternion.identity);
        }
    }

    public void TowerUp()
    {
        if (gameManager.money - (int)Mathf.Round(selectedTower.GetComponent<Tower>().cost * 1.6f) >= 0)
        {
            gameManager.ChangeMoney(-(int)Mathf.Round(selectedTower.GetComponent<Tower>().cost * 1.6f));
            contextMenuUp.SetActive(false);
            Destroy(selectedTower);
            Instantiate(selectedTower.GetComponent<Tower>().nextTower, selectedTower.transform.position, Quaternion.identity);
        }
    }
}
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

    private GameManager gameManager;

    private InputAction touchPositionAction;
    private InputAction touchPressAction;

    void Start()
    {
        // Автоматически находим GameManager
        gameManager = FindObjectOfType<GameManager>();

        mainCamera = Camera.main;
        contextMenu.SetActive(false);
        contextMenuUp.SetActive(false);
        contextMenuMaxUp.SetActive(false);

        playerInput = GetComponent<PlayerInput>();
        touchPressAction = playerInput.actions.FindAction("Fire", true); // Стандартное действие
        touchPositionAction = playerInput.actions.FindAction("Point", true); // Для позиции

        if (touchPressAction == null || touchPositionAction == null)
        {
            Debug.LogError("Не найдены необходимые Input Actions!");
            enabled = false;
        }
    }

    void Update()
    {
        // Обработка тапа вместо правого клика
        if (touchPressAction == null || touchPositionAction == null) return;

        if (touchPressAction.WasPressedThisFrame())
        {
            Vector2 touchPosition = touchPositionAction.ReadValue<Vector2>();

            bool isClickInsideMenu =
            (contextMenu.activeSelf && RectTransformUtility.RectangleContainsScreenPoint(contextMenu.GetComponent<RectTransform>(), touchPosition)) ||
            (contextMenuUp.activeSelf && RectTransformUtility.RectangleContainsScreenPoint(contextMenuUp.GetComponent<RectTransform>(), touchPosition)) ||
            (contextMenuMaxUp.activeSelf && RectTransformUtility.RectangleContainsScreenPoint(contextMenuMaxUp.GetComponent<RectTransform>(), touchPosition));

            if (isClickInsideMenu)
            {
                // Нажатие внутри меню — игнорируем
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(touchPosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("BuildPoint"))
                {
                    selectedBuildPoint = hit.collider.gameObject;
                    ShowContextMenu(touchPosition);
                }
                if (hit.collider.CompareTag("Tower"))
                {
                    selectedTower = hit.collider.gameObject;
                    ShowContextMenuUp(touchPosition);
                }
                if (hit.collider.CompareTag("TowerMaxUp"))
                {
                    selectedTower = hit.collider.gameObject;
                    ShowContextMenuMaxUp(touchPosition);
                }
            }
        }

        // Закрытие меню по тапу вне области
        if (touchPressAction.WasPressedThisFrame())
        {
            Vector2 touchPos = touchPositionAction.ReadValue<Vector2>();

            if (contextMenu.activeSelf && !RectTransformUtility.RectangleContainsScreenPoint(contextMenu.GetComponent<RectTransform>(), touchPos))
            {
                contextMenu.SetActive(false);
            }
            if (contextMenuUp.activeSelf && !RectTransformUtility.RectangleContainsScreenPoint(contextMenuUp.GetComponent<RectTransform>(), touchPos))
            {
                contextMenuUp.SetActive(false);
            }
            if (contextMenuMaxUp.activeSelf && !RectTransformUtility.RectangleContainsScreenPoint(contextMenuMaxUp.GetComponent<RectTransform>(), touchPos))
            {
                contextMenuMaxUp.SetActive(false);
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


using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[RequireComponent(typeof(PlayerInput))]
public class TowerManager : MonoBehaviour
{
    [Header("Context Menus")]
    public GameObject contextMenu;
    public GameObject contextMenuUp;
    public GameObject contextMenuMaxUp;
    public RectTransform menuParent;

    private GameObject selectedBuildPoint;
    private GameObject selectedTower;
    private Camera mainCamera;
    private PlayerInput playerInput;
    private InputAction touchPositionAction;
    private InputAction touchPressAction;
    private bool inputBlocked = false;

    [Header("Level Settings")]
    public bool isLevel1 = false;
    public bool isLevel2 = false;

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        isLevel1 = sceneName.Contains("level_1");
        isLevel2 = sceneName.Contains("level_2");

        mainCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();

        InitializeInputActions();
        HideAllContextMenus();

        GameManager.OnGameEnded += HandleGameEnded;
    }

    void OnDestroy()
    {
        GameManager.OnGameEnded -= HandleGameEnded;
    }

    void InitializeInputActions()
    {
        touchPressAction = playerInput.actions.FindAction("Fire");
        touchPositionAction = playerInput.actions.FindAction("Point");

        if (touchPressAction == null || touchPositionAction == null)
        {
            Debug.LogError("Required Input Actions not found!");
            enabled = false;
        }
    }

    void HandleGameEnded(bool gameEnded)
    {
        inputBlocked = gameEnded;
        HideAllContextMenus();

        // Блокируем взаимодействие через отключение компонентов Button
        SetButtonsInteractable(contextMenu, !gameEnded);
        SetButtonsInteractable(contextMenuUp, !gameEnded);
        SetButtonsInteractable(contextMenuMaxUp, !gameEnded);
    }

    void SetButtonsInteractable(GameObject menu, bool interactable)
    {
        if (menu == null) return;

        var buttons = menu.GetComponentsInChildren<Button>(true);
        foreach (var button in buttons)
        {
            button.interactable = interactable;
        }
    }

    void HideAllContextMenus()
    {
        if (contextMenu != null) contextMenu.SetActive(false);
        if (contextMenuUp != null) contextMenuUp.SetActive(false);
        if (contextMenuMaxUp != null) contextMenuMaxUp.SetActive(false);
    }

    void Update()
    {
        if (inputBlocked) return;

        HandleTouchInput();
    }

    void HandleTouchInput()
    {
        if (inputBlocked || GameManager.IsInputBlocked) return;

        if (touchPressAction.WasPressedThisFrame())
        {
            Vector2 touchPosition = touchPositionAction.ReadValue<Vector2>();

            // Усиленная проверка UI
            if (IsPointerOverUI(touchPosition)) return;

            ProcessRaycast(touchPosition);
            CheckForMenuClose(touchPosition);
        }
    }

    // Новый метод для точной проверки UI
    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        // Проверка через EventSystem 
        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        // Дополнительная проверка наших меню
        return IsClickInsideAnyMenu(screenPosition) ||
               (GameManager.Instance != null &&
                (IsClickOnPanel(GameManager.Instance.levelFailedPanel, screenPosition) ||
                 IsClickOnPanel(GameManager.Instance.levelCompletePanel, screenPosition)));
    }

   

    bool IsClickOnPanel(GameObject panel, Vector2 position)
    {
        return panel != null && panel.activeSelf &&
               RectTransformUtility.RectangleContainsScreenPoint(
                   panel.GetComponent<RectTransform>(),
                   position
               );
    }

    bool IsClickInsideAnyMenu(Vector2 touchPosition)
    {
        return (contextMenu != null && contextMenu.activeSelf && IsPositionInMenu(contextMenu, touchPosition)) ||
               (contextMenuUp != null && contextMenuUp.activeSelf && IsPositionInMenu(contextMenuUp, touchPosition)) ||
               (contextMenuMaxUp != null && contextMenuMaxUp.activeSelf && IsPositionInMenu(contextMenuMaxUp, touchPosition));
    }

    bool IsPositionInMenu(GameObject menu, Vector2 position)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            menu.GetComponent<RectTransform>(),
            position
        );
    }

    void ProcessRaycast(Vector2 touchPosition)
    {
        if (inputBlocked) return;

        Ray ray = mainCamera.ScreenPointToRay(touchPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("BuildPoint"))
            {
                selectedBuildPoint = hit.collider.gameObject;
                ShowContextMenu(touchPosition);
            }
            else if (hit.collider.CompareTag("Tower"))
            {
                selectedTower = hit.collider.gameObject;
                ShowContextMenuUp(touchPosition);
            }
            else if (hit.collider.CompareTag("TowerMaxUp"))
            {
                selectedTower = hit.collider.gameObject;
                ShowContextMenuMaxUp(touchPosition);
            }
        }
    }

    void CheckForMenuClose(Vector2 touchPosition)
    {
        if (contextMenu != null && contextMenu.activeSelf && !IsPositionInMenu(contextMenu, touchPosition))
            contextMenu.SetActive(false);

        if (contextMenuUp != null && contextMenuUp.activeSelf && !IsPositionInMenu(contextMenuUp, touchPosition))
            contextMenuUp.SetActive(false);

        if (contextMenuMaxUp != null && contextMenuMaxUp.activeSelf && !IsPositionInMenu(contextMenuMaxUp, touchPosition))
            contextMenuMaxUp.SetActive(false);
    }

    void ShowContextMenu(Vector2 screenPosition)
    {
        if (contextMenu == null || inputBlocked) return;

        if (isLevel1 || isLevel2)
        {
            // Получаем первую кнопку (индекс 0)
            Transform firstButton = contextMenu.transform.GetChild(0);
            Button button = firstButton.GetComponent<Button>();

            // Получаем TextCost (индекс 1 в BuildPanel)
            if (contextMenu.transform.childCount > 1) // Проверка, чтобы не было ошибки
            {
                TMP_Text priceText = contextMenu.transform.GetChild(1).GetComponent<TMP_Text>();

                if (priceText != null && button != null)
                {
                    // Получаем префаб башни из OnClick
                    if (button.onClick.GetPersistentEventCount() > 0)
                    {
                        GameObject towerPrefab = button.onClick.GetPersistentTarget(0) as GameObject;
                        if (towerPrefab != null)
                        {
                            Tower tower = towerPrefab.GetComponent<Tower>();
                            if (tower != null)
                            {
                                priceText.text = tower.cost.ToString(); // Устанавливаем цену
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("Не найден TextCost или компонент Button!");
                }
            }

            // Отключаем остальные кнопки (начиная с индекса 2)
            for (int i = 2; i < contextMenu.transform.childCount; i++)
            {
                contextMenu.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        // Показываем меню и устанавливаем позицию
        contextMenu.SetActive(true);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            menuParent,
            screenPosition,
            null,
            out Vector2 localPoint
        );
        contextMenu.GetComponent<RectTransform>().localPosition = localPoint;
    }

    void ShowContextMenuUp(Vector2 screenPosition)
    {
        if (contextMenuUp == null || selectedTower == null || inputBlocked) return;

        // Получаем компонент Tower
        Tower tower = selectedTower.GetComponent<Tower>();
        if (tower == null) return;

        // Для Level 1 показываем меню максимального уровня
        if (isLevel1)
        {
            ShowContextMenuMaxUp(screenPosition);
            return;
        }

        // Для Level 2 особая логика
        if (isLevel2)
        {
            // Активируем только нужные кнопки
            contextMenuUp.transform.GetChild(0).gameObject.SetActive(true); // Улучшить
            contextMenuUp.transform.GetChild(1).gameObject.SetActive(true); // Продать

            // Устанавливаем цены для Level 2
            TMP_Text upgradePriceText = contextMenuUp.transform.Find("TextCost")?.GetComponent<TMP_Text>();
            TMP_Text sellPriceText = contextMenuUp.transform.Find("TextCostSell")?.GetComponent<TMP_Text>();

            if (upgradePriceText != null)
                upgradePriceText.text = Mathf.RoundToInt(tower.cost * 1.6f).ToString();

            if (sellPriceText != null)
                sellPriceText.text = Mathf.RoundToInt(tower.cost * 0.6f).ToString();
        }
        else
        {
            // Стандартная логика для других уровней
            // Получаем текстовые элементы (улучшение и продажа)
            TMP_Text upgradePriceText = contextMenuUp.transform.Find("TextCost")?.GetComponent<TMP_Text>();
            TMP_Text sellPriceText = contextMenuUp.transform.Find("TextCostSell")?.GetComponent<TMP_Text>();

            // Устанавливаем цены
            if (upgradePriceText != null)
                upgradePriceText.text = Mathf.RoundToInt(tower.cost * 1.6f).ToString();

            if (sellPriceText != null)
                sellPriceText.text = Mathf.RoundToInt(tower.cost * 0.6f).ToString();
        }

        // Активируем меню и устанавливаем позицию
        contextMenuUp.SetActive(true);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            menuParent,
            screenPosition,
            null,
            out Vector2 localPoint
        );
        contextMenuUp.GetComponent<RectTransform>().localPosition = localPoint;
    }

    void ShowContextMenuMaxUp(Vector2 screenPosition)
    {
        if (contextMenuMaxUp == null || selectedTower == null || inputBlocked) return;

        var tower = selectedTower.GetComponent<Tower>();
        if (tower == null) return;

        contextMenuMaxUp.transform.GetChild(1).GetComponent<TMP_Text>().text = ((int)Mathf.Round(tower.cost * 0.6f)).ToString();
        contextMenuMaxUp.SetActive(true);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(menuParent, screenPosition, null, out Vector2 localPoint);
        contextMenuMaxUp.GetComponent<RectTransform>().localPosition = localPoint;
    }

    public void BuildTower(GameObject towerPrefab)
    {
        if (inputBlocked || towerPrefab == null || selectedBuildPoint == null) return;

        var tower = towerPrefab.GetComponent<Tower>();
        if (tower == null) return;

        if (GameManager.Instance.money - tower.cost >= 0)
        {
            GameManager.Instance.ChangeMoney(-tower.cost);

            // Создаем башню
            GameObject newTower = Instantiate(towerPrefab, selectedBuildPoint.transform.position, Quaternion.identity);

            // Меняем тег для уровня 1
            if (isLevel1)
            {
                newTower.tag = "TowerMaxUp"; // Меняем тег только у этого экземпляра
                Tower towerComponent = newTower.GetComponent<Tower>();
                if (towerComponent != null)
                {
                    towerComponent.nextTower = null; // Отключаем улучшение
                }
            }

            Destroy(selectedBuildPoint);
            contextMenu.SetActive(false);
        }
    }

    public void SellTower(GameObject BuildPoint)
    {
        if (inputBlocked || selectedTower == null || BuildPoint == null) return;

        var tower = selectedTower.GetComponent<Tower>();
        if (tower == null) return;

        GameManager.Instance.ChangeMoney((int)Mathf.Round(tower.cost * 0.6f));
        contextMenuUp.SetActive(false);
        contextMenuMaxUp.SetActive(false);
        Destroy(selectedTower);
        Instantiate(BuildPoint, selectedTower.transform.position, Quaternion.identity);
    }

    public void TowerUp()
    {
        if (inputBlocked || selectedTower == null) return;

        var tower = selectedTower.GetComponent<Tower>();
        if (tower == null || tower.nextTower == null) return;

        int upgradeCost = (int)Mathf.Round(tower.cost * 1.6f);
        if (GameManager.Instance.money - upgradeCost >= 0)
        {
            GameManager.Instance.ChangeMoney(-upgradeCost);
            contextMenuUp.SetActive(false);
            Destroy(selectedTower);
            Instantiate(tower.nextTower, selectedTower.transform.position, Quaternion.identity);
        }
    }
}
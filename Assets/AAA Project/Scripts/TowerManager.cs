using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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

    void Start()
    {
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
        if (touchPressAction.WasPressedThisFrame())
        {
            Vector2 touchPosition = touchPositionAction.ReadValue<Vector2>();

            if (IsClickInsideAnyMenu(touchPosition))
                return;

            ProcessRaycast(touchPosition);
            CheckForMenuClose(touchPosition);
        }
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

        contextMenu.SetActive(true);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(menuParent, screenPosition, null, out Vector2 localPoint);
        contextMenu.GetComponent<RectTransform>().localPosition = localPoint;
    }

    void ShowContextMenuUp(Vector2 screenPosition)
    {
        if (contextMenuUp == null || selectedTower == null || inputBlocked) return;

        var tower = selectedTower.GetComponent<Tower>();
        if (tower == null) return;

        contextMenuUp.transform.GetChild(2).GetComponent<TMP_Text>().text = ((int)Mathf.Round(tower.cost * 1.6f)).ToString();
        contextMenuUp.transform.GetChild(3).GetComponent<TMP_Text>().text = ((int)Mathf.Round(tower.cost * 0.6f)).ToString();
        contextMenuUp.SetActive(true);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(menuParent, screenPosition, null, out Vector2 localPoint);
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
            Destroy(selectedBuildPoint);
            Instantiate(towerPrefab, selectedBuildPoint.transform.position, Quaternion.identity);
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
using UnityEngine;
using Zenject;
using System.Collections;

public class FakeTrashChute : BaseInteractable
{
    [Header("Настройки")]
    [SerializeField] private string interactPrompt = "Выкинуть мусор";
    [SerializeField] private string trashItemName = "Мусорный пакет";
    [SerializeField] private int requiredTaskIndex = 9; // Индекс задания "Выкинуть мусор на улицу"

    [Header("UI Мыслей (Опционально)")]
    [SerializeField] private TMPro.TextMeshProUGUI thoughtText;
    [SerializeField] private string thoughtMessage = "Заварено... Нужно спуститься на этаж ниже.";
    [SerializeField] private float thoughtDuration = 3f;

    private bool isLocked = true;
    private bool isThoughtShowing = false;
    
    private IPlayerInventory inventory;
    private ITaskManager taskManager;

    [Inject]
    public void Construct(IPlayerInventory inventory, ITaskManager taskManager)
    {
        this.inventory = inventory;
        this.taskManager = taskManager;
    }

    public override string InteractionPrompt 
    {
        get
        {
            if (isLocked || inventory == null || !inventory.HasItem(trashItemName))
                return "";
            
            return isThoughtShowing ? "" : interactPrompt;
        }
    }

    private void OnEnable()
    {
        GameEventManager.OnTaskChanged += HandleTaskChanged;
        
        if (taskManager != null)
        {
            HandleTaskChanged(taskManager.GetCurrentTaskIndex());
        }
    }

    private void OnDisable()
    {
        GameEventManager.OnTaskChanged -= HandleTaskChanged;
    }

    private void HandleTaskChanged(int newTaskIndex)
    {
        isLocked = (newTaskIndex != requiredTaskIndex);
    }

    public override bool Interact(GameObject interactor)
    {
        if (isLocked || isThoughtShowing) return false;

        if (inventory != null && inventory.HasItem(trashItemName))
        {
            // Показываем мысли
            if (thoughtText != null)
            {
                StartCoroutine(ShowThoughtCoroutine());
            }
            else
            {
                Debug.Log(thoughtMessage);
            }
            return true;
        }

        return false;
    }

    private IEnumerator ShowThoughtCoroutine()
    {
        isThoughtShowing = true;
        thoughtText.text = thoughtMessage;
        thoughtText.gameObject.SetActive(true);

        yield return new WaitForSeconds(thoughtDuration);

        thoughtText.gameObject.SetActive(false);
        isThoughtShowing = false;
    }

    public override void OnHoverEnter()
    {
        if (isLocked || inventory == null || !inventory.HasItem(trashItemName)) return;
        base.OnHoverEnter();
    }
}
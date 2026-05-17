using UnityEngine;
using Zenject;

public class TrashChute : BaseInteractable
{
    [Header("Настройки мусоропровода")]
    [SerializeField] private string throwPrompt = "Выкинуть мусор";
    [SerializeField] private string trashItemName = "Мусорный пакет";
    [SerializeField] private int requiredTaskIndex = 9; // Индекс задания "Выкинуть мусор на улицу"

    private bool isLocked = true;
    
    private IPlayerInventory inventory;
    private ITaskManager taskManager;

    [Inject]
    public void Construct(IPlayerInventory inventory, ITaskManager taskManager)
    {
        this.inventory = inventory;
        this.taskManager = taskManager;
    }

    public override string InteractionPrompt => isLocked || !inventory.HasItem(trashItemName) ? "" : throwPrompt;

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
        if (isLocked) return false;

        if (inventory != null && inventory.HasItem(trashItemName))
        {
            inventory.ClearHand();
            Debug.Log("Мусор выкинут!");
            
            if (taskManager != null)
            {
                taskManager.CompleteCurrentTask();
            }
            return true;
        }

        return false;
    }

    public override void OnHoverEnter()
    {
        if (isLocked || inventory == null || !inventory.HasItem(trashItemName)) return;
        base.OnHoverEnter();
    }
}
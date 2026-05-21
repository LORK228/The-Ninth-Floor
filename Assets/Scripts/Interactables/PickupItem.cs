using UnityEngine;
using Zenject;

public class PickupItem : BaseInteractable
{
    [Header("Настройки предмета")]
    [SerializeField] private string prompt = "Взять";
    [SerializeField] private string itemName = "Предмет";
    [SerializeField] private GameObject itemPrefabInHand; 
    
    [Header("Настройки квеста")]
    [Tooltip("Индекс квеста, на котором этот предмет можно взять. -1, если можно взять всегда.")]
    [SerializeField] private int requiredTaskIndex = -1; 
    [Tooltip("Завершить текущий квест при подборе этого предмета?")]
    [SerializeField] private bool completeTaskOnPickup = false;

    [Header("Положение в руке")]
    [SerializeField] private Vector3 handPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 handRotationOffset = Vector3.zero;

    private bool isLocked = false;
    
    // Зависим от ИНТЕРФЕЙСОВ
    private IPlayerInventory inventory;
    private ITaskManager taskManager;

    [Inject]
    public void Construct(IPlayerInventory inventory, ITaskManager taskManager)
    {
        this.inventory = inventory;
        this.taskManager = taskManager;
    }

    public override string InteractionPrompt => isLocked ? "" : prompt;

    private void OnEnable()
    {
        if (requiredTaskIndex != -1)
        {
            GameEventManager.OnTaskChanged += HandleTaskChanged;
            
            // Проверяем состояние сразу при включении
            if (taskManager != null)
            {
                HandleTaskChanged(taskManager.GetCurrentTaskIndex());
            }
        }
    }

    private void OnDisable()
    {
        if (requiredTaskIndex != -1)
        {
            GameEventManager.OnTaskChanged -= HandleTaskChanged;
        }
    }

    private void HandleTaskChanged(int newTaskIndex)
    {
        isLocked = (newTaskIndex != requiredTaskIndex);
    }

    public override bool Interact(GameObject interactor)
    {
        if (isLocked) return false;

        if (inventory != null)
        {
            inventory.GiveItem(itemName, itemPrefabInHand, handPositionOffset, handRotationOffset);

            // Если нужно, завершаем текущий квест
            if (completeTaskOnPickup && taskManager != null)
            {
                // Убедимся, что мы завершаем именно тот квест, на котором подобрали предмет
                if (requiredTaskIndex == -1 || taskManager.GetCurrentTaskIndex() == requiredTaskIndex)
                {
                    taskManager.CompleteCurrentTask();
                }
            }

            Destroy(gameObject);
            return true;
        }

        return false;
    }

    public override void OnHoverEnter()
    {
        if (isLocked) return;
        base.OnHoverEnter();
    }
}
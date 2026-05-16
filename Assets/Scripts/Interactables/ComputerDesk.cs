using UnityEngine;
using Zenject;

public class ComputerDesk : BaseInteractable
{
    [Header("Настройки")]
    [SerializeField] private string requiredItem = "Горячая еда";
    [SerializeField] private string prompt = "Поставить еду";
    [SerializeField] private int taskIndexRequired = 4; 
    
    [SerializeField] private GameObject foodOnDeskPrefab;
    [SerializeField] private Transform foodSpawnPoint;

    private bool hasFood = false;
    private bool isLocked = true;

    // Зависим от ИНТЕРФЕЙСОВ
    private IPlayerInventory inventory;
    private ITaskManager taskManager;

    [Inject]
    public void Construct(IPlayerInventory inventory, ITaskManager taskManager)
    {
        this.inventory = inventory;
        this.taskManager = taskManager;
    }

    public override string InteractionPrompt => hasFood || isLocked ? "" : (inventory != null && inventory.HasItem(requiredItem)) ? prompt : "Нужна горячая еда";

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
        isLocked = (newTaskIndex != taskIndexRequired);
    }

    public override bool Interact(GameObject interactor)
    {
        if (hasFood || isLocked) return false;

        if (inventory != null && inventory.HasItem(requiredItem))
        {
            inventory.ClearHand();
            
            if (foodOnDeskPrefab && foodSpawnPoint)
            {
                Instantiate(foodOnDeskPrefab, foodSpawnPoint.position, foodSpawnPoint.rotation);
            }

            hasFood = true;
            OnHoverExit();

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
        if (hasFood || isLocked) return;
        base.OnHoverEnter();
    }
}
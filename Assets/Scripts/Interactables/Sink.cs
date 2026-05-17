using UnityEngine;
using Zenject;

public class Sink : BaseInteractable
{
    [Header("Настройки")]
    [SerializeField] private string requiredItem = "Грязная тарелка";
    [SerializeField] private string prompt = "Положить тарелку";
    [SerializeField] private int taskIndexRequired = 7; 
    
    [SerializeField] private GameObject plateInSinkPrefab;
    [SerializeField] private Transform plateSpawnPoint;

    private bool hasPlate = false;
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

    public override string InteractionPrompt => hasPlate || isLocked ? "" : (inventory != null && inventory.HasItem(requiredItem)) ? prompt : "Нужна грязная тарелка";

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
        print(isLocked);
        
        if (hasPlate || isLocked) return false;

        if (inventory != null && inventory.HasItem(requiredItem))
        {
            inventory.ClearHand();
            
            if (plateInSinkPrefab && plateSpawnPoint)
            {
                // Тут можно использовать обычный Instantiate, так как тарелка в раковине 
                // скорее всего просто декорация и не требует инъекций.
                Instantiate(plateInSinkPrefab, plateSpawnPoint.position, plateSpawnPoint.rotation, plateSpawnPoint);
            }

            hasPlate = true;
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
        if (hasPlate || isLocked) return;
        base.OnHoverEnter();
    }
}
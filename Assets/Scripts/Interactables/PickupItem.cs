using UnityEngine;
using Zenject;

public class PickupItem : BaseInteractable
{
    [Header("Настройки предмета")]
    [SerializeField] private string prompt = "Взять еду";
    [SerializeField] private string itemName = "Холодная еда";
    [SerializeField] private GameObject itemPrefabInHand; 
    
    [SerializeField] private int requiredTaskIndex = 3; 

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
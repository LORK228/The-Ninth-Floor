using UnityEngine;
using Zenject;

/// <summary>
/// Скрипт для предметов, которые кладутся в "карман" (без появления в руках).
/// </summary>
public class PocketItem : BaseInteractable
{
    [Header("Настройки предмета")]
    [SerializeField] private string prompt = "Взять";
    [SerializeField] private string itemName = "Ключ от квартиры";
    [Tooltip("Если -1, можно взять когда угодно.")]
    [SerializeField] private int requiredTaskIndex = -1; 

    private bool isLocked = false;
    
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
            inventory.AddToPocket(itemName);
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
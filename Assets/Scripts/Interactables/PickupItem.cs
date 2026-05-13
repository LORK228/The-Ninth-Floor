using UnityEngine;

public class PickupItem : BaseInteractable
{
    [Header("Настройки предмета")]
    [SerializeField] private string prompt = "Взять еду";
    [SerializeField] private string itemName = "Холодная еда";
    [SerializeField] private GameObject itemPrefabInHand; 
    
    [Tooltip("Если -1, то можно взять всегда. Иначе - только на определенном этапе игры.")]
    [SerializeField] private int requiredTaskIndex = 3; 

    private bool isLocked = false;

    public override string InteractionPrompt => isLocked ? "" : prompt;

    private void OnEnable()
    {
        if (requiredTaskIndex != -1)
        {
            GameEventManager.OnTaskChanged += HandleTaskChanged;
            // Проверяем текущее состояние при включении
            if (TaskManager.Instance != null)
            {
                HandleTaskChanged(TaskManager.Instance.GetCurrentTaskIndex());
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

        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.GiveItem(itemName, itemPrefabInHand);
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
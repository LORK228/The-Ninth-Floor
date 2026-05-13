using UnityEngine;

public class PickupItem : BaseInteractable
{
    [Header("Настройки предмета")]
    [SerializeField] private string prompt = "Взять еду";
    [SerializeField] private string itemName = "Холодная еда";
    [SerializeField] private GameObject itemPrefabInHand; 
    
    [Tooltip("Если -1, то можно взять всегда. Иначе - только на определенном этапе игры.")]
    [SerializeField] private int requiredTaskIndex = 3; 

    [Header("Положение в руке")]
    [Tooltip("Смещение объекта в руке (относительно HandPoint)")]
    [SerializeField] private Vector3 handPositionOffset = Vector3.zero;
    [Tooltip("Поворот объекта в руке (относительно HandPoint)")]
    [SerializeField] private Vector3 handRotationOffset = Vector3.zero;

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
            // Передаем настройки смещения в инвентарь
            PlayerInventory.Instance.GiveItem(itemName, itemPrefabInHand, handPositionOffset, handRotationOffset);
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
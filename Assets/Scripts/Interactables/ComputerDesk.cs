using UnityEngine;

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

    public override string InteractionPrompt => hasFood || isLocked ? "" : (PlayerInventory.Instance != null && PlayerInventory.Instance.HasItem(requiredItem)) ? prompt : "Нужна горячая еда";

    private void OnEnable()
    {
        GameEventManager.OnTaskChanged += HandleTaskChanged;
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

        if (PlayerInventory.Instance != null && PlayerInventory.Instance.HasItem(requiredItem))
        {
            PlayerInventory.Instance.ClearHand();
            
            if (foodOnDeskPrefab != null && foodSpawnPoint != null)
            {
                Instantiate(foodOnDeskPrefab, foodSpawnPoint.position, foodSpawnPoint.rotation);
            }

            hasFood = true;
            OnHoverExit();

            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.CompleteCurrentTask();
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
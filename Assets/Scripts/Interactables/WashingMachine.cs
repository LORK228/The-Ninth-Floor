using UnityEngine;

public class WashingMachine : BaseInteractable
{
    [Header("Настройки")]
    [SerializeField] private string prompt = "Достать белье";
    [SerializeField] private int taskIndexRequired = 1;

    [Header("Выдаваемый предмет")]
    [SerializeField] private string itemName = "Таз с бельем";
    [SerializeField] private GameObject basketPrefab; 

    [Header("Положение в руке")]
    [Tooltip("Смещение объекта в руке (относительно HandPoint)")]
    [SerializeField] private Vector3 handPositionOffset = Vector3.zero;
    [Tooltip("Поворот объекта в руке (относительно HandPoint)")]
    [SerializeField] private Vector3 handRotationOffset = Vector3.zero;

    private bool isEmptied = false;
    private bool isLocked = true; 

    public override string InteractionPrompt => isEmptied || isLocked ? "" : prompt;

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
        if (newTaskIndex == taskIndexRequired && !isEmptied)
        {
            isLocked = false;
        }
        else
        {
            isLocked = true;
        }
    }

    public override bool Interact(GameObject interactor)
    {
        if (isEmptied || isLocked) return false;

        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.GiveItem(itemName, basketPrefab, handPositionOffset, handRotationOffset);
            isEmptied = true;
            isLocked = true;
            OnHoverExit(); 
            
            return true;
        }
        
        return false;
    }

    public override void OnHoverEnter()
    {
        if (isEmptied || isLocked) return;
        base.OnHoverEnter();
    }
}
using UnityEngine;

public class WashingMachine : BaseInteractable
{
    [Header("Настройки")]
    [SerializeField] private string prompt = "Достать белье";
    [SerializeField] private int taskIndexRequired = 1;

    [Header("Выдаваемый предмет")]
    [SerializeField] private string itemName = "Таз с бельем";
    [SerializeField] private GameObject basketPrefab; 

    private bool isEmptied = false;
    private bool isLocked = true; // Заблокировано, пока не наступит нужное задание

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
            PlayerInventory.Instance.GiveItem(itemName, basketPrefab);
            isEmptied = true;
            isLocked = true;
            OnHoverExit(); 
            
            // Если нужно сразу переключить задание после взятия таза - можно раскомментировать
            // if (TaskManager.Instance != null) TaskManager.Instance.CompleteCurrentTask();
            
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
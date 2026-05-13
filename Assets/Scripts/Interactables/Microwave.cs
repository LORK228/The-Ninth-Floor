using System.Collections;
using UnityEngine;

public class Microwave : BaseInteractable
{
    [Header("Настройки")]
    [SerializeField] private string requiredItem = "Холодная еда";
    [SerializeField] private string heatedItemName = "Горячая еда";
    [SerializeField] private GameObject heatedItemPrefab; 
    
    [SerializeField] private GameObject foodInsidePrefab;
    [SerializeField] private Transform insidePoint;
    
    [SerializeField] private float heatTime = 10f;
    [SerializeField] private int taskIndexRequired = 3; 

    [Header("Положение в руке (готовая еда)")]
    [Tooltip("Смещение объекта в руке (относительно HandPoint)")]
    [SerializeField] private Vector3 handPositionOffset = Vector3.zero;
    [Tooltip("Поворот объекта в руке (относительно HandPoint)")]
    [SerializeField] private Vector3 handRotationOffset = Vector3.zero;
    
    public enum MicrowaveState { Idle, Heating, Done, Locked }
    private MicrowaveState currentState = MicrowaveState.Locked;
    
    private GameObject currentFoodInside; 

    public override string InteractionPrompt
    {
        get
        {
            switch (currentState)
            {
                case MicrowaveState.Locked:
                    return ""; 
                case MicrowaveState.Idle:
                    return (PlayerInventory.Instance != null && PlayerInventory.Instance.HasItem(requiredItem)) ? "Поставить еду" : "Нужна холодная еда";
                case MicrowaveState.Heating:
                    return "Греется...";
                case MicrowaveState.Done:
                    return "Забрать еду";
                default:
                    return "";
            }
        }
    }

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
        if (newTaskIndex == taskIndexRequired && currentState == MicrowaveState.Locked)
        {
            currentState = MicrowaveState.Idle;
        }
        else if (newTaskIndex != taskIndexRequired && currentState == MicrowaveState.Idle)
        {
            currentState = MicrowaveState.Locked;
        }
    }

    public override bool Interact(GameObject interactor)
    {
        if (currentState == MicrowaveState.Locked) return false;

        if (currentState == MicrowaveState.Idle)
        {
            if (PlayerInventory.Instance != null && PlayerInventory.Instance.HasItem(requiredItem))
            {
                PlayerInventory.Instance.ClearHand();
                
                if (foodInsidePrefab != null && insidePoint != null)
                {
                    currentFoodInside = Instantiate(foodInsidePrefab, insidePoint.position, insidePoint.rotation, insidePoint);
                }

                StartCoroutine(HeatRoutine());
                return true;
            }
        }
        else if (currentState == MicrowaveState.Done)
        {
            if (PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.GiveItem(heatedItemName, heatedItemPrefab, handPositionOffset, handRotationOffset);
                
                if (currentFoodInside != null)
                {
                    Destroy(currentFoodInside);
                }

                currentState = MicrowaveState.Idle;
                OnHoverExit();
                
                if (TaskManager.Instance != null)
                {
                    TaskManager.Instance.CompleteCurrentTask();
                }
                
                return true;
            }
        }

        return false;
    }

    private IEnumerator HeatRoutine()
    {
        currentState = MicrowaveState.Heating;
        yield return new WaitForSeconds(heatTime);
        currentState = MicrowaveState.Done;
    }

    public override void OnHoverEnter()
    {
        if (currentState == MicrowaveState.Heating || currentState == MicrowaveState.Locked) return;
        base.OnHoverEnter();
    }
}
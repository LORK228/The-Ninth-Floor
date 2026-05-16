using System.Collections;
using UnityEngine;
using Zenject;
using DG.Tweening;

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
    [SerializeField] private Vector3 handPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 handRotationOffset = Vector3.zero;
    
    public enum MicrowaveState { Idle, Heating, Done, Locked }
    private MicrowaveState currentState = MicrowaveState.Locked;
    
    private GameObject currentFoodInside; 

    private IPlayerInventory inventory;
    private ITaskManager taskManager;

    [Inject]
    public void Construct(IPlayerInventory inventory, ITaskManager taskManager)
    {
        this.inventory = inventory;
        this.taskManager = taskManager;
    }

    public override string InteractionPrompt
    {
        get
        {
            switch (currentState)
            {
                case MicrowaveState.Locked:
                    return ""; 
                case MicrowaveState.Idle:
                    return (inventory != null && inventory.HasItem(requiredItem)) ? "Поставить еду" : "Нужна холодная еда";
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
            if (inventory != null && inventory.HasItem(requiredItem))
            {
                inventory.ClearHand();
                
                if (foodInsidePrefab && insidePoint)
                {
                    // 1. Создаем пустой пивот и делаем его дочерним к insidePoint
                    GameObject centerPivot = new GameObject("FoodCenterPivot");
                    centerPivot.transform.SetParent(insidePoint, false);
                    centerPivot.transform.position = insidePoint.position;
                    centerPivot.transform.rotation = insidePoint.rotation;

                    // 2. Спавним еду сразу как дочернюю к пивоту, чтобы она правильно унаследовала Scale
                    GameObject actualFood = Instantiate(foodInsidePrefab, insidePoint.position, insidePoint.rotation, centerPivot.transform);
                    
                    // 3. Вычисляем геометрический центр еды
                    Renderer[] renderers = actualFood.GetComponentsInChildren<Renderer>();
                    if (renderers.Length > 0)
                    {
                        Bounds bounds = renderers[0].bounds;
                        for (int i = 1; i < renderers.Length; i++)
                        {
                            bounds.Encapsulate(renderers[i].bounds);
                        }
                        
                        // 4. Чтобы переместить пивот в центр, не сдвинув саму еду:
                        // Временно отвязываем еду
                        actualFood.transform.SetParent(insidePoint, true); 
                        
                        // Ставим пивот ровно в геометрический центр
                        centerPivot.transform.position = bounds.center;
                        
                        // Привязываем еду обратно к пивоту
                        actualFood.transform.SetParent(centerPivot.transform, true);
                    }

                    currentFoodInside = centerPivot;
                }

                StartCoroutine(HeatRoutine());
                return true;
            }
        }
        else if (currentState == MicrowaveState.Done)
        {
            if (inventory != null)
            {
                inventory.GiveItem(heatedItemName, heatedItemPrefab, handPositionOffset, handRotationOffset);
                
                if (currentFoodInside)
                {
                    currentFoodInside.transform.DOKill();
                    Destroy(currentFoodInside);
                }

                currentState = MicrowaveState.Idle;
                OnHoverExit();
                
                if (taskManager != null)
                {
                    taskManager.CompleteCurrentTask();
                }
                
                return true;
            }
        }

        return false;
    }

    private IEnumerator HeatRoutine()
    {
        currentState = MicrowaveState.Heating;
        
        if (currentFoodInside)
        {
            currentFoodInside.transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
                .SetRelative()
                .SetEase(Ease.Linear)
                .SetLoops(-1);
        }

        yield return new WaitForSeconds(heatTime);
        
        if (currentFoodInside)
        {
            currentFoodInside.transform.DOKill();
        }

        currentState = MicrowaveState.Done;
    }

    public override void OnHoverEnter()
    {
        if (currentState == MicrowaveState.Heating || currentState == MicrowaveState.Locked) return;
        base.OnHoverEnter();
    }
}
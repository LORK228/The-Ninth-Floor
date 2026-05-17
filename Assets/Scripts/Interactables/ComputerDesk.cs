using UnityEngine;
using Zenject;

public class ComputerDesk : BaseInteractable
{
    [Header("Настройки")]
    [SerializeField] private string requiredItem = "Горячая еда";
    [SerializeField] private string prompt = "Поставить еду";
    [SerializeField] private int taskIndexRequired = 4; 
    
    [SerializeField] private GameObject foodOnDeskPrefab;
    [Tooltip("Точка на столе (должна быть пустым объектом). Поверните этот объект, чтобы настроить поворот еды.")]
    [SerializeField] private Transform foodSpawnPoint;

    [Header("Связи для еды")]
    [Tooltip("Ссылки, которые будут переданы заспавненной еде")]
    [SerializeField] private ComputerMonitor computerMonitor;
    [SerializeField] private Chair chair;

    private bool hasFood = false;
    private bool isLocked = true;

    // Зависим от ИНТЕРФЕЙСОВ
    private IPlayerInventory inventory;
    private ITaskManager taskManager;
    private DiContainer container; 

    [Inject]
    public void Construct(IPlayerInventory inventory, ITaskManager taskManager, DiContainer container)
    {
        this.inventory = inventory;
        this.taskManager = taskManager;
        this.container = container;
    }

    public override string InteractionPrompt => hasFood || isLocked ? "" : (inventory != null && inventory.HasItem(requiredItem)) ? prompt : "Нужна горячая еда";

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
        if (hasFood || isLocked) return false;

        if (inventory != null && inventory.HasItem(requiredItem))
        {
            inventory.ClearHand();
            
            if (foodOnDeskPrefab && foodSpawnPoint)
            {
                // 1. Создаем еду через Zenject БЕЗ привязки к родителю
                GameObject spawnedFood = container.InstantiatePrefab(foodOnDeskPrefab);

                // 2. Делаем её дочерней, но false означает "не пытайся сохранить мировые координаты/скейл, 
                // просто примени локальные нули". Это защищает от искажений скейла стола.
                spawnedFood.transform.SetParent(foodSpawnPoint, false);
                
                // 3. Жестко сбрасываем локальные координаты в 0
                // Теперь еда будет ровно в центре foodSpawnPoint, а её размер будет зависеть 
                // только от размера самого префаба и масштаба foodSpawnPoint.
                spawnedFood.transform.localPosition = Vector3.zero;
                spawnedFood.transform.localRotation = Quaternion.identity;
                
                // Восстанавливаем оригинальный размер префаба
                spawnedFood.transform.localScale = foodOnDeskPrefab.transform.localScale;

                // Инициализируем еду ссылками на монитор и кресло
                EatableFood eatableScript = spawnedFood.GetComponent<EatableFood>();
                if (eatableScript != null)
                {
                    eatableScript.Initialize(computerMonitor, chair);
                }
            }

            hasFood = true;
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
        if (hasFood || isLocked) return;
        base.OnHoverEnter();
    }
}
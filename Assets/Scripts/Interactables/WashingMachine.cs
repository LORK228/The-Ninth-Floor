using UnityEngine;
using Zenject;

public class WashingMachine : BaseInteractable
{
    [Header("Настройки")]
    [SerializeField] private string prompt = "Достать белье";
    [SerializeField] private int taskIndexRequired = 1;

    [Header("Выдаваемый предмет")]
    [SerializeField] private string itemName = "Таз с бельем";
    [SerializeField] private GameObject basketPrefab; 

    [Header("Положение в руке")]
    [SerializeField] private Vector3 handPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 handRotationOffset = Vector3.zero;

    [Header("Звуки")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip takeClothesSound;

    private bool isEmptied = false;
    private bool isLocked = true; 

    // Зависим от ИНТЕРФЕЙСОВ
    private IPlayerInventory inventory;
    private ITaskManager taskManager;

    [Inject]
    public void Construct(IPlayerInventory inventory, ITaskManager taskManager)
    {
        this.inventory = inventory;
        this.taskManager = taskManager;
    }

    public override string InteractionPrompt => isEmptied || isLocked ? "" : prompt;

    protected override void Awake()
    {
        base.Awake();
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
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

        if (inventory != null)
        {
            inventory.GiveItem(itemName, basketPrefab, handPositionOffset, handRotationOffset);
            isEmptied = true;
            isLocked = true;
            OnHoverExit(); 

            if (audioSource != null && takeClothesSound != null)
            {
                audioSource.PlayOneShot(takeClothesSound);
            }
            
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
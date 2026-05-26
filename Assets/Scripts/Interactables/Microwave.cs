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

    [Header("Звуки")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip startSound;
    [SerializeField] private AudioClip workingLoopSound;
    [SerializeField] private AudioClip doneSound;
    
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
                    GameObject centerPivot = new GameObject("FoodCenterPivot");
                    centerPivot.transform.SetParent(insidePoint, false);
                    centerPivot.transform.position = insidePoint.position;
                    centerPivot.transform.rotation = insidePoint.rotation;

                    GameObject actualFood = Instantiate(foodInsidePrefab, insidePoint.position, insidePoint.rotation, centerPivot.transform);
                    
                    Renderer[] renderers = actualFood.GetComponentsInChildren<Renderer>();
                    if (renderers.Length > 0)
                    {
                        Bounds bounds = renderers[0].bounds;
                        for (int i = 1; i < renderers.Length; i++)
                        {
                            bounds.Encapsulate(renderers[i].bounds);
                        }
                        
                        actualFood.transform.SetParent(insidePoint, true); 
                        centerPivot.transform.position = bounds.center;
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
        
        if (audioSource != null)
        {
            if (startSound != null) audioSource.PlayOneShot(startSound);
            yield return new WaitForSeconds(startSound != null ? startSound.length : 0.5f);
            
            if (workingLoopSound != null)
            {
                audioSource.clip = workingLoopSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        
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

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            if (doneSound != null)
            {
                audioSource.PlayOneShot(doneSound);
            }
        }

        currentState = MicrowaveState.Done;
    }

    public override void OnHoverEnter()
    {
        if (currentState == MicrowaveState.Heating || currentState == MicrowaveState.Locked) return;
        base.OnHoverEnter();
    }
}
using System.Collections;
using UnityEngine;
using Zenject;

public class EatableFood : BaseInteractable
{
    [Header("Настройки еды")]
    [SerializeField] private string prompt = "Съесть";
    [SerializeField] private int taskIndexRequired = 5; 
    
    [Tooltip("Кусочки еды, которые будут исчезать по одному (тарелку сюда не добавлять!)")]
    [SerializeField] private GameObject[] foodPieces;
    
    [Tooltip("Префаб грязной тарелки (с PickupItem), который появится, когда вы доедите")]
    [SerializeField] private GameObject dirtyPlatePrefab;
    
    [Tooltip("Сколько раз нужно кликнуть. Настроится автоматически.")]
    [SerializeField] private int bitesTotal = 3;
    
    [Tooltip("Задержка (пережевывание) между кликами")]
    [SerializeField] private float chewCooldown = 1.5f;

    [Header("Аудио")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip eatingSound;
    
    private int currentBites = 0;
    private bool isChewing = false;
    private bool isLocked = true;

    private ComputerMonitor monitor;
    private Chair chair;
    
    private ITaskManager taskManager;
    private DiContainer container;

    public override string InteractionPrompt 
    {
        get 
        {
            if (isLocked) return "";
            if (chair != null && !chair.IsOccupied()) return "Нужно сесть";
            if (monitor != null && !monitor.IsVideoPlaying()) return "Сначала включи видео";
            if (isChewing) return "Жую...";
            
            return prompt;
        }
    }

    [Inject]
    public void Construct(ITaskManager taskManager, DiContainer container)
    {
        this.taskManager = taskManager;
        this.container = container;
    }

    public void Initialize(ComputerMonitor sceneMonitor, Chair sceneChair)
    {
        this.monitor = sceneMonitor;
        this.chair = sceneChair;
    }

    protected override void Awake()
    {
        base.Awake();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        
        if (foodPieces != null && foodPieces.Length > 0)
        {
            bitesTotal = foodPieces.Length;
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
        isLocked = (newTaskIndex != taskIndexRequired);
    }

    public override bool Interact(GameObject interactor)
    {
        if (isLocked || isChewing) return false;
        
        if (chair != null && !chair.IsOccupied()) return false;
        if (monitor != null && !monitor.IsVideoPlaying()) return false;

        StartCoroutine(EatRoutine());
        return true;
    }

    private IEnumerator EatRoutine()
    {
        isChewing = true;
        currentBites++;
        
        if (audioSource != null && eatingSound != null)
        {
            // Если звук еще не играет, запускаем его зацикленно
            if (!audioSource.isPlaying)
            {
                audioSource.clip = eatingSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        if (foodPieces != null && currentBites - 1 < foodPieces.Length)
        {
            if (foodPieces[currentBites - 1] != null)
            {
                foodPieces[currentBites - 1].SetActive(false); 
            }
        }

        if (currentBites >= bitesTotal)
        {
            OnHoverExit();
            
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }
            
            if (taskManager != null)
            {
                taskManager.CompleteCurrentTask();
            }
            
            if (dirtyPlatePrefab != null && container != null)
            {
                GameObject plate = container.InstantiatePrefab(dirtyPlatePrefab);
                plate.transform.SetParent(transform.parent, false);
                plate.transform.localPosition = transform.localPosition;
                plate.transform.localRotation = transform.localRotation;
            }
            
            Destroy(gameObject);
            
            yield break;
        }

        yield return new WaitForSeconds(chewCooldown);
        isChewing = false;
    }

    public override void OnHoverEnter()
    {
        if (isLocked || isChewing) return;
        base.OnHoverEnter();
    }
}
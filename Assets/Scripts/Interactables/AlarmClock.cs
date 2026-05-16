using UnityEngine;
using Zenject;

public class AlarmClock : BaseInteractable
{
    [Header("Настройки Будильника")]
    [SerializeField] private string prompt = "Выключить будильник";
    [SerializeField] private int taskIndexRequired = 0;
    
    [Header("Аудио")]
    [SerializeField] private AudioSource audioSource;

    private bool isTurnedOff = false;
    private bool isLocked = true; 

    // Зависим от ИНТЕРФЕЙСА, а не от класса
    private ITaskManager taskManager;

    [Inject]
    public void Construct(ITaskManager taskManager)
    {
        this.taskManager = taskManager;
    }

    public override string InteractionPrompt => isTurnedOff || isLocked ? "" : prompt;

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
        if (newTaskIndex == taskIndexRequired && !isTurnedOff)
        {
            isLocked = false;
            if (audioSource != null && !audioSource.isPlaying)
                audioSource.Play();
        }
    }

    public override bool Interact(GameObject interactor)
    {
        if (isTurnedOff || isLocked) return false;

        TurnOff();
        return true;
    }

    private void TurnOff()
    {
        isTurnedOff = true;
        OnHoverExit(); 

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        if (taskManager != null)
        {
            taskManager.CompleteCurrentTask();
        }
        
        Debug.Log("Будильник выключен!");
    }

    public override void OnHoverEnter()
    {
        if (isTurnedOff || isLocked) return;
        base.OnHoverEnter();
    }
}
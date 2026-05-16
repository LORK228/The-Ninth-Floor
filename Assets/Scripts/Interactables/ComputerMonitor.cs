using UnityEngine;
using UnityEngine.Video;
using Zenject;

public class ComputerMonitor : BaseInteractable
{
    [Header("Настройки")]
    [SerializeField] private string prompt = "Включить видео";
    [SerializeField] private int taskIndexRequired = 5; 
    
    [SerializeField] private Chair playerChair;
    
    [Header("Видеоплеер")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject videoScreenObject;

    private bool isPlaying = false;
    private bool isLocked = true;

    // Зависим от ИНТЕРФЕЙСА
    private ITaskManager taskManager;

    [Inject]
    public void Construct(ITaskManager taskManager)
    {
        this.taskManager = taskManager;
    }

    public override string InteractionPrompt => isPlaying || isLocked ? "" : (playerChair != null && playerChair.IsOccupied()) ? prompt : "Нужно сесть за стол";

    protected override void Awake()
    {
        base.Awake();

        if (videoScreenObject != null)
        {
            videoScreenObject.SetActive(false);
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
        isLocked = (newTaskIndex < taskIndexRequired - 1);
    }

    public override bool Interact(GameObject interactor)
    {
        if (isPlaying || isLocked) return false;

        if (playerChair && playerChair.IsOccupied())
        {
            if (videoPlayer)
            {
                if (videoScreenObject)
                {
                    videoScreenObject.SetActive(true);
                }
                videoPlayer.Play();
            }
            
            isPlaying = true;
            OnHoverExit();

            if (taskManager != null && taskManager.GetCurrentTaskIndex() == taskIndexRequired)
            {
                 // taskManager.CompleteCurrentTask();
            }

            return true;
        }

        return false;
    }

    public override void OnHoverEnter()
    {
        if (isPlaying || isLocked) return;
        base.OnHoverEnter();
    }
}
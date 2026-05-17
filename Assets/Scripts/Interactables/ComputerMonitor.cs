using UnityEngine;
using UnityEngine.Video;
using Zenject;

public class ComputerMonitor : BaseInteractable
{
    [Header("Настройки")]
    [SerializeField] private string turnOnPrompt = "Включить видео";
    [SerializeField] private string turnOffPrompt = "Выключить компьютер";
    [SerializeField] private int turnOnTaskIndexRequired = 5; 
    [SerializeField] private int turnOffTaskIndexRequired = 6; 
    
    [SerializeField] private Chair playerChair;
    
    [Header("Видеоплеер")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject videoScreenObject;

    private bool isPlaying = false;
    private bool isLocked = true;

    private ITaskManager taskManager;

    public bool IsVideoPlaying() => isPlaying;

    [Inject]
    public void Construct(ITaskManager taskManager)
    {
        this.taskManager = taskManager;
    }

    public override string InteractionPrompt 
    {
        get
        {
            if (isLocked) return "";
            if (playerChair != null && !playerChair.IsOccupied()) return "Нужно сесть за стол";
            return isPlaying ? turnOffPrompt : turnOnPrompt;
        }
    }

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
        // Разрешаем взаимодействие либо когда нужно включить (>= 4, чтобы позволить заранее), 
        // либо когда нужно выключить (== 6). В данном случае строго разрешаем только для конкретных заданий.
        isLocked = !(newTaskIndex == turnOnTaskIndexRequired || newTaskIndex == turnOffTaskIndexRequired);
    }

    public override bool Interact(GameObject interactor)
    {
        if (isLocked) return false;

        if (playerChair && playerChair.IsOccupied())
        {
            if (!isPlaying && taskManager.GetCurrentTaskIndex() == turnOnTaskIndexRequired)
            {
                if (videoPlayer)
                {
                    if (videoScreenObject) videoScreenObject.SetActive(true);
                    videoPlayer.Play();
                }
                
                isPlaying = true;
                OnHoverExit();
                return true;
            }
            else if (isPlaying && taskManager.GetCurrentTaskIndex() == turnOffTaskIndexRequired)
            {
                if (videoPlayer)
                {
                    videoPlayer.Stop();
                    if (videoScreenObject) videoScreenObject.SetActive(false);
                }
                
                isPlaying = false;
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

    public override void OnHoverEnter()
    {
        if (isLocked) return;
        base.OnHoverEnter();
    }
}
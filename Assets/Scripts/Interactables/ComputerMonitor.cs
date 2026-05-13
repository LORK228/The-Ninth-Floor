using UnityEngine;
using UnityEngine.Video;

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
    }

    private void OnDisable()
    {
        GameEventManager.OnTaskChanged -= HandleTaskChanged;
    }

    private void HandleTaskChanged(int newTaskIndex)
    {
        // Включить видео можно, когда задание 5 или выше (если по ТЗ это не блокирует дальнейшую игру)
        isLocked = (newTaskIndex < taskIndexRequired - 1);
    }

    public override bool Interact(GameObject interactor)
    {
        if (isPlaying || isLocked) return false;

        if (playerChair != null && playerChair.IsOccupied())
        {
            if (videoPlayer != null)
            {
                if (videoScreenObject != null)
                {
                    videoScreenObject.SetActive(true);
                }
                videoPlayer.Play();
            }
            
            isPlaying = true;
            OnHoverExit();

            if (TaskManager.Instance != null && TaskManager.Instance.GetCurrentTaskIndex() == taskIndexRequired)
            {
                 // TaskManager.Instance.CompleteCurrentTask();
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
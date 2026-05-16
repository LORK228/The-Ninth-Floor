using UnityEngine;
using Zenject;

public class Peephole : BaseInteractable
{
    [Header("Настройки глазка")]
    [SerializeField] private string prompt = "Посмотреть в глазок";
    [SerializeField] private int taskIndexRequired = 2;
    
    [Tooltip("Камера, которая находится внутри глазка")]
    [SerializeField] private Camera peepholeCamera;
    
    [Header("Крышка глазка")]
    [SerializeField] private Transform cover;
    [SerializeField] private Vector3 coverMoveDirection = Vector3.up;
    [SerializeField] private float maxCoverDistance = 0.1f;
    [SerializeField] private float coverOpenSensitivity = 0.01f;

    private bool isLooking = false;
    private bool isLocked = true;
    
    // Кэшируем контроллер для оптимизации
    private FirstPersonController fpc;
    
    private Vector3 coverClosedPosition;
    private float currentCoverDistance = 0f;

    private ITaskManager taskManager;

    [Inject]
    public void Construct(ITaskManager taskManager)
    {
        this.taskManager = taskManager;
    }

    public override string InteractionPrompt => isLooking ? "ЛКМ / ESC - отойти" : (isLocked ? "" : prompt);

    protected override void Awake()
    {
        base.Awake();

        if (peepholeCamera != null)
        {
            peepholeCamera.gameObject.SetActive(false); 
        }

        if (cover != null)
        {
            coverClosedPosition = cover.localPosition;
            cover.gameObject.SetActive(false);
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

    private void Update()
    {
        if (isLooking)
        {
            if (cover && currentCoverDistance < maxCoverDistance)
            {
                float mouseY = Input.GetAxis("Mouse Y");
                
                if (mouseY > 0)
                {
                    currentCoverDistance += mouseY * coverOpenSensitivity;
                    currentCoverDistance = Mathf.Clamp(currentCoverDistance, 0f, maxCoverDistance);
                    cover.localPosition = coverClosedPosition + coverMoveDirection.normalized * currentCoverDistance;
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (currentCoverDistance > 0.01f || Input.GetKeyDown(KeyCode.Escape)) 
                {
                    StopLooking();
                }
            }
        }
    }

    public override bool Interact(GameObject interactor)
    {
        if (isLooking || isLocked) return false;

        StartLooking(interactor);
        return true;
    }

    private void StartLooking(GameObject interactor)
    {
        // ОПТИМИЗАЦИЯ: Ищем компонент только один раз
        if (!fpc)
        {
            fpc = interactor.GetComponent<FirstPersonController>();
            if (!fpc) fpc = interactor.GetComponentInParent<FirstPersonController>();
        }
        
        if (!fpc) return;

        isLooking = true;

        fpc.playerCanMove = false;
        fpc.cameraCanMove = false;
        fpc.enableJump = false;
        fpc.enableCrouch = false;
        
        fpc.playerCamera.gameObject.SetActive(false);
        if (peepholeCamera)
        {
            peepholeCamera.gameObject.SetActive(true);
        }

        if (cover)
        {
            cover.gameObject.SetActive(true);
        }

        OnHoverExit(); 
    }

    private void StopLooking()
    {
        isLooking = false;

        if (fpc)
        {
            fpc.playerCanMove = true;
            fpc.cameraCanMove = true;
            fpc.enableJump = true;
            fpc.enableCrouch = true;
            
            fpc.playerCamera.gameObject.SetActive(true);
            // Не обнуляем fpc, чтобы использовать кэш в следующий раз
        }

        if (peepholeCamera)
        {
            peepholeCamera.gameObject.SetActive(false);
        }

        if (cover)
        {
            currentCoverDistance = 0f;
            cover.localPosition = coverClosedPosition;
            cover.gameObject.SetActive(false);
        }

        if (taskManager != null)
        {
            taskManager.CompleteCurrentTask();
        }
    }

    public override void OnHoverEnter()
    {
        if (isLooking || isLocked) return;
        base.OnHoverEnter();
    }
}
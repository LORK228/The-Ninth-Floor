using UnityEngine;
using Zenject;
using System.Collections;

public class HoldToInteractDoor : BaseInteractable
{
    [Header("Настройки двери")]
    [SerializeField] private string openPrompt = "Удерживайте E, чтобы открыть";
    [SerializeField] private string closePrompt = "Удерживайте E, чтобы закрыть";
    [SerializeField] private string lockedPrompt = "Дверь заперта";
    [SerializeField] private string keyName = "Ключ от квартиры";
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Настройки удержания")]
    [Tooltip("Сколько секунд нужно удерживать кнопку")]
    [SerializeField] private float timeToHold = 2f;
    [Tooltip("Ссылка на UI компонент круга")]
    [SerializeField] private InteractionHoldUI holdUI;

    [Header("Настройки анимации")]
    [SerializeField] private bool useExactAngles = true;
    [SerializeField] private Vector3 closedAngles = new Vector3(-90, 0, 0);
    [SerializeField] private Vector3 openAngles = new Vector3(-90, 90, 0);
    [SerializeField] private float openAngle = 90f; 
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Звуки")]
    [Tooltip("AudioSource для воспроизведения звуков двери")]
    [SerializeField] private AudioSource doorAudioSource;
    [Tooltip("Обычный звук открытия")]
    [SerializeField] private AudioClip openSound;
    [Tooltip("Обычный звук закрытия")]
    [SerializeField] private AudioClip closeSound;
    [Tooltip("Звук шагов (близко), играется при открытии на 13 квесте")]
    [SerializeField] private AudioClip closeFootstepSound;
    [Tooltip("Звук сердцебиения (квест 13)")]
    [SerializeField] private AudioClip heartbeatSound;
    
    [Header("Настройки сердцебиения")]
    [Tooltip("Через сколько секунд после закрытия двери сердцебиение прекратится")]
    [SerializeField] private float stopHeartbeatDelay = 2f;

    private bool isOpen = false;
    private bool isAnimating = false;
    private float currentHoldTime = 0f;
    private bool isBeingLookedAt = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private IPlayerInventory inventory;
    private ITaskManager taskManager;
    
    private Coroutine heartbeatStopCoroutine;

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
            if (string.IsNullOrEmpty(keyName) || (inventory != null && inventory.HasAnyItem(keyName)))
            {
                return isOpen ? closePrompt : openPrompt;
            }
            return lockedPrompt;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        if (useExactAngles)
        {
            closedRotation = Quaternion.Euler(closedAngles);
            openRotation = Quaternion.Euler(openAngles);
            transform.localRotation = closedRotation;
        }
        else
        {
            closedRotation = transform.localRotation;
            openRotation = closedRotation * Quaternion.AngleAxis(openAngle, rotationAxis.normalized);
        }
        
        if (doorAudioSource == null)
        {
            doorAudioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (!isBeingLookedAt || isAnimating)
        {
            ResetHold();
            return;
        }

        if (!string.IsNullOrEmpty(keyName) && (inventory == null || !inventory.HasAnyItem(keyName)))
        {
            return; 
        }

        if (Input.GetKey(interactKey))
        {
            if (currentHoldTime == 0 && !isOpen && taskManager != null && taskManager.GetCurrentTaskIndex() == 13)
            {
                // Начинаем открывать на 13 квесте - играем близкие шаги
                if (doorAudioSource != null && closeFootstepSound != null)
                {
                    doorAudioSource.PlayOneShot(closeFootstepSound);
                }
            }

            currentHoldTime += Time.deltaTime;

            if (holdUI != null)
            {
                holdUI.UpdateProgress(currentHoldTime / timeToHold);
            }

            if (currentHoldTime >= timeToHold)
            {
                ExecuteAction();
            }
        }
        else if (Input.GetKeyUp(interactKey) || (!Input.GetKey(interactKey) && currentHoldTime > 0))
        {
            ResetHold();
        }
    }

    private void ExecuteAction()
    {
        ResetHold();
        
        isOpen = !isOpen;
        
        if (doorAudioSource != null)
        {
            if (isOpen && openSound != null)
            {
                doorAudioSource.PlayOneShot(openSound);
            }
            else if (!isOpen && closeSound != null)
            {
                doorAudioSource.PlayOneShot(closeSound);
            }

            if (isOpen && taskManager != null && taskManager.GetCurrentTaskIndex() == 13 && heartbeatSound != null)
            {
                doorAudioSource.clip = heartbeatSound;
                doorAudioSource.loop = true;
                doorAudioSource.Play();
                
                if (heartbeatStopCoroutine != null) StopCoroutine(heartbeatStopCoroutine);
            }
            else if (!isOpen && taskManager != null && taskManager.GetCurrentTaskIndex() == 14 && doorAudioSource.clip == heartbeatSound)
            {
                if (heartbeatStopCoroutine != null) StopCoroutine(heartbeatStopCoroutine);
                heartbeatStopCoroutine = StartCoroutine(StopHeartbeatRoutine());
            }
        }
        
        StartCoroutine(AnimateDoor(isOpen ? openRotation : closedRotation));

        CheckTaskCompletion();
    }
    
    private IEnumerator StopHeartbeatRoutine()
    {
        yield return new WaitForSeconds(stopHeartbeatDelay);
        
        if (doorAudioSource != null && doorAudioSource.clip == heartbeatSound)
        {
            doorAudioSource.Stop();
            doorAudioSource.loop = false;
            doorAudioSource.clip = null;
        }
    }
    
    private void CheckTaskCompletion()
    {
        if (taskManager == null) return;
        
        int currentTask = taskManager.GetCurrentTaskIndex();
        
        if (isOpen)
        {
            if (currentTask == 9 || currentTask == 13)
            {
                taskManager.CompleteCurrentTask();
            }
        }
        else
        {
            if (currentTask == 10 || currentTask == 14)
            {
                taskManager.CompleteCurrentTask();
            }
        }
    }

    private IEnumerator AnimateDoor(Quaternion targetRotation)
    {
        isAnimating = true;

        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }

        transform.localRotation = targetRotation; 
        isAnimating = false;
    }

    private void ResetHold()
    {
        currentHoldTime = 0f;
        if (holdUI != null)
        {
            holdUI.ResetAndHide();
        }
    }

    public override bool Interact(GameObject interactor)
    {
        return false;
    }

    public override void OnHoverEnter()
    {
        base.OnHoverEnter();
        isBeingLookedAt = true;
    }

    public override void OnHoverExit()
    {
        base.OnHoverExit();
        isBeingLookedAt = false;
        ResetHold();
    }
}
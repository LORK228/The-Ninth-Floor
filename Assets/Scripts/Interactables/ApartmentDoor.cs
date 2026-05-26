using UnityEngine;
using Zenject;
using System.Collections;

public class ApartmentDoor : BaseInteractable
{
    [Header("Настройки двери")]
    [SerializeField] private string openPrompt = "Открыть";
    [SerializeField] private string closePrompt = "Закрыть";
    
    [Header("Настройки анимации")]
    [SerializeField] private bool useExactAngles = true;
    [SerializeField] private Vector3 closedAngles = new Vector3(-90, 0, 0);
    [SerializeField] private Vector3 openAngles = new Vector3(-90, 90, 0);
    [SerializeField] private float openAngle = 90f; 
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Квест 'Вернуться в квартиру'")]
    [SerializeField] private int returnTaskIndex = 10; 

    [Header("Звуки")]
    [SerializeField] private AudioSource doorAudioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private bool isOpen = false;
    private bool isAnimating = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private ITaskManager taskManager;

    [Inject]
    public void Construct(ITaskManager taskManager)
    {
        this.taskManager = taskManager;
    }

    public override string InteractionPrompt => isOpen ? closePrompt : openPrompt;

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

    public override bool Interact(GameObject interactor)
    {
        if (isAnimating) return false;

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
        }

        StartCoroutine(AnimateDoor(isOpen ? openRotation : closedRotation));
        
        // Проверяем, выполняет ли это закрытие двери квест "Вернуться в квартиру"
        if (!isOpen && taskManager != null && taskManager.GetCurrentTaskIndex() == returnTaskIndex)
        {
            taskManager.CompleteCurrentTask();
        }
        
        return true; 
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
}
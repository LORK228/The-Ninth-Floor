using UnityEngine;
using Zenject;
using System.Collections;

public class DoorWithKey : BaseInteractable
{
    [Header("Настройки двери")]
    [SerializeField] private string openPrompt = "Открыть";
    [SerializeField] private string closePrompt = "Закрыть";
    [SerializeField] private string lockedPrompt = "Нужен ключ";
    [SerializeField] private string keyName = "Ключ от квартиры";
    
    [Header("Настройки анимации")]
    [SerializeField] private bool useExactAngles = true;
    [SerializeField] private Vector3 closedAngles = new Vector3(-90, 0, 0);
    [SerializeField] private Vector3 openAngles = new Vector3(-90, 90, 0);
    [SerializeField] private float openAngle = 90f; 
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float rotationSpeed = 5f;

    private bool isOpen = false;
    private bool isAnimating = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private IPlayerInventory inventory;

    [Inject]
    public void Construct(IPlayerInventory inventory)
    {
        this.inventory = inventory;
    }

    public override string InteractionPrompt 
    {
        get
        {
            // Используем HasAnyItem, чтобы проверять ключ и в руках, и в кармане
            if (inventory != null && inventory.HasAnyItem(keyName))
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
    }

    public override bool Interact(GameObject interactor)
    {
        if (isAnimating) return false;

        // Проверяем наличие ключа в инвентаре (в руке или в кармане)
        if (inventory == null || !inventory.HasAnyItem(keyName))
        {
            Debug.Log("Дверь заперта. Нужен ключ.");
            return false;
        }

        isOpen = !isOpen;
        StartCoroutine(AnimateDoor(isOpen ? openRotation : closedRotation));
        
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
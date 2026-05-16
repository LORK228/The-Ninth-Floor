using System.Collections;
using UnityEngine;

public class FridgeDoor : BaseInteractable
{
    [Header("Настройки двери")]
    [SerializeField] private string openPrompt = "Открыть холодильник";
    [SerializeField] private string closePrompt = "Закрыть холодильник";
    
    [SerializeField] private bool useExactAngles = true;
    [SerializeField] private Vector3 closedAngles = new Vector3(-90, 0, 0);
    [SerializeField] private Vector3 openAngles = new Vector3(-90, 90, 0);

    [SerializeField] private float openAngle = 90f; 
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Еда внутри (Опционально)")]
    [SerializeField] private GameObject foodInside;

    private bool isOpen = false;
    private bool isAnimating = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    public override string InteractionPrompt => isOpen ? closePrompt : openPrompt;

    protected override void Awake()
    {
        base.Awake();

        if (foodInside != null)
        {
            foodInside.SetActive(false);
        }

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

        isOpen = !isOpen;
        StartCoroutine(AnimateDoor(isOpen ? openRotation : closedRotation));
        
        if (foodInside)
        {
            foodInside.SetActive(isOpen);
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
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Настройки взаимодействия")]
    [SerializeField] private float interactionDistance = 2.5f;
    [SerializeField] private LayerMask interactableLayerMask;
    [SerializeField] private KeyCode interactKey = KeyCode.Mouse0;

    [Header("Ссылки для UI")]
    [SerializeField] private TMPro.TextMeshProUGUI promptText;

    private Camera cam;
    private IInteractable currentInteractable;
    private Collider lastHitCollider; // Кэшируем коллайдер для оптимизации

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("PlayerInteractor должен висеть на объекте с компонентом Camera!");
        }

        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        HandleRaycast();
        HandleInteractionInput();
    }

    private void HandleRaycast()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayerMask))
        {
            // ОПТИМИЗАЦИЯ: вызываем GetComponentInParent только если мы посмотрели на новый объект
            if (hit.collider != lastHitCollider)
            {
                lastHitCollider = hit.collider;
                IInteractable interactableObj = hit.collider.GetComponentInParent<IInteractable>();

                if (interactableObj != currentInteractable)
                {
                    if (currentInteractable != null)
                    {
                        currentInteractable.OnHoverExit();
                    }

                    currentInteractable = interactableObj;
                    
                    if (currentInteractable != null)
                    {
                        currentInteractable.OnHoverEnter();
                    }
                    UpdateUI();
                }
            }
            else
            {
                // Если мы смотрим на тот же объект, просто обновляем UI (если его состояние изменилось)
                UpdateUI();
            }
        }
        else
        {
            lastHitCollider = null;
            ClearCurrentInteractable();
        }
    }

    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(interactKey) && currentInteractable != null)
        {
            // Передаем сам корень игрока (чтобы внутри объектов не использовать GetComponentInParent)
            if (currentInteractable.Interact(transform.root.gameObject))
            {
                UpdateUI();
            }
        }
    }

    private void ClearCurrentInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable.OnHoverExit();
            currentInteractable = null;
            
            if (promptText)
            {
                promptText.gameObject.SetActive(false);
            }
        }
    }
    
    private void UpdateUI()
    {
        if (promptText && currentInteractable != null)
        {
            promptText.text = currentInteractable.InteractionPrompt;
            promptText.gameObject.SetActive(true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }
        
        if (cam != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(cam.transform.position, cam.transform.forward * interactionDistance);
        }
    }
}
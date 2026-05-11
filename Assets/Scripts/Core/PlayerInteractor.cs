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
            // Поскольку у нас скрипт теперь на пустом объекте (родителе),
            // а коллайдер скорее всего на самой двери (ребенке),
            // используем GetComponentInParent
            IInteractable interactableObj = hit.collider.GetComponentInParent<IInteractable>();

            if (interactableObj != null)
            {
                if (interactableObj != currentInteractable)
                {
                    if (currentInteractable != null)
                    {
                        currentInteractable.OnHoverExit();
                    }

                    currentInteractable = interactableObj;
                    currentInteractable.OnHoverEnter();
                    UpdateUI();
                }
                else
                {
                    // Обновляем текст, так как состояние объекта могло измениться (например, открылась дверь)
                    UpdateUI();
                }
            }
            else
            {
                ClearCurrentInteractable();
            }
        }
        else
        {
            ClearCurrentInteractable();
        }
    }

    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(interactKey) && currentInteractable != null)
        {
            if (currentInteractable.Interact(this.gameObject))
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
            
            if (promptText != null)
            {
                promptText.gameObject.SetActive(false);
            }
        }
    }
    
    private void UpdateUI()
    {
        if (promptText != null && currentInteractable != null)
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
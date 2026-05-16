using UnityEngine;

public class Chair : BaseInteractable
{
    [Header("Настройки кресла")]
    [SerializeField] private string prompt = "Сесть";
    [SerializeField] private Transform sitPoint; 
    [SerializeField] private KeyCode standUpKey = KeyCode.Space;
    
    private bool isOccupied = false;
    private GameObject currentPlayerObj;
    
    // Кэшируем ссылку для оптимизации
    private FirstPersonController fpc; 
    
    private Vector3 standPosition;
    
    private bool wasPlayerCanMove;
    private bool wasEnableJump;
    private bool wasEnableCrouch;
    private bool wasEnableHeadBob;
    
    public override string InteractionPrompt => prompt;

    public bool IsOccupied() => isOccupied;

    private void Update()
    {
        if (isOccupied && Input.GetKeyDown(standUpKey))
        {
            StandUp();
        }
    }

    public override bool Interact(GameObject interactor)
    {
        if (isOccupied) return false;
        
        SitDown(interactor);
        return true; 
    }

    private void SitDown(GameObject interactor)
    {
        // ОПТИМИЗАЦИЯ: Ищем компонент только один раз
        if (!fpc)
        {
            fpc = interactor.GetComponent<FirstPersonController>();
            if (!fpc) fpc = interactor.GetComponentInParent<FirstPersonController>();
        }
        
        if (!fpc) return;
        
        currentPlayerObj = fpc.gameObject;
        standPosition = currentPlayerObj.transform.position;
        
        wasPlayerCanMove = fpc.playerCanMove;
        wasEnableJump = fpc.enableJump;
        wasEnableCrouch = fpc.enableCrouch;
        wasEnableHeadBob = fpc.enableHeadBob;
        
        fpc.playerCanMove = false;
        fpc.enableJump = false;
        fpc.enableCrouch = false;
        fpc.enableHeadBob = false;
        
        // Оптимизация: кэшируем и Rigidbody, так как мы используем его дважды
        Rigidbody rb = currentPlayerObj.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (sitPoint)
        {
            currentPlayerObj.transform.position = sitPoint.position;
        }
        else
        {
            currentPlayerObj.transform.position = transform.position + Vector3.up * 1.5f; 
        }

        isOccupied = true;
        OnHoverExit();
    }

    private void StandUp()
    {
        if (fpc)
        {
            fpc.playerCanMove = wasPlayerCanMove;
            fpc.enableJump = wasEnableJump;
            fpc.enableCrouch = wasEnableCrouch;
            fpc.enableHeadBob = wasEnableHeadBob;
        }

        if (currentPlayerObj)
        {
            Rigidbody rb = currentPlayerObj.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.isKinematic = false;
            }
            
            currentPlayerObj.transform.position = standPosition;
        }
        
        isOccupied = false;
        currentPlayerObj = null;
        // Не обнуляем fpc, чтобы использовать закэшированное значение в следующий раз
    }

    public override void OnHoverEnter()
    {
        if (isOccupied) return;
        base.OnHoverEnter();
    }
}
using UnityEngine;
using Zenject;
using System.Collections;

public class Chair : BaseInteractable
{
    [Header("Настройки кресла")]
    [SerializeField] private string prompt = "Сесть";
    [SerializeField] private Transform sitPoint; 
    [SerializeField] private KeyCode standUpKey = KeyCode.Space;
    
    [Header("Начало игры")]
    [Tooltip("Если включено, игрок заспавнится в этом кресле, если стартовый квест совпадает с Start Task Index")]
    [SerializeField] private bool spawnPlayerHereOnStart = true;
    [SerializeField] private int startTaskIndex = 0;
    
    private bool isOccupied = false;
    private GameObject currentPlayerObj;
    
    // Кэшируем ссылку для оптимизации
    private FirstPersonController fpc; 
    
    private Vector3 standPosition;
    
    private bool wasPlayerCanMove;
    private bool wasEnableJump;
    private bool wasEnableCrouch;
    private bool wasEnableHeadBob;
    
    private ITaskManager taskManager;

    [Inject]
    public void Construct(ITaskManager taskManager)
    {
        this.taskManager = taskManager;
    }

    public override string InteractionPrompt => prompt;

    public bool IsOccupied() => isOccupied;

    private IEnumerator Start()
    {
        // Ждем один кадр, чтобы все остальные скрипты (включая игрока) успели проинициализироваться
        yield return null;

        if (spawnPlayerHereOnStart && taskManager != null && taskManager.GetCurrentTaskIndex() == startTaskIndex)
        {
            // Ищем игрока на сцене (так как у нас нет прямой ссылки)
            FirstPersonController playerController = FindObjectOfType<FirstPersonController>();
            if (playerController != null)
            {
                // Сажаем игрока принудительно, но без сохранения позиции возврата (так как он только заспавнился)
                ForceSitDown(playerController.gameObject);
            }
        }
    }

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
        standPosition = currentPlayerObj.transform.position; // Запоминаем, откуда пришли
        
        ApplySittingState();
    }

    private void ForceSitDown(GameObject interactor)
    {
        if (!fpc)
        {
            fpc = interactor.GetComponent<FirstPersonController>();
            if (!fpc) fpc = interactor.GetComponentInParent<FirstPersonController>();
        }
        
        if (!fpc) return;
        
        currentPlayerObj = fpc.gameObject;
        // Если мы спавнимся в кресле, то точкой возврата делаем позицию немного спереди от кресла
        standPosition = transform.position + transform.forward * 1.5f;
        
        ApplySittingState();
    }

    private void ApplySittingState()
    {
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
            // Также поворачиваем игрока туда же, куда смотрит sitPoint
            currentPlayerObj.transform.rotation = sitPoint.rotation;
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
    }

    public override void OnHoverEnter()
    {
        if (isOccupied) return;
        base.OnHoverEnter();
    }
}
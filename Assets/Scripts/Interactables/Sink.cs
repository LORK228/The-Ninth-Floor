using System.Collections;
using UnityEngine;
using Zenject;

public class Sink : BaseInteractable
{
    [Header("Квест: Отнести тарелку")]
    [SerializeField] private int depositPlateTaskIndex = 7;
    [SerializeField] private string depositPrompt = "Положить тарелку";
    [SerializeField] private GameObject plateInSinkPrefab;
    [SerializeField] private Transform plateSpawnPoint;
    
    [Header("Квест: Мыть посуду")]
    [SerializeField] private int washDishesTaskIndex = 15;
    [SerializeField] private string washPrompt = "Мыть посуду";
    [SerializeField] private int platesToWash = 3;

    [Header("Событие со стуком")]
    [Tooltip("После какой помытой тарелки произойдет событие")]
    [SerializeField] private int platesToWashForEvent = 2;
    [Tooltip("Точка, куда посмотрит игрок")]
    [SerializeField] private Transform windowLookTarget;
    [Tooltip("Звук стука в окно")]
    [SerializeField] private AudioClip knockSound;
    
    [Header("Звуки")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip washingSound;

    private int washedPlatesCount = 0;
    private bool isEventTriggered = false;
    private bool hasDepositedPlate = false;

    private IPlayerInventory inventory;
    private ITaskManager taskManager;

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
            if (taskManager == null) return "";

            int currentTask = taskManager.GetCurrentTaskIndex();

            if (currentTask == depositPlateTaskIndex && !hasDepositedPlate)
            {
                return (inventory != null && inventory.HasItem("Грязная тарелка")) ? depositPrompt : "Нужна грязная тарелка";
            }
            
            if (currentTask == washDishesTaskIndex)
            {
                return washPrompt;
            }

            return "";
        }
    }

    protected override void Awake()
    {
        base.Awake();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public override bool Interact(GameObject interactor)
    {
        if (taskManager == null || isEventTriggered) return false;

        int currentTask = taskManager.GetCurrentTaskIndex();

        // Логика для квеста "Отнести тарелку"
        if (currentTask == depositPlateTaskIndex && !hasDepositedPlate)
        {
            if (inventory != null && inventory.HasItem("Грязная тарелка"))
            {
                inventory.ClearHand();
                hasDepositedPlate = true;
                
                if (plateInSinkPrefab && plateSpawnPoint)
                {
                    Instantiate(plateInSinkPrefab, plateSpawnPoint.position, plateSpawnPoint.rotation, plateSpawnPoint);
                }
                
                taskManager.CompleteCurrentTask();
                return true;
            }
        }
        
        // Логика для квеста "Мыть посуду"
        if (currentTask == washDishesTaskIndex)
        {
            washedPlatesCount++;

            if (audioSource != null && washingSound != null)
            {
                audioSource.PlayOneShot(washingSound);
            }

            if (washedPlatesCount == platesToWashForEvent)
            {
                StartCoroutine(KnockEventRoutine(interactor));
            }
            
            if (washedPlatesCount >= platesToWash) 
            {
                taskManager.CompleteCurrentTask();
                // Можно добавить логику, чтобы тарелки в раковине исчезли
                if(plateSpawnPoint != null && plateSpawnPoint.childCount > 0)
                {
                    Destroy(plateSpawnPoint.GetChild(0).gameObject);
                }
            }
            return true;
        }

        return false;
    }

    private IEnumerator KnockEventRoutine(GameObject player)
    {
        isEventTriggered = true;
        
        FirstPersonController fpc = player.GetComponentInParent<FirstPersonController>();
        if (fpc) fpc.cameraCanMove = false;

        yield return new WaitForSeconds(0.5f);

        if (audioSource != null && knockSound != null)
        {
            audioSource.PlayOneShot(knockSound);
        }

        if (fpc && windowLookTarget != null)
        {
            Transform cameraTransform = fpc.playerCamera.transform;
            Quaternion startRotation = cameraTransform.rotation;
            Vector3 direction = (windowLookTarget.position - cameraTransform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            float timer = 0f;
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                cameraTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, timer);
                yield return null;
            }
        }

        yield return new WaitForSeconds(2f);

        if (fpc) fpc.cameraCanMove = true;
        
        isEventTriggered = false;
    }
}
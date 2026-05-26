using System.Collections;
using UnityEngine;
using Zenject;

public class FlowerWatering : BaseInteractable
{
    [Header("Настройки")]
    [SerializeField] private string prompt = "Полить цветок";
    [SerializeField] private int taskIndexRequired = 16;
    [Tooltip("Опционально: Имя предмета (например, 'Вода' или 'Лейка'). Если пусто, можно поливать без предмета.")]
    [SerializeField] private string requiredItem = "Вода";

    [Header("Камеры")]
    [Tooltip("Камера, которая смотрит вниз на цветок")]
    [SerializeField] private Camera flowerCamera;
    [Tooltip("Камера, которая смотрит на окно")]
    [SerializeField] private Camera windowCamera;

    [Header("Событие с монстром")]
    [Tooltip("Голова монстра за окном")]
    [SerializeField] private GameObject windowMonster;
    [SerializeField] private float wateringDuration = 3f;
    [SerializeField] private float monsterLookDuration = 3f;

    [Header("Звуки")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip wateringSound;
    [SerializeField] private AudioClip tensionSound; // Звук для напряга
    [SerializeField] private AudioClip quietHeartbeatSound; // Тихий стук сердца

    private bool isLocked = true;
    private bool isWatering = false;

    private FirstPersonController fpc;
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
            if (isLocked || isWatering) return "";
            if (!string.IsNullOrEmpty(requiredItem) && (inventory == null || !inventory.HasItem(requiredItem)))
            {
                return "Нужна вода";
            }
            return prompt;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        if (flowerCamera != null) flowerCamera.gameObject.SetActive(false);
        if (windowCamera != null) windowCamera.gameObject.SetActive(false);
        if (windowMonster != null) windowMonster.SetActive(false);
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        GameEventManager.OnTaskChanged += HandleTaskChanged;
        if (taskManager != null) HandleTaskChanged(taskManager.GetCurrentTaskIndex());
    }

    private void OnDisable()
    {
        GameEventManager.OnTaskChanged -= HandleTaskChanged;
    }

    private void HandleTaskChanged(int newTaskIndex)
    {
        isLocked = (newTaskIndex != taskIndexRequired);
    }

    public override bool Interact(GameObject interactor)
    {
        if (isLocked || isWatering) return false;
        
        if (!string.IsNullOrEmpty(requiredItem) && (inventory == null || !inventory.HasItem(requiredItem)))
        {
            return false;
        }

        StartCoroutine(WateringRoutine(interactor));
        return true;
    }

    private IEnumerator WateringRoutine(GameObject interactor)
    {
        isWatering = true;
        OnHoverExit();

        if (!fpc)
        {
            fpc = interactor.GetComponent<FirstPersonController>();
            if (!fpc) fpc = interactor.GetComponentInParent<FirstPersonController>();
        }

        if (fpc)
        {
            fpc.playerCanMove = false;
            fpc.cameraCanMove = false;
            fpc.playerCamera.gameObject.SetActive(false);
        }

        if (flowerCamera != null) flowerCamera.gameObject.SetActive(true);
        if (audioSource != null && wateringSound != null)
        {
            audioSource.PlayOneShot(wateringSound);
        }

        yield return new WaitForSeconds(wateringDuration);

        if (flowerCamera != null) flowerCamera.gameObject.SetActive(false);
        if (windowCamera != null) windowCamera.gameObject.SetActive(true);
        
        if (windowMonster != null) windowMonster.SetActive(true);
        if (audioSource != null && tensionSound != null) audioSource.PlayOneShot(tensionSound);

        yield return new WaitForSeconds(monsterLookDuration);

        if (windowMonster != null) windowMonster.SetActive(false);

        yield return new WaitForSeconds(1f);

        if (windowCamera != null) windowCamera.gameObject.SetActive(false);
        if (fpc)
        {
            fpc.playerCamera.gameObject.SetActive(true);
            fpc.playerCanMove = true;
            fpc.cameraCanMove = true;
        }

        if (inventory != null && !string.IsNullOrEmpty(requiredItem)) inventory.ClearHand();

        if (audioSource != null && quietHeartbeatSound != null)
        {
            audioSource.clip = quietHeartbeatSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        if (taskManager != null)
        {
            taskManager.CompleteCurrentTask();
        }

        this.enabled = false;
    }
}
using UnityEngine;
using Zenject;

public class ClothesDryer : BaseInteractable
{
    [Header("Настройки")]
    [SerializeField] private string requiredItem = "Таз с бельем";
    [SerializeField] private int clicksRequired = 5; 
    [SerializeField] private string prompt = "Повесить белье";

    [SerializeField] private GameObject[] clothesPieces;

    [Header("Звуки")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hangClothSound;

    [Header("Событие скрежета")]
    [Tooltip("Дверь, которая должна издать звук скрежета")]
    [SerializeField] private HoldToInteractDoor mainDoor;

    private int currentClicks = 0;
    private bool isDone = false;

    // Зависим от ИНТЕРФЕЙСОВ
    private IPlayerInventory inventory;
    private ITaskManager taskManager;

    [Inject]
    public void Construct(IPlayerInventory inventory, ITaskManager taskManager)
    {
        this.inventory = inventory;
        this.taskManager = taskManager;
    }

    public override string InteractionPrompt => isDone ? "" : (inventory != null && inventory.HasItem(requiredItem)) ? prompt : "Нужно белье";

    protected override void Awake()
    {
        base.Awake();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (clothesPieces == null || clothesPieces.Length == 0)
        {
            int childCount = transform.childCount;
            clothesPieces = new GameObject[childCount];
            for (int i = 0; i < childCount; i++)
            {
                clothesPieces[i] = transform.GetChild(i).gameObject;
            }
        }

        if (clothesPieces != null && clothesPieces.Length > 0)
        {
            clicksRequired = clothesPieces.Length;
        }

        if (clothesPieces != null)
        {
            foreach (var piece in clothesPieces)
            {
                if (piece != null) piece.SetActive(false);
            }
        }
    }

    public override bool Interact(GameObject interactor)
    {
        if (isDone) return false;

        if (inventory != null && inventory.HasItem(requiredItem))
        {
            currentClicks++;
            
            if (clothesPieces != null && currentClicks - 1 < clothesPieces.Length)
            {
                if (clothesPieces[currentClicks - 1] != null)
                {
                    clothesPieces[currentClicks - 1].SetActive(true);
                }
            }

            if (audioSource != null && hangClothSound != null)
            {
                audioSource.PlayOneShot(hangClothSound);
            }

            if (currentClicks >= clicksRequired)
            {
                FinishHanging();
            }
            return true;
        }

        return false;
    }

    private void FinishHanging()
    {
        isDone = true;
        OnHoverExit();

        if (inventory != null)
        {
            inventory.ClearHand();
        }

        // Вызываем звук скрежета на двери
        if (mainDoor != null)
        {
            mainDoor.PlayScrapeSound();
        }

        if (taskManager != null)
        {
            taskManager.CompleteCurrentTask();
        }

        Debug.Log("Всё белье развешано!");
    }

    public override void OnHoverEnter()
    {
        if (isDone) return;
        base.OnHoverEnter();
    }
}
using UnityEngine;

public class ClothesDryer : MonoBehaviour, IInteractable
{
    [Header("Настройки")]
    [SerializeField] private string requiredItem = "Таз с бельем";
    [Tooltip("Сколько раз нужно нажать. Теперь вычисляется автоматически по количеству одежды.")]
    [SerializeField] private int clicksRequired = 5; 
    [SerializeField] private string prompt = "Повесить белье";

    [Header("Визуализация одежды")]
    [Tooltip("Если оставить пустым, скрипт автоматически соберет все дочерние объекты сушилки.")]
    [SerializeField] private GameObject[] clothesPieces;

    [Header("Подсветка")]
    [SerializeField] private Renderer[] meshRenderers;
    [SerializeField] private Color highlightColor = new Color(0.8f, 0.8f, 0.5f, 1f);
    
    private Color[][] originalColors;
    private int currentClicks = 0;
    private bool isDone = false;

    public string InteractionPrompt => isDone ? "" : (PlayerInventory.Instance != null && PlayerInventory.Instance.HasItem(requiredItem)) ? prompt : "Нужно белье";

    private void Awake()
    {
        // Если одежда не задана вручную, собираем все прямые дочерние объекты
        if (clothesPieces == null || clothesPieces.Length == 0)
        {
            int childCount = transform.childCount;
            clothesPieces = new GameObject[childCount];
            for (int i = 0; i < childCount; i++)
            {
                clothesPieces[i] = transform.GetChild(i).gameObject;
            }
        }

        // Автоматически подстраиваем количество нужных кликов под количество одежды
        if (clothesPieces != null && clothesPieces.Length > 0)
        {
            clicksRequired = clothesPieces.Length;
        }

        // Прячем всю одежду на сушилке при старте
        if (clothesPieces != null)
        {
            foreach (var piece in clothesPieces)
            {
                if (piece != null) piece.SetActive(false);
            }
        }

        // Ищем рендереры для подсветки (самой сушилки, если они не заданы)
        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            meshRenderers = GetComponents<Renderer>(); // Ищем только на самой сушилке, чтобы не подсвечивать одежду
        }

        if (meshRenderers != null && meshRenderers.Length > 0)
        {
            originalColors = new Color[meshRenderers.Length][];
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (meshRenderers[i] != null)
                {
                    Material[] mats = meshRenderers[i].materials;
                    originalColors[i] = new Color[mats.Length];
                    for (int j = 0; j < mats.Length; j++)
                    {
                        if (mats[j].HasProperty("_BaseColor"))
                            originalColors[i][j] = mats[j].GetColor("_BaseColor");
                        else if (mats[j].HasProperty("_Color"))
                            originalColors[i][j] = mats[j].color;
                    }
                }
            }
        }
    }

    public bool Interact(GameObject interactor)
    {
        if (isDone) return false;

        // Проверяем, есть ли у игрока нужный предмет (тазик с бельем)
        if (PlayerInventory.Instance != null && PlayerInventory.Instance.HasItem(requiredItem))
        {
            currentClicks++;
            
            // Включаем отображение одной шмотки
            if (clothesPieces != null && currentClicks - 1 < clothesPieces.Length)
            {
                if (clothesPieces[currentClicks - 1] != null)
                {
                    clothesPieces[currentClicks - 1].SetActive(true);
                }
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

        // Убираем тазик из рук
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.ClearHand();
        }

        // Выполняем задание в менеджере
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteCurrentTask();
        }

        Debug.Log("Всё белье развешано!");
    }

    public void OnHoverEnter()
    {
        if (isDone || meshRenderers == null) return;
        
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null)
            {
                Material[] mats = meshRenderers[i].materials;
                for (int j = 0; j < mats.Length; j++)
                {
                    if (mats[j].HasProperty("_BaseColor"))
                        mats[j].SetColor("_BaseColor", highlightColor);
                    else if (mats[j].HasProperty("_Color"))
                        mats[j].color = highlightColor;
                }
            }
        }
    }

    public void OnHoverExit()
    {
        if (meshRenderers == null || originalColors == null) return;
        
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null && originalColors[i] != null)
            {
                Material[] mats = meshRenderers[i].materials;
                for (int j = 0; j < mats.Length; j++)
                {
                    if (j < originalColors[i].Length)
                    {
                        if (mats[j].HasProperty("_BaseColor"))
                            mats[j].SetColor("_BaseColor", originalColors[i][j]);
                        else if (mats[j].HasProperty("_Color"))
                            mats[j].color = originalColors[i][j];
                    }
                }
            }
        }
    }
}
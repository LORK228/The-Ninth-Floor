using UnityEngine;

public class WashingMachine : MonoBehaviour, IInteractable
{
    [Header("Настройки")]
    [SerializeField] private string prompt = "Достать белье";
    [SerializeField] private int taskIndexRequired = 1; // Задание: Развесить белье

    [Header("Выдаваемый предмет")]
    [SerializeField] private string itemName = "Таз с бельем";
    [SerializeField] private GameObject basketPrefab; // Префаб тазика, который появится в руках

    [Header("Визуализация")]
    [SerializeField] private Renderer[] meshRenderers;
    [SerializeField] private Color highlightColor = new Color(0.8f, 0.8f, 0.5f, 1f);
    
    private Color[][] originalColors;
    private bool isEmptied = false;

    public string InteractionPrompt => isEmptied ? "" : prompt;

    private void Awake()
    {
        if (meshRenderers == null || meshRenderers.Length == 0)
            meshRenderers = GetComponentsInChildren<Renderer>();

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
        if (isEmptied) return false;

        if (TaskManager.Instance != null && TaskManager.Instance.GetCurrentTaskIndex() == taskIndexRequired)
        {
            // Выдаем предмет игроку
            if (PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.GiveItem(itemName, basketPrefab);
                isEmptied = true;
                OnHoverExit(); // Снимаем подсветку
                return true;
            }
        }
        return false;
    }

    public void OnHoverEnter()
    {
        if (isEmptied || meshRenderers == null) return;
        
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
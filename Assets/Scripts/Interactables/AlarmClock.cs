using UnityEngine;

public class AlarmClock : MonoBehaviour, IInteractable
{
    [Header("Настройки")]
    [SerializeField] private string prompt = "Выключить будильник";
    [SerializeField] private int taskIndexRequired = 0; // Индекс задания (0 = Выключить будильник)

    [Header("Визуализация/Аудио")]
    [Tooltip("Рендереры для подсветки. Если пусто, скрипт найдет их сам.")]
    [SerializeField] private Renderer[] meshRenderers;
    [SerializeField] private Color highlightColor = new Color(0.8f, 0.8f, 0.5f, 1f);
    [SerializeField] private AudioSource audioSource;
    
    private Color[][] originalColors;
    private bool isTurnedOff = false;

    public string InteractionPrompt => isTurnedOff ? "" : prompt;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            meshRenderers = GetComponentsInChildren<Renderer>();
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

    private void Start()
    {
        // Если это самое первое задание, будильник должен звенеть сразу
        if (TaskManager.Instance != null && TaskManager.Instance.GetCurrentTaskIndex() == taskIndexRequired)
        {
            if (audioSource != null && !audioSource.isPlaying)
                audioSource.Play();
        }
    }

    public bool Interact(GameObject interactor)
    {
        if (isTurnedOff) return false;

        if (TaskManager.Instance != null && TaskManager.Instance.GetCurrentTaskIndex() == taskIndexRequired)
        {
            TurnOff();
            return true;
        }
        
        return false;
    }

    private void TurnOff()
    {
        isTurnedOff = true;
        OnHoverExit();

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteCurrentTask();
        }
        
        Debug.Log("Будильник выключен!");
    }

    public void OnHoverEnter()
    {
        if (isTurnedOff || meshRenderers == null) return;
        
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
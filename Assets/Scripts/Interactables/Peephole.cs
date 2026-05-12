using UnityEngine;

public class Peephole : MonoBehaviour, IInteractable
{
    [Header("Настройки глазка")]
    [SerializeField] private string prompt = "Посмотреть в глазок";
    [SerializeField] private int taskIndexRequired = 2; // Задание: Посмотреть в глазок
    
    [Tooltip("Камера, которая находится внутри глазка")]
    [SerializeField] private Camera peepholeCamera;
    
    [Header("Крышка глазка")]
    [Tooltip("Объект заслонки (шторки) глазка, которую мы будем поднимать")]
    [SerializeField] private Transform cover;
    
    [Tooltip("Направление движения заслонки (например, (0, 1, 0) для движения вверх по локальной оси Y)")]
    [SerializeField] private Vector3 coverMoveDirection = Vector3.up;
    
    [Tooltip("Максимальное расстояние, на которое поднимается заслонка (в юнитах)")]
    [SerializeField] private float maxCoverDistance = 0.1f;
    
    [Tooltip("Скорость открытия (чувствительность мыши)")]
    [SerializeField] private float coverOpenSensitivity = 0.01f;

    [Header("Подсветка")]
    [SerializeField] private Renderer[] meshRenderers;
    [SerializeField] private Color highlightColor = new Color(0.8f, 0.8f, 0.5f, 1f);

    private Color[][] originalColors;
    private bool isLooking = false;
    private FirstPersonController fpc;
    
    // Переменные для заслонки
    private Vector3 coverClosedPosition;
    private float currentCoverDistance = 0f;

    public string InteractionPrompt => isLooking ? "ЛКМ / ESC - отойти" : prompt;

    private void Awake()
    {
        if (peepholeCamera != null)
        {
            peepholeCamera.gameObject.SetActive(false); 
        }

        if (cover != null)
        {
            coverClosedPosition = cover.localPosition;
            // Изначально отключаем объект заслонки, чтобы он не мешал, когда мы не смотрим в глазок
            cover.gameObject.SetActive(false);
        }

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

    private void Update()
    {
        if (isLooking)
        {
            // Логика поднятия заслонки мышкой (теперь только вверх)
            if (cover != null && currentCoverDistance < maxCoverDistance)
            {
                float mouseY = Input.GetAxis("Mouse Y");
                
                // Разрешаем двигать заслонку только в одном направлении (открывать)
                if (mouseY > 0)
                {
                    currentCoverDistance += mouseY * coverOpenSensitivity;
                    currentCoverDistance = Mathf.Clamp(currentCoverDistance, 0f, maxCoverDistance);

                    // Смещаем заслонку по локальной оси
                    cover.localPosition = coverClosedPosition + coverMoveDirection.normalized * currentCoverDistance;
                }
            }

            // Логика выхода из глазка
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Mouse0))
            {
                // Позволяем выйти только если заслонка открыта хотя бы немного,
                // либо если нажали Escape.
                if (currentCoverDistance > 0.01f || Input.GetKeyDown(KeyCode.Escape)) 
                {
                    StopLooking();
                }
            }
        }
    }

    public bool Interact(GameObject interactor)
    {
        if (isLooking) return false;

        if (TaskManager.Instance != null && TaskManager.Instance.GetCurrentTaskIndex() == taskIndexRequired)
        {
            StartLooking(interactor);
            return true;
        }

        return false;
    }

    private void StartLooking(GameObject interactor)
    {
        fpc = interactor.GetComponentInParent<FirstPersonController>();
        if (fpc == null) return;

        isLooking = true;

        fpc.playerCanMove = false;
        fpc.cameraCanMove = false;
        fpc.enableJump = false;
        fpc.enableCrouch = false;
        
        fpc.playerCamera.gameObject.SetActive(false);
        if (peepholeCamera != null)
        {
            peepholeCamera.gameObject.SetActive(true);
        }

        // Включаем заслонку, когда начинаем смотреть в глазок
        if (cover != null)
        {
            cover.gameObject.SetActive(true);
        }

        OnHoverExit(); 
    }

    private void StopLooking()
    {
        isLooking = false;

        if (fpc != null)
        {
            fpc.playerCanMove = true;
            fpc.cameraCanMove = true;
            fpc.enableJump = true;
            fpc.enableCrouch = true;
            
            fpc.playerCamera.gameObject.SetActive(true);
            fpc = null;
        }

        if (peepholeCamera != null)
        {
            peepholeCamera.gameObject.SetActive(false);
        }

        // Закрываем заслонку обратно при выходе и отключаем её
        if (cover != null)
        {
            currentCoverDistance = 0f;
            cover.localPosition = coverClosedPosition;
            cover.gameObject.SetActive(false);
        }

        if (TaskManager.Instance != null && TaskManager.Instance.GetCurrentTaskIndex() == taskIndexRequired)
        {
            TaskManager.Instance.CompleteCurrentTask();
        }
    }

    public void OnHoverEnter()
    {
        if (isLooking || meshRenderers == null) return;
        
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
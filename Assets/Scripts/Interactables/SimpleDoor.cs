using System.Collections;
using UnityEngine;

public class SimpleDoor : MonoBehaviour, IInteractable
{
    [Header("Настройки двери")]
    [SerializeField] private string openPrompt = "Открыть";
    [SerializeField] private string closePrompt = "Закрыть";
    
    [Tooltip("Угол, на который открывается дверь (относительно начального).")]
    [SerializeField] private float openAngle = -90f; 
    
    [Tooltip("Ось вращения (X, Y или Z). Обычно это Y (0, 1, 0), но если модель импортирована иначе, попробуйте X (1, 0, 0) или Z (0, 0, 1)")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Визуализация")]
    [Tooltip("Рендереры для подсветки. Если пусто, скрипт найдет их сам.")]
    [SerializeField] private Renderer[] meshRenderers;
    [SerializeField] private Color highlightColor = new Color(0.8f, 0.8f, 0.5f, 1f);
    
    private Color[] originalColors;
    private bool isOpen = false;
    private bool isAnimating = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    public string InteractionPrompt => isOpen ? closePrompt : openPrompt;

    private void Awake()
    {
        // Если рендереры не заданы вручную, ищем все рендереры в этом объекте и его детях
        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            meshRenderers = GetComponentsInChildren<Renderer>();
        }

        // Сохраняем оригинальные цвета всех частей двери
        if (meshRenderers != null && meshRenderers.Length > 0)
        {
            originalColors = new Color[meshRenderers.Length];
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (meshRenderers[i].material != null)
                {
                    originalColors[i] = meshRenderers[i].material.color;
                }
            }
        }

        // Запоминаем начальное положение
        closedRotation = transform.localRotation;
        
        // Вычисляем позицию открытой двери по заданной оси
        openRotation = closedRotation * Quaternion.AngleAxis(openAngle, rotationAxis.normalized);
    }

    public bool Interact(GameObject interactor)
    {
        if (isAnimating) return false;

        isOpen = !isOpen;
        StartCoroutine(AnimateDoor(isOpen ? openRotation : closedRotation));
        
        return true; 
    }

    private IEnumerator AnimateDoor(Quaternion targetRotation)
    {
        isAnimating = true;

        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }

        transform.localRotation = targetRotation; 
        isAnimating = false;
    }

    public void OnHoverEnter()
    {
        if (meshRenderers == null) return;
        
        foreach (var rend in meshRenderers)
        {
            if (rend != null && rend.material != null)
                rend.material.color = highlightColor;
        }
    }

    public void OnHoverExit()
    {
        if (meshRenderers == null || originalColors == null) return;

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null && meshRenderers[i].material != null)
            {
                meshRenderers[i].material.color = originalColors[i];
            }
        }
    }
}
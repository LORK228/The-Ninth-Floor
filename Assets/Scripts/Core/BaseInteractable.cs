using UnityEngine;

/// <summary>
/// Базовый класс для всех интерактивных объектов.
/// Избавляет от необходимости дублировать логику подсветки.
/// </summary>
public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    protected InteractableHighlighter highlighter;

    public abstract string InteractionPrompt { get; }

    protected virtual void Awake()
    {
        // Пытаемся найти подсветку. Если её нет - ничего страшного, объект просто не будет светиться.
        highlighter = GetComponent<InteractableHighlighter>();
    }

    public abstract bool Interact(GameObject interactor);

    public virtual void OnHoverEnter()
    {
        // Базовая реализация - включить подсветку
        if (highlighter != null)
        {
            highlighter.Highlight();
        }
    }

    public virtual void OnHoverExit()
    {
        // Базовая реализация - выключить подсветку
        if (highlighter != null)
        {
            highlighter.RemoveHighlight();
        }
    }
}
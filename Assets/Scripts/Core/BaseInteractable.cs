using UnityEngine;

/// <summary>
/// Базовый класс для всех интерактивных объектов.
/// Избавляет от необходимости дублировать логику подсветки.
/// </summary>
[RequireComponent(typeof(InteractableHighlighter))]
public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    protected InteractableHighlighter highlighter;

    public abstract string InteractionPrompt { get; }

    protected virtual void Awake()
    {
        highlighter = GetComponent<InteractableHighlighter>();
        
        // Если компонента почему-то нет (например, скрипт висел на объекте до рефакторинга),
        // создаем его автоматически прямо во время игры.
        if (highlighter == null)
        {
            highlighter = gameObject.AddComponent<InteractableHighlighter>();
        }
    }

    public abstract bool Interact(GameObject interactor);

    public virtual void OnHoverEnter()
    {
        // Базовая реализация - включить подсветку
        if (highlighter)
        {
            highlighter.Highlight();
        }
    }

    public virtual void OnHoverExit()
    {
        // Базовая реализация - выключить подсветку
        if (highlighter)
        {
            highlighter.RemoveHighlight();
        }
    }
}
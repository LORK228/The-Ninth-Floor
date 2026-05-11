using UnityEngine;

/// <summary>
/// Интерфейс для всех объектов, с которыми можно взаимодействовать.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Текст, который будет показан игроку в качестве подсказки.
    /// </summary>
    string InteractionPrompt { get; }

    /// <summary>
    /// Метод, который вызывается при взаимодействии с объектом (например, по нажатию кнопки).
    /// </summary>
    /// <param name="interactor">Объект, который инициировал взаимодействие (например, игрок).</param>
    /// <returns>Возвращает true, если взаимодействие было успешным.</returns>
    bool Interact(GameObject interactor);

    /// <summary>
    /// Вызывается, когда игрок начинает смотреть на этот объект.
    /// </summary>
    void OnHoverEnter();

    /// <summary>
    /// Вызывается, когда игрок перестает смотреть на этот объект.
    /// </summary>
    void OnHoverExit();
}
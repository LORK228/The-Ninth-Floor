using System;

/// <summary>
/// Класс для управления событиями (паттерн Observer/Event Bus).
/// Позволяет отвязать логику квестов от конкретных объектов.
/// </summary>
public static class GameEventManager
{
    // Событие: Задание изменилось. Передает индекс нового задания.
    public static event Action<int> OnTaskChanged;

    public static void TriggerTaskChanged(int newTaskIndex)
    {
        OnTaskChanged?.Invoke(newTaskIndex);
    }
}
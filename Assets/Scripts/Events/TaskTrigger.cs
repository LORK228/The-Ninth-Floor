using UnityEngine;
using Zenject;

public class TaskTrigger : MonoBehaviour
{
    [Header("Настройки триггера")]
    [Tooltip("Индекс квеста, который должен быть активен, чтобы триггер сработал")]
    [SerializeField] private int requiredTaskIndex = 12; // 12 = "Вернуться в квартиру"
    [Tooltip("Уничтожить триггер после срабатывания?")]
    [SerializeField] private bool destroyAfterTrigger = true;

    private ITaskManager taskManager;

    [Inject]
    public void Construct(ITaskManager taskManager)
    {
        this.taskManager = taskManager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (taskManager != null && taskManager.GetCurrentTaskIndex() == requiredTaskIndex)
        {
            Debug.Log($"Игрок вошел в триггер. Выполняю квест {requiredTaskIndex}.");
            taskManager.CompleteCurrentTask();

            if (destroyAfterTrigger)
            {
                Destroy(gameObject);
            }
        }
    }
}
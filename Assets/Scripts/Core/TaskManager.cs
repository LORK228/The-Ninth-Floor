using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI taskText; 
    
    [Header("Настройки")]
    [SerializeField] private string prefix = "- "; 

    [TextArea(2, 3)]
    [SerializeField] private List<string> allTasks = new List<string>()
    {
        "Выключить будильник",
        "Развесить белье из стиралки",
        "Посмотреть в глазок (из-за скрежета)",
        "Разогреть еду",
        "Поставить еду за компьютер",
        "Собрать мусор",
        "Выкинуть мусор на улицу",
        "Вернуться в квартиру",
        "Помыть посуду",
        "Полить цветок"
    };

    private int currentTaskIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateUI();
        // При старте оповещаем всех о текущем задании
        GameEventManager.TriggerTaskChanged(currentTaskIndex);
    }

    public void CompleteCurrentTask()
    {
        if (currentTaskIndex < allTasks.Count - 1)
        {
            currentTaskIndex++;
            Debug.Log($"Задание выполнено! Новое задание: {allTasks[currentTaskIndex]}");
            UpdateUI();
            
            // Оповещаем другие скрипты о смене задания через Event Bus
            GameEventManager.TriggerTaskChanged(currentTaskIndex);
        }
        else
        {
            Debug.Log("Все задания выполнены!");
            if (taskText != null)
            {
                taskText.text = ""; 
            }
        }
    }

    public int GetCurrentTaskIndex()
    {
        return currentTaskIndex;
    }

    private void UpdateUI()
    {
        if (taskText != null && currentTaskIndex < allTasks.Count)
        {
            taskText.text = prefix + allTasks[currentTaskIndex];
        }
    }
}
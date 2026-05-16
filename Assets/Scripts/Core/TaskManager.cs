using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Zenject;

public class TaskManager : MonoBehaviour, ITaskManager
{
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

    [Inject]
    public void Construct()
    {
    }

    private void Start()
    {
        UpdateUI();
        GameEventManager.TriggerTaskChanged(currentTaskIndex);
    }

    public void CompleteCurrentTask()
    {
        if (currentTaskIndex < allTasks.Count - 1)
        {
            currentTaskIndex++;
            Debug.Log($"Задание выполнено! Новое задание: {allTasks[currentTaskIndex]}");
            UpdateUI();
            
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
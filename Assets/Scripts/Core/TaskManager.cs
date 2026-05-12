using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI taskText; // Ссылка на UI текст задания на экране
    
    [Header("Настройки")]
    [SerializeField] private string prefix = "- "; // Префикс перед заданием

    // Список всех заданий (хронология)
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
        // Делаем этот класс Singleton, чтобы к нему можно было обращаться из любого скрипта: TaskManager.Instance
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
    }

    /// <summary>
    /// Вызывать этот метод, когда текущее задание выполнено.
    /// </summary>
    public void CompleteCurrentTask()
    {
        if (currentTaskIndex < allTasks.Count - 1)
        {
            currentTaskIndex++;
            Debug.Log($"Задание выполнено! Новое задание: {allTasks[currentTaskIndex]}");
            UpdateUI();
        }
        else
        {
            Debug.Log("Все задания выполнены!");
            if (taskText != null)
            {
                taskText.text = ""; // Очищаем текст
            }
        }
    }

    /// <summary>
    /// Вызывать этот метод, если нужно проверить, какое сейчас активно задание
    /// (например, чтобы микроволновка работала только когда активно задание "Разогреть еду")
    /// </summary>
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
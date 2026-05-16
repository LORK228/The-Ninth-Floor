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

    [Header("Дебаг (Только для тестов)")]
    [Tooltip("Задает стартовый индекс квеста. Удобно для пропуска начальных этапов при тестировании.")]
    [SerializeField] private int startTaskIndex = 0;

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

    private void Awake()
    {
        // Инициализируем стартовый индекс из настроек в инспекторе.
        // Clamp гарантирует, что мы не выйдем за пределы списка, даже если введем число 100.
        currentTaskIndex = Mathf.Clamp(startTaskIndex, 0, allTasks.Count - 1);
    }

    private void Start()
    {
        UpdateUI();
        
        // Оповещаем всю игру о том, на каком мы сейчас этапе.
        // Благодаря паттерну Observer, все нужные объекты (микроволновка и т.д.) 
        // сами разблокируются, если текущий этап соответствует их требованиям.
        GameEventManager.TriggerTaskChanged(currentTaskIndex);
        
        if (currentTaskIndex > 0)
        {
            Debug.Log($"[DEBUG] Игра начата с квеста №{currentTaskIndex}: {allTasks[currentTaskIndex]}");
        }
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
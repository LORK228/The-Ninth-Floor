using UnityEngine;
using UnityEngine.Rendering;
using Zenject;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour, IPlayerInventory
{
    [Header("Настройки руки")]
    [Tooltip("Пустой объект дочерний к камере, где будут появляться предметы")]
    [SerializeField] private Transform handPoint;

    [Tooltip("Индекс слоя для предметов в руках (например, 6 для Portable)")]
    [SerializeField] private int portableLayerIndex = 6;
    
    [Tooltip("Маска Rendering Layer (Light Layer) для предметов в руках. 2 = Light Layer 1 (HandItems)")]
    [SerializeField] private uint handItemRenderingLayer = 2;

    [Header("UI Кармана (Опционально)")]
    [Tooltip("Иконка или любой UI элемент, который покажет, что у нас есть предмет (например, ключ)")]
    [SerializeField] private GameObject pocketUIIndicator;

    private GameObject currentItemObj;
    public string CurrentItemName { get; private set; }

    // Хранилище для "карманов" (предметы без физической модели в руках)
    private HashSet<string> pocketItems = new HashSet<string>();

    [Inject]
    public void Construct()
    {
    }

    private void Start()
    {
        if (pocketUIIndicator != null)
        {
            pocketUIIndicator.SetActive(false);
        }
    }

    // --- Логика руки ---

    public void GiveItem(string itemName, GameObject itemPrefab)
    {
        GiveItem(itemName, itemPrefab, Vector3.zero, Vector3.zero);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void GiveItem(string itemName, GameObject itemPrefab, Vector3 localPositionOffset, Vector3 localRotationOffset)
    {
        ClearHand();

        CurrentItemName = itemName;

        if (itemPrefab && handPoint)
        {
            currentItemObj = Instantiate(itemPrefab, handPoint);
            
            currentItemObj.transform.localPosition = localPositionOffset;
            currentItemObj.transform.localRotation = Quaternion.Euler(localRotationOffset);
            
            // Отключаем коллайдеры
            Collider[] colliders = currentItemObj.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            // Настраиваем рендереры предмета в руках
            Renderer[] renderers = currentItemObj.GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers)
            {
                rend.receiveShadows = false;
                rend.shadowCastingMode = ShadowCastingMode.Off;
                
                rend.lightProbeUsage = LightProbeUsage.Off;
                rend.reflectionProbeUsage = ReflectionProbeUsage.Off;
                rend.renderingLayerMask = handItemRenderingLayer;
            }

            // Меняем слой объекту и всем его дочерним элементам
            SetLayerRecursively(currentItemObj, portableLayerIndex);
        }
        
        Debug.Log($"Игрок взял в руку: {itemName}");
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void ClearHand()
    {
        if (currentItemObj)
        {
            Destroy(currentItemObj);
        }
        CurrentItemName = "";
    }

    // --- Логика кармана ---

    public void AddToPocket(string itemName)
    {
        pocketItems.Add(itemName);
        Debug.Log($"Предмет добавлен в карман: {itemName}");

        // Если это ключ, включаем иконку
        if (itemName == "Ключ от квартиры" && pocketUIIndicator != null)
        {
            pocketUIIndicator.SetActive(true);
        }
    }

    public bool HasItemInPocket(string itemName)
    {
        return pocketItems.Contains(itemName);
    }

    // --- Проверки ---

    public bool HasItem(string itemName)
    {
        return CurrentItemName == itemName;
    }

    public bool HasAnyItem(string itemName)
    {
        return HasItem(itemName) || HasItemInPocket(itemName);
    }
}
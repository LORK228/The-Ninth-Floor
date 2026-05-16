using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

public class PlayerInventory : MonoBehaviour, IPlayerInventory
{
    [Header("Настройки")]
    [Tooltip("Пустой объект дочерний к камере, где будут появляться предметы")]
    [SerializeField] private Transform handPoint;

    [Tooltip("Индекс слоя для предметов в руках (например, 6 для Portable)")]
    [SerializeField] private int portableLayerIndex = 6;
    
    [Tooltip("Маска Rendering Layer (Light Layer) для предметов в руках. 2 = Light Layer 1 (HandItems)")]
    [SerializeField] private uint handItemRenderingLayer = 2;

    private GameObject currentItemObj;
    public string CurrentItemName { get; private set; }

    [Inject]
    public void Construct()
    {
    }

    public void GiveItem(string itemName, GameObject itemPrefab)
    {
        GiveItem(itemName, itemPrefab, Vector3.zero, Vector3.zero);
    }

    public void GiveItem(string itemName, GameObject itemPrefab, Vector3 localPositionOffset, Vector3 localRotationOffset)
    {
        ClearHand();

        CurrentItemName = itemName;

        if (itemPrefab != null && handPoint != null)
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
        
        Debug.Log($"Игрок взял: {itemName}");
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
        if (currentItemObj != null)
        {
            Destroy(currentItemObj);
        }
        CurrentItemName = "";
    }

    public bool HasItem(string itemName)
    {
        return CurrentItemName == itemName;
    }
}
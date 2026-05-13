using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Настройки")]
    [Tooltip("Пустой объект дочерний к камере, где будут появляться предметы")]
    [SerializeField] private Transform handPoint;

    private GameObject currentItemObj;
    public string CurrentItemName { get; private set; }

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

    /// <summary>
    /// Выдать предмет игроку в руки со стандартными настройками (без смещения)
    /// </summary>
    public void GiveItem(string itemName, GameObject itemPrefab)
    {
        GiveItem(itemName, itemPrefab, Vector3.zero, Vector3.zero);
    }

    /// <summary>
    /// Выдать предмет игроку в руки с кастомным смещением и поворотом
    /// </summary>
    public void GiveItem(string itemName, GameObject itemPrefab, Vector3 localPositionOffset, Vector3 localRotationOffset)
    {
        ClearHand();

        CurrentItemName = itemName;

        if (itemPrefab != null && handPoint != null)
        {
            currentItemObj = Instantiate(itemPrefab, handPoint);
            
            // Применяем локальное смещение и поворот относительно handPoint
            currentItemObj.transform.localPosition = localPositionOffset;
            currentItemObj.transform.localRotation = Quaternion.Euler(localRotationOffset);
            
            // Отключаем коллайдеры у предмета в руках, чтобы они не мешали лучу взаимодействия и физике игрока
            Collider[] colliders = currentItemObj.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }
        }
        
        Debug.Log($"Игрок взял: {itemName}");
    }

    /// <summary>
    /// Убрать текущий предмет из рук
    /// </summary>
    public void ClearHand()
    {
        if (currentItemObj != null)
        {
            Destroy(currentItemObj);
        }
        CurrentItemName = "";
    }

    /// <summary>
    /// Проверить, держит ли игрок определенный предмет
    /// </summary>
    public bool HasItem(string itemName)
    {
        return CurrentItemName == itemName;
    }
}
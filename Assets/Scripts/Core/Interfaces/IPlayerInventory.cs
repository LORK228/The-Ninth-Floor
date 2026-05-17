using UnityEngine;

public interface IPlayerInventory
{
    string CurrentItemName { get; }
    void GiveItem(string itemName, GameObject itemPrefab);
    void GiveItem(string itemName, GameObject itemPrefab, Vector3 localPositionOffset, Vector3 localRotationOffset);
    
    // Новые методы для "кармана" (скрытого инвентаря)
    void AddToPocket(string itemName);
    bool HasItemInPocket(string itemName);
    
    void ClearHand();
    bool HasItem(string itemName); // Проверяет только то, что в руке (для совместимости)
    bool HasAnyItem(string itemName); // Проверяет и руки, и карманы
}
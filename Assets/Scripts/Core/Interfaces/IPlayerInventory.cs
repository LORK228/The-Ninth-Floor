using UnityEngine;

public interface IPlayerInventory
{
    string CurrentItemName { get; }
    void GiveItem(string itemName, GameObject itemPrefab);
    void GiveItem(string itemName, GameObject itemPrefab, Vector3 localPositionOffset, Vector3 localRotationOffset);
    void ClearHand();
    bool HasItem(string itemName);
}
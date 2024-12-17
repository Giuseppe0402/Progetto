using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [SerializeField] private List<Image> inventorySlots;
    private List<GameObject> inventoryItems = new List<GameObject>();

    public void InitializeInventory(List<Image> slots)
    {
        inventorySlots = slots;
    }

    public bool AddToInventory(GameObject item, Sprite itemIcon)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].sprite == null)
            {
                inventorySlots[i].sprite = itemIcon;
                inventoryItems.Add(item);
                item.SetActive(false);
                return true;
            }
        }
        return false;
    }

    public bool HasItem(Sprite itemIcon)
    {
        return inventorySlots.Exists(slot => slot.sprite == itemIcon);
    }

    public void RemoveItem(Sprite itemIcon)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].sprite == itemIcon)
            {
                inventorySlots[i].sprite = null;
                if (i < inventoryItems.Count)
                {
                    inventoryItems.RemoveAt(i);
                }
                return;
            }
        }
    }
}
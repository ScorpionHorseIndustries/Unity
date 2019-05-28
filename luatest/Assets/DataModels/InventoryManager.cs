using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager {
  private World world;

  public Dictionary<string, List<InventoryItem>> inventories; 
  //private List<InventoryItem> items;
  public InventoryManager(World world) {
    this.world = world;
    inventories = new Dictionary<string, List<InventoryItem>>();
  }


  public bool PlaceItemOnTile(Tile t, InventoryItem item) {

    if (t.PlaceInventoryItem(item)) {
      if (item.currentStack == 0) {
        RemoveInventoryItem(item);
      }

      if (t.inventoryItem != null) {
        AddInventoryItem(t.inventoryItem);
      }
      return true;
    } else {
      return false;
    }
  }

  public void AddInventoryItem(InventoryItem item) {
    if (!inventories.ContainsKey(item.type)) {
      if (!inventories[item.type].Contains(item)) {
        inventories[item.type].Add(item);
      }

    } else {
      inventories[item.type] = new List<InventoryItem>();
      inventories[item.type].Add(item);
    }

  } 

  public void RemoveInventoryItem(InventoryItem item) {
    if (inventories.ContainsKey(item.type)) {
      if (inventories[item.type].Contains(item)) {
        inventories[item.type].Remove(item);
      }

    }

  }
}

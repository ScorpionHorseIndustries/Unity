using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager {
  private World world;

  public Dictionary<string, List<InventoryItem>> inventories;
  //private List<InventoryItem> items;
  public InventoryManager(World world) {
    this.world = world;
    inventories = new Dictionary<string, List<InventoryItem>>();
  }


  public List<InventoryItem> GetNearest(Tile fromHere, string itemType, int qty) {
    Dictionary<InventoryItem, float> tileDist = new Dictionary<InventoryItem, float>();
    List<InventoryItem> tiles = new List<InventoryItem>();
    if (inventories.ContainsKey(itemType)) {
      List<InventoryItem> items = inventories[itemType];
      foreach (InventoryItem item in items) {
        tileDist[item] = Funcs.TaxiDistance(fromHere, item.tile);

      }
      int qtyTaken = 0;
      foreach (KeyValuePair<InventoryItem, float> kvp in tileDist.OrderBy(key => key.Value)) {
        if (qtyTaken == qty) {
          break;
        }
        if (kvp.Key.currentStack < (qty - qtyTaken)) {
          qtyTaken += kvp.Key.currentStack;

        } else {
          qtyTaken = qty;
        }
      }

    }

    return tiles;
  }

  public Tile GetNearest(Tile fromHere, string itemType) {

    if (inventories.ContainsKey(itemType)) {
      List<Tile> tiles = new List<Tile>();

      List<InventoryItem> items = inventories[itemType];

      foreach (InventoryItem item in items) {
        tiles.Add(item.tile);
      }

      float lowest = 0;
      Tile nearest = null;
      float dist = 0;
      foreach (Tile t in tiles) {
        dist = Funcs.TaxiDistance(fromHere, t);

        if (nearest == null || dist < lowest) {
          nearest = t;
          lowest = dist;
        }
      }

      return nearest;

    }
    return null;
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

    if (inventories.ContainsKey(item.type)) {
      if (!inventories[item.type].Contains(item)) {
        inventories[item.type].Add(item);
      }

    } else {
      inventories[item.type] = new List<InventoryItem>();
      inventories[item.type].Add(item);
    }

  }

  public InventoryItem RemoveQty(Tile tile, string name, int qty) {

    if (tile.inventoryItem != null && tile.inventoryItem.type == name && tile.inventoryItem.currentStack >= qty) {
      if (tile.inventoryItem.currentStack == qty) {
        RemoveInventoryItem(tile.inventoryItem);
        

      } else {

      }


    }

    return null;

  }


  public void RemoveInventoryItem(InventoryItem item) {
    if (item == null) return;
    if (inventories.ContainsKey(item.type)) {
      if (inventories[item.type].Contains(item)) {
        inventories[item.type].Remove(item);
      }

    }

  }
}

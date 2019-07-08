using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

using NoYouDoIt.TheWorld;
using NoYouDoIt.Utils;
using Unity.Jobs;
using Unity.Collections;

namespace NoYouDoIt.DataModels {

  //public struct StockPileJob : IJob {
  //  public NativeHashMap<int,int> naStockPileSettings;



  //  public void Execute() {

  //  }
  //}

  public class StockPileSetting {
    private static int ID = 0;
    public int id { get; private set; }
    public InventoryItem item;
    public string name { get; private set; }
    public int currentQty;
    public int maxQty;
    public bool all = false;
    public int stackSize { get; private set; }
    public bool pendingWork = false;

    


    public StockPileSetting(InventoryItem item) {
      this.item = item;
      this.name = item.type;
      this.stackSize = InventoryItem.GetStackSize(this.name);
      this.maxQty = this.stackSize;
      StockPileSetting.ID += 1;
      this.id = ID;


    }
  }

  public class InventoryManager {
    //StockPileJob job;
    private World world;

    public Dictionary<string, StockPileSetting> stockpileSettings;

    NYDITimer timer;

    //public Dictionary<string, List<InventoryItem>> inventories;
    public List<Inventory> inventories;
    //private List<InventoryItem> items;
    public InventoryManager(World world) {
      this.world = world;
      inventories = new List<Inventory>();//new Dictionary<string, List<InventoryItem>>();
      stockpileSettings = new Dictionary<string, StockPileSetting>();
      timer = new NYDITimer("updateTimer", 1, UpdateStockPileSettings);
    }

    public void SetStockPileSettingWork(string name, bool b) {
      stockpileSettings[name].pendingWork = b;

    }

    public void RegisterInventory(Inventory inventory) {
      if (!inventories.Contains(inventory)) {
        inventories.Add(inventory);
      }
    }

    public void UnregisterInventory(Inventory inventory) {
      if (inventories.Contains(inventory)) {
        inventories.Remove(inventory);
        if (inventory.TotalQty() > 0) {
          inventory.Explode();
        }
      }
    }

    public void InitStockpile() {
      //job.naStockPileSettings = new NativeHashMap<int, int>();
      foreach (string name in InventoryItem.GetAllPrototypeNames()) {
        InventoryItem item = InventoryItem.GetPrototype(name);

        StockPileSetting sps = new StockPileSetting(item);
        stockpileSettings[name] = sps;

        //job.naStockPileSettings.TryAdd(sps.id, 0);
      }
    }

    public void Update(float deltaTime) {
      timer.Update(deltaTime);

    }

    public int GetLooseQty(string name) {
      int qty = 0;
      foreach (Inventory inv in inventories.Where(e => !e.IsStockPile && e.tile != null)) {
        qty += inv.HowMany(name);
      }

      return qty;
    }

    public int GetStockpileQty(string name) {
      int qty = 0;
      foreach (Inventory inv in inventories.Where(e => e.IsStockPile)) {
        qty += inv.HowMany(name);
      }

      return qty;
    }

    public void UpdateStockPileSettings() {
      //job.Schedule();

      foreach (StockPileSetting sps in stockpileSettings.Values) {
        sps.currentQty = GetStockpileQty(sps.name);
      }
    }


    //public List<Tile> GetNearest(Tile fromHere, string itemType, int qty) {
    //  Dictionary<Tile, float> tileDist = new Dictionary<Tile, float>();
    //  List<Tile> tiles = new List<Tile>();
    //  //if (inventories.ContainsKey(itemType)) {
    //  foreach (Inventory inventory in inventories) {
    //    if (inventory.tile == null) continue;
    //    int hasQty = inventory.HowMany(itemType);

    //    if (hasQty > 0) {
    //      tileDist[inventory.tile] = Funcs.TaxiDistance(fromHere, inventory.tile);


    //    }
    //  }


    //  int qtyTaken = 0;
    //  foreach (KeyValuePair<Tile, float> kvp in tileDist.OrderBy(key => key.Value)) {
    //    if (qtyTaken == qty) {
    //      break;
    //    }
    //    if (kvp.Key.currentStack < (qty - qtyTaken)) {
    //      qtyTaken += kvp.Key.currentStack;

    //    } else {
    //      qtyTaken = qty;
    //    }
    //  }



    //  return tiles;
    //}

    public Tile GetNearest(Tile fromHere, string itemType, bool includeStockPiles = true) {
      if (itemType == InventoryItem.ANY) {


        itemType = InventoryItem.GetRandomPrototype().type;
      }

      List<Tile> tiles = new List<Tile>();
      Dictionary<Tile, float> nearestTiles = new Dictionary<Tile, float>();
      float maxSoFar = float.MaxValue;
      foreach (Inventory inventory in inventories.Where(e => e.HowMany(itemType) > 0 && e.tile != null)) {
        if (!includeStockPiles && inventory.tile.installedItem != null && inventory.tile.installedItem.type == "installed::stockpile") continue;

        float dist = Funcs.TaxiDistance(fromHere, inventory.tile);

        if (dist < maxSoFar) {
          maxSoFar = dist;
          nearestTiles[inventory.tile] = dist;
        }
      }
      foreach (KeyValuePair<Tile, float> kvp in nearestTiles.OrderBy(key => key.Value)) {
        return kvp.Key;
      }

      return null;
    }


    //if (inventories.ContainsKey(itemType)) {
    //  List<Tile> tiles = new List<Tile>();

    //  List<InventoryItem> items = inventories[itemType];

    //  foreach (InventoryItem item in items) {
    //    if (!includeStockPiles) {
    //      if (item.tile.installedItem != null && item.tile.installedItem.type == "installed::stockpile") continue;
    //    }
    //    tiles.Add(item.tile);
    //  }

    //  float lowest = 0;
    //  Tile nearest = null;
    //  float dist = 0;
    //  foreach (Tile t in tiles) {
    //    dist = Funcs.TaxiDistance(fromHere, t);

    //    if (nearest == null || dist < lowest) {
    //      nearest = t;
    //      lowest = dist;
    //    }
    //  }

    //  return nearest;

    //}
    //return null;
    //}

    //public bool PlaceItemOnTile(Tile t, InventoryItem item) {

    //  if (t.PlaceInventoryItem(item)) {
    //    if (item.currentStack == 0) {
    //      RemoveInventoryItem(item);
    //    }

    //    if (t.inventoryItem != null) {
    //      AddInventoryItem(t.inventoryItem);
    //    }
    //    return true;
    //  } else {
    //    return false;
    //  }
    //}

    //public void AddInventoryItem(InventoryItem item) {

    //  if (inventories.ContainsKey(item.type)) {
    //    if (!inventories[item.type].Contains(item)) {
    //      inventories[item.type].Add(item);
    //    }

    //  } else {
    //    inventories[item.type] = new List<InventoryItem>();
    //    inventories[item.type].Add(item);
    //  }

    //}

    //public InventoryItem RemoveQty(Tile tile, string name, int qty) {

    //  if (tile.inventoryItem != null && tile.inventoryItem.type == name && tile.inventoryItem.currentStack >= qty) {
    //    if (tile.inventoryItem.currentStack == qty) {
    //      RemoveInventoryItem(tile.inventoryItem);


    //    } else {

    //    }


    //  }

    //  return null;

    //}


    //public void RemoveInventoryItem(InventoryItem item) {
    //  if (item == null) return;
    //  if (inventories.ContainsKey(item.type)) {
    //    if (inventories[item.type].Contains(item)) {
    //      inventories[item.type].Remove(item);
    //    }

    //  }

    //}
  }
}
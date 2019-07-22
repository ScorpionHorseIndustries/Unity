using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using NoYouDoIt.DataModels;
using NoYouDoIt.Utils;

namespace NoYouDoIt.TheWorld {

  //public class TileOccupant {
  //  public string name { get; private set; }
  //  public string type { get; private set; }
  //  public TileOccupant(string name, string type) {
  //    this.name = name;
  //    this.type = type;
  //  }

  //  public Action CBPleaseMove;

  //  public void PleaseMove() {
  //    if (CBPleaseMove!=null) {
  //      CBPleaseMove();
  //    }
  //  }



  //}

  public class Tile : IComparable<Tile>{

    public enum CAN_ENTER {
      YES, NEVER, SOON
    }
    //public enum TYPE {DIRT,GRASS,EMPTY,WALL }
    Action<Tile> cbChanged;
    Action<Tile> cbInventoryItemChanged;
    //Action<Tile> cbInventoryItemRemoved;
    public TileChunk chunk { get; protected set; }
    public Room room;
    //public Tile.TYPE type { get { return type; } set { type = value; if (cbTypeChanged != null) cbTypeChanged(this); } }
    public TileType type { get; private set; }
    //public InventoryItem inventoryItem { get; private set; }
    public InstalledItem installedItem { get; private set; }
    private Inventory inventory;

    public Dictionary<string, Tile> neighbours = new Dictionary<string, Tile>();
    //public Dictionary<string, Tile> neighboursDiag = new Dictionary<string, Tile>();

    public List<Tile> neighboursList = new List<Tile>();
    public Dictionary<Tile, float> edges = new Dictionary<Tile, float>();
    //public List<Tile> neighboursListDiag = new List<Tile>();

    public int local_x { get; private set; }
    public int local_y { get; private set; }
    public int world_x {
      get {
        return local_x + (chunk.world_x);
      }
    }

    public int world_y {
      get {
        return local_y + (chunk.world_y);
      }
    }
    private World world;

    //private List<Character> occupiedBy = new List<Character>();
    private List<Entity> occupiedBy = new List<Entity>();
    public Entity GetOccupant(int i) {
      if (i >= 0 && i <= occupiedBy.Count - 1) {
        return occupiedBy[i];
      }

      return null;
    }
    public int countOfOccupied { get { return occupiedBy.Count; } }

    public Tile North { get { return GetNeighbour(World.NORTH); } }
    public Tile East { get { return GetNeighbour(World.EAST); } }
    public Tile South { get { return GetNeighbour(World.SOUTH); } }
    public Tile West { get { return GetNeighbour(World.WEST); } }
    //private List<Job> pendingJobs = new List<Job>();
    private List<WorkItem> pendingWork= new List<WorkItem>();
    
    public TileZone zone { get; protected set; }


    public Tile(World world, TileChunk chunk, TileType type, int x, int y) {
      this.type = type;
      this.local_x = x;
      this.local_y = y;
      this.world = world;
      this.chunk = chunk;
      this.inventory = null; // new Inventory(world, 1, INVENTORY_TYPE.TILE, this);
      //world.GetNeighbours(this, true);
      //foreach(Tile t in neighbours.Values) {
      //  neighboursList.Add(t);
      //}

    }

    public void SetZone(TileZone zone) {
      this.zone = zone;
      if (cbChanged != null) {
        cbChanged(this);
      }
    }


    public bool HasPendingWork {
      get {
        return pendingWork.Count > 0;
      }
    }

    public int countPendingWork {
      get {
        return pendingWork.Count;
      }
    }


    public bool AddWork(WorkItem work) {
      if (!pendingWork.Contains(work)) {
        pendingWork.Add(work);
        return true;
      } else {
        return false;
      }
    }

    //public bool AddJob(Job job) {
    //  if (!pendingJobs.Contains(job)) {
    //    pendingJobs.Add(job);
    //    return true;
    //  } else {
    //    return false;
    //  }
    //}

    public bool RemoveWork(WorkItem work) {
      if (pendingWork.Contains(work)) {
        pendingWork.Remove(work);


        return true;
      } else {
        return false;
      }
    }



    public void Enter(Entity c) {
      if (!occupiedBy.Contains(c)) {
        occupiedBy.Add(c);
      }
    }

    public string WhoIsHere() {
      string s = "";
      int c = 0;
      foreach (Entity to in occupiedBy) {
        if (c > 0) {
          s += "\n";
        }
        s += to.typeName + "..." + to.name;// + "(" + robot.state.ToString() + ")";
        c += 1;
      }


      return s;
    }

    public bool IsItMe(Entity me) {
      return occupiedBy.Count == 1 && occupiedBy.Contains(me);
    }

    public void PleaseMove() {
      foreach (Entity to in occupiedBy) {
        to.PleaseMove();
      }
    }

    public void Leave(Entity c) {
      if (occupiedBy.Contains(c)) {
        occupiedBy.Remove(c);
      }
    }


    public int AddToInventory(string type, int qty) {
      InventoryInit();
      int acceptedQty = inventory.AddItem(type, qty);

      if (cbInventoryItemChanged != null) {
        cbInventoryItemChanged(this);
      }
      return acceptedQty;

    }

    public int RemoveFromInventory(string type, int qty) {
      if (inventory == null) return 0;
      int qtyGiven = inventory.RemoveItem(type, qty);
      if (cbInventoryItemChanged != null) {
        cbInventoryItemChanged(this);
      }

      if (inventory.IsEmpty()) {
        World.current.inventoryManager.UnregisterInventory(inventory);
        inventory = null;
      }

      return qtyGiven;
    }

    public void InventoryInit() {
      if (inventory == null) {
        inventory = new Inventory(World.current, 1, INVENTORY_TYPE.TILE, this);
      }
    }

    public Inventory InventoryGetRef() {
      InventoryInit();
      return inventory;
    }

    public int InventoryAllocate(string type, int qty) {
      if (inventory == null) return 0;
      return inventory.Allocate(type, qty);
    }

    public int InventoryDeAllocate(string type, int qty) {
      if (inventory == null) return 0;
      return inventory.DeAllocate(type, qty);
    }



    public int InventoryTotalAllocated(string type) {
      if (inventory == null) {
        return 0;
      }
      return inventory.GetAllocatedQty(type);
    }

    public int InventoryTotal(string type) {
      if (inventory == null) return 0;
      return inventory.HowMany(type);
    }

    public float movementFactor {
      get {
        float ws = type.movementFactor;

        if (installedItem != null) {
          ws *= installedItem.movementFactor;

        } else if (inventory != null && !inventory.IsEmpty()) {
          ws *= 0.9f;
        }
        return ws;
      }
    }



    public override String ToString() {
      return "tile: " + type + " (" + world_x + "," + world_y + ") local(" + local_x + "," + local_y + ")";
      //return "tile: " + type + " (" + x + "," + y + "), p:" + pendingJob + ", i: " + (installedItem != null) + ", l: " + (looseItem != null);
    }

    public void cbRegisterOnChanged(Action<Tile> cb) {
      cbChanged += cb;
    }

    public void cbUnregisterOnChanged(Action<Tile> cb) {
      cbChanged -= cb;
    }

    public void cbRegisterOnItemChanged(Action<Tile> cb) {
      cbInventoryItemChanged += cb;
    }

    public void cbUnregisterOnItemChanged(Action<Tile> cb) {
      cbInventoryItemChanged -= cb;
    }

    public TileType getType() {
      return type;
    }

    public void SetType(TileType t) {
      type = t;
      if (cbChanged != null) {
        cbChanged(this);
      }
    }

    //public InventoryItem RemoveInventoryItem(int qty) {

    //  if ()
    //  return null;

    //}


    //private void OnInventoryItemPlaced(Tile t) {

    //}





    //public bool PlaceInventoryItem(InventoryItem inv) {

    //  if (inv == null) {
    //    Debug.LogError("cannot place null inventory item");

    //    return false;
    //  }

    //  if (inventoryItem != null) {
    //    //something here already... combine?
    //    if (inv.type == inventoryItem.type) {
    //      if (inventoryItem.currentStack + inv.currentStack > inv.maxStackSize) {
    //        inv.currentStack -= inv.maxStackSize - inventoryItem.currentStack;
    //        inventoryItem.currentStack = inventoryItem.maxStackSize;
    //      } else {
    //        inventoryItem.currentStack += inv.currentStack;
    //        inv.currentStack = 0;

    //      }
    //      if (cbInventoryItemChanged != null) {
    //        cbInventoryItemChanged(this);
    //      }
    //      return true;
    //    } else {
    //      Debug.LogError("cannot add " + inv.type + " to a stack of " + inventoryItem.type);

    //    }
    //  } else {
    //    inventoryItem = inv.Copy();
    //    inv.currentStack = 0;
    //    inventoryItem.SetTile(this);
    //    if (cbInventoryItemChanged != null) {
    //      cbInventoryItemChanged(this);
    //    }
    //    return true;
    //  }

    //  return false;
    //}

    public void ForceInstalledItem(InstalledItem item) {
      this.installedItem = item;
    }

    public bool placeInstalledItem(InstalledItem instobj) {
      if (instobj == null) {
        this.installedItem = null;
        return true;
      }
      if (this.installedItem == null) {
        this.installedItem = instobj;
        for (int xx = world_x; xx < world_x + instobj.width; xx += 1) {
          for (int yy = world_y; yy < world_y + instobj.height; yy += 1) {
            Tile t = world.GetTileAt(xx, yy);
            t.ForceInstalledItem(instobj);
          }
        }


        return true;
      } else {
        return false;
      }
    }

    public bool RemoveInstalledItem() {
      installedItem = null;
      return true;
    }

    public bool InventoryHasSpaceFor(string type, int qty) {
      InventoryInit();
      return inventory.HasSpaceFor(type, qty);
    }

    public bool IsInventoryEmpty() {
      if (inventory == null) return true;
      return inventory.IsEmpty();
    }

    public bool IsEmpty() {
      return (installedItem == null && (inventory == null || inventory.IsEmpty()) && movementFactor > 0.3 && !HasPendingWork);
    }

    

    public bool IsEmptyApartFromInventory() {
      return (installedItem == null && movementFactor > 0.3 && !HasPendingWork);
    }

    public string GetContents() {
      if (inventory == null) return "";
      return inventory.ToString();
    }

    public string GetFirstInventoryItem() {
      if (inventory == null) return null;
      return inventory.GetFirst();
    }

    public int GetInventoryTotal() {
      if (inventory == null) return 0;
      return inventory.TotalQty();
    }

    public string GetInventorySpriteName() {
      if (inventory == null) return null;
      return inventory.SpriteName();
    }

    public int GetInventorySpace() {
      if (inventory == null) {
        return 1;
      }
      string itemName = inventory.GetFirst();
      if (itemName != null) {
        int current = inventory.HowMany(itemName, false);
        int space = InventoryItem.GetStackSize(itemName) - current;
        return space;
      }
      return -1;
    }

    public void SetNeighbours(bool allowDiagonalNeighbours) {
      neighboursList.Clear();
      neighbours.Clear();

      world.GetNeighbours(this, allowDiagonalNeighbours);
      foreach (Tile tile in neighbours.Values) {
        if (tile != null) {
          neighboursList.Add(tile);
        }
      }


    }

    public CAN_ENTER CanEnter(Entity r) {
      if (movementFactor == 0) return CAN_ENTER.NEVER;


      if (installedItem != null && installedItem.enterRequestedFunc != null) {
        return InstalledItemActions.CallEnterRequested(installedItem.enterRequestedFunc, installedItem);
        //installedItem.enterRequested(installedItem);
      } 
      //else if (!IsItMe(c) && countOfOccupied > 0) {
      //  return CAN_ENTER.SOON;
      //}




      return CAN_ENTER.YES;
    }

    public Tile GetRandomNeighbour() {
      return neighboursList[UnityEngine.Random.Range(0, neighboursList.Count)];
    }

    public Tile GetNeighbour(string dir) {
      if (neighbours.ContainsKey(dir)) {
        return neighbours[dir];
      } else {
        return null;
      }
    }


    public bool IsNeighbour(Tile o, bool diagonal = false) {
      int xdiff = Mathf.Abs(world_x - o.world_x);
      int ydiff = Mathf.Abs(world_y - o.world_y);


      if (xdiff + ydiff == 1) {
        return true;
      } else if (diagonal) {
        if (xdiff == 1 && ydiff == 1) {

          return true;
        } else {
          return false;
        }
      } else {


        return false;

      }

    }

    public string JobsToString(int w) {
      string output = Funcs.PadPair(w, "work", "outstanding");
      foreach (WorkItem work in pendingWork) {
        if (work.inventoryItemName != null) {
          output += "\n" + Funcs.PadPair(w, work.inventoryItemName, work.inventoryItemQtyRemaining + "/" + work.inventoryItemQtyRequired);
        } else { 
          output += "\n" + Funcs.PadPair(w, work.description, "...");
        }
        if (work.assignedRobot != null) {
          output += "\n" + Funcs.PadPair(w, "assigned", work.assignedRobot.name);
        }

      }

      return output;
    }

    public bool Equals(Tile t) {
      if (world_x == t.world_x && world_y == t.world_y) return true;

      return false;
    }

    public int CompareTo(Tile other) {
      return (world_x + world_y).CompareTo((other.world_x + other.world_y));
    }
  }
}
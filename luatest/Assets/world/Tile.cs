using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;




public class Tile {

  public enum CAN_ENTER {
    YES, NEVER, SOON
  }
  //public enum TYPE {DIRT,GRASS,EMPTY,WALL }
  Action<Tile> cbChanged;

  public TileChunk chunk { get; protected set; }
  public Room room;
  //public Tile.TYPE type { get { return type; } set { type = value; if (cbTypeChanged != null) cbTypeChanged(this); } }
  public TileType type { get; private set; }
  public InventoryItem inventoryItem { get; private set; }
  public InstalledItem installedItem { get; private set; }

  public Dictionary<string, Tile> neighbours = new Dictionary<string, Tile>();
  //public Dictionary<string, Tile> neighboursDiag = new Dictionary<string, Tile>();

  public List<Tile> neighboursList = new List<Tile>();
  public Dictionary<Tile, float> edges = new Dictionary<Tile, float>();
  //public List<Tile> neighboursListDiag = new List<Tile>();

  public int local_x { get; private set; }
  public int local_y { get; private set; }
  public int world_x { get {
      return local_x + (chunk.world_x);
    } }

  public int world_y {
    get {
      return local_y + (chunk.world_y);
    }
  }
  public World world { get; private set; }

  private List<Character> occupiedBy = new List<Character>();
  public int countOfOccupied { get { return occupiedBy.Count; } }

  public Tile North { get { return GetNeighbour(World.NORTH); } }
  public Tile East { get { return GetNeighbour(World.EAST); } }
  public Tile South { get { return GetNeighbour(World.SOUTH); } }
  public Tile West { get { return GetNeighbour(World.WEST); } }
  private List<Job> pendingJobs = new List<Job>();
  public TileZone zone { get; protected set; }


  public Tile(World world, TileChunk chunk, TileType type, int x, int y) {
    this.type = type;
    this.local_x = x;
    this.local_y = y;
    this.world = world;
    this.chunk = chunk;
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


  public bool HasPendingJob {
    get {
      return pendingJobs.Count > 0;
    }
  }

  public int countPendingJobs {
    get {
      return pendingJobs.Count;
    }
  }

  public bool AddJob(Job job) {
    if (!pendingJobs.Contains(job)) {
      pendingJobs.Add(job);
      return true;
    } else {
      return false;
    }
  }

  public bool RemoveJob(Job job) {
    if (pendingJobs.Contains(job)) {
      pendingJobs.Remove(job);

      if (job.jobType == JOB_TYPE.BUILD) {

      }
      return true;
    } else {
      return false;
    }
  }



  public void Enter(Character c) {
    if (!occupiedBy.Contains(c)) {
      occupiedBy.Add(c);
    }
  }

  public string WhoIsHere() {
    string s = "";
    int c = 0;
    foreach (Character chr in occupiedBy) {
      if (c > 0) {
        s += "\n";
      }
      s += chr.name + "(" + chr.state.ToString() + ")";
      c += 1;
    }


    return s;
  }

  public bool IsItMe(Character me) {
    return occupiedBy.Count == 1 && occupiedBy.Contains(me);
  }

  public void PleaseMove(Character asker) {
    foreach (Character c in occupiedBy) {
      c.PleaseMove(asker);
    }
  }

  public void Leave(Character c) {
    if (occupiedBy.Contains(c)) {
      occupiedBy.Remove(c);
    }
  }



  public float movementFactor {
    get {
      float ws = type.movementFactor;

      if (installedItem != null) {
        ws *= installedItem.movementFactor;

      } else if (inventoryItem != null) {
        ws *= 0.8f;
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

  public void RemoveInventoryItem() {
    inventoryItem = null;
  }


  



  public bool PlaceInventoryItem(InventoryItem inv) {

    if (inv == null) {
      inventoryItem = null;
      return true;
    }

    if (inventoryItem != null) {
      //something here already... combine?
      if (inv.type == inventoryItem.type) {
        if (inventoryItem.currentStack + inv.currentStack > inv.maxStackSize) {
          inv.currentStack -= inv.maxStackSize - inventoryItem.currentStack;
          inventoryItem.currentStack = inventoryItem.maxStackSize;
        } else {
          inventoryItem.currentStack += inv.currentStack;
          inv.currentStack = 0;

        }
        return true;
      } else {
        Debug.LogError("cannot add " + inv.type + " to a stack of " + inventoryItem.type);

      }
    } else {
      inventoryItem = inv.Copy();
      inv.currentStack = 0;
      inventoryItem.SetTile(this);
      return true;
    }

    return false;
  }

  public bool placeInstalledObject(InstalledItem instobj) {
    if (instobj == null) {
      this.installedItem = null;
      return true;
    }
    if (this.installedItem == null) {
      this.installedItem = instobj;
      return true;
    } else {
      return false;
    }
  }

  public bool IsEmpty() {
    return (installedItem == null && inventoryItem == null && movementFactor > 0.3 && !HasPendingJob);
  }

  public void SetNeighbours(bool allowDiagonalNeighbours) {
    neighboursList.Clear();
    neighbours.Clear();

    world.GetNeighbours(this, allowDiagonalNeighbours);
    foreach (Tile tile in neighbours.Values) {
      neighboursList.Add(tile);
    }
    

  }

  public CAN_ENTER CanEnter(Character c) {
    if (movementFactor == 0) return CAN_ENTER.NEVER;


    if (installedItem != null && installedItem.enterRequested != null) {
      return installedItem.enterRequested(installedItem);
    }
    //else if (!IsItMe(c) && countOfOccupied > 0) {
    //    return CAN_ENTER.SOON;





    return CAN_ENTER.YES;
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

  public string JobsToString() {
    string output = " (" + pendingJobs.Count + "): ";
    foreach (Job j in pendingJobs) {
      output += j.jobType.ToString() ;
      if (j.recipe != null) {
        output += j.recipe;

      }
    }

    return output;
  }
}

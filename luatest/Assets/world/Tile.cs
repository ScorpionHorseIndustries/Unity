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


  public Room room;
  //public Tile.TYPE type { get { return type; } set { type = value; if (cbTypeChanged != null) cbTypeChanged(this); } }
  public TileType type { get; private set; }
  public InventoryItem inventoryItem { get; private set; }
  public InstalledItem installedItem { get; private set; }

  public Dictionary<string, Tile> neighbours = new Dictionary<string, Tile>();
  public Dictionary<string, Tile> neighboursDiag = new Dictionary<string, Tile>();

  public List<Tile> neighboursList = new List<Tile>();
  public List<Tile> neighboursListDiag = new List<Tile>();

  public int x { get; private set; }
  public int y { get; private set; }
  public World world { get; private set; }

  private List<Character> occupiedBy = new List<Character>();
  public int countOfOccupied { get { return occupiedBy.Count; } }

  public Tile North { get { return GetNeighbour(World.NORTH); } }
  public Tile East { get { return GetNeighbour(World.EAST); } }
  public Tile South { get { return GetNeighbour(World.SOUTH); } }
  public Tile West { get { return GetNeighbour(World.WEST); } }
  public bool pendingJob = false;

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

  public Tile(World world, TileType type, int x, int y) {
    this.type = type;
    this.x = x;
    this.y = y;
    this.world = world;

  }

  public override String ToString() {
    return "tile: " + type + " (" + x + "," + y + ")";
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
    return (installedItem == null && inventoryItem == null && movementFactor > 0.3 && !pendingJob);
  }

  public void SetNeighbours(bool allowDiagonalNeighbours) {
    neighboursList.Clear();
    neighbours.Clear();

    neighbours = world.GetNeighbours(this, allowDiagonalNeighbours);
    neighboursList = world.GetNeighboursList(this, allowDiagonalNeighbours);

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
    int xdiff = Mathf.Abs(x - o.x);
    int ydiff = Mathf.Abs(y - o.y);


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



}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Tile {
  //public enum TYPE {DIRT,GRASS,EMPTY,WALL }
  Action<Tile> cbChanged;

  //public Tile.TYPE type { get { return type; } set { type = value; if (cbTypeChanged != null) cbTypeChanged(this); } }
  public TileType type { get; private set; }
  public LooseItem looseItem { get; private set; }
  public InstalledItem installedItem { get; private set; }

  public Dictionary<string, Tile> neighbours = new Dictionary<string, Tile>();
  public Dictionary<string, Tile> neighboursDiag = new Dictionary<string, Tile>();
  
  public List<Tile> neighboursList = new List<Tile>();
  public List<Tile> neighboursListDiag = new List<Tile>();

  public int x { get; private set; }
  public int y { get; private set; }
  public World world { get; private set; }

  public bool pendingJob = false;

  public float movementFactor { get {
      float ws = type.movementFactor;

      if (installedItem != null) {
        ws *= installedItem.movementFactor;
        
      } else if (looseItem != null) {
        ws *= 0.8f;
      }
      return ws;
    } }

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

  public void SetNeighbours(bool allowDiagonalNeighbours) {
    neighboursList.Clear();
    neighbours.Clear();

    neighbours = world.GetNeighbours(this, allowDiagonalNeighbours);
    neighboursList = world.GetNeighboursList(this, allowDiagonalNeighbours);

  }

  public bool IsNeighbour(Tile o, bool diagonal = false) {
    int xdiff = Mathf.Abs(x - o.x);
    int ydiff = Mathf.Abs(y - o.y);


    if (xdiff + ydiff == 1) {
      return true;
    } else if (diagonal) {
      if ( xdiff == 1 && ydiff == 1) {

        return true;
      } else {
        return false;
      }
    } else {


      return false;

    }

  }



}

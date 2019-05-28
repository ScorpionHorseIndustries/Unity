using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {


  //static properties
  private static int ROOM_ID = 0;
  List<Tile> tiles;
  //-----------------------PROPERTIES-----------------------
  public float temp = 0;
  public int id { get; private set; }
  public World world { get; private set; }
  public Color roomColour;


  public Room(World world) {
    this.world = world;
    ROOM_ID += 1;
    id = ROOM_ID;
    tiles = new List<Tile>();
    roomColour = new Color(Random.value, Random.value, Random.value);


  }

  public List<Tile> getTiles() {
    return tiles;

  }

  public void AssignList(List<Tile> tiles) {
    foreach(Tile t  in tiles) {
      AssignTile(t);
    }
  }

  public void AssignTile(Tile t) {
    if (t.room != null) {
      t.room.tiles.Remove(t);
    }
    t.room = this;
    if (!tiles.Contains(t)) {
      tiles.Add(t);
    }
  }


  public void RemoveAllTiles() {
    for (int i = tiles.Count - 1; i >= 0; i -= 1) {
      Tile t = tiles[i];

      WorldController.Instance.world.outside.AssignTile(t);
      tiles.RemoveAt(i);


    }

    tiles = new List<Tile>();
  }

  //-------------------------STATIC FUNCTIONS-----------------
  public static void CalculateRooms(InstalledItem source) {
    Tile tile = source.tile;
    World world = tile.world;
    Room oldRoom = tile.room;

    closedSet.Clear();
    tileQu.Clear();

    ActualFloodFill(world, tile, tile.North, oldRoom);
    ActualFloodFill(world, tile, tile.East, oldRoom);
    ActualFloodFill(world, tile, tile.South, oldRoom);
    ActualFloodFill(world, tile, tile.West, oldRoom);

    if (oldRoom != world.outside) {
      tile.room = world.outside;
      oldRoom.tiles.Remove(tile);
    }




    if (oldRoom != world.outside) {
      if (oldRoom.tiles.Count == 0) {
        world.DeleteRoom(tile.room);
      }


    }


  }

  private static List<Tile> closedSet = new List<Tile>();
  private static Queue<Tile> tileQu = new Queue<Tile>();

  public static void ActualFloodFill(World world, Tile sourceTile, Tile tile, Room oldRoom) {

    if (tile == null) {
      world.outside.AssignTile(sourceTile);
      return;
    }


    if (!OkToAdd(tile, oldRoom)) {
      
      return;
    }


    Room newRoom = new Room(world);
    Room outside = world.outside;
    List<Tile> changedList = new List<Tile>();
    bool assignToOutside = false;

    tileQu.Enqueue(tile);
    while (tileQu.Count > 0) {
      Tile t = tileQu.Dequeue();
      closedSet.Add(t);

      if (t.room == oldRoom) {
        newRoom.AssignTile(t);
        changedList.Add(t);


        if (t.North == null || t.East == null || t.South == null || t.West == null) {
          assignToOutside = true;
        } else {
          AddToTileQu(t.North, oldRoom);
          AddToTileQu(t.East, oldRoom);
          AddToTileQu(t.South, oldRoom);
          AddToTileQu(t.West, oldRoom);
        }

      }
    }
    newRoom.AssignTile(tile);
    if (assignToOutside) {
      outside.AssignList(changedList);
    }

  }

  private static bool OkToAdd(Tile t, Room oldRoom) {
    if (t == null) return false;
    if (closedSet.Contains(t)) return false;

    if (t.room != oldRoom) return false;
    if (t.installedItem != null && t.installedItem.roomEnclosure) return false;

    return true;
  }

  private static void AddToTileQu(Tile t, Room oldRoom) {
    if (OkToAdd(t, oldRoom)) {
      tileQu.Enqueue(t);
    }

  }
}

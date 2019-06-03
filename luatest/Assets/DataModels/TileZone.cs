using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileZone
{

  public List<Tile> tiles;

  public TileZone (List<Tile> tiles) {
    this.tiles = tiles;

    foreach(Tile tile in tiles) {
      tile.SetZone(this);
    }
  }




}

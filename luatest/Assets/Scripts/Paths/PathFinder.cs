using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using NoYouDoIt.TheWorld;

namespace NoYouDoIt.NYDIPaths {
  public static class PathFinder {

    private static Dictionary<Tile, float> dct = new Dictionary<Tile, float>();
    public static void AddTile(Tile tile) {
      float v = 0;
      if (tile != null) {
        //return (installedItem == null && inventoryItem == null && movementFactor > 0.3 && !pendingJob);

        if (tile.installedItem != null) {
          return;
        }

        if (tile.movementFactor == 0) {
          return;
        }

        if (tile.HasPendingWork) {
          v += 10;
        }

        //if (tile.inventoryItem != null) {
        //  v += 5;
        //}
        if (tile.movementFactor < 0.3) {
          v += 50;

        }

        dct[tile] = v;
      }
    }
    public static TilePathAStar FindPath(World world, Tile start, Tile end, bool allowNeighbours, bool preferNeighbours) {

      dct.Clear();
      //float start_time = UnityEngine.Time.realtimeSinceStartup;

      List<Tile> tiles = new List<Tile>();
      Dictionary<Tile, TilePathAStar> paths = new Dictionary<Tile, TilePathAStar>();

      tiles.Add(end);
      dct[end] = 100;

      if (allowNeighbours) {
        //Dictionary<string, Tile> neighbours = new Dictionary<string, Tile>();
        AddTile(end.neighbours[World.NORTH]);
        AddTile(end.neighbours[World.EAST]);
        AddTile(end.neighbours[World.SOUTH]);
        AddTile(end.neighbours[World.WEST]);



      }

      foreach (KeyValuePair<Tile, float> kvp in dct.OrderBy(key => key.Value)) {
        //Debug.Log("kvp:" + kvp.Key + ":" + kvp.Value);
        TilePathAStar pp = new TilePathAStar(world, start, kvp.Key);
        if (pp.foundPath) {
          return pp;
        }

      }

      return null;



      //List<PathAStar> pathsToConsider = new List<PathAStar>();
      //foreach (Tile t in tiles) {

      //  PathAStar pp = new PathAStar(world, start, t);

      //  if (pp.foundPath) {
      //    pathsToConsider.Add(pp);
      //  }

      //}

      //PathAStar path = null;
      //float lowest = 0; // float.PositiveInfinity;
      //foreach (PathAStar p in pathsToConsider) {

      //  if (p != null) {
      //    if (path == null || p.totalCost < lowest) {
      //      path = p;
      //      lowest = p.totalCost;

      //    }

      //  }
      //}

      ////float end_time = UnityEngine.Time.realtimeSinceStartup;
      ////Debug.Log("found a path in " + (end_time - start_time));
      //return path;

    }



  }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public static class PathFinder {


  public static void AddTile(List<Tile> tiles, Tile tile) {
    if (tile != null) {
      if (tile.IsEmpty()) {
        tiles.Add(tile);
      }
    }
  }
  public static PathAStar FindPath(World world, Tile start, Tile end, bool allowNeighbours, bool preferNeighbours) {


    float start_time = UnityEngine.Time.realtimeSinceStartup;
    
    List<Tile> tiles = new List<Tile>();
    Dictionary<Tile, PathAStar> paths = new Dictionary<Tile, PathAStar>();

    tiles.Add(end);

    if (allowNeighbours) {
      //Dictionary<string, Tile> neighbours = new Dictionary<string, Tile>();
      AddTile(tiles, end.neighbours[World.NORTH]);
      AddTile(tiles, end.neighbours[World.EAST]);
      AddTile(tiles, end.neighbours[World.SOUTH]);
      AddTile(tiles, end.neighbours[World.WEST]);



    }



    List<PathAStar> pathsToConsider = new List<PathAStar>();
    foreach (Tile t in tiles) {
      
      PathAStar pp = new PathAStar(world, start, t);

      if (pp.foundPath) {
        pathsToConsider.Add(pp);
      }

    }

    PathAStar path = null;
    float lowest = 0; // float.PositiveInfinity;
    foreach (PathAStar p in pathsToConsider) {

      if (p != null) {
        if (path == null || p.totalCost < lowest) {
          path = p;
          lowest = p.totalCost;

        }

      }
    }

    float end_time = UnityEngine.Time.realtimeSinceStartup;
    //Debug.Log("found a path in " + (end_time - start_time));
    return path;

  }



}


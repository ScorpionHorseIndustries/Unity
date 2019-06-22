using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace NoYouDoIt.NYDIPaths {
  using NoYouDoIt.TheWorld;
  using NoYouDoIt.Utils;
  //public class TilePathEdge : PathEdge<Tile> {
  //  public new PathNode node;
  //  public TilePathEdge(Tile type, PathNode n, float cost) : base(type, n, cost) {

  //  }
  //}

  //public class PathNode : PathNode<Tile> {
  //  public new TilePathEdge[] edges;
  //  public PathNode(Tile data) : base(data) {
  //  }
  //}
  public class TileNodeMap {

    //makes a path finding node map
    //tile = node
    //path between tiles = edge
    World world;
    public int width {
      get {
        return world.width;
      }
    }

    public int height {
      get {
        return world.height;
      }
    }
    PathNode<Tile>[,] nodes;
    Dictionary<Tile, PathNode<Tile>> TilesToNodesDct;
    //Dictionary<PathNode<Tile>, Tile> NodesToTilesDct;

    //public Tile GetTile(PathNode<Tile> p) {
    //  if (NodesToTilesDct.ContainsKey(p)) {
    //    return NodesToTilesDct[p];
    //  } else {
    //    return null;
    //  }
    //}

    public PathNode<Tile> GetNode(Tile t) {
      if (TilesToNodesDct.ContainsKey(t)) {
        return TilesToNodesDct[t];
      } else {
        return null;
      }
    }

    public TileNodeMap(World world) {
      this.world = world;
      nodes = new PathNode<Tile>[world.width, world.height];
      TilesToNodesDct = new Dictionary<Tile, PathNode<Tile>>();
      //NodesToTilesDct = new Dictionary<PathNode<Tile>, Tile>();

      //foreach (TileChunk chunk in world.chunkList) {
      //  for (int x = 0; x < TileChunk.CHUNK_WIDTH; x += 1) {
      //    for (int y = 0; y < TileChunk.CHUNK_HEIGHT; y += 1) {


      //    }
      //  }
      //}

      for (int x = 0; x < width; x += 1) {
        for (int y = 0; y < height; y += 1) {
          Tile t = world.GetTileAt(x, y);

          float mv = t.movementFactor;
          //if (mv > 0) {
          nodes[x, y] = new PathNode<Tile>(t);
          TilesToNodesDct.Add(t, nodes[x, y]);
          //NodesToTilesDct.Add(nodes[x, y], t);
          //}
        }
      }
      int countEdges = 0;
      foreach (Tile tile in TilesToNodesDct.Keys) {
        PathNode<Tile> node = TilesToNodesDct[tile];
        List<PathEdge<Tile>> edges = new List<PathEdge<Tile>>();
        if (tile.neighboursList.Count > 0) {
          foreach (Tile tile_neighbour in tile.neighboursList) {
            if (tile_neighbour != null && tile_neighbour.movementFactor > 0) {


              if (IsClippingCorner(tile, tile_neighbour)) continue;
              float mf = 1.0f / Mathf.Pow(tile_neighbour.movementFactor, 2);
              if (tile_neighbour.HasPendingWork) {
                mf *= 2f;
              }
              PathEdge<Tile> e = new PathEdge<Tile>(tile_neighbour, TilesToNodesDct[tile_neighbour], mf);
              edges.Add(e);
              countEdges += 1;

            }
          }
        }
        node.edges = edges.ToArray();



      }

      //Debug.Log("number of nodes " + nodes.Length);
      ////Debug.Log("number of dct nodes " + TilesToNodesDct.Count);
      //Debug.Log("number of edges = " + countEdges);

      //for (int x = 0; x < width; x += 1) {
      //  for (int y = 0; y < height; y += 1) {
      //    Tile t = world.getTileAt(x, y);
      //  }
      //}
    }

    bool IsClippingCorner(Tile c, Tile n) {

      if (c == null || n == null) return false;
      int td = (int)Funcs.TaxiDistance(c, n);

      if (td == 2) {
        int dx = c.world_x - n.world_x;
        int dy = c.world_y - n.world_y;

        Tile t = world.GetTileIfChunkExists(c.world_x - dx, c.world_y);


        if (t == null || t.movementFactor <= 0.1) {
          return true;
        } else {
          Tile tt = world.GetTileIfChunkExists(c.world_x, c.world_y - dy);
          if (tt == null || tt.movementFactor <= 0.1) {
            return true;
          }
        }



      }
      return false;
    }

  }
}
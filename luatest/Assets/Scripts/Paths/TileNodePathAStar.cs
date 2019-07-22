using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using NoYouDoIt.TheWorld;
using NoYouDoIt.Utils;
namespace NoYouDoIt.NYDIPaths {

  public class TileNode : IHeapItem<TileNode> {
    public Tile t;
    public int gCost;
    public int hCost;
    public int fCost {
      get {
        return gCost + hCost;
      }
    }
    int heapIndex;

    public TileNode(Tile t) {
      this.t = t;
    }

    public int HeapIndex { get => heapIndex; set => heapIndex = value; }

    public int CompareTo(TileNode other) {
      int comp = fCost.CompareTo(other.fCost);
      if (comp == 0) {
        comp = hCost.CompareTo(other.hCost);
      }
      return -comp;
    }
  }


  public class TileNodePathAStar {

    protected World world;
    Tile start, end;
    TileNode startNode, endNode;
    //PathNode<Tile> startNode, endNode;
    //TileNodeMap nodeMap;
    //List<PathNode<Tile><Tile>> path = new List<PathNode<Tile><Tile>>();
    //Queue<PathNode<Tile>> pathQ = new Queue<PathNode<Tile>>();
    Queue<Tile> pathQ = new Queue<Tile>();
    //Dictionary<PathNode<Tile>, Tile> nodesToTile = new Dictionary<PathNode<Tile>, Tile>();

    HashSet<Tile> closed = new HashSet<Tile>();
    HashSet<Tile> openTiles = new HashSet<Tile>();
    Heap<TileNode> open;
    Dictionary<TileNode, TileNode> cameFrom = new Dictionary<TileNode, TileNode>();
    //Dictionary<TileNode, float> gScore = new Dictionary<Tile, float>();
    //Dictionary<TileNode, float> fScore = new Dictionary<Tile, float>();

    public float totalCost { get; private set; }

    public int Length {
      get {
        return pathQ.Count;
      }
    }

    public Tile[] path {
      get {
        return pathQ.ToArray();
      }
    }


    private readonly float VLARGE = Mathf.Pow(10, 10);
    public bool foundPath { get; private set; } = false;

    public TileNodePathAStar(World world, Tile start, Tile end) {
      open = new Heap<TileNode>(world.chunkList.Count * TileChunk.CHUNK_WIDTH * TileChunk.CHUNK_HEIGHT);
      this.world = world;
      this.start = start;
      this.end = end;
      startNode = new TileNode(start);
      endNode = new TileNode(end);
      //if (world.nodeMap == null) {
      //  world.nodeMap = new TileNodeMap(world);
      //}
      //nodeMap = world.nodeMap;
      //startNode = nodeMap.GetNode(start);
      //endNode = nodeMap.GetNode(end);

      //Debug.Log("start == start" + (startNode == startNode));
      //Debug.Log("startNode == endNode" + (startNode == endNode));


      open.Add(startNode);
      openTiles.Add(start);
      startNode.gCost = 0;
      startNode.hCost = Heuristic(start, end);

      //fScore[start] = Heuristic(start, end);
      //gScore[start] = 0;

      foundPath = FindPath();

      //foreach (PathNode<Tile><Tile> pnt in gScore.Keys) {
      //  Debug.Log("g " + pnt.GetHashCode() + ": " + gScore[pnt]);
      //}

      //foreach (PathNode<Tile><Tile> pnt in fScore.Keys) {
      //  Debug.Log("f " + pnt.GetHashCode() + ": " + fScore[pnt]);
      //}

      //foreach (PathNode<Tile><Tile> t in pathQ) {
      //  //Debug.Log("tile: " + t.data.x + "," + t.data.y);
      //}

      closed = null;
      //gScore = null;
      //fScore = null;
      cameFrom = null;
      open = null;




    }



    private void ReconstructPath(TileNode current) {
      List<TileNode> path2 = new List<TileNode>();
      totalCost = 0;
      //Debug.Log("number of items in CameFrom: " + cameFrom.Count);
      path2.Add(current);

      //pathQ.Enqueue(current);
      while (cameFrom.ContainsKey(current)) {
        TileNode hold = current;
        current = cameFrom[current];

        //foreach (Tile e in current.edges) {
        //  if (e.node == hold) {
        //    totalCost += e.cost;
        //    break;
        //  }
        //}
        //pathQ.Enqueue(current);
        path2.Add(current);

      }

      ////put ya thing down flip and reverse it
      for (int i = path2.Count - 1; i >= 0; i -= 1) {

        pathQ.Enqueue(path2[i].t);
      }


    }



    private bool FindPath() {
      int attempts = 0;
      while (open.Count > 0 && attempts < 10000) {
        //Debug.Log("gscore size: " + gScore.Count);
        //Debug.Log("fscore size: " + fScore.Count);
        //Debug.Log("camefrom size: " + cameFrom.Count);
        //Debug.Log("open: " + open.Count);
        //Debug.Log("closed size: " + closed.Count);

        attempts += 1;
        TileNode current = open.RemoveFirst();//LowestFScore();
        openTiles.Remove(current.t);
        closed.Add(current.t);
        //Debug.Log("current = " + current);
        if (current != null) {
          if (current == endNode) {
            //Debug.Log("found a path! from (" + start + ") to (" + end+ ")");
            ReconstructPath(current);
            return true;
          } else {
            //Debug.Log("removing current from open and adding to closed");



            //Debug.Log("checking " + current.edges.Count + " edges");
            foreach (Tile currentEdge in current.t.edges.Keys) {
              int cost = (int)current.t.edges[currentEdge] * 10;

              Tile neighbour = currentEdge;
              //Debug.Log("current edge: " + currentEdge + " " + cost + " nn = " + neighbour);

              if (closed.Contains(neighbour)) {
                //Debug.Log("nn is already in closed");
                continue;
              } else {
                int tg = current.gCost + cost;
                //Debug.Log("tentative G = " + tg);
                if (!openTiles.Contains(neighbour)) {


                }


              }

            }
          }


        } else {
          Debug.LogError("Could not find a node with the lowest F score");
          return false;
        }

      }


      return false;
    }



    //private float GetF(Tile p) {
    //  if (fScore.ContainsKey(p)) {
    //    return fScore[p];
    //  } else {
    //    return Mathf.Infinity;
    //  }
    //}

    //private float GetG(Tile p) {
    //  if (gScore.ContainsKey(p)) {
    //    return gScore[p];
    //  } else {
    //    return Mathf.Infinity;
    //  }
    //}
    //private float Heuristic(Tile start, Tile end) {

    //  Tile A = start.data; // nodeMap.GetTile(start);
    //  Tile B = end.data; // nodeMap.GetTile(start);

    //  return Heuristic(A, B);

    //}

    private int Heuristic(Tile start, Tile end) {
      return GetDistance(start, end);


    }

    private Tile GetNextNode() {
      if (pathQ.Count == 0) return null;
      return pathQ.Dequeue();
    }

    public Tile GetNextTile() {
      Tile pt = GetNextNode();
      if (pt == null) return null;
      return pt;
    }

    public Tile GetCurrentTile() {
      return null;
    }

    int GetDistance(Tile tileA, Tile tileB) {

      int dstX = Mathf.Abs(tileA.world_x - tileB.world_x);
      int dstY = Mathf.Abs(tileA.world_y - tileB.world_y);

      if (dstX > dstY)
        return 14 * dstY + 10 * (dstX - dstY);
      return 14 * dstX + 10 * (dstY - dstX);
    }

  }
}
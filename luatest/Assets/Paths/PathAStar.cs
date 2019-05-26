using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PathAStar {

  protected World world;
  PathNode<Tile> startNode, endNode;
  TileNodeMap nodeMap;
  //List<PathNode<Tile><Tile>> path = new List<PathNode<Tile><Tile>>();
  Queue<PathNode<Tile>> pathQ = new Queue<PathNode<Tile>>();
  Dictionary<PathNode<Tile>, Tile> nodesToTile = new Dictionary<PathNode<Tile>, Tile>();

  List<PathNode<Tile>> closed = new List<PathNode<Tile>>();
  List<PathNode<Tile>> open = new List<PathNode<Tile>>();
  Dictionary<PathNode<Tile>, PathNode<Tile>> cameFrom = new Dictionary<PathNode<Tile>, PathNode<Tile>>();
  Dictionary<PathNode<Tile>, float> gScore = new Dictionary<PathNode<Tile>, float>();
  Dictionary<PathNode<Tile>, float> fScore = new Dictionary<PathNode<Tile>, float>();

  public float totalCost { get; private set; }

  public int Length {
    get {
      return pathQ.Count;
    }
  }

  public PathNode<Tile>[] path {
    get {
      return pathQ.ToArray();
    }
  }


  private readonly float VLARGE = Mathf.Pow(10, 10);
  public bool foundPath { get; private set; } = false;

  public PathAStar(World world, Tile start, Tile end) {
    this.world = world;

    if (world.nodeMap == null) {
      world.nodeMap = new TileNodeMap(world);
    }
    nodeMap = world.nodeMap;
    startNode = nodeMap.GetNode(start);
    endNode = nodeMap.GetNode(end);

    //Debug.Log("start == start" + (startNode == startNode));
    //Debug.Log("startNode == endNode" + (startNode == endNode));


    open.Add(startNode);
    fScore[startNode] = Heuristic(start, end);
    gScore[startNode] = 0;

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
    gScore = null;
    fScore = null;
    cameFrom = null;
    open = null;




  }



  private void ReconstructPath(PathNode<Tile> current) {
    List<PathNode<Tile>> path2 = new List<PathNode<Tile>>();
    totalCost = 0;
    //Debug.Log("number of items in CameFrom: " + cameFrom.Count);
    path2.Add(current);

    //pathQ.Enqueue(current);
    while (cameFrom.ContainsKey(current)) {
      PathNode<Tile> hold = current;
      current = cameFrom[current];

      foreach (PathEdge<Tile> e in current.edges) {
        if (e.node == hold) {
          totalCost += e.cost;
          break;
        }
      }
      //pathQ.Enqueue(current);
      path2.Add(current);

    }

    ////put ya thing down flip and reverse it
    for (int i = path2.Count - 1; i >= 0; i -= 1) {

      pathQ.Enqueue(path2[i]);
    }


  }

  private PathNode<Tile> LowestFScore() {
    float lowest = float.MaxValue;
    PathNode<Tile> r = null;
    foreach (PathNode<Tile> t in open) {
      float s = GetF(t);
      if (r == null || s < lowest) {
        lowest = s;
        r = t;
      }
    }

    return r;
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
      PathNode<Tile> current = LowestFScore();
      //Debug.Log("current = " + current);
      if (current != null) {
        if (current == endNode) {
          //Debug.Log("found a path! from (" + startNode + ") to (" + endNode + ")");
          ReconstructPath(current);
          return true;
        } else {
          //Debug.Log("removing current from open and adding to closed");
          open.Remove(current);
          closed.Add(current);

          //Debug.Log("checking " + current.edges.Length + " edges");
          foreach (PathEdge<Tile> currentNodeEdge in current.edges) {

            PathNode<Tile> neighbour = currentNodeEdge.node;
            //Debug.Log("current edge: " + currentNodeEdge + " nn = " + neighbour);

            if (closed.Contains(neighbour)) {
              //Debug.Log("nn is already in closed");
              continue;
            } else {
              float tg = GetG(current) + currentNodeEdge.cost;
              //Debug.Log("tentative G = " + tg);
              if (!open.Contains(neighbour)) {
                //Debug.Log("Adding " + neighbour + " to open");
                open.Add(neighbour);
              }

              if (tg >= GetG(neighbour)) {
                //Debug.Log("skipping " + tg + " < " + GetG(neighbour));
                continue;
              }
              //Debug.Log("adding " + current + " to cameFrom");
              cameFrom[neighbour] = current;
              gScore[neighbour] = tg;
              fScore[neighbour] = tg + Heuristic(neighbour, endNode) + currentNodeEdge.cost;


            }

          }
        }


      } else {
        //Debug.LogError("Could not find a node with the lowest F score");
        return false;
      }

    }


    return false;
  }



  private float GetF(PathNode<Tile> p) {
    if (fScore.ContainsKey(p)) {
      return fScore[p];
    } else {
      return Mathf.Infinity;
    }
  }

  private float GetG(PathNode<Tile> p) {
    if (gScore.ContainsKey(p)) {
      return gScore[p];
    } else {
      return Mathf.Infinity;
    }
  }
  private float Heuristic(PathNode<Tile> start, PathNode<Tile> end) {

    Tile A = start.data; // nodeMap.GetTile(start);
    Tile B = end.data; // nodeMap.GetTile(start);

    return Heuristic(A, B);

  }

  private float Heuristic(Tile start, Tile end) {
    float r = Funcs.Distance(start, end);

    return r;
  }

  public PathNode<Tile> GetNextNode() {
    if (pathQ.Count == 0) return null;
    return pathQ.Dequeue();
  }

  public Tile GetNextTile() {
    PathNode<Tile> pt = GetNextNode();
    if (pt == null) return null;
    return pt.data;
  }

  public Tile GetCurrentTile() {
    return null;
  }

}
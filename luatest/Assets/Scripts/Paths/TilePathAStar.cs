using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using NoYouDoIt.TheWorld;
using NoYouDoIt.Utils;
namespace NoYouDoIt.NYDIPaths {




  public class TilePathAStar {

    protected World world;
    Tile start, end;
    //PathNode<Tile> startNode, endNode;
    //TileNodeMap nodeMap;
    //List<PathNode<Tile><Tile>> path = new List<PathNode<Tile><Tile>>();
    //Queue<PathNode<Tile>> pathQ = new Queue<PathNode<Tile>>();
    Queue<Tile> pathQ = new Queue<Tile>();
    //Dictionary<PathNode<Tile>, Tile> nodesToTile = new Dictionary<PathNode<Tile>, Tile>();

    HashSet<Tile> closed = new HashSet<Tile>();
    HashSet<Tile> open = new HashSet<Tile>();
    Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
    Dictionary<Tile, float> gScore = new Dictionary<Tile, float>();
    Dictionary<Tile, float> fScore = new Dictionary<Tile, float>();

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

    public TilePathAStar(World world, Tile start, Tile end) {
      this.world = world;
      this.start = start;
      this.end = end;
      //if (world.nodeMap == null) {
      //  world.nodeMap = new TileNodeMap(world);
      //}
      //nodeMap = world.nodeMap;
      //startNode = nodeMap.GetNode(start);
      //endNode = nodeMap.GetNode(end);

      //Debug.Log("start == start" + (startNode == startNode));
      //Debug.Log("startNode == endNode" + (startNode == endNode));


      open.Add(start);
      fScore[start] = Heuristic(start, end);
      gScore[start] = 0;

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



    private void ReconstructPath(Tile current) {
      List<Tile> path2 = new List<Tile>();
      totalCost = 0;
      //Debug.Log("number of items in CameFrom: " + cameFrom.Count);
      path2.Add(current);

      //pathQ.Enqueue(current);
      while (cameFrom.ContainsKey(current)) {
        Tile hold = current;
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

        pathQ.Enqueue(path2[i]);
      }


    }

    private Tile LowestFScore() {
      float lowest = float.MaxValue;
      Tile r = null;
      foreach (Tile t in open) {
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
        Tile current = LowestFScore();
        //Debug.Log("current = " + current);
        if (current != null) {
          if (current == end) {
            //Debug.Log("found a path! from (" + start + ") to (" + end+ ")");
            ReconstructPath(current);
            return true;
          } else {
            //Debug.Log("removing current from open and adding to closed");
            open.Remove(current);
            closed.Add(current);

            //Debug.Log("checking " + current.edges.Count + " edges");
            foreach (Tile currentEdge in current.edges.Keys) {
              float cost = current.edges[currentEdge];

              Tile neighbour = currentEdge;
              //Debug.Log("current edge: " + currentEdge + " " + cost + " nn = " + neighbour);

              if (closed.Contains(neighbour)) {
                //Debug.Log("nn is already in closed");
                continue;
              } else {
                float tg = GetG(current) + cost;
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
                fScore[neighbour] = tg + Heuristic(neighbour, end) + cost;


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



    private float GetF(Tile p) {
      if (fScore.ContainsKey(p)) {
        return fScore[p];
      } else {
        return Mathf.Infinity;
      }
    }

    private float GetG(Tile p) {
      if (gScore.ContainsKey(p)) {
        return gScore[p];
      } else {
        return Mathf.Infinity;
      }
    }
    //private float Heuristic(Tile start, Tile end) {

    //  Tile A = start.data; // nodeMap.GetTile(start);
    //  Tile B = end.data; // nodeMap.GetTile(start);

    //  return Heuristic(A, B);

    //}

    private float Heuristic(Tile start, Tile end) {
      float r = Funcs.Distance(start, end);

      return r;
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

  }
}
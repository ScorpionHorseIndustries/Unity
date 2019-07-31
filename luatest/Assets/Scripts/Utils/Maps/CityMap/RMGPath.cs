using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NoYouDoIt.Utils.Maps.CityMap {
  public class RMGPath {
    List<RMGNode> open = new List<RMGNode>();
    HashSet<RMGNode> closed = new HashSet<RMGNode>();

    Dictionary<RMGNode, RMGNode> cameFrom = new Dictionary<RMGNode, RMGNode>();
    RMGNode[,] nodes;
    RMGNode start, end;
    public List<RMTile> path;
    RoadMapGen map;
    readonly int width, height;
    public RMGPath(RoadMapGen map, RMTile start, RMTile end) {
      this.map = map;
      this.width = map.width;
      this.height = map.height;
      nodes = new RMGNode[width, height];
      for (int x = 0; x < width; x += 1) {
        for (int y = 0; y < height; y += 1) {
          nodes[x, y] = new RMGNode(map, x, y);
        }
      }

      for (int x = 0; x < width; x += 1) {
        for (int y = 0; y < height; y += 1) {
          RMGNode n = nodes[x, y];

          n.neighbours[0] = getNode(x - 1, y);
          n.neighbours[1] = getNode(x + 1, y);
          n.neighbours[2] = getNode(x, y - 1);
          n.neighbours[3] = getNode(x, y + 1);
        }
      }



      this.start = getNode(start.x, start.y);
      this.end = getNode(end.x, end.y);

      //Debug.Log("end = " + end);
    }

    RMGNode getNode(int x, int y) {
      if (!map.OOB(x, y)) {
        RMGNode n = nodes[x, y];
        if (n.moveFactor == 0) {
          return null;
        }
        return n;
      }
      return null;
    }

    public bool findPath() {

      if (start == null || end == null) return false;
      open.Add(start);
      start.gCost = 0;
      start.hCost = qDist(start, end);
      while (open.Count > 0) {
        RMGNode current = open[0];

        //find lowest f
        for (int i = 1; i < open.Count; i += 1) {
          RMGNode o = open[i];
          if (o.fCost < current.fCost || (o.fCost == current.fCost && o.hCost < current.hCost)) {
            current = o;
          }

        }

        open.Remove(current);
        closed.Add(current);

        if (current == null) {
          return false;
        } else {
          if (current == end) {
            //Debug.Log("found path!");
            retracePath();
            return true;
          } else {
            //Debug.Log("current = " + current);
            for (int i = 0; i < current.neighbours.Length; i += 1) {
              RMGNode n = current.neighbours[i];

              if (n != null) {
                //Debug.Log("neighbour = " + n);
                if (n.moveFactor <= 0 || closed.Contains(n)) {
                  continue;
                }

                int newmc = (int)(current.gCost + qDist(current, n) + heur(current, n));

                if (newmc < n.gCost || !open.Contains(n)) {
                  n.gCost = newmc;
                  n.hCost = qDist(n, end);
                  n.parent = current;
                  cameFrom[n] = current;
                }
                if (!open.Contains(n))
                  open.Add(n);
              }
            }
          }


        }



      }


      return false;
    }

    float heur(RMGNode a, RMGNode b) {
      float h = a.tile.h - b.tile.h;
      return -h * 100;
    }

    void retracePath() {
      List<RMGNode> npath = new List<RMGNode>();

      RMGNode current = end;

      while (current != start) {
        npath.Add(current);
        current = current.parent;
      }
      npath.Reverse();

      path = new List<RMTile>();
      for (int i = 0; i < npath.Count; i += 1) {
        path.Add(map.GetTile(npath[i].x, npath[i].y));
      }
    }

    public static int qDist(RMGNode a, RMGNode b) {

      return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
  }




  public class RMGNode {
    public RMTile tile;
    public RMGNode parent;
    public int gCost = int.MaxValue;
    public int hCost = int.MaxValue;
    public float moveFactor = 1;

    public float fCost {
      get { return gCost + hCost; }
    }

    public override string ToString() {
      return tile.ToString() + " node:" + moveFactor + " g:" + gCost + " h:" + hCost + " f:" + fCost;
    }
    public RMGNode[] neighbours = new RMGNode[4];

    public int x, y;

    public RMGNode(RoadMapGen map, int x, int y) {
      this.x = x;
      this.y = y;

      this.tile = map.GetTile(x, y);
      moveFactor = tile.GetMovementFactor();
    }

    void setNeighbours() {
    }
  }
}

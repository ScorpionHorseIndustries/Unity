using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoYouDoIt.Utils.Maps.CityMap {
  public class RMTile {


    public RoadMapGen map;
    public TYPE showsAs;
    public TYPE markedAs;
    public BUILDING_TYPE btype = BUILDING_TYPE.NONE;
    public int h = 0;

    public readonly int x, y;
    public int d = 0;
    public float fd = 0;
    public Dictionary<RMTile, float> neighbours = new Dictionary<RMTile, float>();

    public override string ToString() {
      return "tile:(" + x + "," + y + ")";
    }

    public float GetMovementFactor() {
      if (markedAs == TYPE.ROAD) return 1;
      if (markedAs == TYPE.BUILDING) return 0;

      return 1 / (h + 1);
    }


    public void SetNeighbours() {
      neighbours.Clear();
      RMTile t = map.GetTile(x - 1, y);
      if (t != null) {
        neighbours.Add(t, t.GetMovementFactor());
      }
      t = map.GetTile(x + 1, y);
      if (t != null) {
        neighbours.Add(t, t.GetMovementFactor());
      }
      t = map.GetTile(x, y - 1);
      if (t != null) {
        neighbours.Add(t, t.GetMovementFactor());
      }
      t = map.GetTile(x, y + 1);
      if (t != null) {
        neighbours.Add(t, t.GetMovementFactor());
      }

    }

    public RMTile(RoadMapGen map, int x, int y) {
      this.x = x;
      this.y = y;
      this.map = map;
    }

    public void Update() {
      d = map.IsObstacle(x, y) ? -1 : 0;
      fd = 0;

    }
  }
}

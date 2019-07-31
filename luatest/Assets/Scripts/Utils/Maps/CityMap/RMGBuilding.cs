using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoYouDoIt.Utils.Maps.CityMap {
  public class RMGBuilding {
    public int tlx, tly;
    public int brx, bry;
    public int xDir, yDir;
    public int area;

    public List<RMTile> tiles = new List<RMTile>();

    public RMGBuilding(int tlx, int tly, int brx, int bry, int xDir, int yDir) {
      this.tlx = tlx;
      this.tly = tly;
      this.brx = brx;
      this.bry = bry;
      this.yDir = yDir;
      this.xDir = xDir;

      area = (tlx - brx) * (tly - bry);
    }
  }
}

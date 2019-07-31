using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoYouDoIt.Utils.Maps.CityMap.Rivers {
  using NoYouDoIt.Controller;
  using UnityEngine;
  public class RMGRiver {
    public int maxSplits = 4;
    public int splits = 0;
    public Texture2D tex;

    RMGRiverNode start;
    public RoadMapGen map;
    public readonly float x, y;
    public readonly int w, h;
    public readonly int resizeFactor = 10;
    public readonly float facing;
    public int nodeCount = 0;
    private int noChangeCount = 100;
    public bool finished { get; private set; } = false;
    public readonly float minAngle, maxAngle;
    public int iterations { get; private set; } = 0;
    public int leftChildren = 0;
    public int rightChildren = 0;
    public RMGRiver(RoadMapGen map, float x, float y, int w, int h, int resize, float facing) {
      this.resizeFactor = resize;
      this.map = map;
      this.x = x * resizeFactor;
      this.y = y * resizeFactor;
      this.w = w * resizeFactor;
      this.h = h * resizeFactor;
      tex = new Texture2D(this.w, this.h, NYDISpriteManager.DEFAULT_TEXTURE_FORMAT, false);
      
      this.facing = facing;
      this.minAngle = facing - (Mathf.PI / 2.0f) - (Mathf.Deg2Rad * 10);
      this.maxAngle = facing + (Mathf.PI / 2.0f) + (Mathf.Deg2Rad * 10);

    }

    public void Update() {
      iterations += 1;
      int countAtStart = nodeCount;
      if (start == null) {
        start = new RMGRiverNode(this, this.facing, this.x, this.y);
        return;
      } else {
        start.Update();
      }
      if (countAtStart == nodeCount) {
        noChangeCount -= 1;
      }

      if (noChangeCount <= 0) {
        finished = true;
      }
    }

    public void Draw() {
      if (start != null) {
        start.Draw();
      }
      tex.Apply();

      RoadMapGen.SaveTexToPng("rivers", tex);
      Debug.Log("finished river with " + nodeCount + " node(s), it took " + iterations + " iteration(s), children L/R: " + leftChildren + "/" + rightChildren);
    }
  }
}

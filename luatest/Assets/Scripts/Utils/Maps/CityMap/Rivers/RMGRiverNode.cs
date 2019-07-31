using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace NoYouDoIt.Utils.Maps.CityMap.Rivers {
  public class RMGRiverNode {

    public RMGRiverNode parent = null, childLeft = null, childRight = null;
    public RMGRiver river;
    float facing;
    public float x, y;

    bool alive = true;
    float splitChance = 0.1f;
    float deathChance = 0.01f;
    bool canHaveLeft = true;
    bool canHaveRight = true;


    float w = 0;
    float narrowChance = 1f;
    float widenChance = 0.5f;



    public RMGRiverNode(RMGRiver river, float facing, float x, float y) {
      this.river = river;
      this.facing = facing;
      this.x = x;
      this.y = y;
      this.river.nodeCount += 1;
      this.w = 4 + (this.river.resizeFactor / 2);
    }


    public RMGRiverNode(RMGRiverNode parent, float dir, bool doubleSplit) {
      this.parent = parent;
      this.river = parent.river;
      this.river.nodeCount += 1;
      this.facing = parent.facing + (Funcs.Random(0.01f,Mathf.PI/4) * dir);
      this.facing = Mathf.Clamp(this.facing, this.river.minAngle, this.river.maxAngle);
      this.deathChance = parent.deathChance * Funcs.Random(0.99f, 1.01f);
      this.splitChance = parent.splitChance * Funcs.Random(0.99f, 1.01f);
      this.narrowChance = parent.narrowChance * Funcs.Random(0.99f, 1.01f);
      this.widenChance = parent.widenChance* Funcs.Random(0.99f, 1.01f);
      this.w = parent.w;
      if (Funcs.fChance(narrowChance)) {
        this.w -= 1;
      } else if (Funcs.fChance(widenChance)) {
        this.w += 0.5f;
      }
      if (doubleSplit) {
        this.w -= 1;
        deathChance += 0.01f;
        splitChance -= 0.1f;
      }


      Vector2 p = new Vector2(parent.x, parent.y);
      Vector2 pa = Funcs.RadianToVector2(this.facing);
      pa *= 3;
      p += pa;
      this.x = p.x;
      this.y = p.y;



    }

    public void Update() {

      if (!alive) return;
      if (this.w <= 0) {
        alive = false;
        Debug.Log("too narrow");
      }
      if (x < 0 || x > river.w || y < 0 || y > river.h) {
        alive = false;
      }



      if (childLeft == null && childRight == null) {
        if (Funcs.fChance(deathChance)) {
          //alive = false;
        } else {


          if (Funcs.fChance()) {
            childLeft = new RMGRiverNode(this, -1, false);
            canHaveLeft = false;
            river.leftChildren += 1;
          } else {
            childRight = new RMGRiverNode(this, 1, false);
            canHaveRight = false;
            river.rightChildren += 1;
          }
        }
        return;
      }

      if (river.splits >= river.maxSplits) {
        canHaveLeft = canHaveRight = false;
      }

      if (childLeft != null) {
        childLeft.Update();
        if (canHaveRight) {
          canHaveRight = false;
          if (Funcs.fChance(splitChance)) {
            childRight = new RMGRiverNode(this, 1, true);
            river.splits += 1;
            river.rightChildren += 1;
            return;
          } else {
          }
        }
      }

      if (childRight != null) {
        childRight.Update();


        if (canHaveLeft) {
          canHaveLeft = false;
          if (Funcs.fChance(splitChance)) {
            childLeft = new RMGRiverNode(this, -1, true);
            river.splits += 1;
            canHaveLeft = false;
            river.leftChildren += 1;
            return;
          }
        }
      }
    }

    public void Draw() {
      if (childLeft != null) {
        MakeWide(x, y, childLeft.x, childLeft.y, childLeft.w);
        childLeft.Draw();
      }

      if (childRight != null) {
        MakeWide(x, y, childRight.x, childRight.y, childRight.w);
        childRight.Draw();
      }
    }

    void MakeWide(float x1, float y1, float x2, float y2, float w) {
      Vector2 a = new Vector2(x1, y1);
      Vector2 b = new Vector2(x2, y2);
      Vector2 c = b - a;
      c.Normalize();
      c.Set(-c.y, c.x);
      Vector2 d = new Vector2(c.x, c.y);
      d *= -1;
      c *= w;
      d *= w;

      for (float t = 0; t < 1; t += 0.1f) {
        Vector2 aa = new Vector2(a.x, a.y);
        Vector2 ab = new Vector2(a.x, a.y);
        aa += c * t;
        ab += d * t;

        river.tex.SetPixel((int)aa.x, (int)aa.y, Color.black);
        river.tex.SetPixel((int)ab.x, (int)ab.y, Color.black);


        int xx1 = (int)aa.x / river.resizeFactor;
        int yy1 = (int)aa.y / river.resizeFactor;
        int xx2 = (int)ab.x / river.resizeFactor;
        int yy2 = (int)ab.y / river.resizeFactor;


        RMTile tla = river.map.GetTile(xx1, yy1);
        RMTile tlb = river.map.GetTile(xx2, yy2);

        if (tla != null) {
          if (tla.showsAs != TYPE.ROAD && tla.markedAs != TYPE.BUILDING) {
            tla.markedAs = TYPE.WATER;
            tla.showsAs = TYPE.WATER;
          }
        }

        if (tlb != null) {
          if (tlb.showsAs != TYPE.ROAD && tlb.markedAs != TYPE.BUILDING) {
            tlb.markedAs = TYPE.WATER;
            tlb.showsAs = TYPE.WATER;
          }
        }
      }
    }

  }
}

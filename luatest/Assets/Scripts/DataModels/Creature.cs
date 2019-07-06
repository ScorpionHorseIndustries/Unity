using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NoYouDoIt.DataModels {
  using NoYouDoIt.TheWorld;
  using NoYouDoIt.NYDIPaths;
  using NoYouDoIt.Utils;
  using NoYouDoIt.Controller;
  class Creature {
    public string state;
    public string NewState;
    public string type { get; private set; }
    public Creature proto { get; private set; }
    public ItemParameters info;
    public string name { get; private set; }
    public Tile pos { get; private set; }
    public Tile dst { get; private set; }
    public TileOccupant occupier;
    public float moveProgress { get; private set; }
    private Vector2 vPos = new Vector2();
    private TilePathAStar path;
    private Action<Creature> CBOnChanged;
    private float waitTimer;
    public string spriteName { get; private set; }
    public string onUpdate { get; private set; } = "OnUpdate_Creature";

    public int Xint {
      get {
        return pos.world_x;
      }
    }

    public int Yint {
      get {
        return pos.world_y;
      }
    }
    public float X {
      get {
        return Mathf.Lerp(pos.world_x, dst.world_x, moveProgress);
      }
    }
    public float Y {
      get {
        return Mathf.Lerp(pos.world_y, dst.world_y, moveProgress);
      }
    }
    public Vector2 position {
      get {
        vPos.Set(X, Y);
        return vPos;
      }
    }

    public static Creature MakeCreature(Tile t, Creature proto) {
      Creature nc = proto.MemberwiseClone() as Creature;
      nc.name = World.current.GetName();
      nc.occupier = new TileOccupant(nc.name, proto.type);
      nc.occupier.CBPleaseMove += nc.PleaseMove;
      nc.info = new ItemParameters(proto.info);


      return nc;
    }

    private Creature() {

    }

    public void PleaseMove() {
      if (state == "idle") {
        NewState = "wander";
        state = "wander";
      }
    }

    public bool SetDst(Tile tile) {
      if (tile != null && tile.movementFactor > 0 && (tile.IsNeighbour(pos, true) || tile == pos)) {
        dst = tile;
        return true;
      } else {
        //Debug.Log("Character SetDestination was given a tile that was not a neighbour of the current tile...");
        return false;
      }



    }

    public bool SetPos(Tile t) {
      if (t != pos) {
        if (pos != null) {
          //leave the tile
          pos.Leave(occupier);

        }
        pos = t;
        pos.Enter(occupier);
        return true;


      }
      return false;

    }


    public bool FindPath(Tile end, bool neighboursOk) {

      path = PathFinder.FindPath(World.current, pos, end, neighboursOk, neighboursOk);
      if (path != null && path.foundPath) {
        return true;
      } else {
        path = null;
        return false;
      }

    }


    public void CBRegisterOnChanged(Action<Creature> cb) {
      CBOnChanged -= cb;
      CBOnChanged += cb;

    }

    public void CBUnregisterOnChanged(Action<Creature> cb) {
      CBOnChanged -= cb;
    }


    public void Move(float deltaTime) {
      if (pos.movementFactor == 0 || dst.movementFactor == 0) {
        NewState = "find_new_path";
        SetPos(World.current.FindEmptyTile_NotThisOne(pos));
        SetDst(pos);

      }


      if (pos != dst) {
        float distToTravel = Funcs.Distance(pos, dst);
        bool never = false, soon = false;
        switch (dst.CanEnter(occupier)) {
          case Tile.CAN_ENTER.YES:
            break;
          case Tile.CAN_ENTER.NEVER:
            never = true;
            break;
          case Tile.CAN_ENTER.SOON:
            soon = true;
            break;
          default:
            break;
        }

        if (never) {
          NewState = "find_new_path";
        } else if (soon) {
          //wait
          dst.PleaseMove();
          waitTimer += deltaTime;

          if (waitTimer > 3) {
            NewState = "find_new_path";
            waitTimer = 0;
          }

        } else {


          float speed = info.GetFloat("movement_speed");

          speed *= Mathf.Clamp(dst.movementFactor, 0.2f, 1.5f);

          float distThisFrame = speed * deltaTime;

          moveProgress += (distThisFrame / distToTravel);

          if (moveProgress >= 1) {
            SetPos(dst);
            moveProgress = 0;
          }
        }
      } else {
        moveProgress = 0;
      }
    }


  }

  //-------------------------STATIC--------------------

}

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
  public class Robot {

    Action<Robot> CBOnChanged;



    public string state;
    public string typeName { get; private set; }
    public RobotType proto { get; private set; }

    public ItemParameters info;
    public string name { get; private set; }
    public readonly string uuid = Guid.NewGuid().ToString();
    public Tile pos { get; private set; }
    public Tile dst { get; private set; }
    private float moveProgress = 0;

    private Vector2 vPos = new Vector2();
    private Vector2 vDst = new Vector2();
    private Vector2 vJob = new Vector2();

    PathAStar currentPath;
    string currentInstruction;
    public WorkItem work;
    public Inventory inventory;
    public string spriteName { get; private set; }
    string OnUpdate = "OnUpdate_Robot";

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

    public void Say(string s) {
      WorldController.Instance.SpawnText(s, Xint, Yint);
    }



    public void PleaseMove(Robot asker) {
      throw new NotImplementedException();
    }


    private Robot() {
      throw new Exception("invalid constructor used");
    }
    private Robot(Tile tile, RobotType proto) {
      inventory = new Inventory(World.current, 1, INVENTORY_TYPE.ROBOT, this);
      name = World.current.GetName();
      info = new ItemParameters();
      info.SetFloat("movement_speed", 3);
      this.typeName = proto.typeName;
      this.spriteName = proto.spriteName;
      this.proto = proto;
      this.SetPos(tile);
      this.SetDst(tile);


    }
    public static Robot MakeRobot(Tile t, string typeName) {



      if (t.IsEmpty() && t.countOfOccupied == 0) {
        RobotType proto = RobotType.GetPrototype(typeName);
        if (proto != null) {
          Robot robot = new Robot(t, proto);
          robot.pos = t;
          robot.dst = t;

          return robot;
        } else {
          Debug.LogError("could not find robot type of name [" + typeName + "]");
        }

        return null;
      }

      return null;

    }

    public bool GiveInstruction(string instruction) {

      if (currentInstruction == null) {
        currentInstruction = instruction;
        return true;
      } else {
        return false;
      }
    }

    public void Update(float deltaTime) {
      float old = (X * 11) + (Y * 3);
      World.CallLuaFunction(OnUpdate, this, deltaTime);

      float n = (X * 11) + (Y * 3);

      if (old != n) {
        if (CBOnChanged != null) {
          CBOnChanged(this);
        }
      }

    }

    public void Work(float deltaTime) {
      
    }

    public bool PlaceItemAtJob() {
      if (work != null) {
        string holding = work.inventoryItemName;
        int holdingQty = inventory.HowMany(holding);
        int placed = work.inventory.AddItem(holding, holdingQty);
        inventory.RemoveItem(holding, placed);
        work.inventoryItemQtyRemaining -= placed;
        return true;
      }
      return false;
    }

    public bool PlaceItem(Inventory inventory, string itemName, int qty) {



      return false;
    }

    public bool Pickup(Tile t, string itemName, int qty) {

      if (t == pos || t.IsNeighbour(pos, false)) {
        int qtyTaken = t.RemoveFromInventory(itemName, qty);
        int qtyAdded = inventory.AddItem(itemName, qty);
        if (qtyAdded < qtyTaken) {
          t.AddToInventory(itemName, qtyTaken - qtyAdded);

        }
        return true;

      }

      return false;



    }

    public void GetWork() {
      if (work == null) {
        work = World.current.workManager.GetNearestWork(pos);
        if (work != null) {
          work.Assign(this);
          
        }
      }
    }

    public void Move(float deltaTime) {
      if (pos != dst) {
        float distToTravel = Funcs.Distance(pos, dst);
        float speed = info.GetFloat("movement_speed");

        speed *= Mathf.Clamp(dst.movementFactor, 0.2f, 1.5f);

        float distThisFrame = speed * deltaTime;

        moveProgress += (distThisFrame / distToTravel);

        if (moveProgress >= 1) {
          SetPos(dst);
          moveProgress = 0;
        }
      } else {
        moveProgress = 0;
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
          pos.Leave(this);

        }
        pos = t;
        pos.Enter(this);
        return true;


      }
      return false;

    }

    public bool FindPath(int x, int y, bool neighboursOk) {
      Tile end = World.current.GetTileAt(x, y);
      if (end == null) return false;

      path = PathFinder.FindPath(World.current, pos, end, neighboursOk, neighboursOk);
      if (path != null && path.foundPath) {
        return true;
      } else {
        path = null;
        return false;
      }
    }

    public Tile target { get; private set; } = null;
    public TilePathAStar path;
    public bool FindPath(Tile end, bool neighboursOk) {
      //findNewPath = false;
      //Debug.Log(name + " Finding a path...");
      //pathAttempts = 0;
      path = PathFinder.FindPath(World.current, pos, end, neighboursOk, neighboursOk);
      if (path != null && path.foundPath ) {
        return true;
      } else {
        path = null;
        return false;
      }

      //if (target != null) {
      //  if (myJob != null && myJob.jobType != JOB_TYPE.WORK_AT_STATION) {
      //    path = PathFinder.FindPath(world, PosTile, target, true, true);
      //  } else {
      //    path = new TilePathAStar(world, PosTile, target);
      //  }
      //  if (path != null && path.foundPath) {
      //    state = STATE.MOVE;
      //  } else {
      //    state = STATE.RESET;
      //  }
      //} else {
      //  state = STATE.RESET;
      //}
    }


    public void CBRegisterOnChanged(Action<Robot> cb) {
      CBOnChanged += cb;

    }

    public void CBUnregisterOnChanged(Action<Robot> cb) {
      CBOnChanged -= cb;
    }
  }
}

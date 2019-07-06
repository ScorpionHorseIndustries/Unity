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
  using System.IO;
  using Newtonsoft.Json.Linq;

  public class Robot {

    Action<Robot> CBOnChanged;

    public TileOccupant occupier;
    private float waitTimer;
    public string state;
    public string NewState = null;
    public string typeName { get; private set; }
    public Robot proto { get; private set; }

    public ItemParameters info;
    public string name { get; private set; }
    public readonly string uuid = Guid.NewGuid().ToString();
    public Tile pos { get; private set; }
    public Tile dst { get; private set; }
    private float moveProgress = 0;

    private Vector2 vPos = new Vector2();


    //PathAS currentPath;


    public Tile target { get; private set; } = null;
    public TilePathAStar path;
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



    public void PleaseMove() {
      if (state == "idle") {
        NewState = "wander";
        state = "wander";
      }
    }


    private Robot() {
      
    }
    private Robot(Tile tile, Robot proto) {
      inventory = new Inventory(World.current, 1, INVENTORY_TYPE.ROBOT, this);
      name = World.current.GetName();
      info = new ItemParameters(proto.info);
      info.SetFloat("movement_speed", 5);
      this.typeName = proto.typeName;
      this.spriteName = proto.spriteName;
      this.proto = proto;
      this.SetPos(tile);
      this.SetDst(tile);
      occupier = new TileOccupant(name, proto.typeName);
      occupier.CBPleaseMove += PleaseMove;



    }

    public void ReturnWork() {
      World.current.workManager.ReturnWork(work);
    }

    private void DropAll() {

      while (!inventory.IsEmpty()) {
        Tile t = World.current.FindEmptyTile(pos);
        if (t != null) {
          string itemName = inventory.GetFirst();
          int qtyToPlace = inventory.HowMany(itemName);
          int qtyPlaced = t.AddToInventory(itemName, qtyToPlace);
          inventory.RemoveItem(itemName, qtyPlaced);
        } else {
          break;
        }
      }
    }

    public static Robot MakeRobot(Tile t, string typeName) {



      if (t.IsEmpty() && t.countOfOccupied == 0) {
        Robot proto = GetPrototype(typeName);
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
      World.CallLuaFunctions(work.OnWork.ToArray(), this, deltaTime);
    }

    public bool PlaceItemAtJob() {
      if (work != null) {
        string holding = work.inventoryItemName;
        int holdingQty = inventory.HowMany(holding);
        int placed = work.inventory.AddItem(holding, holdingQty);
        inventory.RemoveItem(holding, placed);
        work.inventoryItemQtyRemaining -= placed;
        //Debug.Log("robot.PlaceItemAtJob: " + work);
        return true;
      }
      return false;
    }

    public bool PlaceItemOnTile() {
      if (work != null) {
        string holding = work.inventoryItemName;
        int holdingQty = inventory.HowMany(holding);
        int placed = work.workTile.AddToInventory(holding, holdingQty);
        inventory.RemoveItem(holding, placed);
        work.inventoryItemQtyRemaining -= placed;
      }

      return false;
    }

    public bool PlaceItem(Inventory inventory, string itemName, int qty) {



      return false;
    }

    public bool Pickup(Tile t, string itemName, int qty) {

      if (t == pos || t.IsNeighbour(pos, false)) {
        int qtyTaken = t.RemoveFromInventory(itemName, qty);
        //Debug.Log("qty actually removed:" + qtyTaken);
        if (qtyTaken == 0) return false;

        int qtyAdded = inventory.AddItem(itemName, qtyTaken);
        if (qtyAdded < qtyTaken) {
          t.AddToInventory(itemName, qtyTaken - qtyAdded);

        }
        return true;

      }

      return false;



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

    public bool FindPath(Tile end, bool neighboursOk) {

      path = PathFinder.FindPath(World.current, pos, end, neighboursOk, neighboursOk);
      if (path != null && path.foundPath) {
        return true;
      } else {
        path = null;
        return false;
      }

    }


    public void CBRegisterOnChanged(Action<Robot> cb) {
      CBOnChanged += cb;

    }

    public void CBUnregisterOnChanged(Action<Robot> cb) {
      CBOnChanged -= cb;
    }
    //-------------------------------- STATIC- -------------------------------------
    private static Dictionary<string, Robot> prototypes = new Dictionary<string, Robot>();

    public static void LoadFromFile() {
      string path = Path.Combine(Application.streamingAssetsPath, "data", "RobotTypes");

      string[] files = Directory.GetFiles(path, "*.json");

      foreach (string file in files) {
        string fcontents = File.ReadAllText(file);
        JObject json = JObject.Parse(fcontents);

        CreateRobotPrototype(json);
      }
    }

    private static void CreateRobotPrototype(JObject json) {
      Robot proto = new Robot();
      proto.name = "prototype";
      proto.typeName = Funcs.jsonGetString(json["name"], null);
      proto.spriteName = Funcs.jsonGetString(json["spriteName"], null);
      proto.info = new ItemParameters();

      prototypes.Add(proto.typeName, proto);
    }

    private static Robot GetPrototype(string typeName) {
      if (prototypes.ContainsKey(typeName)) {
        return prototypes[typeName];
      } else {
        return null;
      }
    }
  }





}

﻿using System;
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

  public class Entity {

    Action<Entity> CBOnChanged;

    //public TileOccupant occupier;
    private float waitTimer;
    public string state;
    public string NewState = null;
    public string typeName { get; private set; }
    public Entity proto { get; private set; }
    public string getNameType { get; private set; }

    public ItemParameters info;
    public string name { get; private set; }
    public readonly string uuid = Guid.NewGuid().ToString();
    public Tile pos { get; private set; }
    public Tile dst { get; private set; }
    private float moveProgress = 0;

    private Vector2 vPos = new Vector2();
    public string facing { get; private set; } = World.SOUTH;
    public string prevFacing { get; private set; } = World.SOUTH;

    public bool directionChanged { get; set; } = false;

    public Dictionary<string, string> facingSprites;

    public Dictionary<string, NYDIAnimation> animations;
    public NYDIAnimator animator;



    //PathAS currentPath;


    public Tile target { get; private set; } = null;
    public TilePathAStar path;
    string currentInstruction;
    public WorkItem work;
    public Inventory inventory;
    public string spriteName { get; private set; }
    string OnUpdate = null;//"OnUpdate_Entity";

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

    private float pX, pY;
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


    private Entity() {

    }
    private Entity(Tile tile, Entity proto) {
      inventory = new Inventory(World.current, 1, INVENTORY_TYPE.ENTITY, this);

      this.getNameType = proto.getNameType;

      switch (proto.getNameType) {
        case "pet":
          name = World.current.GetPetName();
          break;
        case "robot":
          name = World.current.GetRobotName();
          break;
        default:
          name = World.current.GetName();
          break;
      }

      info = new ItemParameters(proto.info);

      this.typeName = proto.typeName;
      this.spriteName = proto.spriteName;
      this.animations = proto.animations;
      this.proto = proto;

      this.SetPos(tile);
      this.SetDst(tile);
      //this.occupier = new TileOccupant(name, proto.typeName);
      //this.occupier.CBPleaseMove += PleaseMove;
      this.facingSprites = proto.facingSprites;
      this.OnUpdate = proto.OnUpdate;
      this.animator = new NYDIAnimator(proto.animations);



    }

    public void ReturnWork() {
      World.current.workManager.ReturnWork(work);
    }

    public  void DropAll() {

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

    public static Entity MakeEntity(Tile t, string typeName) {



      if (t.IsEmpty() && t.countOfOccupied == 0) {
        Entity proto = GetPrototype(typeName);
        if (proto != null) {
          Entity entity = new Entity(t, proto);
          entity.pos = t;
          entity.dst = t;

          return entity;
        } else {
          Debug.LogError("could not find entity type of name [" + typeName + "]");
        }

        return null;
      }

      return null;

    }



    //public bool GiveInstruction(string instruction) {

    //  if (currentInstruction == null) {
    //    currentInstruction = instruction;
    //    return true;
    //  } else {
    //    return false;
    //  }
    //}

    public void Update(float deltaTime) {
      float old = (X * 11) + (Y * 3);
      string oldState = state;
      World.CallLuaFunction(OnUpdate, this, deltaTime);

      float n = (X * 11) + (Y * 3);
      if (animator.active) {
        animator.Update(deltaTime);
      }
      if (old != n || oldState != state || animator.changed) {

        if (animator.valid) {
          if (oldState != state) {
            switch (state) {
              case "idle":
                animator.Set("idle");
                break;
              case "move":
                //if (directionChanged) {
                animator.Set(facing);
                //}
                break;
              default:
                break;
            }
          }
        }

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
      pX = X;
      pY = Y;

      if (pos.movementFactor == 0 || dst.movementFactor == 0) {
        NewState = "find_new_path";
        SetPos(World.current.FindEmptyTile_NotThisOne(pos));
        SetDst(pos);

      }


      if (pos != dst) {
        float distToTravel = Funcs.Distance(pos, dst);
        bool never = false, soon = false;
        switch (dst.CanEnter(this)) {
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
          facing = Funcs.GetSpriteDirection(pX, pY, X, Y);
          if (prevFacing != facing) {
            if (animator.valid) {
              if (animator.Set(facing)) {

              }
            }
            directionChanged = true;
          }
          prevFacing = facing;
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

    public bool FindPath(Tile end, bool neighboursOk) {

      path = PathFinder.FindPath(World.current, pos, end, neighboursOk, neighboursOk);
      if (path != null && path.foundPath) {
        return true;
      } else {
        path = null;
        return false;
      }

    }


    public void CBRegisterOnChanged(Action<Entity> cb) {
      CBOnChanged += cb;

    }

    public void CBUnregisterOnChanged(Action<Entity> cb) {
      CBOnChanged -= cb;
    }

    //-------------------------------- STATIC- -------------------------------------
    private static Dictionary<string, Entity> prototypes = new Dictionary<string, Entity>();
    public static Entity GetPrototype(string name) {
      if (prototypes.ContainsKey(name)) {
        return prototypes[name];
      } else {
        return null;
      }
    }

    public static void LoadFromFile() {
      string path = Path.Combine(Application.streamingAssetsPath, "data", "Entities");

      string[] files = Directory.GetFiles(path, "*.json");

      foreach (string file in files) {
        string fcontents = File.ReadAllText(file);
        JObject json = JObject.Parse(fcontents);

        CreateEntityPrototype(json);
      }
    }

    private static void CreateEntityPrototype(JObject json) {
      Entity proto = new Entity();
      proto.name = "prototype";
      proto.typeName = Funcs.jsonGetString(json["name"], null);
      proto.spriteName = Funcs.jsonGetString(json["spriteName"], null);
      proto.OnUpdate = Funcs.jsonGetString(json["onUpdate"], null);

      proto.facingSprites = new Dictionary<string, string>();
      proto.facingSprites[World.NORTH] = Funcs.jsonGetString(json["spriteNorth"], proto.spriteName);
      proto.facingSprites[World.EAST] = Funcs.jsonGetString(json["spriteEast"], proto.spriteName);
      proto.facingSprites[World.SOUTH] = Funcs.jsonGetString(json["spriteSouth"], proto.spriteName);
      proto.facingSprites[World.WEST] = Funcs.jsonGetString(json["spriteWest"], proto.spriteName);
      proto.info = new ItemParameters();
      float mv = Funcs.jsonGetFloat(json["movement_speed"], 4);
      proto.info.SetFloat("movement_speed", mv);
      proto.getNameType = Funcs.jsonGetString(json["getName"], "human");
      proto.animations = new Dictionary<string, NYDIAnimation>();
      JArray animations = Funcs.jsonGetArray(json, "animations");

      if (animations != null) {
        foreach (JObject anim in animations) {
          NYDIAnimation animation = new NYDIAnimation();
          animation.name = Funcs.jsonGetString(anim["name"], null);
          JArray frames = Funcs.jsonGetArray(anim, "frames");
          List<NYDIAnimationFrame> animFrames = new List<NYDIAnimationFrame>();
          if (frames != null) {
            foreach (JObject frame in frames) {

              //NYDIAnimationFrame frm = new NYDIAnimationFrame();
              //frm.sprite = Funcs.jsonGetString(frame["sprite"], null);
              //frm.time = Funcs.jsonGetFloat(frame["time"], 0);
              animFrames.Add(NYDIAnimationFrame.MakeFrame(frame));
            }
          }
          animation.frames = animFrames.ToArray();
          proto.animations[animation.name] = animation;
        }
      }

      prototypes.Add(proto.typeName, proto);
    }

 
  }





}

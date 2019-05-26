using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

public class Character : IXmlSerializable {

  public enum STATE {
    IDLE,
    RESET,
    FIND_JOB,
    FIND_PATH,
    MOVE,
    WORK_JOB,
    FIND_EMPTY
  }
  private static Dictionary<string, STATE> static_STATES = null;

  /// <summary>
  /// returns the state for the string version of the state
  /// if you send "IDLE" you get the actual enum STATE.IDLE
  /// Get it?
  /// got it?
  /// good.
  /// </summary>
  /// <param name="state"></param>
  /// <returns></returns>
  public static STATE GetState(string state) {


    if (static_STATES == null) {
      static_STATES = new Dictionary<string, STATE>();
      var val = Enum.GetValues(typeof(STATE));
      foreach (STATE s in val) {
        static_STATES[s.ToString()] = s;
      }
    }

    if (static_STATES.ContainsKey(state)) {
      return static_STATES[state];
    }



    return STATE.IDLE;

  }

  //properties

  public STATE state { get; private set; }
  private float findJobCoolDown = 0.5f;
  private float coolDown = 0;
  private Tile target = null;
  Action<Character> cbCharacterChanged;
  Action<Character> cbCharacterKilled;
  World world;
  public float X {
    get {
      return Mathf.Lerp(PosTile.x, DstTile.x, movementPerc);
    }
  }

  public float Y {
    get {
      return Mathf.Lerp(PosTile.y, DstTile.y, movementPerc);
    }
  }

  public Vector2 pos {
    get {
      pPos.Set(this.X, this.Y);
      return pPos;
    }
  }

  public Vector2 dst {
    get {
      pDst.Set(DstTile.x, DstTile.y);
      return pDst;
    }
  }

  public Vector2 job {
    get {
      if (myJob != null) {
        pJob.Set(myJob.tile.x, myJob.tile.y);
        return pJob;
      } else {
        return pos;
      }
    }
  }

  private Vector2 pPos = new Vector2();
  private Vector2 pDst = new Vector2();
  private Vector2 pJob = new Vector2();

  public string name { get; private set; }

  public bool CanMoveDiagonally { get; private set; } = true;
  public string spriteName { get; private set; } = "robot_front";
  public string spriteName_IDLE { get; private set; } = "robot_sitting";
  public string spriteNameNorth { get; private set; } = "robot_back";
  public string spriteNameEast { get; private set; } = "robot_side";
  public string spriteNameSouth { get; private set; } = "robot_front";
  public string spriteNameWest { get; private set; } = "robot_side";

  float movementSpeed = 5; //tiles per second
  //float avgMoveSpd = 0;
  //float avgMoveSpdSum = 0;
  //float avgMoveSpdCount = 0;
  //float avgMoveSpdReset = 0;
  //int avgMoveSpdWarnings = 0;
  //int avgMoveSpdWarningsLimit = 2;
  //int pathAttempts = 0;
  //int pathAttemptsLimit = 3;

  public Tile PosTile { get; private set; }
  public Tile DstTile { get; private set; }

  float movementPerc = 0;
  //float cd = 1;
  private bool changed = false;
  public Job myJob;
  public PathAStar path;

  private bool findNewPath = false;


  private float waitingTimer = 0;




  //constructors
  public Character(World world, Tile tile, string name, string state) {
    DstTile = tile;
    SetPosTile(tile);
    this.world = world;
    this.name = name;
    this.state = GetState(state);
  }
  public Character(World world, Tile startTile) {
    DstTile = startTile;
    SetPosTile(startTile);
    this.world = world;
    state = STATE.IDLE;
    this.name = world.GetName();

  }
  public void PleaseMove(Character asker) {
    
    if (state == STATE.IDLE) {
      state = STATE.FIND_EMPTY;
      //Debug.Log(asker.name + " asked " + name + " to move. They said \"ok\"");
    } else {
      //Debug.Log(asker.name + " asked " + name + " to move. They said \"no\"");
    }
  }


  public void Update(float deltaTime) {
    //Debug.Log(state + " (" + PosTile.x + "," + PosTile.y + ")-->(" + DstTile.x + "," + DstTile.y + ")");

    //if (PosTile.movementFactor == 0) {
    //  state = STATE.RESET;
    //}
    STATE hold = state;
    switch (state) {
      case STATE.IDLE:
        if (PosTile.movementFactor == 0) {
          Debug.Log(name + ": I CAN'T MOVE!");
          Tile t = FindEmpty(PosTile);
          changed = true;
          if (t != null) {
            PosTile = t;
            DstTile = t;
            movementPerc = 0;

          } else {
            //while (countOfTrash < amountOfTrash) {
            bool found = false;
            while (!found) {
              int x = UnityEngine.Random.Range(0, world.width);
              int y = UnityEngine.Random.Range(0, world.height);

              Tile tile = world.getTileAt(x, y);

              //InstalledItem item = trashList[Random.Range(0, trashList.Count)];
              if (IsEmpty(tile)) {
                PosTile = t;
                DstTile = t;
                found = true;
                //wcon.world.PlaceInstalledObject(item.type, tile);
                //countOfTrash += 1;
              }
            }
          }
        } else if (findJobCoolDown <= 0) {
          findJobCoolDown = 0;
          state = STATE.FIND_JOB;
        } else {
          findJobCoolDown -= deltaTime;
        }
        break;
      case STATE.RESET:
        //Debug.Log(name + " resetting...");
        ReturnJob();
        myJob = null;
        path = null;
        findJobCoolDown = UnityEngine.Random.Range(0.5f, 1f);
        target = null;
        findNewPath = false;
        movementPerc = 0;
        state = STATE.IDLE;
        waitingTimer = 0;
        //pathAttempts = 0;

        break;
      case STATE.FIND_JOB:
        findNewPath = false;
        //Debug.Log(name + " Finding a job...");
        myJob = world.jobQueue.GetNextJob();
        if (myJob != null) {
          //do something
          target = myJob.tile;
          myJob.cbRegisterJobComplete(OnJobEnded);
          myJob.cbUnregisterJobCancelled(OnJobEnded);
          state = STATE.FIND_PATH;
        } else {
          state = STATE.RESET;
        }
        break;
      case STATE.FIND_PATH:
        findNewPath = false;
        //Debug.Log(name + " Finding a path...");
        //pathAttempts = 0;
        if (target != null) {
          if (myJob != null) {
            path = PathFinder.FindPath(world, PosTile, target, true, true);
          } else {
            path = new PathAStar(world, PosTile, target);
          }
          if (path != null && path.foundPath) {
            state = STATE.MOVE;
          } else {
            state = STATE.RESET;
          }
        } else {
          state = STATE.RESET;
        }
        break;
      case STATE.MOVE:

        //if (findNewPath) {
        //  state = STATE.FIND_PATH;
        //} else {


        if (PosTile != DstTile) {
          float distanceToTravel = Funcs.Distance(PosTile, DstTile);

          bool never = false;
          bool soon = false;
          switch (DstTile.CanEnter(this)) {
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
            state = STATE.RESET;
          } else if (soon) {
            waitingTimer += deltaTime;
            if (waitingTimer > 3) {
              state = STATE.RESET;
              waitingTimer = 0;
            }
            //wait;
          } else {
            waitingTimer = 0;



            float sp = movementSpeed;
            //if (movementPerc > 0.5) {
            sp *= Mathf.Clamp(DstTile.movementFactor, 0.2f, 1f);
            //} else {
            //  sp *= PosTile.movementFactor;

            //}
            float distThisFrame = sp * deltaTime;



            movementPerc += (distThisFrame / distanceToTravel);
            movementPerc = Mathf.Clamp(movementPerc, 0, 1);



            if (movementPerc >= 1) {
              SetPosTile(DstTile);
              //PosTile = DstTile;
              movementPerc = 0;
              if (findNewPath) {
                findNewPath = false;
                state = STATE.FIND_PATH;

              }
            }
          }
          changed = true;
        } else {
          if (path != null) {
            //DstTile = path.GetNextTile();
            if (!SetDestination(path.GetNextTile())) {
              SetDestination(PosTile);
              //DstTile = PosTile;
              path = null;
              //state = STATE.RESET;
            }
          } else {
            if (myJob != null) {
              state = STATE.WORK_JOB;
            } else {
              state = STATE.RESET;
            }
          }

        }

        break;
      case STATE.WORK_JOB:
        
        //Debug.Log(name + " working...");
        if (myJob != null && (myJob.tile.IsNeighbour(PosTile) || PosTile == myJob.tile)) {
          if (coolDown > 0) {
            coolDown -= deltaTime;
            coolDown = Mathf.Clamp(coolDown, 0f, 10f);
          } else {
            if (myJob.tile.countOfOccupied == 0 || (myJob.tile.countOfOccupied == 1 && myJob.tile.IsItMe(this))) {
              myJob.Work(deltaTime);
            } else {
              coolDown = UnityEngine.Random.Range(0.5f, 1.5f);
              myJob.tile.PleaseMove(this);
            }
          }
        } else {
          state = STATE.RESET;
        }
        break;
      case STATE.FIND_EMPTY:
        //Debug.Log(name + " Finding a place to sit...");
        target = FindEmpty(PosTile);
        if (target != null) {
          state = STATE.FIND_PATH;

        } else {
          state = STATE.RESET;
        }
        break;
      default:
        break;
    }

    if (hold != state || state == STATE.MOVE || changed) {
      if (cbCharacterChanged != null) {
        cbCharacterChanged(this);
      }
    }
    findNewPath = false;

  }

  public void UpdateOld(float deltaTime) {

    if (myJob == null) {
      if (findJobCoolDown > 0) {
        findJobCoolDown -= deltaTime;

      } else {
        findJobCoolDown = UnityEngine.Random.Range(0.5f, 1f);
        myJob = world.jobQueue.GetNextJob();

        if (myJob != null) {
          myJob.cbRegisterJobComplete(OnJobEnded);
          myJob.cbUnregisterJobCancelled(OnJobEnded);

          path = new PathAStar(world, PosTile, myJob.tile);

          if (path.foundPath) {
            //Debug.Log("found path");
          } else {
            path = null;
            Debug.LogError("Could not find path");
            ReturnJob();


          }
          //SetDestination(myJob.tile);
        }
      }
    }


    changed = false;
    //Debug.Log("Character.Update" + deltaTime);
    if (PosTile != DstTile) {
      float distanceToTravel = Funcs.Distance(PosTile, DstTile);
      float sp = movementSpeed;
      //if (movementPerc > 0.5) {
      sp *= DstTile.movementFactor;
      //} else {
      //  sp *= PosTile.movementFactor;

      //}
      float distThisFrame = sp * deltaTime;
      movementPerc += (distThisFrame / distanceToTravel);
      movementPerc = Mathf.Clamp(movementPerc, 0, 1);

      if (movementPerc >= 1) {
        SetPosTile(DstTile);
        movementPerc = 0;
      }
      changed = true;
    } else {
      if (path != null) {
        //DstTile = 
        if (!SetDestination(path.GetNextTile())) {
          SetDestination(PosTile);
          path = null;
        }
      } else {
        if (myJob != null && PosTile == myJob.tile) {
          if (myJob != null) {
            myJob.Work(deltaTime);

          }
        }
      }

    }

    if (changed) {
      if (cbCharacterChanged != null) {
        cbCharacterChanged(this);
      }
    }

  }

  private void ReturnJob() {
    if (myJob != null) {

      world.jobQueue.ReturnJob(myJob);
      myJob.cbUnregisterJobComplete(OnJobEnded);
      myJob.cbUnregisterJobCancelled(OnJobEnded);
      myJob = null;
    }

  }

  private void OnJobEnded(Job j) {
    //job completed or cancelled
    if (j != myJob || myJob == null) {
      Debug.LogError("telling character about a job that does not belong to them");

    } else {
      myJob = null;
      target = FindEmpty(PosTile);
      if (target != null) {
        state = STATE.FIND_PATH;
        //path = new PathAStar(world, PosTile, temp);
        //if (!path.foundPath) {
        //  path = null;
        //}
      }
    }
  }

  public bool SetDestination(Tile dst) {
    if (dst != null && dst.movementFactor > 0 && (dst.IsNeighbour(PosTile, CanMoveDiagonally) || dst == PosTile) ) {
      DstTile = dst;
      return true;
    } else {
      //Debug.Log("Character SetDestination was given a tile that was not a neighbour of the current tile...");
      return false;
    }



  }

  private void SetPosTile(Tile t) {
    if (t != PosTile) {
      if (PosTile != null) {
        PosTile.Leave(this);
      }
      PosTile = t;
      t.Enter(this);
    }
  }

  public bool IsEmpty(Tile t) {
    return t.IsEmpty();

  }

  public Tile FindEmpty(Tile t) {
    if (IsEmpty(t)) {
      return t;
    } else {
      foreach (Tile tn in t.neighboursList) {
        if (IsEmpty(tn)) {
          return tn;
        }


      }

      foreach (Tile tn in t.neighboursList) {
        Tile r = FindEmpty(tn);
        if (r != null) {
          return r;
        }
      }
    }

    return null;

  }

  public void Kill() {
    if (cbCharacterKilled != null) {
      cbCharacterKilled(this);
    }
  }

  public void CBRegisterOnChanged(Action<Character> cb) {
    cbCharacterChanged += cb;
  }

  public void CBUnregisterOnChanged(Action<Character> cb) {
    cbCharacterChanged -= cb;
  }

  public void CBRegisterOnKilled(Action<Character> cb) {
    cbCharacterKilled += cb;
  }

  public void CBUnregisterOnKilled(Action<Character> cb) {
    cbCharacterKilled -= cb;
  }

  public XmlSchema GetSchema() {
    return null;
  }

  public void ReadXml(XmlReader reader) {

  }

  public void PathNodesDestroyed() {
    if (Funcs.Chance(30)) {
      findNewPath = true;
    }

  }

  public void WriteXml(XmlWriter writer) {

    writer.WriteStartElement("character");
    writer.WriteElementString("name", name);
    writer.WriteElementString("xPos", ((int)X).ToString());
    writer.WriteElementString("yPos", ((int)Y).ToString());
    writer.WriteElementString("state", state.ToString());
    if (myJob != null) {
      myJob.WriteXml(writer);
      //writer.WriteElementString("job", "....");
    } else {
      writer.WriteStartElement("Job");
      writer.WriteEndElement();
    }

    writer.WriteEndElement();

  }
}
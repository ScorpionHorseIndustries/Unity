using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NoYouDoIt.DataModels {
  using NoYouDoIt.Controller;
  using NoYouDoIt.TheWorld;
  using NoYouDoIt.Utils;
  public class WorkItem {
    public Tile workTile { get; private set; }
    public Entity assignedRobot { get; private set; }
    public bool complete { get; private set; } = false;
    public bool isReady { get; private set; } = false;
    public Inventory inventory;
    public Recipe recipe;
    public InstalledItem installedItemProto;
    public string inventoryItemName;
    public int inventoryItemQtyRequired;
    public int inventoryItemQtyRemaining;
    public bool inventorySearchStockpiles = true;
    public WorkItem parent;
    public float workTime = 1;
    private List<WorkItem> prereq;
    public float IsReadyCoolDown = 0;
    public string description = "work";

    public List<Tile> relatedTiles = new List<Tile>();
    public bool canWorkFromNeighbourTiles = false;
  


    public void Assign(Entity robot) {
      this.assignedRobot = robot;
    }

    public void Unassign() {
      if (this.assignedRobot != null) {
        //Debug.Log("Unassigning assigned worker");
        this.assignedRobot.work = null;
        this.assignedRobot.NewState = "wander";
        this.assignedRobot = null;
      } else {
        //Debug.Log("Could not Unassign worker");
      }

    }

    public int GetCountOfPrereqs() {
      return prereq.Count;
    }



    public LuaFunctionList OnWork = new LuaFunctionList();
    public LuaFunctionList OnComplete = new LuaFunctionList();
    public LuaFunctionList OnCancel = new LuaFunctionList();
    public string IsComplete;
    public string IsReady;

    public void DoOnComplete() {
      if (complete) return;
      foreach (Tile t in relatedTiles) {
        t.RemoveWork(this);
      }
      World.CallLuaFunctions(OnComplete.ToArray(), this);
      complete = true;
      if (recipe != null) {
        WorldController.Instance.DeductMoney(recipe.cost);
      }

    }



    public bool IsItReadyYet() {
      if (this.isReady) return true;
      bool loc_isReady = false;
      System.Object[] results = World.GetLuaResult(IsReady, this);
      if (results != null && results.Length > 0) {
        
        if (bool.TryParse(results[0].ToString(), out loc_isReady)) {
          if (loc_isReady) {
            this.isReady = true;
          }
          
          //World.CallLuaFunctions(OnComplete.ToArray(), this);
          

        }
      }

      if (!loc_isReady) {
        IsReadyCoolDown = UnityEngine.Random.Range(1, 5);
      }

      return loc_isReady;
    }



    public override string ToString() {
      return "workItem (" + description + ") " +
        "\nonWork(" + OnWork.ToString() + ")" +
        "\nonComplete(" + OnComplete.ToString() + ")" +
        "\nonCancel(" + OnCancel.ToString() + ")" +
        "\nIsReady(" + IsReady + ")" +
        "\nIsComplete(" + IsComplete + ")" +
        "\nworktime(" + workTime + ")" +
        "\nTile(" + workTile + ")" +
        "\nprereqs(" + GetCountOfPrereqs() + ")" +
        "\ninventoryItem: " +
        (inventoryItemName != null ?
          inventoryItemName + ":remain:" + inventoryItemQtyRemaining + "/rqrd:" + inventoryItemQtyRequired : "none") +
          "\nAssigned: " + (assignedRobot != null ? assignedRobot.name : "none");

        
        ;
    }

    
    
    public void AddPrereq(WorkItem work) {
      if (!prereq.Contains(work)) {
        prereq.Add(work);
      }
    }

    public void RemovePrereq(WorkItem work) {
      if (prereq.Contains(work)) {
        prereq.Remove(work);
      }
    }

    private WorkItem() {
      prereq = new List<WorkItem>();

    }

    public void OnItemAdded(string itemName, int qty) {
      //if (prereq.Count > 0) {
      //  int qtyRemaining = qty;
      //  foreach(WorkItem p in prereq.Where(e => e.inventoryItemName == itemName)) {
      //    if (qtyRemaining <= 0) break;
      //    if (qtyRemaining >= p.inventoryItemQtyRemaining) {
      //      qtyRemaining -= p.inventoryItemQtyRemaining;
      //      p.inventoryItemQtyRemaining = 0;
              
      //    } else {
      //      qtyRemaining -= p.inventoryItemQtyRemaining;
      //      p.inventoryItemQtyRemaining -= qtyRemaining;
      //    }

      //  }
      //}

    }

    public void UnnassignTiles() {
      
    }

    public static WorkItem MakeWorkItem(Tile tile, string function, params System.Object[] args) {
      WorkItem w = new WorkItem();
      w.workTile = tile;
      w.inventory = new Inventory(World.current, 99, INVENTORY_TYPE.ENTITY, w);
      w.inventory.CBRegisterOnItemAdded(w.OnItemAdded);

      args = args.Prepend(w).Cast<System.Object>().ToArray();

      World.CallLuaFunction(function, args);


      World.current.workManager.AddWorkItem(w);
      tile.AddWork(w);



      return w;
    }

    private void Cancel() {
      World.CallLuaFunctions(OnCancel.ToArray(), this);
      this.workTile.RemoveWork(this);
    }

    public void Work(float deltaTime) {
      if (complete) return;
      if (OnWork != null) {
        World.CallLuaFunctions(OnWork.ToArray(), this, deltaTime);

        System.Object[] results = World.GetLuaResult(IsComplete, this);
        if (results != null && results.Length > 0) {
          bool isComplete = false;
          if (bool.TryParse(results[0].ToString(), out isComplete)) {
            if (isComplete) {
              DoOnComplete();

            }

          }
        }
      }

    }
  }
}

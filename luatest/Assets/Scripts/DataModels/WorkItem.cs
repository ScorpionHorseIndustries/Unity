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
    public Robot assignedRobot { get; private set; }
    public bool complete { get; private set; } = false;
    public Inventory inventory;
    public Recipe recipe;
    public InstalledItem installedItemProto;
    public string inventoryItemName;
    public int inventoryItemQtyRequired;
    public int inventoryItemQtyRemaining;
    public WorkItem parent;
    public float workTime = 1;






    public LuaFunctionList OnWork = new LuaFunctionList();
    public LuaFunctionList OnComplete = new LuaFunctionList();
    public LuaFunctionList OnCancel = new LuaFunctionList();
    public string IsComplete;
    public string IsReady;

    public void DoOnComplete() {
      World.CallLuaFunctions(OnComplete.ToArray(), this);
      complete = true;

    }


    public override string ToString() {
      return "workItem " + "" +
        "onWork(" + OnWork.ToString() + ")" +
        "onComplete(" + OnComplete.ToString() + ")" +
        "onCancel(" + OnCancel.ToString() + ")" +
        "IsReady(" + IsReady + ")" +
        "IsComplete(" + IsComplete + ")" +
        "worktime(" + workTime + ")" +
        "Tile(" + workTile + ")";
    }


    public List<WorkItem> prereq;
    public void AddPrereq(WorkItem work) {
      if (!prereq.Contains(work)) {
        prereq.Add(work);
      }
    }

    private WorkItem() {

    }


    public static WorkItem MakeWorkItem(Tile tile, string function, params System.Object[] args) {
      WorkItem w = new WorkItem();
      w.workTile = tile;
      w.inventory = new Inventory(World.current, 99, INVENTORY_TYPE.ROBOT, w);

      args = args.Prepend(w).Cast<System.Object>().ToArray();

      World.CallLuaFunction(function, args);


      World.current.workManager.AddWorkItem(w);



      return w;
    }

    private void Cancel() {
      World.CallLuaFunctions(OnCancel.ToArray(), this);
    }

    public void Work(float deltaTime) {
      if (complete) return;
      if (OnWork != null) {
        World.CallLuaFunctions(OnWork.ToArray(), this, deltaTime);

        System.Object[] results = World.GetLuaResult(IsComplete, this);
        if (results != null && results.Length > 0) {
          bool isComplete = false;
          if (bool.TryParse(results[0].ToString(), out isComplete)) {
            World.CallLuaFunctions(OnComplete.ToArray(), this);
            complete = true;

          }
        }
      }

    }







  }
}

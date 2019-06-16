using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class InstalledItemActions {


  public static void RunLua(string function) {
    Debug.Log("run lua function:" + function);
    //World.current.lua.CallFunction(function,)
    //LuaActions.
  }

  public static Tile.CAN_ENTER CallEnterRequested(string func, InstalledItem item) {
    LuaFunction luaFunc = World.current.lua[func] as LuaFunction;
    Tile.CAN_ENTER enter = Tile.CAN_ENTER.YES;
    if (luaFunc != null) {
      enter = (Tile.CAN_ENTER) luaFunc.Call(item)[0];

    }

    return enter;
  }




  public static void CallFunctions(string[] functions, InstalledItem item, float deltaTime) {

    foreach(string func in functions) {
      LuaFunction luaFunc = World.current.lua[func] as LuaFunction;
      if (luaFunc != null) {
        luaFunc.Call(item, deltaTime);
        
      }

      //World.current.lua.       CallFunction( (func, new[] { item, deltaTime });
    }

  }

  public static void Stockpile_UpdateActions(InstalledItem item, float deltaTime) {
    //Debug.Log("hello");


    if (item.tile.IsInventoryEmpty() && !item.tile.HasPendingJob) {

      string itemName = InventoryItem.GetRandomPrototype().type;

      Tile nearest = World.current.inventoryManager.GetNearest(item.tile, itemName, false);
      if (nearest == null) return;

      Job j = Job.MakeTileToTileJob(
        nearest,
          item.tile,
          //World.current.jobQueue.HaulToTileComplete,
          //World.current.jobQueue.HaulJobCancelled,
          itemName,
          InventoryItem.GetStackSize(itemName)
          );
      j.cbLuaRegisterJobComplete("JobQueue_HaulToTileComplete");
      j.cbLuaRegisterJobCancelled("JobQueue_HaulJobCancelled");
      j.cancelIfReturned = true;

      World.current.jobQueue.Push(j);

    } else if (!item.tile.IsInventoryEmpty() && !item.tile.HasPendingJob) {
      string itemName = item.tile.GetFirstInventoryItem();
      int qtyRequired = InventoryItem.GetStackSize(itemName) - item.tile.InventoryTotal(itemName);
      Tile nearest = World.current.inventoryManager.GetNearest(item.tile, itemName, false);
      if (nearest != null) {
        qtyRequired = Mathf.Min(qtyRequired, nearest.InventoryTotal(itemName));

        if (qtyRequired > 0) {
          Job j = Job.MakeTileToTileJob(
            nearest,
            item.tile,
            //World.current.jobQueue.HaulToTileComplete,
            //World.current.jobQueue.HaulJobCancelled,
            itemName,
            qtyRequired

              );
          j.cancelIfReturned = true;
          j.cbLuaRegisterJobComplete("JobQueue_HaulToTileComplete");
          j.cbLuaRegisterJobCancelled("JobQueue_HaulJobCancelled");

          World.current.jobQueue.Push(j);
        }
      }
    }
  }

  //public static void MiningController_UpdateActions(InstalledItem item, float deltaTime) {
  //  Tile tile = item.GetWorkSpot();


  //  if (!tile.HasPendingJob) {
  //    Job j = new Job(
  //      tile,
  //      MiningController_JobComplete,
  //      MiningController_JobCancelled,
  //      JOB_TYPE.WORK_AT_STATION,
  //      1,
  //      "recipe::installed::mining_controller::work"

  //      );

  //    WorldController.Instance.world.jobQueue.Push(j);
  //  }

  //}

  

  public static void Workstation_OnDemand(InstalledItem item) {
    //Workstation_UpdateActions(item, 1);
    World.CallLuaFunction("Workstation_UpdateActions", item, 1);

  }

  //public static void Workstation_UpdateActions(InstalledItem item, float deltaTime) {
  //  Tile tile = item.GetWorkSpot();


  //  if (!tile.HasPendingJob) {
  //    Job j = Job.MakeStandardJob(
  //      tile,
  //      Workstation_JobComplete,
  //      Workstation_JobCancelled,
  //      JOB_TYPE.WORK_AT_STATION,
  //      1,
  //      item.workRecipeName

  //      );
      
  //    World.current.jobQueue.Push(j);
  //  }

  //}

  public static void Workstation_JobComplete(Job job) {
    Recipe.RecipeProduct rp = job.recipe.GetProduct();
    if (rp != null) {
      //Debug.Log("job finished. recipe type: " + rp.ToString());
      if (rp.type == Recipe.RECIPE_PRODUCT_TYPE.INVENTORY_ITEM) {
        int qty = UnityEngine.Random.Range(rp.qtyMin, rp.qtyMax + 1);
        Tile tile = World.current.FindTileForInventoryItem(job.tile, rp.name, qty);

        if (tile != null) {


          if (rp != null) {
            Inventory inv = new Inventory(World.current, 1, INVENTORY_TYPE.NONE, tile);
            inv.AddItem(rp.name, qty);
            inv.Explode();
            inv.ClearAll();
          }


        }
      } else if (rp.type == Recipe.RECIPE_PRODUCT_TYPE.CHARACTER) {
        Tile tile = World.current.FindEmptyTile(job.tile);
        if (tile != null) {
          World.current.CreateCharacter(tile);
        }
      }
    }

    if (job.recipe.givesCash) {
      WorldController.Instance.addCurrency(UnityEngine.Random.Range(job.recipe.minCash, job.recipe.maxCash));
    }


    job.tile.RemoveJob(job);


  }
  public static void Workstation_JobCancelled(Job job) {

  }



  //public static void Door_UpdateActions(InstalledItem item, float deltaTime) {

  //  float opentime = item.itemParameters.GetFloat("opentime", 0.25f);

  //  if (item.itemParameters.GetBool("opening")) {
  //    item.itemParameters.IncFloat("openness", deltaTime / opentime);
  //  } else {
  //    item.itemParameters.IncFloat("openness", -(deltaTime / opentime));
  //  }
  //  float f = item.itemParameters.GetFloat("openness");
  //  f = Mathf.Clamp01(f);
  //  item.itemParameters.SetFloat("openness", f);

  //  if (item.itemParameters.GetFloat("openness") >= 1) {
  //    item.itemParameters.SetBool("opening", false);
  //  }
  //  //Debug.Log("Update door " + item.itemParameters.ToString());
  //  if (item.cbOnChanged != null) {
  //    item.cbOnChanged(item);
  //  }

  //}

  //public static Tile.CAN_ENTER Door_EnterRequested(InstalledItem item) {
  //  item.itemParameters.SetBool("opening", true);
  //  if (item.itemParameters.GetFloat("openness") >= 1) {
  //    return Tile.CAN_ENTER.YES;
  //  } else {
  //    return Tile.CAN_ENTER.SOON;
  //  }




  //}
}


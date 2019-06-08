using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class InstalledItemActions {

  public static void Stockpile_UpdateActions(InstalledItem item, float deltaTime) {
    //Debug.Log("hello");


    if (item.tile.IsInventoryEmpty() && !item.tile.HasPendingJob) {

      string itemName = InventoryItem.GetRandomPrototype().type;

      Tile nearest = item.tile.world.inventoryManager.GetNearest(item.tile, itemName, false);
      if (nearest == null) return;

      Job j = new Job(
          item.tile,
          WorldController.Instance.world.jobQueue.HaulToTileComplete,
          WorldController.Instance.world.jobQueue.HaulJobCancelled,
          itemName,
          InventoryItem.GetStackSize(itemName)
          );

      WorldController.Instance.world.jobQueue.Push(j);

    } else if (!item.tile.IsInventoryEmpty() && !item.tile.HasPendingJob) {
      string itemName = item.tile.GetFirstInventoryItem();
      int qtyRequired = InventoryItem.GetStackSize(itemName) - item.tile.InventoryTotal(itemName);

      if (qtyRequired > 0) {
        Job j = new Job(
          item.tile,
            WorldController.Instance.world.jobQueue.HaulToTileComplete,
            WorldController.Instance.world.jobQueue.HaulJobCancelled,
            itemName,
            qtyRequired

            );

        WorldController.Instance.world.jobQueue.Push(j);
      }
    }
  }

  public static void MiningController_UpdateActions(InstalledItem item, float deltaTime) {
    Tile tile = item.GetWorkSpot();


    if (!tile.HasPendingJob) {
      Job j = new Job(
        tile,
        MiningController_JobComplete,
        MiningController_JobCancelled,
        JOB_TYPE.WORK_AT_STATION,
        1,
        "recipe::installed::mining_controller::work"

        );

      WorldController.Instance.world.jobQueue.Push(j);
    }
      
  }

  public static void MiningController_JobComplete(Job job) {
    Tile tile = WorldController.Instance.world.FindEmptyTile(job.tile);

    if (tile != null) {
      Recipe.RecipeProduct rp = job.recipe.GetProduct();

      if (rp != null) {
        Inventory inv = new Inventory(WorldController.Instance.world, 1, INVENTORY_TYPE.NONE, tile);
        inv.AddItem(rp.name, UnityEngine.Random.Range(rp.qtyMin, rp.qtyMax + 1));
        inv.Explode();
        inv.ClearAll();
      }

      job.tile.RemoveJob(job);
    }


  }
  public static void MiningController_JobCancelled(Job job) {

  }



  public static void Door_UpdateActions(InstalledItem item, float deltaTime) {

    float opentime = item.itemParameters.GetFloat("opentime", 0.25f);

    if (item.itemParameters.GetBool("opening")) {
      item.itemParameters.IncFloat("openness", deltaTime / opentime);
    } else {
      item.itemParameters.IncFloat("openness", -(deltaTime / opentime));
    }
    float f = item.itemParameters.GetFloat("openness");
    f = Mathf.Clamp01(f);
    item.itemParameters.SetFloat("openness", f);

    if (item.itemParameters.GetFloat("openness") >= 1) {
      item.itemParameters.SetBool("opening", false);
    }
    //Debug.Log("Update door " + item.itemParameters.ToString());
    if (item.cbOnChanged != null) {
      item.cbOnChanged(item);
    }

  }

  public static Tile.CAN_ENTER Door_EnterRequested(InstalledItem item) {
    item.itemParameters.SetBool("opening", true);
    if (item.itemParameters.GetFloat("openness") >= 1) {
      return Tile.CAN_ENTER.YES;
    } else {
      return Tile.CAN_ENTER.SOON;
    }




  }
}


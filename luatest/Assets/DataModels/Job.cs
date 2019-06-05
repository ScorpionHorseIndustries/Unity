using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;


public enum JOB_TYPE {
  NONE, BUILD, HAUL
}

public enum JOB_SUBTYPE {
  NONE, HAUL_TO_TILE
}

public class Job : IXmlSerializable {



  //struct JobResource {
  //  string name;
  //  int qtyRequired;
  //  int qtyDelivered;
  //  int qtyAssigned;
  //}

  //private List<JobResource> resourcesRequired;


  public readonly string jobId = "job_" + Guid.NewGuid().ToString();
  public bool finished { get; private set; } = false;
  public bool cancelled { get; private set; } = false;
  public Recipe recipe;

  public Tile tile { get; private set; }
  public float jobTime { get; private set; }
  public Action<Job> cbJobComplete;
  public Action<string> cbCarryComplete;
  public Action<Job> cbJobCancelled;
  public JOB_TYPE jobType { get; protected set; }
  public JOB_SUBTYPE subType { get; protected set; } = JOB_SUBTYPE.NONE;
  public string recipeResourceName { get; private set; }
  public int recipeResouceQty { get; set; }
  public int qtyFulfilled = 0;
  public string description { get; private set; }
  public Job parent;
  public Character worker;
  public Inventory inventory; 

  public Job(Tile tile, Action<Job> cbJobComplete, Action<Job> cbJobCancelled, string resourceName, int qty) {
    inventory = new Inventory(tile.world, 99, INVENTORY_TYPE.JOB, this);
    this.tile = tile;
    this.jobTime = 0.1f;
    this.jobType = JOB_TYPE.HAUL;
    this.subType = JOB_SUBTYPE.HAUL_TO_TILE;
    this.recipeResourceName = resourceName;
    this.recipeResouceQty = qty;
    this.cbJobComplete += cbJobComplete;
    this.cbJobCancelled += cbJobCancelled;
    this.description = "HAUL " + qty + " " + resourceName;

    if (recipeResourceName == InventoryItem.ANY) {

      recipeResourceName = InventoryItem.GetRandomPrototype().type;
      recipeResouceQty = InventoryItem.GetStackSize(recipeResourceName);
    }

    


    Debug.Log("created new haul " + this.ToString());
  }

  public Job(Tile tile, Action<Job> cbJobComplete, Action<Job> cbJobCancelled, JOB_TYPE type, string description, Recipe recipe, string name, Job parent) {
    this.tile = tile;
    this.jobTime = 1f;

    if (type == JOB_TYPE.HAUL) {
      this.jobTime = 0.1f;
    }

    this.parent = parent;

    this.cbJobComplete += cbJobComplete;
    if (cbJobCancelled != null) {
      this.cbJobCancelled += cbJobCancelled;

    }
    //this.cbJobCancelled += cbJobCancelled;
    this.jobType = type;
    this.description = description;

    this.recipe = recipe;
    this.recipeResourceName = name;
    this.recipeResouceQty = recipe.resources[name].qtyRequired;
    inventory = new Inventory(tile.world, 99, INVENTORY_TYPE.JOB, this);
    inventory.AddRestriction(recipeResourceName, recipeResouceQty);


  }

  public Job(Tile tile, Action<Job> cbJobComplete, Action<Job> cbJobCancelled, JOB_TYPE type, float jobTime, string description) {
    this.tile = tile;
    this.jobTime = jobTime;
    this.cbJobComplete += cbJobComplete;
    this.cbJobCancelled += cbJobCancelled;
    this.description = description;
    this.jobType = type;
    inventory = new Inventory(tile.world,99, INVENTORY_TYPE.JOB, this);

    //resourcesRequired = new List<JobResource>();

    if (jobType == JOB_TYPE.BUILD) {
      string recipeName = InstalledItem.prototypeRecipes[description];
      if (recipeName != null) {
        recipe = Recipe.GetRecipe(recipeName);
        this.jobTime = recipe.buildTime;


        foreach (Recipe.RecipeResource rr in recipe.resources.Values) {
          inventory.AddRestriction(rr.name, rr.qtyRequired);
        }
        //Debug.Log("new job time is" + this.jobTime);
      }
    }

  }

  public bool IsRecipeFinished() {
    if (recipe == null) {
      return true;
    }

    bool r = true;

    foreach (Recipe.RecipeResource rr in recipe.resources.Values) {
      if (rr.qtyRemaining > 0) {
        r = false;
        break;
      }

    }

    return r;
  }

  //public void ResourceAdded(string name) {
  //  if (recipe != null) {
  //    recipe.Add(name, recipe.resources[name].qtyRequired);
  //  }
  //}

  public override string ToString() {
    return "job (" + jobTime + ") " + jobType + " @" + tile.ToString();
  }


  public void Work(float time) {
    jobTime -= time;

    if (jobTime <= 0) {
      WorldController.Instance.world.inventoryManager.UnregisterInventory(inventory);
      if (cbJobComplete != null) {
        cbJobComplete(this);
      }
      finished = true;
    }
  }

  public void Cancel() {
    finished = true;
    cancelled = true;
    if (inventory != null) {
      WorldController.Instance.world.inventoryManager.UnregisterInventory(inventory);
    }
    if (cbJobCancelled != null) {
      cbJobCancelled(this);
    }
  }
  public void cbRegisterJobComplete(Action<Job> cb) {
    this.cbJobComplete += cb;
  }

  public void cbUnregisterJobComplete(Action<Job> cb) {
    this.cbJobComplete -= cb;
  }

  public void cbRegisterJobCancelled(Action<Job> cb) {
    this.cbJobCancelled += cb;
  }

  public void cbUnregisterJobCancelled(Action<Job> cb) {
    this.cbJobCancelled -= cb;
  }

  public XmlSchema GetSchema() {
    return null;
  }

  public void ReadXml(XmlReader reader) {

  }

  public void WriteXml(XmlWriter writer) {
    Job j = this;
    writer.WriteStartElement("job");

    writer.WriteElementString("jobType", j.jobType.ToString());
    writer.WriteElementString("description", j.description);
    writer.WriteElementString("id", j.jobId);
    writer.WriteElementString("onComplete", cbJobComplete.ToString());
    writer.WriteElementString("onCancelled", cbJobCancelled.ToString());
    writer.WriteElementString("time", j.jobTime.ToString());
    writer.WriteStartElement("tile");
    writer.WriteElementString("x", j.tile.world_x.ToString());
    writer.WriteElementString("y", j.tile.world_y.ToString());


    writer.WriteEndElement();

    writer.WriteEndElement();
  }
}

class JobTask {


}

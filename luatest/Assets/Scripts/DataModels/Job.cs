using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;


public enum JOB_TYPE {
  NONE, BUILD, HAUL, DECONSTRUCT, WORK_AT_STATION
}

public enum JOB_SUBTYPE {
  NONE, HAUL_TO_TILE, HAUL_FROM_TILE_TO_TILE
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
  public Tile fromTile { get; private set; }
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
  public float priority = 1;
  public float age = 0;
  public float minPriority { get; private set; }
  public float maxPriority { get; private set; }
  public bool cancelIfReturned { get; set; }
  public bool preAllocated { get; private set; }
  public float cost { get; private set; } = 0f;


  public InstalledItem installedItemPrototype { get; private set; }

  public void SetPriority() {

    float variation = 0;
    switch (jobType) {

      case JOB_TYPE.BUILD:
        minPriority = 1;
        maxPriority = 3;
        if (!IsRecipeFinished()) {
          variation = 3;
        }
        break;
      case JOB_TYPE.HAUL:
        minPriority = 1;
        maxPriority = 2;
        break;
      case JOB_TYPE.DECONSTRUCT:
        minPriority = 0.5f;
        maxPriority = 1.5f;
        break;
      case JOB_TYPE.WORK_AT_STATION:
        minPriority = 0.5f;
        maxPriority = 1.5f;
        break;
      default:
        minPriority = 1;
        maxPriority = 5;

        break;
    }
    float x = Mathf.Deg2Rad * (age/10f);
    float radius = maxPriority - minPriority;
    float mid = minPriority + radius;
    priority = variation + mid + (radius * Mathf.Sin(x));


  }

  public Job(Tile fromTile, Tile toTile, Action<Job> cbJobComplete, Action<Job> cbJobCancelled, string resourceName, int qty) {
    this.tile = toTile;
    this.fromTile = fromTile;
    this.cbJobComplete += cbJobComplete;
    this.cbJobCancelled += cbJobCancelled;

    this.description = "HAUL " + qty + " " + resourceName;
    this.jobType = JOB_TYPE.HAUL;
    this.subType = JOB_SUBTYPE.HAUL_FROM_TILE_TO_TILE;
    int aqty = fromTile.InventoryAllocate(resourceName, qty);

    this.recipeResourceName = resourceName;
    this.recipeResouceQty = aqty; // tile.InventoryAllocate(resourceName, qty);
    this.preAllocated = true;
    this.jobTime = 0.1f;
  }


  public Job(Tile tile, Action<Job> cbJobComplete, Action<Job> cbJobCancelled, string resourceName, int qty) {
    inventory = new Inventory(World.current, 99, INVENTORY_TYPE.JOB, this);
    this.tile = tile;
    this.jobTime = 0.1f;
    this.jobType = JOB_TYPE.HAUL;
    this.subType = JOB_SUBTYPE.HAUL_TO_TILE;

    this.recipeResourceName = resourceName;
    this.recipeResouceQty = qty; // tile.InventoryAllocate(resourceName, qty);
    this.cbJobComplete += cbJobComplete;
    this.cbJobCancelled += cbJobCancelled;
    this.description = "HAUL " + qty + " " + resourceName;

    if (recipeResourceName == InventoryItem.ANY) {

      recipeResourceName = InventoryItem.GetRandomPrototype().type;
      recipeResouceQty = InventoryItem.GetStackSize(recipeResourceName);
    }

    priority = 5;




    Debug.Log("created new haul " + this.ToString());
  }

  public Job(Tile tile, Action<Job> cbJobComplete, Action<Job> cbJobCancelled, JOB_TYPE type, string description, Recipe recipe, string name, Job parent) {

    priority = 1;
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


    this.recipeResouceQty = recipe.resources[name].qtyRequired;// tile.InventoryAllocate(name, recipe.resources[name].qtyRequired);

    inventory = parent.inventory;//new Inventory(tile.world, 99, INVENTORY_TYPE.JOB, this);
    inventory.CBRegisterOnItemAdded(parent.OnItemAdded);
    //inventory.AddRestriction(recipeResourceName, recipeResouceQty);



  }



  public Job(Tile tile, Action<Job> cbJobComplete, Action<Job> cbJobCancelled, JOB_TYPE type, float jobTime, string description) {
    priority = 2;
    this.tile = tile;
    this.jobTime = jobTime;
    this.cbJobComplete += cbJobComplete;
    this.cbJobCancelled += cbJobCancelled;
    this.description = description;
    this.jobType = type;


    //resourcesRequired = new List<JobResource>();

    if (jobType == JOB_TYPE.BUILD) {
      inventory = new Inventory(World.current, 99, INVENTORY_TYPE.JOB, this);
      installedItemPrototype = InstalledItem.prototypes[description];//.spriteName;
      string recipeName = InstalledItem.prototypeRecipes[description];
      CreateJobRecipe(recipeName);
    } else if (jobType == JOB_TYPE.WORK_AT_STATION) {
      inventory = new Inventory(World.current, 99, INVENTORY_TYPE.JOB, this);
      recipe = Recipe.GetRecipe(description);
      CreateJobRecipe(description);
      this.jobTime = recipe.buildTime;
    }

  }

  private void CreateJobRecipe(string recipeName) {
    if (recipeName != null) {
      recipe = Recipe.GetRecipe(recipeName);
      this.jobTime = recipe.buildTime;
      this.cost = recipe.cost;

      foreach (Recipe.RecipeResource rr in recipe.resources.Values) {
        inventory.AddRestriction(rr.name, rr.qtyRequired);
      }
      //Debug.Log("new job time is" + this.jobTime);
    }
  }

  public void OnItemAdded(string item, int qty) {
    recipe.Add(item, qty);
  }

  public void OnItemRemoved(string item, int qty) {

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
      finished = true;
      AddToLog("work finished by worker", worker);

      if (cbJobComplete != null) {
        cbJobComplete(this);
      }
      if (parent == null) {
        if (inventory != null) {
          inventory.ClearAll();
          World.current.inventoryManager.UnregisterInventory(inventory);
        }
      }

    }
  }

  public void Cancel() {
    AddToLog("cancelled", worker);
    finished = true;
    cancelled = true;
    if (cbJobCancelled != null) {
      cbJobCancelled(this);
    }
    if (parent == null && inventory != null) {
      
      inventory.ClearAll();
      World.current.inventoryManager.UnregisterInventory(inventory);
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

  //------------------------------------------LOGGING------------------------------------------
  public List<string> log = new List<string>();
  public void AddToLog(string logString, Character chr = null) {


    //if (chr != null) {
    //  logString = DateTime.Now.ToString() + "\t" + chr.name + "\t" + logString;
    //} else {
    //  logString = DateTime.Now.ToString() + "\t\t" + logString;
    //}
    //log.Add(logString);
  }

  public string GetLog() {
    string output = "\nLog for " + this.ToString();
    foreach (string s in log) {
      output += "\n" + s;
    }

    return output;
  }
  //------------------------------------------END LOGGING------------------------------------------
}

class JobTask {


}

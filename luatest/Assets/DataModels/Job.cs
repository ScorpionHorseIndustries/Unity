using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;


public enum JOB_TYPE {
  NONE,BUILD, HAUL
}

public class Job : IXmlSerializable {

   

  //struct JobResource {
  //  string name;
  //  int qtyRequired;
  //  int qtyDelivered;
  //  int qtyAssigned;
  //}

  //private List<JobResource> resourcesRequired;


  public readonly string jobId;
  public bool finished { get; private set; } = false;
  public bool cancelled { get; private set; } = false;
  public Recipe recipe;

  public Tile tile { get; private set; }
  public float jobTime { get; private set; }
  Action<Job> cbJobComplete;
  Action<string> cbCarryComplete;
  Action<Job> cbJobCancelled;
  public JOB_TYPE jobType { get; protected set; } 
  public string recipeResourceName { get; private set; }
  public int recipeResouceQty { get; private set; }
  public string description { get; private set; }
  public Job parent;
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
    this.jobId = "job_" + Guid.NewGuid().ToString();
    this.recipe = recipe;
    this.recipeResourceName = name;
    this.recipeResouceQty = recipe.resources[name].qtyRequired;


  }

  public Job(Tile tile, Action<Job> cbJobComplete, Action<Job> cbJobCancelled, JOB_TYPE type, float jobTime, string description ) {
    this.tile = tile;
    this.jobTime = jobTime;
    this.cbJobComplete += cbJobComplete;
    this.cbJobCancelled += cbJobCancelled;
    this.description = description;
    this.jobType = type;
    this.jobId = "job_" + Guid.NewGuid().ToString();

    //resourcesRequired = new List<JobResource>();

    if (jobType == JOB_TYPE.BUILD) {
      string recipeName = InstalledItem.prototypeRecipes[description];
      if (recipeName != null) {
        recipe = Recipe.GetRecipe(recipeName);
        this.jobTime = recipe.buildTime;
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

  public void ResourceAdded(string name) {
    if (recipe != null) {
      recipe.Add(name, recipe.resources[name].qtyRequired);
    }
  }

  public override string ToString() {
    return "job (" + jobTime + ") " + jobType + " @" + tile.ToString();
  }


  public void Work(float time) {
    jobTime -= time;

    if (jobTime <= 0) {
      if (cbJobComplete != null) {
        cbJobComplete(this);
      }
      finished = true;
    }
  }

  public void Cancel() {
    finished = true;
    cancelled = true;
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
    writer.WriteElementString("x", j.tile.x.ToString());
    writer.WriteElementString("y", j.tile.y.ToString());


    writer.WriteEndElement();

    writer.WriteEndElement();
  }
}

class JobTask {


}

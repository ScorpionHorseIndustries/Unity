using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Priority_Queue;

public class JobQueue : IXmlSerializable
{

  public List<Job> allJobs;
  Action<Job> cbJobCreated;
  //protected Queue<Job> jobs = new Queue<Job>();
  protected SimplePriorityQueue<Job> jobs = new SimplePriorityQueue<Job>();
  private Queue<Job> problemJobs = new Queue<Job>();
  public List<Job> publicJobs = new List<Job>();
  World world;

  public int Count {
    get {
      return jobs.Count;
    }
  }

  public JobQueue(World world)
  {
    this.world = world;
    allJobs = new List<Job>();

  }

  public void Push(Job j)
  {
    //Debug.Log("adding job: " + j);
    j.tile.AddJob(j);

    if (j.jobTime < 0)
    {
      j.Work(0);
    }
    else
    {
      if (j.recipe != null)
      {
        foreach (string resourceName in j.recipe.resources.Keys)
        {
          //(Tile tile, Action<Job> cbJobComplete, Action<Job> cbJobCancelled, JOB_TYPE type, string description, Recipe recipe, string name) {
          Job nj = new Job(j.tile, HaulJobComplete, HaulJobCancelled, JOB_TYPE.HAUL, JOB_TYPE.HAUL.ToString(), j.recipe, resourceName, j);
          Add(nj, 1f);
          //publicJobs.Add(nj);


        }
      }
      Add(j, 5);
      //publicJobs.Add(j);

      if (cbJobCreated != null)
      {
        cbJobCreated(j);
      }
    }

  }

  public void Update()
  {
    //List<Job> tempJobs = new List<Job>();

    //while (jobs.Count > 0) {
    //  tempJobs.Add(jobs.Dequeue());
    //}



    foreach (Job job in publicJobs)
    {

      if (job.jobType == JOB_TYPE.HAUL)
      {
        job.priority = 1;

        //Add(job, 1);
      }
      else if (job.jobType == JOB_TYPE.BUILD)
      {
        if (job.IsRecipeFinished())
        {
          job.priority = 0.5f;
        }
        else
        {
          job.priority = 1.5f;
        }
      }
      jobs.UpdatePriority(job, job.priority);
    }
  }

  public void HaulToTileComplete(Job job)
  {
    //job.tile.PlaceInventoryItem(job)
    job.AddToLog("haul to tile complete");
    job.tile.RemoveJob(job);
  }

  public void HaulJobComplete(Job job)
  {
    job.AddToLog("haul complete");
    job.recipe.Add(job.recipeResourceName, job.qtyFulfilled);
    job.tile.RemoveJob(job);

  }

  public void HaulJobCancelled(Job job)
  {
    job.AddToLog("haul cancelled");
    job.tile.RemoveJob(job);
  }

  public void ReturnJob(Job job)
  {
    job.AddToLog("job returned");
    problemJobs.Enqueue(job);
  }

  public Job GetNextJob()
  {

    if (problemJobs.Count > 0)
    {
      Job pj = problemJobs.Dequeue();
      Add(pj, pj.priority + 10);
    }

    for (int i = 0; i < 3; i += 1)
    {

      Job j = Pop();
      if (j == null) continue;
      if (j.recipe != null)
      {
        if (j.jobType == JOB_TYPE.HAUL)
        {

        }
        else if (!j.IsRecipeFinished())
        {
          j.AddToLog("recipe not finished returned to queue");
          Add(j, 2);
          continue;

        }
      }
      j.AddToLog("given to requester");
      return j;
    }
    return null;


  }

  private int GetCheckSum()
  {
    return (jobs.Count * 11) + (publicJobs.Count * 3);
  }

  private void Add(Job j, float p)
  {
    int c = GetCheckSum();
    jobs.Enqueue(j, p);
    publicJobs.Add(j);
    if (c + 14 != GetCheckSum())
    {
      Debug.LogError("ERROR CHECK SUM DOES NOT MATCH");
    }
    allJobs.Add(j);
  }

  private Job Pop()
  {
    Job j = null;
    while (j == null)
    {
      if (jobs.Count == 0)
      {
        break;
      }
      j = jobs.Dequeue();
      j.AddToLog("popped from jobs");
      publicJobs.Remove(j);
      //Debug.Log(j.jobType + " " + j.description + " " + j.jobTime);
      if (j.jobTime <= 0)
      {
        if (j.cbJobComplete != null)
        {
          j.cbJobComplete(j);
        }
        j = null;
      }


    }

    return j;
  }

  public void cbRegisterJobCreated(Action<Job> cb)
  {
    cbJobCreated += cb;

  }
  public void cbUnregisterJobCreated(Action<Job> cb)
  {
    cbJobCreated -= cb;

  }

  public void OnJobComplete(Job j)
  {
    j.AddToLog("onJobComplete");
  }

  public XmlSchema GetSchema()
  {
    return null;
  }

  public void ReadXml(XmlReader reader)
  {


  }

  public void WriteXml(XmlWriter writer)
  {

    writer.WriteStartElement("Jobs");

    foreach (Job j in jobs)
    {
      j.WriteXml(writer);
    }

    writer.WriteEndElement();
  }
}

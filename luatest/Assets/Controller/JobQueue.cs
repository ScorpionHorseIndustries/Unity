using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Priority_Queue;

public class JobQueue : IXmlSerializable {

  Action<Job> cbJobCreated;
  //protected Queue<Job> jobs = new Queue<Job>();
  protected SimplePriorityQueue<Job> jobs = new SimplePriorityQueue<Job>();
  private Queue<Job> problemJobs = new Queue<Job>();


  public int Count {
    get {
      return jobs.Count;
    }
  }

  public JobQueue() {

  }

  public void Push(Job j) {

    if (j.recipe != null) {
      foreach (string resourceName in j.recipe.resources.Keys) {
        //(Tile tile, Action<Job> cbJobComplete, Action<Job> cbJobCancelled, JOB_TYPE type, string description, Recipe recipe, string name) {
        Job nj = new Job(j.tile, HaulJobComplete, HaulJobCancelled, JOB_TYPE.HAUL, JOB_TYPE.HAUL.ToString(), j.recipe, resourceName, j);
        jobs.Enqueue(nj, 1f);



      }
    }
    jobs.Enqueue(j, 5);
    if (cbJobCreated != null) {
      cbJobCreated(j);
    }

  }

  public void Update() {
    List<Job> tempJobs = new List<Job>();

    while (jobs.Count > 0) {
      tempJobs.Add(jobs.Dequeue());
    }

    foreach (Job job in tempJobs) {
      if (job.jobType == JOB_TYPE.HAUL) {
        jobs.Enqueue(job, 1);
      } else if (job.jobType == JOB_TYPE.BUILD) { 
        if (job.IsRecipeFinished()) {
          jobs.Enqueue(job, 0.5f);
        } else {
          jobs.Enqueue(job, 1.5f);
        }
      }
    }
  }

  public void HaulJobComplete(Job job) {
    job.recipe.Add(job.recipeResourceName, job.recipe.resources[job.recipeResourceName].qtyRequired);
    job.tile.RemoveJob(job);

  }

  public void HaulJobCancelled(Job job) {
    job.tile.RemoveJob(job);
  }

  public void ReturnJob(Job j) {
    problemJobs.Enqueue(j);
  }

  public Job GetNextJob() {
    if (jobs.Count == 0) {
      if (problemJobs.Count > 0) {
        jobs.Enqueue(problemJobs.Dequeue(), 1);


      }
      return null;
    } else {


      for (int i = 0; i < 3; i += 1) {
        Job j = Pop();

        if (j.recipe != null) {
          if (j.jobType == JOB_TYPE.HAUL) {

          } else if (!j.IsRecipeFinished()) {
            jobs.Enqueue(j, 2);
            continue;

          }
        }
        return j;
      }
      return null;
    }

  }

  private Job Pop() {
    return jobs.Dequeue();
  }

  public void cbRegisterJobCreated(Action<Job> cb) {
    cbJobCreated += cb;

  }
  public void cbUnregisterJobCreated(Action<Job> cb) {
    cbJobCreated -= cb;

  }

  public void OnJobComplete(Job j) {

  }

  public XmlSchema GetSchema() {
    return null;
  }

  public void ReadXml(XmlReader reader) {


  }

  public void WriteXml(XmlWriter writer) {

    writer.WriteStartElement("Jobs");

    foreach (Job j in jobs) {
      j.WriteXml(writer);
    }

    writer.WriteEndElement();
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;



public class Job : IXmlSerializable {

  struct JobResource {
    string name;
    int qtyRequired;
    int qtyDelivered;
    int qtyAssigned;
  }

  private List<JobResource> resourcesRequired = new List<JobResource>();
  

  public readonly string jobId;
  public bool finished { get; private set; } = false;
  public bool cancelled { get; private set; } = false;

  public Tile tile { get; private set; }
  public float jobTime { get; private set; }
  Action<Job> cbJobComplete;
  Action<Job> cbJobCancelled;
  public string jobType { get; protected set; } = "NONE";
  public Job(Tile tile, Action<Job> cbJobComplete, Action<Job> cbJobCancelled, float jobTime, string jobType) {
    this.tile = tile;
    this.jobTime = jobTime;
    this.cbJobComplete += cbJobComplete;
    this.cbJobCancelled += cbJobCancelled;
    this.jobType = jobType;
    this.jobId = "job_"+Guid.NewGuid().ToString();

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

    writer.WriteElementString("type", j.jobType);
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

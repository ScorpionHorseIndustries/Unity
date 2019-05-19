using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class JobQueue : IXmlSerializable {

  Action<Job> cbJobCreated;
  protected Queue<Job> jobs = new Queue<Job>();
  private Queue<Job> problemJobs = new Queue<Job>();


  public int Count { get {
      return jobs.Count;
    } }

  public JobQueue() {

  }

  public void Push(Job j) {
    jobs.Enqueue(j);
    if (cbJobCreated != null) {
      cbJobCreated(j);
    }
    
  }

  public void ReturnJob(Job j) {
    problemJobs.Enqueue(j);
  }

  public Job GetNextJob() {
    if (jobs.Count == 0) {
      if (problemJobs.Count > 0) {
        return problemJobs.Dequeue();
        
      }
      return null;
    } else {
      return Pop();
    }
    
  }

  private Job Pop() {
    return jobs.Dequeue();
  }

  public Job Peek() {
    return jobs.Peek();
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

    foreach(Job j in jobs) {
      j.WriteXml(writer);
    }

    writer.WriteEndElement();
  }
}

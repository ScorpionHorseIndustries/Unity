using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class JobQueue {

  Action<Job> cbJobCreated;
  protected Queue<Job> jobs = new Queue<Job>();
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

  public Job Pop() {
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
}

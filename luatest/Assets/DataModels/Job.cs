using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Job {

	public Tile tile { get; private set; }
	float jobTime;
	Action<Job> cbJobComplete;
	Action<Job> cbJobCancelled;
  public string jobType { get; protected set; } = "NONE";
	public Job(Tile tile, Action<Job> cbJobComplete, Action<Job> cbJobCancelled, float jobTime, string jobType) {
		this.tile = tile;
		this.jobTime = jobTime;
		this.cbJobComplete += cbJobComplete;
		this.cbJobCancelled += cbJobCancelled;
    this.jobType = jobType;

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
		}
	}

	public void Cancel() {
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
}

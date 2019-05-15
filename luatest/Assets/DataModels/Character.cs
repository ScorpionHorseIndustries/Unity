using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public class Character  {


  Action<Character> cbCharacterChanged;
  World world;
  public float X {
    get {
      return Mathf.Lerp(PosTile.x, DstTile.x, movementPerc);
    }
  }

  public float Y {
    get {
      return Mathf.Lerp(PosTile.y, DstTile.y, movementPerc);
    }
  }

  public string name { get; private set; }

  public bool CanMoveDiagonally { get; private set; } = true;
  public string spriteName { get; private set; } = "robot_front";

  float movementSpeed = 2; //tiles per second

  public Tile PosTile { get; private set; }
  public Tile DstTile { get; private set; }
  
  float movementPerc = 0;
  float cd = 1;
  private bool changed = false;
  private Job myJob;

  public Character (World world, Tile startTile) {
    PosTile = DstTile = startTile;
    this.world = world;


  }

  public void Update(float deltaTime) {

    if (myJob == null) {
      myJob = world.jobQueue.GetNextJob();


      if (myJob != null) {
        myJob.cbRegisterJobComplete(OnJobEnded);
        myJob.cbUnregisterJobCancelled(OnJobEnded);
        DstTile = myJob.tile;
        //SetDestination(myJob.tile);
      }
    }


    changed = false;
    //Debug.Log("Character.Update" + deltaTime);
    if (PosTile != DstTile) {
      float distanceToTravel = Funcs.Distance(PosTile, DstTile);
      float distThisFrame = movementSpeed * deltaTime;
      movementPerc += (distThisFrame / distanceToTravel);
      movementPerc = Mathf.Clamp(movementPerc, 0, 1);

      if (movementPerc >= 1) {
        PosTile = DstTile;
        movementPerc = 0;
      }
      changed = true;
    } else {
      if (myJob != null) {
        myJob.Work(deltaTime);
 
      }

    }

    if (changed) {
      if (cbCharacterChanged != null) {
        cbCharacterChanged(this);
      }
    }

  }

  private void OnJobEnded(Job j) {
    //job completed or cancelled
    if (j != myJob || myJob == null) {
      Debug.LogError("telling character about a job that does not belong to them");

    } else {
      myJob = null;
    }
  }

  public bool SetDestination(Tile dst) {
    if (dst.IsNeighbour(PosTile, CanMoveDiagonally)) {
      DstTile = dst;
      return true;
    } else {
      Debug.Log("Character SetDestination was given a tile that was not a neighbour of the current tile...");
      return false;
    }



  }

  public void CBRegisterOnChanged(Action<Character> cb) {
    cbCharacterChanged += cb;
  }

  public void CBUnregisterOnChanged(Action<Character> cb) {
    cbCharacterChanged -= cb;
  }

}
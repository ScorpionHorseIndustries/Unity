using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BuildController : MonoBehaviour {
  public WorldController wcon;
  //public World world;
  public InputController inputCon;
 

  private void Awake() {

  }

  public void Init() {
    //WorldController.Instance.cbRegisterReady(Init);
    Debug.Log("init " + this.name);
    wcon = WorldController.Instance;
    //world = wcon.world;
  }
  private void Start() {


  }
  public enum BUILD_MODE {
    NONE,
    INSTALLEDITEM,
    TILE,
    ZONE, //shouldn't be used...so... why is it there?
    DECONSTRUCT
  }

  public BUILD_MODE buildMode { get; private set; } = BUILD_MODE.NONE;
  public string build { get; private set; } = "";

  public void SetBuildMode(BUILD_MODE bt, string b) {
    buildMode = bt;
    build = b;
  }

  public bool CreateBuildJobs(List<Tile> tiles) {
    bool didSomething = true;
    string localBuild = build; 

    if (buildMode != BUILD_MODE.NONE) {
      switch (buildMode) {
        //InstalledItem item = 
        case BUILD_MODE.INSTALLEDITEM:
          CreateInstalledItemJobs(tiles, localBuild);
          break;
        case BUILD_MODE.TILE:
          foreach (Tile tile in tiles) {
            TileType tt = TileType.TYPES[build];
            tile.SetType(tt);
          }
          break;
        case BUILD_MODE.ZONE:
          //wcon.world.zones.Add(new TileZone(tiles));
          //do nothing zones not implemented yet
          break;
        case BUILD_MODE.DECONSTRUCT:
          CreateRemoveInstalledItemJobs(tiles);
          break;


      }
    } else {
      didSomething = false;
    }
    build = "";
    buildMode = BUILD_MODE.NONE;

    return didSomething;
  }

  private void CreateInstalledItemJobs(List<Tile> tiles, string localBuild) {
    foreach (Tile tile in tiles) {
      //tile.placeInstalledObject();
      string localRecipe = InstalledItem.GetRecipeName(localBuild);
      if (wcon.world.isInstalledItemPositionValid(wcon.world, build, tile)) {
        Job j = new Job(
              tile,
              OnInstalledItemJobComplete, //(theJob) => { OnInstalledItemJobComplete(localBuild, theJob.tile); },
              OnInstalledItemJobCancelled,
              JOB_TYPE.BUILD,
              1,
              localBuild
            );
        tile.AddJob(j);
        wcon.world.jobQueue.Push(j);

        //tile.pendingJob = true;
        //Debug.Log("jobs in queue: " +world.jobQueue.Count);

      }
    }
  }
  private void CreateRemoveInstalledItemJobs(List<Tile> tiles) {
    foreach (Tile tile in tiles) {
      //tile.placeInstalledObject();
      //string localRecipe = InstalledItem.GetRecipeName(localBuild);
      if (tile.installedItem != null && tile.installedItem.tile == tile) { //if the tile has an installed item and that tile is the primary tile for that installed item
        Job j = new Job(
              tile,
              OnRemoveInstalledItemJobComplete, //(theJob) => { OnInstalledItemJobComplete(localBuild, theJob.tile); },
              OnRemoveInstalledItemJobCancelled,
              JOB_TYPE.DECONSTRUCT,
              1,
              InstalledItem.DECONSTRUCT
            );
        tile.AddJob(j);
        wcon.world.jobQueue.Push(j);

        //tile.pendingJob = true;
        //Debug.Log("jobs in queue: " +world.jobQueue.Count);

      }
    }
  }

  private void OnRemoveInstalledItemJobComplete(Job job) {
    job.tile.RemoveJob(job);
    if (job.tile.installedItem != null) {
      job.tile.installedItem.Deconstruct();
    }
  }

  private void OnRemoveInstalledItemJobCancelled(Job job) {
    job.tile.RemoveJob(job);
  }

  private void OnInstalledItemJobComplete(Job job) {
    //Debug.Log("installed item job complete: " + job);
    if (!job.tile.RemoveJob(job)) {
      Debug.Log("could not remove job from tile");
    }
    //job.tile.pendingJob = false;
    if (wcon.world.PlaceInstalledItem(job.description, job.tile) == null) {
      job.inventory.Explode();
    }

  }

  //private void OnInstalledItemJobComplete(string itemToBuild, Tile tile) {
    
    
  //}

  private void OnInstalledItemJobCancelled(Job job) {

    job.tile.RemoveJob(job);
  }
}

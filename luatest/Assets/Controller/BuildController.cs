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
  public enum BUILDTYPE {
    NONE, INSTALLEDITEM, TILE,ZONE
  }

  private BUILDTYPE buildType = BUILDTYPE.NONE;
  private string build = "";

  public void SetBuild(BUILDTYPE bt, string b) {
    buildType = bt;
    build = b;
  }

  public void CreateBuildJobs(List<Tile> tiles) {

    string localBuild = build; 

    if (buildType != BUILDTYPE.NONE) {
      switch (buildType) {
        //InstalledItem item = 
        case BUILDTYPE.INSTALLEDITEM:
          foreach (Tile tile in tiles) {
            //tile.placeInstalledObject();
            string localRecipe = InstalledItem.GetRecipeName(localBuild);
            if (wcon.world.isInstalledItemPositionValid(wcon.world,build, tile)) {
              Job j = new Job(
                    tile,
                    OnInstalledItemJobComplete, //(theJob) => { OnInstalledItemJobComplete(localBuild, theJob.tile); },
                    OnInstalledItemJobCancelled,
                    JOB_TYPE.BUILD,
                    1,
                    localBuild
                  );
              wcon.world.jobQueue.Push(j);
              tile.AddJob(j);
              //tile.pendingJob = true;
              //Debug.Log("jobs in queue: " +world.jobQueue.Count);

            }
          }
          break;
        case BUILDTYPE.TILE:
          foreach (Tile tile in tiles) {
            TileType tt = TileType.TYPES[build];
            tile.SetType(tt);
          }
          break;
        case BUILDTYPE.ZONE:
          wcon.world.zones.Add(new TileZone(tiles));
          break;


      }
    }
    build = "";
    buildType = BUILDTYPE.NONE;
  }

  private void OnInstalledItemJobComplete(Job job) {
    job.tile.RemoveJob(job);
    //job.tile.pendingJob = false;
    wcon.world.PlaceInstalledItem(job.description, job.tile);

  }

  //private void OnInstalledItemJobComplete(string itemToBuild, Tile tile) {
    
    
  //}

  private void OnInstalledItemJobCancelled(Job job) {

    job.tile.RemoveJob(job);
  }
}

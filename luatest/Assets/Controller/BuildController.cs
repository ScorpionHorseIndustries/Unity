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
    NONE, INSTALLEDITEM, TILE
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
            if (wcon.world.isInstalledItemPositionValid(wcon.world,build, tile)) {
              Job j = new Job(
                    tile,
                    (theJob) => { OnInstalledItemJobComplete(localBuild, theJob.tile); },
                    (theJob) => { OnInstalledItemJobCancelled(theJob); },
                    1,
                    localBuild
                  );
              wcon.world.jobQueue.Push(j);
              tile.pendingJob = true;
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


      }
    }
    build = "";
    buildType = BUILDTYPE.NONE;
  }

  private void OnInstalledItemJobComplete(string itemToBuild, Tile tile) {
    tile.pendingJob = false;
    wcon.world.PlaceInstalledItem(itemToBuild, tile);
    
  }

  private void OnInstalledItemJobCancelled(Job job) {
    job.tile.pendingJob = false;
  }
}

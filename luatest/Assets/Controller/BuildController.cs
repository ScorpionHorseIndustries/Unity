using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BuildController : MonoBehaviour {
  public WorldController wcon;
  public World world;
  public InputController inputCon;
  public static BuildController Instance { get; private set; }

  private void Awake() {
    if (Instance != null) {
      Debug.LogError("THERE SHOULD ONLY BE BUILD CONTROLLER");
    } else {
      Instance = this;
    }
  }

  private void Init(string s) {
    Debug.Log("init " + this.name);
    wcon = WorldController.Instance;
    world = wcon.world;
  }
  private void Start() {
    WorldController.Instance.cbRegisterReady(Init);


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

    if (buildType != BUILDTYPE.NONE) {
      switch (buildType) {
        //InstalledItem item = 
        case BUILDTYPE.INSTALLEDITEM:
          foreach (Tile tile in tiles) {
            //tile.placeInstalledObject();
            if (world.isInstalledItemPositionValid(build, tile)) {
              Job j = new Job(
                    tile,
                    (theJob) => { OnInstalledItemJobComplete(String.Copy(build), theJob.tile); },
                    (theJob) => { OnInstalledItemJobCancelled(theJob); },
                    1,
                    String.Copy(build)
                  );
              world.jobQueue.Push(j);
              tile.pendingJob = true;
              Debug.Log("jobs in queue: " +world.jobQueue.Count);

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
    world.PlaceInstalledObject(itemToBuild, tile);
    tile.pendingJob = false;
  }

  private void OnInstalledItemJobCancelled(Job job) {
    job.tile.pendingJob = false;
  }
}

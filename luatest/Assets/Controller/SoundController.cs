﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour {
  WorldController worldController;
  //World world;
  public static SoundController Instance;

  private void Awake() {
    if (Instance != null) {
      Debug.LogError("THERE SHOULD ONLY BE ONE WORLD CONTROLLER YOU DING DONG");
    }
    Instance = this;
  }

  private SoundController() {

  }


  // Start is called before the first frame update
  void Start() {
    //WorldController.Instance.cbRegisterReady(Init);
    Debug.Log("init " + this.name);
    worldController = WorldController.Instance;
    
    worldController.world.RegisterInstalledItemCB(OnInstalledItemCreated);
    worldController.world.CBRegisterTileChanged(OnTileTypeChanged);
  }

  public void OnTileTypeChanged(Tile t) {
    //Debug.Log("play tile changed sound");

  }

  public void OnInstalledItemCreated(InstalledItem item) {
    //Debug.Log("play item created sound");

  }
}

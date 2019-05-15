using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
	WorldController worldController;
	World world;
	public static SoundController Instance;

	private void Awake() {
		if (Instance != null) {
			Debug.LogError("THERE SHOULD ONLY BE ONE WORLD CONTROLLER YOU DING DONG");
		}
		Instance = this;
	}

	private SoundController() {

	}

  void Init(string s) {
    Debug.Log("init " + this.name);
    worldController = WorldController.Instance;
    world = worldController.world;
    world.RegisterInstalledItemCB(OnInstalledItemCreated);
    world.RegisterTileChangedCB(OnTileTypeChanged);

  }
  // Start is called before the first frame update
  void Start()
    {
    WorldController.Instance.cbRegisterReady(Init);
  
    }

    public void OnTileTypeChanged(Tile t) {
		//Debug.Log("play tile changed sound");

	}

	public void OnInstalledItemCreated(InstalledItem item) {
		//Debug.Log("play item created sound");

	}
}

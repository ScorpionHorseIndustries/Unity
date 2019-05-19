using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashController : MonoBehaviour {

  Dictionary<string, InstalledItem> protos;
  List<InstalledItem> trashList;
  
  WorldController wcon;
  private readonly int amountOfTrash = 10;
  private int countOfTrash = 0;
  // Start is called before the first frame update

  public static TrashController Instance { get; private set; }
  private void Awake() {
    if (Instance != null) {
      Debug.LogError("THERE SHOULD ONLY BE ONE SPRITE CONTROLLER YOU DING DONG");
    }
    Instance = this;
  }
  void Init(string s) {

  }
    
  
  void Start() {
    //WorldController.Instance.cbRegisterReady(Init);

    Debug.Log("init " + this.name);
    wcon = WorldController.Instance;
    protos = wcon.world.getProtoList();
    trashList = new List<InstalledItem>();

    foreach (string ss in protos.Keys) {
      InstalledItem item = protos[ss];
      if (item.trash) {
        trashList.Add(item);
      }
    }
    PlaceTrash();

  }

  void PlaceTrash() {
    while(countOfTrash < amountOfTrash) {
      int x = Random.Range(0, wcon.world.width);
      int y = Random.Range(0, wcon.world.height);

      Tile tile = wcon.world.getTileAt(x, y);
      InstalledItem item = trashList[Random.Range(0, trashList.Count)];
      if (wcon.world.isInstalledItemPositionValid(item.type, tile)) {
        wcon.world.PlaceInstalledObject(item.type, tile);
        countOfTrash += 1;
      }
    }
  }

 
}

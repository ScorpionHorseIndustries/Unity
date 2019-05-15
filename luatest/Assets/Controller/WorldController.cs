
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;


public class WorldController : MonoBehaviour {

  [SerializeField]
  public GameObject buildProgressSprite;
  public GameObject cashText;
  private Dictionary<Tile, GameObject> tilesGameObjectsMap;
  private Dictionary<InstalledItem, GameObject> installedItemGameObjects;


  public EventSystem eventSystem;
  public static WorldController Instance { get; private set; }
  public SpriteController sprCon;
  public JobController jobCon;

  private float countdown = 2f;
  private float money = 0;
  //public Sprite dirtSprite;
  //public Sprite grassSprite;
  public World world { get; private set; }
  // Start is called before the first frame update
  private void Awake() {
    if (Instance != null) {
      Debug.LogError("THERE SHOULD ONLY BE ONE WORLD CONTROLLER YOU DING DONG");
    }
    Instance = this;
  }
  Action<string> cbReady;
  private const int EXPECTED = 5;
  private int actualReady = 0;
  private bool initDone = false;
  public void cbRegisterReady(Action<string> cb) {
    cbReady += cb;
    actualReady += 1;
  }

  void Start() {



    TileType.LoadFromFile();

    tilesGameObjectsMap = new Dictionary<Tile, GameObject>();
    installedItemGameObjects = new Dictionary<InstalledItem, GameObject>();
    eventSystem = EventSystem.current;
    Debug.Log("create world");
    world = new World();
    Debug.Log("create tile objects");
    createTileGameObjects();
    world.RandomiseTiles();
    //create game objects for tiles
    world.RegisterInstalledItemCB(OnInstalledItemCreated);

    sprCon = SpriteController.Instance;
    sprCon.wcon = this;
    sprCon.world = this.world;

    world.jobQueue.cbRegisterJobCreated(OnJobCreated);

  }

  private void addCurrency(float amt) {
    money += amt;
    cashText.GetComponent<Text>().text = string.Format("{0:00.00}", money);

  }

  // Update is called once per frame
  void Update() {

    if (actualReady < EXPECTED) {
      return;
    } else {
      if (!initDone) {
        cbReady("hi");
        initDone = true;
      }
    }

    countdown -= Time.deltaTime;
    if (countdown < 0) {
      addCurrency(UnityEngine.Random.Range(-1f, 4f));
      countdown = 2;
    }


  }

  public GameObject GetGameObjectFromTile(Tile t) {
    if (tilesGameObjectsMap.ContainsKey(t)) {
      return tilesGameObjectsMap[t];
    } else {
      return null;
    }
  }

  public GameObject GetGameObjectFromInstalledItem(InstalledItem item) {
    if (installedItemGameObjects.ContainsKey(item)) {
      return installedItemGameObjects[item];
    } else {
      return null;
    }
  }

  //public void OnTileTypeChanged(Tile t, GameObject go)
  //{
  //	setTileSprite(t, go.GetComponent<SpriteRenderer>());
  //}

  public void createTileGameObjects() {
    for (int x = 0; x < world.width; x += 1) {
      for (int y = 0; y < world.height; y += 1) {
        Tile t = world.getTileAt(x, y);
        GameObject tile_go = new GameObject();
        tile_go.name = "tile_" + x + "_" + y;
        tile_go.transform.Translate(t.x, t.y, 0);
        tile_go.transform.SetParent(this.transform, true);
        tile_go.AddComponent<SpriteRenderer>();

        tilesGameObjectsMap.Add(t, tile_go);

        t.cbRegisterOnChanged(SetTileSprite);

      }
    }
  }

  public void SetTileSprite(Tile t) {
    SpriteController.Instance.SetTileSprite(t);
  }

  public void OnInstalledItemCreated(InstalledItem inst) {
    //create a visible game object

    GameObject go = new GameObject();
    installedItemGameObjects.Add(inst, go);

    go.name = inst.type + "_" + inst.tile.x + "_" + inst.tile.y;
    go.transform.position = new Vector2(inst.tile.x, inst.tile.y);
    SpriteRenderer spr = go.AddComponent<SpriteRenderer>();
    SpriteHolder sh = SpriteController.Instance.GetSprite(inst);
    if (sh.r != 0) {
      spr.transform.Rotate(0, 0, sh.r);
    }
    //Debug.Log(sh.s + " @ r:" + sh.r);

    spr.sprite = sh.s; // sprites[inst.spriteName];
    spr.sortingOrder += 1;
    go.transform.SetParent(this.transform, true);

    //world.RegisterInstalledItemCB(inst);
    inst.RegisterCB(OnInstalledItemChanged);

  }

  void OnJobCreated(Job j) {

    sprCon.JobCreated(j);
    j.cbRegisterJobComplete(OnJobEnded);
    j.cbRegisterJobCancelled(OnJobEnded);

    GameObject g = SimplePool.Spawn(buildProgressSprite, new Vector2(j.tile.x, j.tile.y), Quaternion.identity);
    g.transform.SetParent(this.transform, true);
    /*
foreach (Tile tile in dragArea)
{
  GameObject g = SimplePool.Spawn(cursorPrefab, new Vector2(tile.x, tile.y), Quaternion.identity);
  g.transform.SetParent(this.transform, true);
  dragPreviewList.Add(g);

}
*/
  }

  void OnJobEnded(Job j) {
    //delete sprites
  }


  void OnInstalledItemChanged(InstalledItem item) {
    //Debug.LogError("OnInstalledItemChanged " + item + " NOT IMPLEMENTED");

    if (installedItemGameObjects.ContainsKey(item)) {
      GameObject go = installedItemGameObjects[item];
      SpriteRenderer spr = go.GetComponent<SpriteRenderer>();
      SpriteHolder sh = SpriteController.Instance.GetSprite(item);
      spr.sprite = sh.s;
      spr.transform.rotation = Quaternion.identity;
      spr.transform.Rotate(0, 0, sh.r);
    }

  }

  public Dictionary<string, Tile> getNeighbours(InstalledItem item) {
    return getNeighbours(item.tile);
  }

  public Dictionary<string, Tile> getNeighbours(Tile t) {
    Dictionary<string, Tile> dct = new Dictionary<string, Tile>();

    dct["north"] = world.getTileAt(t.x, t.y + 1);
    dct["south"] = world.getTileAt(t.x, t.y - 1);
    dct["east"] = world.getTileAt(t.x + 1, t.y);
    dct["west"] = world.getTileAt(t.x - 1, t.y);


    return dct;
  }


  private bool hasMatchingNeighbour(Tile t, InstalledItem item) {
    if (t == null || t.installedItem == null || !t.installedItem.type.Equals(item.type)) {
      return false;
    } else {
      return true;
    }

  }

}



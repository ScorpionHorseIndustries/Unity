
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Xml.Serialization;
using System.IO;
using UnityEngine.SceneManagement;

public class WorldController : MonoBehaviour {

  [SerializeField]
  public GameObject buildProgressSprite;
  public GameObject cashText;
  public GameObject LinePrefab;
  public GameObject TextPrefab;
  public GameObject currentTileText;


  private Dictionary<Tile, GameObject> Tiles_GO_Map;
  private Dictionary<InstalledItem, GameObject> InstalledItems_GO_Map;
  private Dictionary<Character, GameObject> Characters_GO_Map;
  private Dictionary<Job, GameObject> Job_GO_Map;
  private Dictionary<InventoryItem, GameObject> invItem_GO_Map;

  public EventSystem eventSystem;
  public static WorldController Instance { get; private set; }
  public GameObject prfSpriteController;
  public GameObject prfJobController;
  public GameObject prfInputController;
  //public GameObject prfTrashController;
  public GameObject prfBuildController;
  public GameObject prfSoundController;
  public GameObject prfInventoryItemText;

  public SpriteController spriteController;
  public JobController jobController;
  public InputController inputController;
  public BuildController buildController;
  public SoundController soundController;
  //public TrashController trashController;
  private List<GameObject> controllers = new List<GameObject>();

  private float countdown = 2f;
  private float money = 0;
  //public Sprite dirtSprite;
  //public Sprite grassSprite;
  public World world { get; private set; }
  public static bool loadWorld { get; private set; }

  // Start is called before the first frame update
  private void Awake() {
    if (Instance != null) {
      Debug.LogError("THERE SHOULD ONLY BE ONE WORLD CONTROLLER YOU DING DONG");
    }
    Instance = this;
  }

  //Action<string> cbReady;
  //private const int EXPECTED = 5;
  //private int actualReady = 0;
  //private bool initDone = false;
  //public void cbRegisterReady(Action<string> cb) {
  //  cbReady += cb;
  //  actualReady += 1;
  //}

  public void CreateWorldFromSave() {
    //Init();

    XmlSerializer xml = new XmlSerializer(typeof(World));

    //Debug.Log("save string = " + PlayerPrefs.GetString("save"));
    TextReader reader = new StringReader(PlayerPrefs.GetString("save"));

    world = (World)xml.Deserialize(reader);

    reader.Close();

    createTileGameObjects();
    foreach (Tile t in Tiles_GO_Map.Keys) {
      SetTileSprite(t);


    }
    world.RegisterInstalledItemCB(OnInstalledItemCreated);

    foreach (Character chr in world.characters) {
      OnCharacterCreated(chr);

    }

    //spriteController = SpriteController.Instance;
    //spriteController.wcon = this;
    //spriteController.world = this.world;

    world.jobQueue.cbRegisterJobCreated(OnJobCreated);
    world.CBRegisterCharacterChanged(OnCharacterChanged);
    world.CBRegisterCharacterCreated(OnCharacterCreated);
    world.CBRegisterCharacterKilled(OnCharacterKilled);
    world.CBRegisterInventoryItemCreated(OnInventoryItemCreated);
    world.CBRegisterInventoryItemChanged(OnInventoryItemChanged);
    world.CBRegisterInventoryItemDestroyed(OnInventryItemDestoyed);
    world.SetAllNeighbours();
    world.SetInstalledFromArray();

    foreach (Tile t in Tiles_GO_Map.Keys) {
      //SetTileSprite(t);
      if (t.installedItem != null) {
        OnInstalledItemCreated(t.installedItem);

      }

    }

    world.SetJoinedSprites();
    //Debug.Log(writer.ToString());
    //world = new World();
  }

  public void CreateNewWorld() {




    world = new World(World.TEST_WIDTH, World.TEST_HEIGHT);

    createTileGameObjects();
    //world.RandomiseTiles();
    MapGenerator.MakeNewMap(world, world.height, world.width);
    //create game objects for tiles

    world.RegisterInstalledItemCB(OnInstalledItemCreated);

    //spriteController = SpriteController.Instance;
    //spriteController.wcon = this;
    //spriteController.world = this.world;

    world.jobQueue.cbRegisterJobCreated(OnJobCreated);
    world.CBRegisterCharacterChanged(OnCharacterChanged);
    world.CBRegisterCharacterCreated(OnCharacterCreated);
    world.CBRegisterCharacterKilled(OnCharacterKilled);
    world.CBRegisterInventoryItemCreated(OnInventoryItemCreated);
    world.CBRegisterInventoryItemChanged(OnInventoryItemChanged);
    world.CBRegisterInventoryItemDestroyed(OnInventryItemDestoyed);
    world.CreateCharacters();

    world.SetAllNeighbours();

    world.PlaceTrash();

    for (int i = 0; i < 4; i += 1) {
      Tile a = world.GetRandomEmptyTile();
      Tile b = world.GetRandomEmptyTile();
      Tile c = world.GetRandomEmptyTile();
      if (a != null)
        world.PlaceInventoryItem("inv::steel_plates", a, UnityEngine.Random.Range(1, 32));
      if (b != null)
        world.PlaceInventoryItem("inv::copper_plates", b, UnityEngine.Random.Range(1, 32));
      if (c != null)
        world.PlaceInventoryItem("inv::stone_slabs", c, UnityEngine.Random.Range(1, 32));
    }
    //world.nodeMap = new TileNodeMap(world);

    //initDone = false;
  }

  public void Init() {
    CreateControllers();

    TileType.LoadFromFile();

    Tiles_GO_Map = new Dictionary<Tile, GameObject>();
    InstalledItems_GO_Map = new Dictionary<InstalledItem, GameObject>();
    Characters_GO_Map = new Dictionary<Character, GameObject>();
    Job_GO_Map = new Dictionary<Job, GameObject>();
    invItem_GO_Map = new Dictionary<InventoryItem, GameObject>();

    eventSystem = EventSystem.current;

  }

  public void Restart() {
    world.Kill();
    world = null;

    foreach (Tile t in Tiles_GO_Map.Keys) {
      GameObject go = Tiles_GO_Map[t];
      if (t.installedItem != null) {
        t.installedItem.Destroy();
      }
      Destroy(go);



    }

    foreach (Job j in Job_GO_Map.Keys) {
      Destroy(Job_GO_Map[j]);
    }

    foreach(InventoryItem item in invItem_GO_Map.Keys) {

    }
    Job_GO_Map = null;
    Tiles_GO_Map.Clear();
    Tiles_GO_Map = null;
    invItem_GO_Map.Clear();
    invItem_GO_Map = null;
    DestroyControllers();
    Init();
    CreateNewWorld();
    InitialiseControllers();
    
  }

  private void CreateControllers() {
    InstantiateController(prfSpriteController);
    InstantiateController(prfInputController);
    InstantiateController(prfBuildController);

    InstantiateController(prfJobController);
    //InstantiateController(prfTrashController);
    InstantiateController(prfSoundController);
    this.spriteController = FindObjectOfType<SpriteController>();//FindSpriteController.Instance;
    this.spriteController.wcon = this;
    this.inputController = FindObjectOfType<InputController>();//InputController.Instance;

    this.buildController = FindObjectOfType<BuildController>();//.Instance;
    this.jobController = FindObjectOfType<JobController>();//.Instance;
    //this.trashController = FindObjectOfType<TrashController>();//.Instance;
    this.soundController = FindObjectOfType<SoundController>();//.Instance;
  }

  private void InitialiseControllers() {
    inputController.Init();
    buildController.Init();
    //trashController.Init();
    soundController.Init();
    jobController.Init();

  }

  private void OnDrawGizmos() {

    //foreach (Room room in world.rooms) {
    //  Gizmos.color = room.roomColour;
    //  foreach(Tile t in room.getTiles()) {
    //    Gizmos.DrawLine()
    //  }
    //}
    Gizmos.color = Color.white;
    if (world != null && world.characters != null) {
      foreach (Character chr in world.characters) {

        if (chr.path != null) {
          Vector2 a = chr.pos;
          Vector2 b = new Vector2();
          foreach (PathNode<Tile> pnt in chr.path.path) {
            b.Set(pnt.data.x, pnt.data.y);
            Gizmos.DrawLine(a, b);
            a = b;
          }

        }
      }
    }
  }

  void Start() {


    Init();
    if (loadWorld) {
      loadWorld = false;
      CreateWorldFromSave();
    } else {
      CreateNewWorld();
    }
    InitialiseControllers();
    Debug.Log("init done " + this.name);
  }

  void InstantiateController(GameObject controllerPrefab) {
    GameObject g = Instantiate(controllerPrefab, this.transform.position, Quaternion.identity);
    g.transform.SetParent(this.transform, true);
    controllers.Add(g);

  }

  private void DestroyControllers() {
    foreach (GameObject g in controllers) {
      Destroy(g);
    }
    controllers.Clear();
  }

  public void SaveWorld() {
    XmlSerializer xml = new XmlSerializer(typeof(World));

    TextWriter writer = new StringWriter();

    xml.Serialize(writer, world);
    writer.Close();
    Debug.Log(writer.ToString());
    PlayerPrefs.SetString("save", writer.ToString());

    StreamWriter sw = new StreamWriter(Application.streamingAssetsPath + "/xml/save.xml");
    sw.Write(writer.ToString());
    sw.Close();

  }

  public void LoadWorld() {
    //reload the scene
    //reset all data
    //remove old references
    loadWorld = true;
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
  }

  private void addCurrency(float amt) {
    money += amt;
    cashText.GetComponent<Text>().text = string.Format("{0:00.00}", money);

  }

  // Update is called once per frame
  void Update() {

    //if (actualReady < EXPECTED) {
    //  Debug.Log("Ready: " + actualReady);
    //  return;
    //} else {
    //  if (!initDone) {
    //    cbReady("hi");
    //    initDone = true;
    //  }
    //}

    countdown -= Time.deltaTime;
    if (countdown < 0) {
      //addCurrency(UnityEngine.Random.Range(-1f, 4f));
      countdown = 2;
    }


    //add pause and speed controls
    world.Update(Time.deltaTime);



  }

  public void UpdateCurrentTile(Tile t) {
    Text txt = currentTileText.GetComponent<Text>();
    string displayMe = t.type.name;
    displayMe += "\nPos:(" + t.x + "," + t.y + ")";
    displayMe += "\nRoom:" + t.room.id;
    displayMe += "\nInstalled:" + (t.installedItem == null ? "" : t.installedItem.niceName);
    displayMe += "\nItems:" + (t.inventoryItem == null ? "" : t.inventoryItem.niceName + " " + t.inventoryItem.currentStack + "/" + t.inventoryItem.maxStackSize);
    displayMe += "\n" + t.WhoIsHere();



    txt.text = displayMe;



  }

  public GameObject GetGameObjectFromTile(Tile t) {
    if (Tiles_GO_Map.ContainsKey(t)) {
      return Tiles_GO_Map[t];
    } else {
      return null;
    }
  }

  public GameObject GetGameObjectFromInstalledItem(InstalledItem item) {
    if (InstalledItems_GO_Map.ContainsKey(item)) {
      return InstalledItems_GO_Map[item];
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
        GameObject go = CreateGameObject("tile_" + x + "_" + y, x, y);
        //GameObject tile_go = new GameObject();
        //tile_go.name = "tile_" + x + "_" + y;
        //tile_go.transform.Translate(t.x, t.y, 0);
        //tile_go.transform.SetParent(this.transform, true);
        //tile_go.AddComponent<SpriteRenderer>();

        Tiles_GO_Map.Add(t, go);
        go.GetComponent<SpriteRenderer>().sortingLayerName = "Ground";

        t.cbRegisterOnChanged(SetTileSprite);

      }
    }
  }

  public GameObject CreateGameObject(string name, int x, int y, bool withSprite = true) {
    GameObject go = new GameObject();
    go.name = name;
    go.transform.Translate(x, y, 0);
    go.transform.SetParent(this.transform, true);

    if (withSprite) {
      go.AddComponent<SpriteRenderer>();
    }

    return go;

  }

  public void SetTileSprite(Tile t) {
    spriteController.SetTileSprite(t);
  }

  public void OnInstalledItemCreated(InstalledItem inst) {
    //create a visible game object

    if (InstalledItems_GO_Map.ContainsKey(inst)) {
      return;
    }
    GameObject go = CreateGameObject(inst.type + "_" + inst.tile.x + "_" + inst.tile.y, inst.tile.x, inst.tile.y);

    InstalledItems_GO_Map.Add(inst, go);
    SpriteRenderer spr = go.GetComponent<SpriteRenderer>();


    SpriteHolder sh = spriteController.GetSprite(inst);
    if (inst.type == "installed::door") {
      spriteController.GetDoorSprite(inst, sh);
    }
    if (sh.r != 0) {
      spr.transform.Rotate(0, 0, sh.r);
    }
    //Debug.Log(sh.s + " @ r:" + sh.r);

    spr.sprite = sh.s; // sprites[inst.spriteName];
    spr.sortingOrder += 1;
    go.transform.SetParent(this.transform, true);
    spr.sortingLayerName = "Objects";

    //world.RegisterInstalledItemCB(inst);
    inst.CBRegisterChanged(OnInstalledItemChanged);
    inst.CBRegisterDestroyed(OnInstalledItemDestroyed);


  }

  void OnInstalledItemChanged(InstalledItem item) {
    //Debug.LogError("OnInstalledItemChanged " + item + " NOT IMPLEMENTED");

    if (InstalledItems_GO_Map.ContainsKey(item)) {
      GameObject go = InstalledItems_GO_Map[item];
      SpriteRenderer spr = go.GetComponent<SpriteRenderer>();

      SpriteHolder sh = spriteController.GetSprite(item);
      if (item.type == "installed::door") {
        spriteController.GetDoorSprite(item, sh);
      }

      spr.sprite = sh.s;
      spr.transform.rotation = Quaternion.identity;
      spr.transform.Rotate(0, 0, sh.r);
    }

  }

  void OnInstalledItemDestroyed(InstalledItem item) {
    if (InstalledItems_GO_Map.ContainsKey(item)) {
      GameObject instgo = InstalledItems_GO_Map[item];
      Destroy(instgo);
      InstalledItems_GO_Map.Remove(item);
    }

  }

  void OnInventoryItemCreated(InventoryItem item) {
    if (!invItem_GO_Map.ContainsKey(item)) {
      GameObject go = CreateGameObject(item.type + "-" + item.GetHashCode(), item.tile.x, item.tile.y, true);
      SpriteRenderer spr = go.GetComponent<SpriteRenderer>();
      spr.sprite = spriteController.GetSprite(item.spriteName);
      spr.sortingLayerName = "Objects";
      GameObject txt = Instantiate(prfInventoryItemText, go.transform.position, Quaternion.identity);
      txt.transform.SetParent(go.transform, true);
      txt.GetComponent<TextMesh>().text = item.currentStack.ToString();
      invItem_GO_Map[item] = go;
    }
  }

  void OnInventoryItemChanged(InventoryItem item) {

  }

  void OnInventryItemDestoyed(InventoryItem item) {

    if (invItem_GO_Map.ContainsKey(item)) {
      GameObject go = invItem_GO_Map[item];
      Destroy(go);
      invItem_GO_Map.Remove(item);
    }

  }

  //-------------------SET BUILD TYPES-------------------------

  public void setBuildType_InstalledItem(string item) {
    buildController.SetBuild(BuildController.BUILDTYPE.INSTALLEDITEM, item);
  }

  public void setBuildType_Tile(string tile) {
    buildController.SetBuild(BuildController.BUILDTYPE.TILE, tile);
  }

  void OnCharacterKilled(Character c) {
    if (Characters_GO_Map.ContainsKey(c)) {
      GameObject go = Characters_GO_Map[c];
      Destroy(go);
      Characters_GO_Map.Remove(c);
    }
  }

  void OnCharacterCreated(Character chr) {
    string name = chr.name + "_" + chr.GetHashCode();


    GameObject go = CreateGameObject(name, chr.PosTile.x, chr.PosTile.y);
    SpriteRenderer spr = go.GetComponent<SpriteRenderer>();
    spr.sprite = spriteController.GetSprite(chr);
    spr.sortingLayerName = "Characters";
    //GameObject gln = Instantiate(LinePrefab, new Vector3(0, 0, 0), Quaternion.identity);
    //gln.transform.SetParent(go.transform, true);

    //GameObject gtx = Instantiate(TextPrefab, go.transform.position, Quaternion.identity);
    //gtx.transform.Translate(3, 0, 0);
    //gtx.transform.SetParent(go.transform, true);


    Characters_GO_Map.Add(chr, go);
  }



  void OnCharacterDestroyed(Character c) {

  }

  void OnCharacterChanged(Character c) {
    bool n, e, s, w;
    n = e = s = w = false;
    if (Characters_GO_Map.ContainsKey(c)) {
      GameObject go = Characters_GO_Map[c];
      Vector2 pos = new Vector2(c.X, c.Y);
      Vector2 oldPos = go.transform.position;
      go.transform.position = pos;

      Vector2 deltaPos = oldPos - pos;


      if (deltaPos.x < 0) {
        e = true;
      } else if (deltaPos.x > 0) {
        w = true;

      } else if (deltaPos.y < 0) {
        n = true;
      } else if (deltaPos.y > 0) {
        s = true;
      }

      SpriteRenderer spr = go.GetComponent<SpriteRenderer>();
      //LineRenderer ln = go.GetComponentInChildren<LineRenderer>();
      //TMPro.TextMeshPro txt = go.GetComponentInChildren<TMPro.TextMeshPro>();
      //txt.text = c.name + "\n" + c.state.ToString();

      //ln.positionCount = 3;
      //ln.SetPosition(0, c.pos);
      //ln.SetPosition(1, c.dst);
      //ln.SetPosition(2, c.pos);


      //if (c.path != null) {
      //  ln.positionCount = ln.positionCount + c.path.Length + 1;
      //  ln.SetPosition(3, c.pos);

      //  PathNode<Tile>[] pa = c.path.path;
      //  for (int i = 0; i < pa.Length; i += 1) {
      //    PathNode<Tile> pn = pa[i];
      //    Tile t = pn.data;

      //    ln.SetPosition(4 + i, new Vector2(t.x, t.y));
      //  }


      //}
      spr.flipX = false;
      switch (c.state) {
        case Character.STATE.RESET:
        case Character.STATE.FIND_PATH:
        case Character.STATE.FIND_JOB:
        case Character.STATE.IDLE:
          spr.sprite = spriteController.GetSprite(c.spriteName_IDLE);
          break;
        //  break;
        //
        //  break;
        //
        //  break;
        //
        //  break;
        //case Character.STATE.MOVE:
        //  break;
        //case Character.STATE.WORK_JOB:
        //  break;
        //case Character.STATE.FIND_EMPTY:
        //  break;
        default:
          spr.sprite = spriteController.GetSprite(c.spriteName);

          if (n) {
            spr.sprite = spriteController.GetSprite(c.spriteNameNorth);
          } else if (e) {
            spr.sprite = spriteController.GetSprite(c.spriteNameEast);
            spr.flipX = true;
          } else if (w) {
            spr.sprite = spriteController.GetSprite(c.spriteNameWest);

          }
          break;
      }
    }

  }

  void OnJobCreated(Job j) {

    if (!Job_GO_Map.ContainsKey(j)) {
      spriteController.JobCreated(j);
      j.cbRegisterJobComplete(OnJobEnded);
      j.cbRegisterJobCancelled(OnJobEnded);

      GameObject g = SimplePool.Spawn(buildProgressSprite, new Vector2(j.tile.x, j.tile.y), Quaternion.identity);
      g.transform.SetParent(this.transform, true);
      Job_GO_Map.Add(j, g);
    }
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
    GameObject go = Job_GO_Map[j];
    SimplePool.Despawn(go);
    Job_GO_Map.Remove(j);
  }






}





using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Xml.Serialization;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;

using NoYouDoIt.Utils;
using NoYouDoIt.TheWorld;
using NoYouDoIt.DataModels;


namespace NoYouDoIt.Controller {
  public enum GAME_STATE {
    PLAY,
    PAUSE
  }

  public class WorldController : MonoBehaviour {
    public float interestRate { get; private set; } = 0.001f;
    MarketSimv2 interestRateMarket;
    [SerializeField]
    public Color positiveBalanceColour;
    public Color negativeBalanceColour;
    public GameObject buildProgressSprite;
    public GameObject cashText;
    public GameObject LinePrefab;
    public GameObject TextPrefab;
    public GameObject currentTileText;
    public GameObject jobsPanelPrefab;
    public GameObject jobsScrollItemPrefab;
    public GameObject jobsScrollContent;
    public GameObject genericSpritePrefab;
    public GameObject prfStockpileManagementEntry;
    public GameObject StockPileScrollContent;
    public GameObject prfInstalledItemOptionsInScene;

    public Text leftMenuRecipeText;

    public GameObject debugCharItemPrefab;
    public GameObject debugCharContent;

    public GameObject uConsoleObject;

    public GAME_STATE gameState = GAME_STATE.PLAY;



    private Dictionary<Tile, GameObject> Tiles_GO_Map;
    private Dictionary<Tile, GameObject> TilesInventoryItems_GO_Map;
    private Dictionary<InstalledItem, GameObject> InstalledItems_GO_Map;
    //private Dictionary<Character, GameObject> Characters_GO_Map;
    private Dictionary<Robot, GameObject> Robot_GO_Map;
    private Dictionary<string, GameObject> StockPileSettings_GO_Map;

    //private Dictionary<Job, GameObject> Job_GO_Map;
    //private Dictionary<Job, GameObject> JobScrollIt
    private Dictionary<WorkItem, GameObject> Work_GO_Map;
    //private Dictionary<WorkItem, GameObject> JobScrollItems_GO_Map;
    private List<GameObject> tempText;
    private List<NYDITimer> timers;

    //private Dictionary<Character, GameObject> Characters_ScrollItem_Map;
    //private Dictionary<InventoryItem, GameObject> invItem_GO_Map;

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

    public Canvas UICanvas;
    public Canvas WorldCanvas;

    private float countdown = 2f;
    public float money { get; private set; } = 0;
    //public Sprite dirtSprite;
    //public Sprite grassSprite;
    private World world;
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

      CreateTileGameObjects();
      foreach (Tile t in Tiles_GO_Map.Keys) {
        SetTileSprite(t);


      }
      world.RegisterInstalledItemCB(OnInstalledItemCreated);

      //foreach (Character chr in world.characters) {
      //  OnCharacterCreated(chr);

      //}

      foreach (Robot r in world.robots) {
        OnRobotCreated(r);
      }

      //spriteController = SpriteController.Instance;
      //spriteController.wcon = this;
      //spriteController.world = this.world;

      //world.jobQueue.cbRegisterJobCreated(OnJobCreated);
      world.workManager.CBRegisterOnCreated(OnWorkCreated);
      //world.CBRegisterCharacterChanged(OnCharacterChanged);
      //world.CBRegisterCharacterCreated(OnCharacterCreated);
      //world.CBRegisterCharacterKilled(OnCharacterKilled);
      world.CBRegisterRobotCreated(OnRobotCreated);
      world.CBRegisterRobotChanged(OnRobotChanged);
      world.CBRegisterRobotRemoved(OnRobotRemoved);
      //world.CBRegisterInventoryItemPlacedOnTile(OnInventoryItemPlacedOnTile);
      world.CBRegisterInventoryItemChangedOnTile(OnInventoryItemChanged);
      //world.CBRegisterInventoryItemRemovedFromTile(OnInventryItemDestoyed);
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

    public void CreateJobPanelItems() {

      //Transform contentPanel = jobsScrollContent.transform;//jobsPanelPrefab.transform.Find("ScrollViewContent");
      //foreach (Job j in world.jobQueue.publicJobs) {

      //  GameObject g = SimplePool.Spawn(jobsScrollItemPrefab, Vector2.zero, Quaternion.identity);
      //  g.transform.SetParent(contentPanel);
      //  scrJobScrollItem scr = g.GetComponent<scrJobScrollItem>();
      //  scr.Setup(j);
      //  JobScrollItems_GO_Map.Add(j, g);

      //}

    }

    public void DestroyJobPanelItems() {
      //foreach (GameObject go in JobScrollItems_GO_Map.Values) {
      //  SimplePool.Despawn(go);

      //}

      //JobScrollItems_GO_Map.Clear();
    }



    public void CreateNewWorld() {




      world = new World(World.TEST_WIDTH, World.TEST_HEIGHT);


      //world.RandomiseTiles();
      //MapGenerator.MakeNewMap(world, world.height, world.width);
      //create game objects for tiles

      world.RegisterInstalledItemCB(OnInstalledItemCreated);

      //spriteController = SpriteController.Instance;
      //spriteController.wcon = this;
      //spriteController.world = this.world;

      //world.jobQueue.cbRegisterJobCreated(OnJobCreated);
      world.workManager.CBRegisterOnCreated(OnWorkCreated);
      //world.CBRegisterCharacterChanged(OnCharacterChanged);
      //world.CBRegisterCharacterCreated(OnCharacterCreated);
      //world.CBRegisterCharacterKilled(OnCharacterKilled);
      world.CBRegisterRobotCreated(OnRobotCreated);
      world.CBRegisterRobotChanged(OnRobotChanged);
      world.CBRegisterRobotRemoved(OnRobotRemoved);
      //world.CBRegisterInventoryItemPlacedOnTile(OnInventoryItemPlacedOnTile);
      world.CBRegisterInventoryItemChangedOnTile(OnInventoryItemChanged);
      //world.CBRegisterInventoryItemRemovedFromTile(OnInventryItemDestoyed);
      world.CBRegisterChunkCreated(OnChunkCreated);



      //world.SetAllNeighbours();

      //world.PlaceTrash();

      //for (int i = 0; i < 5; i += 1) {
      //  Tile a = world.GetRandomEmptyTile();
      //  Tile b = world.GetRandomEmptyTile();
      //  Tile c = world.GetRandomEmptyTile();
      //  Tile d = world.GetRandomEmptyTile();
      //  if (a != null)
      //    world.PlaceTileInventoryItem("inv::steel_plates", a, UnityEngine.Random.Range(16, 32));
      //  if (b != null)
      //    world.PlaceTileInventoryItem("inv::wood_planks", b, UnityEngine.Random.Range(16, 32));
      //  if (c != null)
      //    world.PlaceTileInventoryItem("inv::stone_slabs", c, UnityEngine.Random.Range(16, 32));
      //  if (d != null)
      //    world.PlaceTileInventoryItem("inv::copper_plates", d, UnityEngine.Random.Range(16, 32));
      //}
      //world.nodeMap = new TileNodeMap(world);

      //initDone = false;
    }

    public void Init() {
      CreateControllers();
      interestRateMarket = new MarketSimv2(0.001f, 0.1f);
      TileType.LoadFromFile();

      Tiles_GO_Map = new Dictionary<Tile, GameObject>();
      InstalledItems_GO_Map = new Dictionary<InstalledItem, GameObject>();
      //Characters_GO_Map = new Dictionary<Character, GameObject>();
      //Job_GO_Map = new Dictionary<Job, GameObject>();
      Work_GO_Map = new Dictionary<WorkItem, GameObject>();
      //JobScrollItems_GO_Map = new Dictionary<Job, GameObject>();
      //invItem_GO_Map = new Dictionary<InventoryItem, GameObject>();
      TilesInventoryItems_GO_Map = new Dictionary<Tile, GameObject>();
      //Characters_ScrollItem_Map = new Dictionary<Character, GameObject>();
      Robot_GO_Map = new Dictionary<Robot, GameObject>();
      StockPileSettings_GO_Map = new Dictionary<string, GameObject>();
      tempText = new List<GameObject>();

      eventSystem = EventSystem.current;
      timers = new List<NYDITimer>();

      timers.Add(new NYDITimer("updateMoney", 1, UpdateMoney));
      timers.Add(new NYDITimer("updateStockPile", 2, 3,UpdateStockPile));
      timers.Add(new NYDITimer("checkStockPile", 2, 5, CheckStockPile));

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

      foreach (WorkItem work in Work_GO_Map.Keys) {
        Destroy(Work_GO_Map[work]);
      }

      foreach (GameObject go in TilesInventoryItems_GO_Map.Values) {
        Destroy(go);


      }
      //Job_GO_Map = null;
      Work_GO_Map = null;

      Tiles_GO_Map = null;

      TilesInventoryItems_GO_Map = null;
      DestroyControllers();
      Init();
      CreateNewWorld();
      InitialiseControllers();
      InitWorld();

    }

    private void InitWorld() {
      world.CreateEmptyTiles();
      CreateTileGameObjects();
      //world.CreateCharacters();
      world.CreateSpawnRobots();

    }

    private void CreateControllers() {
      Debug.Log("loading sprites..." + NYDISpriteManager.Instance);
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

      //Gizmos.color = Color.red;

      //Gizmos.DrawLine((Vector2)inputController.camBounds.min, (Vector2)inputController.camBounds.max);
      ////Gizmos.DrawCube(inputController.camBounds.center, Vector3.one);

      Gizmos.color = Color.white;
      if (world != null && world.robots != null) {
        foreach (Robot robot in world.robots) {

          if (robot.path != null) {
            Vector2 a = robot.position;
            Vector2 b = new Vector2();
            foreach (Tile pnt in robot.path.path) {
              b.Set(pnt.world_x, pnt.world_y);
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
        InitialiseControllers();
      } else {
        CreateNewWorld();
        InitialiseControllers();
        InitWorld();
      }
      CreateStockpileManagement();
      Debug.Log("init done " + this.name);
      gameState = GAME_STATE.PLAY;
    }

    void InstantiateController(GameObject controllerPrefab) {
      GameObject g = Instantiate(controllerPrefab, this.transform.position, Quaternion.identity);
      g.transform.SetParent(this.transform, true);
      controllers.Add(g);

    }

    public void WriteLog() {
      //string s = "";
      //foreach (Job j in world.jobQueue.allJobs) {
      //  s += "\n" + j.GetLog();
      //}
      //File.WriteAllText(Application.streamingAssetsPath + "/logs/joblog.txt", s);
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

    public void AddCurrency(float amt) {
      money += amt;


    }

    public void DeductMoney(float amt) {
      //Debug.Log("money = " + money + " - " + amt);
      money -= amt;

    }

    public void UpdateRecipe(string text) {
      leftMenuRecipeText.text = text;
    }

    private void UpdateMoney() {
      
      interestRate = interestRateMarket.GetNextValue();

      money = money + (money * (interestRate / 100f));//Mathf.Pow((1f + (((interestRate) / 100f) / SECONDS_PER_DAY)), SECONDS_PER_DAY);
                                                      //+ (money * interestRate * Time.deltaTime);


      Text t = cashText.GetComponent<Text>();

      t.text = Funcs.PadPair(20, "money",string.Format("{0:00.00}", money), '.');
      t.text += "\n" + Funcs.PadPair(20, "interest", string.Format("{0:0.000}", 100.0 * interestRate), '.');
      
      if (money < 0) {
        t.color = negativeBalanceColour;
      } else {
        t.color = positiveBalanceColour;
      }
    }
    public static readonly float SECONDS_PER_DAY = 60 * 60 * 24;
    // Update is called once per frame
    void Update() {

      for (int i = 0; i < timers.Count; i += 1) {
        timers[i].Update(Time.deltaTime);
      }


      //if (actualReady < EXPECTED) {
      //  Debug.Log("Ready: " + actualReady);
      //  return;
      //} else {
      //  if (!initDone) {
      //    cbReady("hi");
      //    initDone = true;
      //  }
      //}

      for (int i = tempText.Count - 1; i >= 0; i -= 1) {
        GameObject go = tempText[i];
        SetSortingLayer ssl = go.GetComponentInChildren<SetSortingLayer>();
        float a = Mathf.Sin(ssl.lifeTime);
        go.transform.Translate(0, 0.1f * a, 0);
        ssl.lifeTime -= Time.deltaTime;

        if (ssl.lifeTime <= 0) {
          SimplePool.Despawn(go);
          tempText.RemoveAt(i);
        }
      }

      switch (gameState) {
        case GAME_STATE.PLAY:
          countdown -= Time.deltaTime;
          if (countdown < 0) {
            //addCurrency(UnityEngine.Random.Range(-1f, 4f));
            countdown = 2;
          }


          //add pause and speed controls
          world.Update(Time.deltaTime);


          //foreach (Character chr in world.characters) {
          //  if (!Characters_ScrollItem_Map.ContainsKey(chr)) {
          //    GameObject g = Instantiate(debugCharItemPrefab, Vector2.zero, Quaternion.identity);
          //    g.transform.SetParent(debugCharContent.transform);
          //    Characters_ScrollItem_Map[chr] = g;

          //  }

          //  GameObject go = Characters_ScrollItem_Map[chr];
          //  go.GetComponent<scrollItemDebugCharacters>().Set(chr);


          //}

          break;
        case GAME_STATE.PAUSE:
          break;
        default:
          break;
      }





    }



    public void UpdateCurrentTile(SelectionInfo info) {
      Text txt = currentTileText.GetComponent<Text>();
      Tile t = info.tile;
      object o = info.contents[info.subSelect];

      int pw = 38;
      
      string displayMe = "";


      displayMe += Funcs.PadPair(pw, "chunk x", t.chunk.x.ToString(), '.');
      displayMe += "\n" + Funcs.PadPair(pw, "chunk y", t.chunk.y.ToString(), '.');
      displayMe += "\n" + Funcs.PadPair(pw, "world x", t.world_x.ToString(), '.'); 
      displayMe += "\n" + Funcs.PadPair(pw, "world y", t.world_y.ToString(), '.'); 
      
        
        
      displayMe += "\n";
      if (o.GetType() == typeof(Tile)) {
        displayMe += Funcs.PadPair(pw, "tile type", ((Tile)o).type.name, '.');
      } else if (o.GetType() == typeof(Robot)) {
        displayMe += Funcs.PadPair(pw, "robot",((Robot)o).name, '.');
      } else if (o.GetType() == typeof(InstalledItem)) {
        InstalledItem item = (InstalledItem)o;
        displayMe += Funcs.PadPair(pw, "installed item", item.niceName, '.');

        if (item.itemParameters.HasProperty("socialMediaName")) {
          displayMe += "\n" + Funcs.PadPair(pw, "social media", item.itemParameters.GetString("socialMediaName"), '.');
        }

        this.prfInstalledItemOptionsInScene.GetComponent<prfInstalledItemScript>().Set(item);
      } else if (o.GetType() == typeof(string)) {
        string invname = t.GetFirstInventoryItem();
        if (invname != null) {
          string nicename = InventoryItem.GetPrototype(invname).niceName;
          displayMe += Funcs.PadPair(pw, nicename, t.InventoryTotal(invname).ToString());
          displayMe += "\n" + Funcs.PadPair(pw, "allocated", t.InventoryTotalAllocated(invname).ToString());
        }
        //displayMe += InventoryItem.GetPrototype(t.GetFirstInventoryItem()).niceName + ": " + t.InventoryTotal(((string)o));

      }
      //displayMe += "\nNeighbours:" + t.neighbours.Count + ", " + t.edges.Count;
      //displayMe += "\nRoom:" + t.room.id;
      //displayMe += "\nInstalled:" + (t.installedItem == null ? "" : t.installedItem.niceName);
      //displayMe += "\nJobs: " + t.JobsToString();
      //displayMe += "\nItems:" + t.GetContents();//(t.inventoryItem == null ? "" : t.inventoryItem.niceName + " " + t.inventoryItem.currentStack + "/" + t.inventoryItem.maxStackSize);
      //displayMe += "\n" + t.WhoIsHere();



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

    public void CreateTileGameObjects() {
      foreach (int xc in world.chunks.Keys) {
        //Debug.Log("xc = " + xc);
        foreach (int yc in world.chunks[xc].Keys) {
          //Debug.Log("xc = " + xc + " yc = " + yc);
          TileChunk chunk = world.chunks[xc][yc];

          CreateChunkTileGameObjects(chunk);

        }
      }
    }

    public void CreateChunkTileGameObjects(TileChunk chunk) {
      for (int xt = 0; xt < TileChunk.CHUNK_WIDTH; xt += 1) {
        for (int yt = 0; yt < TileChunk.CHUNK_HEIGHT; yt += 1) {
          Tile t = chunk.tiles[xt, yt];
          GameObject go = CreateGameObject("tile_" + t.world_x + "_" + t.world_y, t.world_x, t.world_y, true);
          go.GetComponent<SpriteRenderer>().sortingLayerName = "Ground";
          Tiles_GO_Map.Add(t, go);


          t.cbRegisterOnChanged(SetTileSprite);
          t.SetType(t.type);
        }
      }
    }


    //for (int x = 0; x < world.width; x += 1) {
    //  for (int y = 0; y < world.height; y += 1) {
    //    Tile t = world.GetTileAt(x, y);
    //    GameObject go = CreateGameObject("tile_" + x + "_" + y, x, y);
    //    //GameObject tile_go = new GameObject();
    //    //tile_go.name = "tile_" + x + "_" + y;
    //    //tile_go.transform.Translate(t.x, t.y, 0);
    //    //tile_go.transform.SetParent(this.transform, true);
    //    //tile_go.AddComponent<SpriteRenderer>();

    //    Tiles_GO_Map.Add(t, go);
    //    go.GetComponent<SpriteRenderer>().sortingLayerName = "Ground";

    //    t.cbRegisterOnChanged(SetTileSprite);

    //  }
    //}


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

    public void OnChunkCreated(TileChunk chunk) {
      CreateChunkTileGameObjects(chunk);
    }

    public void OnInstalledItemCreated(InstalledItem inst) {
      //create a visible game object

      if (InstalledItems_GO_Map.ContainsKey(inst)) {
        return;
      }
      GameObject go = CreateGameObject(inst.type + "_" + inst.tile.world_x + "_" + inst.tile.world_y, inst.tile.world_x, inst.tile.world_y);

      InstalledItems_GO_Map.Add(inst, go);
      SpriteRenderer spr = go.GetComponent<SpriteRenderer>();
      float xt = 0, yt = 0;
      if (inst.width > 1) {
        xt = ((float)(inst.width) / 2.0f) - 0.5f;
      }

      if (inst.height > 1) {
        yt = ((float)(inst.height) / 2.0f) - 0.5f;
      }
      spr.transform.Translate(xt, yt, 0);

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

    //void OnInventoryItemPlacedOnTile(Tile tile) {
    //  if (tile.inventoryItem == null) return;
    //  if (!TilesInventoryItems_GO_Map.ContainsKey(tile)) {
    //    GameObject go = CreateGameObject("inv_" + tile.world_x + "," + tile.world_y + "_" + tile.inventoryItem, tile.world_x, tile.world_y, true);
    //    SpriteRenderer spr = go.GetComponent<SpriteRenderer>();
    //    spr.sprite = spriteController.GetSprite(tile.inventoryItem.spriteName);
    //    spr.sortingLayerName = "Objects";
    //    GameObject txt = Instantiate(prfInventoryItemText, go.transform.position, Quaternion.identity);
    //    txt.transform.SetParent(go.transform, true);
    //    txt.GetComponent<TextMesh>().text = tile.inventoryItem.currentStack.ToString();
    //    TilesInventoryItems_GO_Map[tile] = go;
    //  }
    //}

    public void CloseGame() {
      Application.Quit();
    }

    void OnInventoryItemChanged(Tile tile) {
      //Debug.Log("inventory changed: " + tile);
      if (tile.IsInventoryEmpty()) {
        if (TilesInventoryItems_GO_Map.ContainsKey(tile)) {
          GameObject go = TilesInventoryItems_GO_Map[tile];
          Destroy(go);
          TilesInventoryItems_GO_Map.Remove(tile);
          //invItem_GO_Map.Remove(item);
        }
      } else if (TilesInventoryItems_GO_Map.ContainsKey(tile)) {
        GameObject go = TilesInventoryItems_GO_Map[tile];

        if (InventoryItem.GetStackSize(tile.GetFirstInventoryItem()) > 1) {
          TextMesh txt = go.GetComponentInChildren<TextMesh>();

          if (txt != null) {
            txt.text = tile.GetInventoryTotal().ToString();
          }
        }

        SpriteRenderer spr = go.GetComponent<SpriteRenderer>();
        spr.sprite = spriteController.GetSprite(tile.GetInventorySpriteName());
      } else {
        GameObject go = CreateGameObject("inv_" + tile.world_x + "," + tile.world_y + "_" + tile.GetContents(), tile.world_x, tile.world_y, true);
        SpriteRenderer spr = go.GetComponent<SpriteRenderer>();
        spr.sprite = spriteController.GetSprite(tile.GetInventorySpriteName());
        spr.sortingLayerName = "LooseItems";
        if (InventoryItem.GetStackSize(tile.GetFirstInventoryItem()) > 1) {
          GameObject txt = Instantiate(prfInventoryItemText, go.transform.position, Quaternion.identity);
          txt.transform.SetParent(go.transform, true);

          txt.GetComponent<TextMesh>().text = tile.GetInventoryTotal().ToString();
        }

        TilesInventoryItems_GO_Map[tile] = go;
      }

    }

    //void OnInventryItemDestoyed(Tile tile) {

    //  if (TilesInventoryItems_GO_Map.ContainsKey(tile)) {
    //    GameObject go = TilesInventoryItems_GO_Map[tile];
    //    Destroy(go);
    //    TilesInventoryItems_GO_Map.Remove(tile);
    //    //invItem_GO_Map.Remove(item);
    //  }

    //}

    //-------------------SET BUILD TYPES-------------------------

    public void SetBuildType_Clear() {
      inputController.mouseMode = MOUSE_MODE.SELECT;
      buildController.SetBuildMode(BuildController.BUILD_MODE.NONE, "");
    }

    public void SetBuildType_Zone(string zone) {
      inputController.mouseMode = MOUSE_MODE.BUILD;
      buildController.SetBuildMode(BuildController.BUILD_MODE.ZONE, zone);
    }

    public void SetBuildType_InstalledItem(string item) {
      inputController.mouseMode = MOUSE_MODE.BUILD;
      buildController.SetBuildMode(BuildController.BUILD_MODE.INSTALLEDITEM, item);
    }

    public void SetBuildType_Tile(string tile) {
      inputController.mouseMode = MOUSE_MODE.BUILD;
      buildController.SetBuildMode(BuildController.BUILD_MODE.TILE, tile);
    }

    public void SetBuildType_Deconstruct() {
      inputController.mouseMode = MOUSE_MODE.BUILD;
      buildController.SetBuildMode(BuildController.BUILD_MODE.DECONSTRUCT, "");
    }

    //void OnCharacterKilled(Character c) {
    //  if (Characters_GO_Map.ContainsKey(c)) {
    //    GameObject go = Characters_GO_Map[c];
    //    Destroy(go);
    //    Characters_GO_Map.Remove(c);
    //  }
    //}

    void OnRobotRemoved(Robot r) {
      Debug.Log("remove robot " + r.name);
      if (Robot_GO_Map.ContainsKey(r)) {
        GameObject go = Robot_GO_Map[r];
        Destroy(go);
        Robot_GO_Map.Remove(r);
      }

    }

    void OnRobotCreated(Robot r) {
      string name = r.name + "_" + r.GetHashCode();
      Debug.Log(name + " " + r.Xint + "," + r.Yint);

      GameObject go = CreateGameObject(name, r.Xint, r.Yint);
      //Debug.Log(go);
      SpriteRenderer spr = go.GetComponent<SpriteRenderer>();
      spr.sprite = spriteController.GetSprite(r);
      spr.sortingLayerName = "Characters";
      r.CBRegisterOnChanged(OnRobotChanged);


      //GameObject gln = Instantiate(LinePrefab, new Vector3(0, 0, 0), Quaternion.identity);
      //gln.transform.SetParent(go.transform, true);

      //GameObject gtx = Instantiate(TextPrefab, go.transform.position, Quaternion.identity);
      //gtx.transform.Translate(3, 0, 0);
      //gtx.transform.SetParent(go.transform, true);


      Robot_GO_Map.Add(r, go);
    }

    public void CreateStockpileManagement() {
      foreach(string name in World.current.inventoryManager.stockpileSettings.Keys) {
        StockPileSetting sps = World.current.inventoryManager.stockpileSettings[name];

        

        GameObject go = SimplePool.Spawn(prfStockpileManagementEntry, Vector2.zero, Quaternion.identity);
        prfStockPileItemScript scr =  go.GetComponent<prfStockPileItemScript>();
        scr.Set(sps);
        scr.SetCurrentQty(0);
        go.transform.SetParent(this.StockPileScrollContent.transform);
        go.transform.localScale = Vector3.one;
        StockPileSettings_GO_Map[sps.name] = go;
       

      }
    }

    public void CheckStockPile() {
      World.CallLuaFunction("CheckStockpiles");
    }

    public void UpdateStockPile() {
      foreach (StockPileSetting sps in World.current.inventoryManager.stockpileSettings.Values) {
        GameObject go = StockPileSettings_GO_Map[sps.name];
        prfStockPileItemScript scr = go.GetComponent<prfStockPileItemScript>();
        scr.SetCurrentQty(sps.currentQty);


      }
    }

    //void OnCharacterCreated(Character chr) {
    //  //string name = chr.name + "_" + chr.GetHashCode();


    //  //GameObject go = CreateGameObject(name, chr.PosTile.world_x, chr.PosTile.world_y);
    //  //SpriteRenderer spr = go.GetComponent<SpriteRenderer>();
    //  //spr.sprite = spriteController.GetSprite(chr);
    //  //spr.sortingLayerName = "Characters";
    //  ////GameObject gln = Instantiate(LinePrefab, new Vector3(0, 0, 0), Quaternion.identity);
    //  ////gln.transform.SetParent(go.transform, true);

    //  ////GameObject gtx = Instantiate(TextPrefab, go.transform.position, Quaternion.identity);
    //  ////gtx.transform.Translate(3, 0, 0);
    //  ////gtx.transform.SetParent(go.transform, true);


    //  //Characters_GO_Map.Add(chr, go);
    //}



    public void SpawnText(string text, int x, int y) {
      //Canvas cnv = FindObjectOfType<Canvas>();
      GameObject go = SimplePool.Spawn(TextPrefab, Vector2.zero, Quaternion.identity);
      go.transform.Translate(x, y, 0);
      

      
      Transform goct = go.transform.GetChild(0);
      GameObject goc = goct.gameObject;
      TextMeshPro tmp = goc.GetComponent<TextMeshPro>();
      goc.GetComponent<SetSortingLayer>().lifeTime = 1;
      go.transform.SetParent(WorldCanvas.transform,true);
      go.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
      goct.localScale = Vector3.one;
      goct.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
      goct.transform.Translate(0, 0, -1);
      
      
      tmp.text = text;
      

      tempText.Add(go);
    }

    void OnRobotChanged(Robot r) {
      if (Robot_GO_Map.ContainsKey(r)) {
        GameObject go = Robot_GO_Map[r];
        SpriteRenderer spr = go.GetComponent<SpriteRenderer>();
        go.transform.position = r.position;
      }
    }

    /*
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
          case Character.STATE.FIND_RESOURCE:
          case Character.STATE.FIND_EMPTY:
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


            if (n) {
              spr.sprite = spriteController.GetSprite(c.spriteNameNorth);
            } else if (e) {
              spr.sprite = spriteController.GetSprite(c.spriteNameEast);
              //spr.flipX = true;
            } else if (w) {
              spr.sprite = spriteController.GetSprite(c.spriteNameWest);

            } else if (s) {
              spr.sprite = spriteController.GetSprite(c.spriteNameSouth);
            } else {
              spr.sprite = spriteController.GetSprite(c.spriteName);
            }
            break;
        }
      }

    }
    */

    void OnWorkCreated(WorkItem work) {
      if (!Work_GO_Map.ContainsKey(work)) {
        //spriteController.JobCreated(work);
        World.current.workManager.CBRegisterOnCompleted(OnWorkEnded);
        //work.cbRegisterJobComplete(OnJobEnded);
        //j.cbRegisterJobCancelled(OnJobEnded);

        GameObject g = SimplePool.Spawn(buildProgressSprite, new Vector2(work.workTile.world_x, work.workTile.world_y), Quaternion.identity);
        g.transform.SetParent(this.transform, true);

        SpriteRenderer spr = g.GetComponent<SpriteRenderer>();
        //if (work.jobType == JOB_TYPE.BUILD) {
        //  spr.sprite = spriteController.GetSprite(work.installedItemPrototype.spriteName);
        //} else {
        spr.sprite = spriteController.GetSprite("other::build_in_progress");
        //}
        //spr.color = new Color(1, 1, 1, 0.4f);
        //if (work.jobType == JOB_TYPE.BUILD) {
        //  spr.transform.Translate(Funcs.GetInstalledItemSpriteOffset(work.installedItemPrototype.width, work.installedItemPrototype.height), Space.Self);
        //}

        Work_GO_Map.Add(work, g);
      }

    }



    //void OnJobCreated(Job j) {

    //  if (!Job_GO_Map.ContainsKey(j)) {
    //    spriteController.JobCreated(j);
    //    //j.cbRegisterJobComplete(OnJobEnded);
    //    //j.cbRegisterJobCancelled(OnJobEnded);

    //    GameObject g = SimplePool.Spawn(buildProgressSprite, new Vector2(j.tile.world_x, j.tile.world_y), Quaternion.identity);
    //    g.transform.SetParent(this.transform, true);

    //    SpriteRenderer spr = g.GetComponent<SpriteRenderer>();
    //    if (j.jobType == JOB_TYPE.BUILD) {
    //      spr.sprite = spriteController.GetSprite(j.installedItemPrototype.spriteName);
    //    } else {
    //      spr.sprite = spriteController.GetSprite("other::build_in_progress");
    //    }
    //    spr.color = new Color(1, 1, 1, 0.4f);
    //    if (j.jobType == JOB_TYPE.BUILD) {
    //      spr.transform.Translate(Funcs.GetInstalledItemSpriteOffset(j.installedItemPrototype.width, j.installedItemPrototype.height), Space.Self);
    //    }

    //    Job_GO_Map.Add(j, g);
    //  }
    //  /*
    //foreach (Tile tile in dragArea)
    //{
    //GameObject g = SimplePool.Spawn(cursorPrefab, new Vector2(tile.x, tile.y), Quaternion.identity);
    //g.transform.SetParent(this.transform, true);
    //dragPreviewList.Add(g);

    //}
    //*/
    //}

    public void OnWorkEnded(WorkItem work) {
      //delete sprites
      if (Work_GO_Map.ContainsKey(work)) {
        GameObject go = Work_GO_Map[work];
        SimplePool.Despawn(go);
        Work_GO_Map.Remove(work);
      }
    }


    //public void OnJobEnded(Job j) {
    //  //delete sprites
    //  if (Job_GO_Map.ContainsKey(j)) {
    //    GameObject go = Job_GO_Map[j];
    //    SimplePool.Despawn(go);
    //    Job_GO_Map.Remove(j);
    //  }
    //}






  }


}

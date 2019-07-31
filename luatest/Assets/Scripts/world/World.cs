using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text;
using System.Xml.Linq;
using NLua;



namespace NoYouDoIt.TheWorld {
  using NoYouDoIt.Utils;
  using NoYouDoIt.DataModels;
  using NoYouDoIt.NYDIPaths;
  using NoYouDoIt.Controller;
  using NoYouDoIt.Utils.Maps.CityMap;

  public class World : IXmlSerializable {


    //statics and enums and stuffs
    public static readonly string NORTH = "north";
    public static readonly string EAST = "east";
    public static readonly string SOUTH = "south";
    public static readonly string WEST = "west";

    public static readonly string NORTHWEST = "northwest";
    public static readonly string NORTHEAST = "northeast";
    public static readonly string SOUTHWEST = "southwest";
    public static readonly string SOUTHEAST = "southeast";

    public static World current { get; private set; }
    public Lua lua;

    private FastNoise noise;
    public float oreZ { get; private set; }

    //testing
    public static readonly int NUMBER_OF_ROBOTS = Funcs.GetSettingInt("number_of_robots");

    public static readonly int WORLD_WIDTH = Funcs.GetSettingInt("world_width");
    public static readonly int WORLD_HEIGHT = Funcs.GetSettingInt("world_height");

    //public static readonly int M_M_MAXIMUM_TRASH = 11;

    public void OnInventoryItemChangedOnTile(Tile tile) {
      if (cbTileInventoryItemChangedOnTile != null) {
        cbTileInventoryItemChangedOnTile(tile);
      }
    }





    //collections


    public List<InstalledItem> trashPrototypes;
    public List<InstalledItem> trashInstances;
    private List<InstalledItem> installedItems;
    private List<InstalledItem> powerGenerators;
    //public List<Character> characters;
    public List<Entity> entities;
    public string[] firstNames;
    public string[] lastNames;
    public string[] words;
    public string[] adjectives;
    public string[] animals;
    public string[] petNames;
    //private Tile[,] tiles;
    public List<Room> rooms;
    //public Dictionary<int, Dictionary<int, TileChunk>> chunks;
    public TileChunk[,] chunks;
    public List<TileChunk> chunkList;

    //public List<InventoryItem> inventoryItems;
    //properties
    public float xSeed { get; private set; }
    public float ySeed { get; private set; }
    public float noiseFactor { get; private set; }


    public int width { get; private set; }
    public int height { get; private set; }
    public int tileSize { get; private set; }
    //bool allowDiagonalNeighbours = true;
    private float updateJobQueueTimer = 0;
    public List<TileZone> zones;

    public string seed { get; private set; } = "hello";

    //objects
    //public JobQueue jobQueue;
    public WorkItemManager workManager;
    public RoadMapGen citymap;


    public TileNodeMap nodeMap;

    public string LuaString(string code) {
      try {
        System.Object[] result = lua.DoString(code);
        foreach (System.Object o in result) {
          Debug.Log(o.ToString());
        }

        return result[0].ToString();

      } catch (Exception e) {
        Debug.Log(e.Message);

      }

      return "an error occurred";




    }

    public Room outside {
      get {
        if (rooms == null) {
          rooms = new List<Room>();

        }

        if (rooms.Count == 0) {
          rooms.Add(new Room(this));
        }

        return rooms[0];
      }
    }

    public InventoryManager inventoryManager;

    //callbacks
    Action<InstalledItem> cbInstalledItem;
    Action<Tile> cbTileChanged;
    //Action<Character> cbCharacterChanged;
    //Action<Character> cbCharacterCreated;
    //Action<Character> cbCharacterKilled;

    Action<Entity> CBEntityChanged;
    Action<Entity> CBEntityCreated;
    Action<Entity> CBEntityRemoved;
    Action cbPathNodesDestroyed;

    //Action<Tile> cbTileInventoryItemPlacedOnTile;
    Action<Tile> cbTileInventoryItemChangedOnTile;
    //Action<Tile> cbTileInventoryItemRemovedFromTile;

    Action<TileChunk> cbChunkCreated;

    //collections
    string[] savedInstalledItems;
    public bool chunksInitialised { get; private set; } = false;



    //public static readonly int SPAWN_CHUNKS = 4;
    //public static readonly int HALF_SPAWN_CHUNKS = SPAWN_CHUNKS / 2;


    public TileChunk GetChunkIfExists(int x, int y) {
      //TileChunk chunk = null;
      if (x < 0 || y < 0 || x > WORLD_WIDTH - 1 || y > WORLD_HEIGHT - 1) return null;
      return chunks[x, y];

      //if (chunks.ContainsKey(x)) {
      //  if (chunks[x].ContainsKey(y)) {
      //    chunk = chunks[x][y];
      //  }
      //}


      //return chunk;

    }

    public TileChunk GetChunk(int x, int y) {
      TileChunk chunk = null;
      if (x < 0 || y < 0 || x > WORLD_WIDTH - 1 || y > WORLD_HEIGHT - 1) return null;

      bool newChunk = false;
      if (chunks[x, y] == null) {
        chunk = new TileChunk(this, x, y);
        newChunk = true;
        chunks[x, y] = chunk;
      } else {
        chunk = chunks[x, y];
      }
      //if (chunks.ContainsKey(x)) {
      //  if (chunks[x].ContainsKey(y)) {
      //    chunk = chunks[x][y];
      //  } else {
      //    chunk = new TileChunk(this, x, y);
      //    newChunk = true;
      //    chunks[x][y] = chunk;
      //  }
      //} else {
      //  //chunks[x] = new Dictionary<int, TileChunk>();
      //  chunk = new TileChunk(this, x, y);
      //  newChunk = true;
      //  chunks[x][y] = chunk;
      //}

      if (newChunk && chunk != null) {
        chunkList.Add(chunk);
        chunk.Init();
        if (cbChunkCreated != null) {
          cbChunkCreated(chunk);
        }

      }



      return chunk;
    }

    public void CreateEmptyTiles() {

      if (chunksInitialised) return;
      chunksInitialised = true;
      chunks = new TileChunk[WORLD_WIDTH, WORLD_HEIGHT];
      for (int x = 0; x < WORLD_WIDTH; x += 1) {
        //chunks[x] = new Dictionary<int, TileChunk>();
        for (int y = 0; y < WORLD_HEIGHT; y += 1) {
          //int xx = x * TileChunk.CHUNK_WIDTH;
          //int yy = y * TileChunk.CHUNK_HEIGHT;

          chunks[x, y] = new TileChunk(this, x, y);
          chunkList.Add(chunks[x, y]);
          chunks[x, y].Init();
        }

      }

      TileChunk ch = GetChunk(0, 0);
      if (Funcs.GetSettingBool("add_starting_items")) {

        List<string> itemTypes = InventoryItem.GetAllPrototypeNames();
        for (int x = 0; x < TileChunk.CHUNK_WIDTH; x += 1) {
          for (int y = 0; y < TileChunk.CHUNK_HEIGHT; y += 1) {

            if (itemTypes.Count == 0) break;
            string itemType = itemTypes[0];

            Tile t = ch.GetTileAt(x, y);
            if (t != null) {
              if (t.AddToInventory(itemType, InventoryItem.GetStackSize(itemType)) > 0) {

                itemTypes.RemoveAt(0);
              }

            }
          }
        }
      }

      if (Funcs.GetSettingBool("add_starting_ores")) {
        for (int y = 0; y < TileChunk.CHUNK_HEIGHT; y += 1) {
          Tile a = ch.GetTileAt(2, y);
          Tile b = ch.GetTileAt(3, y);
          Tile c = ch.GetTileAt(4, y);
          Tile d = ch.GetTileAt(5, y);

          a.SetType(TileType.TYPES["stone_coal"]);
          b.SetType(TileType.TYPES["stone_iron"]);
          c.SetType(TileType.TYPES["stone_copper"]);
          d.SetType(TileType.TYPES["sand"]);
        }
      }




    }

    //power gen

    private void SetCollections() {
      //installedItemProtos = new Dictionary<string, InstalledItem>();
      //installedItemProtos_BY_ID = new Dictionary<int, string>();
      entities = new List<Entity>();
      //characters = new List<Character>();
      //jobQueue = new JobQueue(this);
      workManager = new WorkItemManager();
      //tiles = new Tile[width, height];
      installedItems = new List<InstalledItem>();
      trashPrototypes = new List<InstalledItem>();
      trashInstances = new List<InstalledItem>();
      powerGenerators = new List<InstalledItem>();
      rooms = new List<Room>();
      rooms.Add(new Room(this));
      //inventoryItems = new List<InventoryItem>();
      inventoryManager = new InventoryManager(this);
      zones = new List<TileZone>();
      chunks = new TileChunk[WORLD_WIDTH, WORLD_HEIGHT]; //new Dictionary<int, Dictionary<int, TileChunk>>();
      chunkList = new List<TileChunk>();
      //outside = rooms[0];
    }

    public float SimplexNoise(float x, float y) {
      float n = noise.GetNoise(x, y);
      n = (n + 1) * 0.5f;
      return n;
    }

    public float SimplexNoise(float x, float y, float z) {
      float n = noise.GetNoise(x, y, z);
      n = (n + 1) * 0.5f;
      return n;
    }

    void InitNew(int width, int height, int tileSize) {
      


      seed = DateTime.Now.ToLongDateString() + "_" + DateTime.Now.ToLongTimeString() + "_" + DateTime.Now.ToUniversalTime();
      noise = new FastNoise(seed.GetHashCode());
      noise.SetNoiseType(FastNoise.NoiseType.Simplex);
      UnityEngine.Random.InitState(seed.GetHashCode());
      xSeed = UnityEngine.Random.Range(-10f, 10f);
      ySeed = UnityEngine.Random.Range(-10f, 10f);
      noiseFactor = Funcs.GetSettingFloat("noise_factor");
      oreZ = UnityEngine.Random.Range(0f, 10f);


      this.width = width;
      this.height = height;
      this.tileSize = tileSize;
      SetCallbacks();
      SetCollections();

      //CreateAllInstalledItemPrototypes();
      //RobotType.LoadFromFile();
      Entity.LoadFromFile();
      Recipe.LoadFromFile();
      InstalledItem.LoadFromFile();
      InventoryItem.LoadFromFile();

      citymap = RoadMapGen.Generate(WORLD_WIDTH * TileChunk.CHUNK_WIDTH, WORLD_HEIGHT * TileChunk.CHUNK_HEIGHT, TileType.countNatural);


      this.trashPrototypes = InstalledItem.trashPrototypes;

      LoadAllNames();

      inventoryManager.InitStockpile();

      CheckRecipesAndItems();
      //CreateEmptyTiles();

    }

    private void CheckRecipesAndItems() {

      foreach (InstalledItem item in InstalledItem.prototypes.Values) {
        item.CheckRecipes();
      }


      foreach (string rn in Recipe.GetRecipeNames()) {

        string output = "recipe: " + rn;
        Recipe rcp = Recipe.GetRecipe(rn);

        foreach (RecipeResource rr in rcp.resources.Values) {
          if (InventoryItem.GetPrototype(rr.name) == null) {
            throw new Exception(output + " item not found " + rr);
          }

        }

        foreach (RecipeProduct rp in rcp.products.Values) {
          switch (rp.type) {
            case RECIPE_PRODUCT_TYPE.INVENTORY_ITEM:
              if (InventoryItem.GetPrototype(rp.name) == null) {
                throw new Exception(output + " item not found " + rp);
              }
              break;
            case RECIPE_PRODUCT_TYPE.ENTITY:
              if (Entity.GetPrototype(rp.name) == null) {
                throw new Exception(output + " entity not found " + rp);
              }
              break;
            case RECIPE_PRODUCT_TYPE.MONEY:
              break;
            default:
              break;
          }

        }

      }
    }


    public void InstalledItems_AssignOnDemandJobs(string type) {
      foreach (InstalledItem item in installedItems.Where(e => e.type == type)) {
        Tile t = item.GetWorkSpot();
        if (!t.HasPendingWork) {
          InstalledItemActions.Workstation_OnDemand(item);
          return;
        }
      }

    }


    void SetCallbacks() {
      //CBRegisterInventoryItemPlacedOnTile(OnInventoryItemPlaced);
      //CBRegisterInventoryItemRemovedFromTile(OnInventoryItemRemoved);
    }

    public Tile GetRandomEmptyTile() {
      Tile t = null;
      int counter = 0;
      while (t == null && counter < 100) {
        counter += 1;
        t = GetRandomTileIfChunkExists();
        if (!t.IsEmpty()) {
          t = null;
        }
      }
      return t;
    }

    public Tile GetRandomTileIfChunkExists() {
      TileChunk chunk = chunkList[UnityEngine.Random.Range(0, chunkList.Count)];

      int x = UnityEngine.Random.Range(0, TileChunk.CHUNK_WIDTH);
      int y = UnityEngine.Random.Range(0, TileChunk.CHUNK_HEIGHT);

      return chunk.tiles[x, y];

    }

    public Tile GetRandomTile() {
      return GetRandomTileIfChunkExists();
      //Tile t = GetTileAt(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));
      //return t;
    }

    public void SetJoinedSprites() {
      for (int x = 0; x < width; x += 1) {
        for (int y = 0; y < height; y += 1) {
          Tile t = GetTileAt(x, y);
          if (t != null && t.installedItem != null) {
            informNeighbours(t.installedItem);

          }
        }
      }

    }

    public int GetEntitiesAtState(string type, string state) {
      return entities.Count(e => e.typeName == type && e.state == state);
    }

    //---POWER---

    //all power gen code
    public float currentPower { get; private set; }
    public float usedPower { get; private set; }

    public void AddPowerGen(InstalledItem item) {
      if (item.powerGenerated > 0) {
        if (!powerGenerators.Contains(item)) {
          powerGenerators.Add(item);
        }
      }
    }

    public void RemovePowerGen(InstalledItem item) {

      if (powerGenerators.Contains(item)) {
        powerGenerators.Remove(item);
      }

    }

    private void UpdatePower() {
      currentPower = 0;
      usedPower = 0;
      foreach (InstalledItem item in powerGenerators) {
        currentPower += item.powerGenerated;
      }

      foreach (InstalledItem item in installedItems) {
        usedPower += item.powerUsed;
      }

    }



    //-----------------------------CONSTRUCTORS------------------------------
    private void InitLua() {
      lua = new Lua();
      lua.LoadCLRPackage();
      lua.DoString(
  @"import 'System'
import 'UnityEngine'
import 'Assembly-CSharp'
import 'NoYouDoIt.TheWorld'
import 'NoYouDoIt.Controller'
import 'NoYouDoIt.Utils'
import 'NoYouDoIt.NYDIPaths'
import 'NoYouDoIt.DataModels'

");
      lua["world"] = this;
      LoadAllLuaFiles();
      //LoadLuaFile("World");
      //LoadLuaFile("WorldController");
      //LoadLuaFile("InstalledItems");
      //LoadLuaFile("Jobs");
      //LoadLuaFile("Robot");
      //LoadLuaFile("WorkItem");



    }



    public static void dbug(System.Object o) {
      Debug.Log("debug=" + o.ToString());
    }

    public World() {
      current = this;
      InitLua();
    }
    public World(int width = 50, int height = 50, int tileSize = 32) {
      current = this;
      InitLua();
      InitNew(width, height, tileSize);
      //CreateCharacters();


    }

    public void SetAllNeighbours() {
      for (int x = 0; x < chunks.GetLength(0); x += 1) {
        for (int y = 0; y < chunks.GetLength(1); y += 1) {
          TileChunk t = chunks[x, y];

          t.SetNeighbourChunks();

        }
      }


      //for (int x = 0; x < width; x += 1) {
      //  for (int y = 0; y < height; y += 1) {
      //    tiles[x, y].SetNeighbours(allowDiagonalNeighbours);
      //  }
      //}
    }

    //-------------------------------LUA-------------------------------
    public static bool EvalLuaString(string s) {
      object[] res = TheWorld.World.current.lua.DoString("return " + s);

      bool returnValue = false;
      bool.TryParse((string)res[0], out returnValue);



      return returnValue;
    }

    public static void CallLuaFunction(string functionName, params System.Object[] args) {
      LuaFunction luaFunc = TheWorld.World.current.lua[functionName] as LuaFunction;
      luaFunc.Call(args);
    }

    public static System.Object[] GetLuaResult(string functionName, params System.Object[] args) {
      LuaFunction luaFunc = TheWorld.World.current.lua[functionName] as LuaFunction;
      return luaFunc.Call(args);
    }

    public static void CallLuaFunctions(string[] functionList, params System.Object[] args) {
      foreach (string fname in functionList) {
        LuaFunction luaFunc = TheWorld.World.current.lua[fname] as LuaFunction;
        luaFunc.Call(args);

      }
    }

    protected static void LoadAllLuaFiles() {
      string path = Path.Combine(Application.streamingAssetsPath, "lua");

      LoadAllLuaFilesInFolder(path);
    }

    protected static void LoadAllLuaFilesInFolder(string folder) {
      string[] files = Directory.GetFiles(folder, "*.lua");

      foreach (string file in files) {
        LoadLuaFile(file);

      }

      string[] folders = Directory.GetDirectories(folder);
      foreach (string ff in folders) {
        LoadAllLuaFilesInFolder(ff);
      }
    }

    protected static void LoadLuaFile(string name) {
      //string path = Application.streamingAssetsPath;
      //path = System.IO.Path.Combine(path, "lua");
      //path = System.IO.Path.Combine(path, name + ".lua");

      string lua = File.ReadAllText(name);
      TheWorld.World.current.lua.DoString(lua);
      Debug.Log("loading lua file " + name);


    }



    //-------------------------------ROOOMS--------------------------

    public void AddRoom(Room room) {
      if (!rooms.Contains(room)) {
        rooms.Add(room);
      }
    }

    public void DeleteAllRooms() {
      for (int i = rooms.Count - 1; i > 0; i -= 1) {
        DeleteRoom(rooms[i]);
      }
      outside.RemoveAllTiles();
    }

    public void DeleteRoom(Room r, bool unnassign = false) {
      if (r == outside) {
        if (unnassign) {
          r.RemoveAllTiles();
        }
      } else {
        if (unnassign) {
          r.RemoveAllTiles();
        }
        rooms.Remove(r);
      }
    }

    //---

    public void Update(float deltaTime) {
      //for (int i = characters.Count - 1; i >= 0; i -= 1) {
      //  Character c = characters[i];
      //  c.Update(deltaTime);
      //}
      UpdatePower();
      inventoryManager.Update(deltaTime);


      for (int i = entities.Count - 1; i >= 0; i -= 1) {
        Entity entity = entities[i];
        entity.Update(deltaTime);
      }


      foreach (InstalledItem item in installedItems) {
        item.Update(deltaTime);
      }
      //foreach (InstalledItem item in installedItems.Where(e => e.type == "installed::stockpile" && !e.inventory.IsEmpty())) {
      //  item.Update(deltaTime);
      //}
      //foreach (InstalledItem item in installedItems.Where(e => e.type == "installed::stockpile" && e.inventory.IsEmpty())) {
      //  item.Update(deltaTime);
      //}
      //foreach (InstalledItem item in installedItems.Where(e => e.type != "installed::stockpile")) {
      //  item.Update(deltaTime);
      //}

      if (updateJobQueueTimer <= 0) {
        updateJobQueueTimer = 1;
        workManager.Update(deltaTime);

      } else {
        updateJobQueueTimer -= deltaTime;
      }
    }


    //-------------------------------------CHARACTERS-------------------------------------

    //public void OnCharacterChanged(Character c) {
    //  if (cbCharacterChanged != null) {
    //    cbCharacterChanged(c);
    //  }
    //}

    public void OnEntityChanged(Entity r) {
      if (CBEntityChanged != null) {
        CBEntityChanged(r);
      }
    }

    public void CreateSpawnRobots() {
      int attempts = 0;
      int created = 0;
      while (created < NUMBER_OF_ROBOTS && attempts < 50) {
        attempts += 1;
        //int x = UnityEngine.Random.Range(0, width);
        //int y = UnityEngine.Random.Range(0, height);
        Debug.Log("create character");
        Tile t = GetRandomEmptyTile();
        if (t == null) continue;

        if (t.type.movementFactor >= 0.5) {

          Entity c = Entity.MakeEntity(t, "entities::basic_robot");//new Character(this, t);

          if (CreateEntity(c)) {
            created += 1;
          }
        }
      }

      //foreach (Entity chr in entities) {
      //  Debug.Log("char:" + chr.name);
      //}

    }

    public bool CreateEntityAtTile(Tile t, string type) {
      Entity c = Entity.MakeEntity(t, type);
      return CreateEntity(c);
    }

    //public bool CreateEntity(Tile t) {
    //  Entity c = Entity.MakeEntity(t, "basic");//new Character(this, t);
    //  return CreateEntity(c);
    //}

    public bool CreateEntity(Entity r) {
      if (r != null) {
        entities.Add(r);
        if (CBEntityCreated != null) {
          CBEntityCreated(r);
        }
        r.CBRegisterOnChanged(OnEntityChanged);

        //CBRegisterPathNodesDestroyed(c.PathNodesDestroyed);
        return true;
      }


      return false;
    }

    /*
    public void CreateCharacters() {
      int attempts = 0;
      int created = 0;
      while (created < NUMBER_OF_ROBOTS && attempts < 50) {
        attempts += 1;
        //int x = UnityEngine.Random.Range(0, width);
        //int y = UnityEngine.Random.Range(0, height);
        Debug.Log("create character");
        Tile t = GetRandomEmptyTile();
        if (t == null) continue;

        if (t.type.movementFactor >= 0.5) {

          Character c = new Character(this, t);

          if (CreateCharacter(c)) {
            created += 1;
          }
        }
      }

      foreach (Character chr in characters) {
        Debug.Log("char:" + chr.name);
      }

    }



    public bool CreateCharacter(Tile t) {
      Character c = new Character(this, t);
      return CreateCharacter(c);
    }

    public bool CreateCharacter(Character c) {

      if (c != null) {
        characters.Add(c);
        if (cbCharacterCreated != null) {
          cbCharacterCreated(c);
        }
        c.CBRegisterOnChanged(OnCharacterChanged);
        CBRegisterPathNodesDestroyed(c.PathNodesDestroyed);
        return true;
      }


      return false;
    }
    */



    //-========================================INSTALLED ITEMS---=========================================

    public Dictionary<string, InstalledItem> getProtoList() {
      return InstalledItem.prototypes;
    }

    //public InstalledItem getInstalledItemProtoById(int id) {
    //  if (InstalledItem.prototypesById.ContainsKey(id)) {
    //    return getInstalledItemProto(InstalledItem.prototypesById[id]);

    //  } else {
    //    return null;
    //  }
    //}

    public InstalledItem getInstalledItemProto(string item) {
      if (InstalledItem.prototypes.ContainsKey(item)) {
        return InstalledItem.prototypes[item];
      } else {
        return null;
      }
    }

    public Tile GetTileIfChunkExists(int x, int y) {
      TileChunk chunk = GetChunkIfExists(x / TileChunk.CHUNK_WIDTH, y / TileChunk.CHUNK_HEIGHT);
      if (chunk == null) return null;
      //Debug.Log("chunk:" + chunk.x + "," + chunk.y);

      return chunk.GetTileAtWorldCoord(x, y);
    }

    public Tile GetTileAt(int x, int y) {
      //if (x < 0 || x > width - 1 || y < 0 || y > height - 1) {
      //  //Debug.LogError("Tile (" + x + "," + y + ") is out of bounds");
      //  return null;
      //}
      TileChunk chunk = GetChunk(x / TileChunk.CHUNK_WIDTH, y / TileChunk.CHUNK_HEIGHT);
      if (chunk == null) return null;
      return chunk.GetTileAtWorldCoord(x, y);
    }

    //------------------------------------TILES--------------------------------------

    //public void RandomiseTiles() {
    //  //Debug.Log("Randomise Tiles");
    //  //float xo = xSeed;// UnityEngine.Random.Range(-10, 10);
    //  //float yo = ySeed; // UnityEngine.Random.Range(-10, 10);
    //  //float xx = 0;
    //  //float yy = 0;
    //  //for (int x = 0; x < width; x += 1) {
    //  //  xx = ((float)x) * noiseFactor;
    //  //  for (int y = 0; y < height; y += 1) {
    //  //    yy = ((float)y) * noiseFactor;
    //  //    //Tile t = tiles[x, y];
    //  //    ////if (UnityEngine.Random.Range(0,2) == 0)
    //  //    //float f = Mathf.PerlinNoise(xx + xo, yy + yo);
    //  //    //TileType tt = TileType.TYPES["empty"];
    //  //    //foreach (string k in TileType.TYPES.Keys) {
    //  //    //  TileType tempT = TileType.TYPES[k];

    //  //    //  if (tempT.name != "empty") {
    //  //    //    if (f >= tempT.rangeLow && f < tempT.rangeHigh) {
    //  //    //      t.SetType(tempT);
    //  //    //      break;
    //  //    //    }
    //  //    //  }

    //  //    //}
    //  //  }


    //  //}
    //}

    //public void PlaceTrash() {
    //  //int countOfTrash = trashInstances.Count;
    //  if (trashPrototypes.Count > 0) {
    //    int attempts = 0;
    //    int countAdd = 0;
    //    while (countAdd < M_M_MAXIMUM_TRASH && attempts < 100) {
    //      attempts += 1;
    //      int x = UnityEngine.Random.Range(0, width);
    //      int y = UnityEngine.Random.Range(0, height);

    //      Tile tile = GetTileAt(x, y);
    //      InstalledItem item = trashPrototypes[UnityEngine.Random.Range(0, trashPrototypes.Count)];
    //      if (isInstalledItemPositionValid(this, item.type, tile)) {
    //        PlaceInstalledItem(item.type, tile);
    //        countAdd += 1;

    //      }
    //    }
    //  }
    //}


    ////inventory items
    //private void OnInventoryItemPlaced(Tile tile) {
    //  //inventoryManager.AddInventoryItem(tile.inventoryItem);
    //}

    //private void OnInventoryItemRemoved(Tile tile) {
    //  //inventoryManager.RemoveInventoryItem(tile.inventoryItem);
    //}

    public Tile FindNearByEmptyTile(Tile t) {
      int n = UnityEngine.Random.Range(5, 10);
      while (n < 1000) {
        Vector2 vec = Funcs.Spiral(n);

        int x = t.world_x + (int)vec.x;
        int y = t.world_y + (int)vec.y;
        if (x >= 0 && y >= 0) {
          Tile tn = GetTileIfChunkExists(x, y);
          if (tn != null && tn.IsEmpty() && tn.countOfOccupied == 0) {
            return tn;

          }
        }

        n += 1;
      }

      return null;
    }

    public Tile FindEmptyTile_NotThisOne(Tile t) {
      int n = 0;
      while (n < 1000) {
        Vector2 vec = Funcs.Spiral(n);

        int x = t.world_x + (int)vec.x;
        int y = t.world_y + (int)vec.y;
        if (x >= 0 && y >= 0) {
          Tile tn = GetTileIfChunkExists(x, y);
          if (tn != null && tn.IsEmpty()) {
            return tn;

          }
        }

        n += 1;
      }

      return null;
    }

    public Tile FindEmptyUnnoccupiedTile(Tile t) {


      int n = 0;
      while (n < 1000) {
        Vector2 vec = Funcs.Spiral(n);

        int x = t.world_x + (int)vec.x;
        int y = t.world_y + (int)vec.y;
        if (x >= 0 && y >= 0) {
          Tile tn = GetTileIfChunkExists(x, y);
          if (tn != null && tn.IsEmpty() && tn.countOfOccupied == 0) {
            return tn;

          }
        }

        n += 1;
      }

      return null;
    }

    public Tile FindEmptyTile(Tile t) {

      if (t.IsEmpty()) return t;

      int n = 0;
      while (n < 1000) {
        Vector2 vec = Funcs.Spiral(n);

        int x = t.world_x + (int)vec.x;
        int y = t.world_y + (int)vec.y;
        if (x >= 0 && y >= 0) {
          Tile tn = GetTileIfChunkExists(x, y);
          if (tn != null && tn.IsEmpty()) {
            return tn;

          }
        }

        n += 1;
      }

      return null;


    }
    public Tile FindTileForInventoryItem(Tile t, string itemName, int qty) {

      if (t.IsEmptyApartFromInventory()) {
        if (t.InventoryHasSpaceFor(itemName, qty)) {
          return t;
        }
      }

      int n = 0;
      while (n < 1000) {
        Vector2 vec = Funcs.Spiral(n);

        int x = t.world_x + (int)vec.x;
        int y = t.world_y + (int)vec.y;
        if (x >= 0 && y >= 0) {
          Tile tn = GetTileIfChunkExists(x, y);
          if (tn != null) {
            if (tn.IsEmptyApartFromInventory()) {
              if (tn.InventoryHasSpaceFor(itemName, qty)) {
                return tn;
              }
            }
          }
        }

        n += 1;
      }

      return null;





      //foreach (Tile tn in t.neighboursList) {

      //}
      //foreach (Tile tn in t.neighboursList) {
      //  Tile r = FindTileForInventoryItem(tn, itemName, qty);
      //  if (r != null) {
      //    return r;
      //  }
      //}


      //return null;
    }


    public Tile FindEmptyStockPile() {
      Tile t = null;

      foreach (InstalledItem item in installedItems.Where(e => e.type == "installed::stockpile")) {
        int space = item.tile.GetInventorySpace();
        string itemName = null;
        int qtyToFetch = 0;
        switch (space) {
          case 0:
            //do nothing
            break;
          case -1:
            //completely empty
            Tile fetchFromTile = inventoryManager.GetNearest(item.tile, InventoryItem.ANY, false);
            if (fetchFromTile != null) {
              itemName = fetchFromTile.GetFirstInventoryItem();
              qtyToFetch = fetchFromTile.InventoryTotal(itemName);
            }
            break;
          default:

            break;
        }
      }


      return t;
    }



    public int PlaceTileInventoryItem(string type, Tile tile, int qty) {

      return tile.AddToInventory(type, qty);
      //InventoryItem proto = InventoryItem.GetPrototype(type);

      //if (proto != null) {
      //  InventoryItem item = InventoryItem.CreateInventoyItemInstance(type);

      //  if (item != null) {

      //    item.currentStack = qty;
      //    if (tile.PlaceInventoryItem(item)) {
      //      if (tile.inventoryItem != null) {
      //        if (cbTileInventoryItemPlacedOnTile != null) {
      //          cbTileInventoryItemPlacedOnTile(tile);
      //        }
      //        return item;
      //      }

      //    }

      //  }


      //}

      //return null;
    }

    //public InventoryItem TakeTileInventoryItem(Tile tile, string name, int qty) {
    //  if (tile.inventoryItem != null) {
    //    if (tile.inventoryItem.type == name) {
    //      if (tile.inventoryItem.currentStack <= qty) {
    //        //take all
    //        InventoryItem item = tile.inventoryItem;

    //        if (cbTileInventoryItemRemovedFromTile != null) {
    //          cbTileInventoryItemRemovedFromTile(tile);
    //        }
    //        tile.RemoveInventoryItem();
    //        return item;
    //      } else if (tile.inventoryItem.currentStack > qty) {

    //        InventoryItem item = InventoryItem.CreateInventoyItemInstance(name);
    //        item.currentStack = qty;
    //        tile.inventoryItem.currentStack -= qty;
    //        if (cbTileInventoryItemChangedOnTile != null) {
    //          cbTileInventoryItemChangedOnTile(tile);
    //        }
    //        return item;
    //      }
    //    }

    //  }

    //  return null;
    //}

    public void RemoveInstalledItem(InstalledItem item) {
      if (installedItems.Contains(item)) {
        installedItems.Remove(item);
      }

      if (trashInstances.Contains(item)) {
        trashInstances.Remove(item);
      }

      if (powerGenerators.Contains(item)) {
        RemovePowerGen(item);
      }

    }

    public InstalledItem PlaceInstalledItem(string buildItem, Tile tile, bool spawn = false) {
      if (buildItem == null) return null;
      ///TODO: Assumes 1x1 tiles
      ///with no rotation
      ///
      //Debug.Log("World.PlaceInstalledObject [" + buildItem + "] [" + tile + "]");
      InstalledItem proto = getInstalledItemProto(buildItem);

      if (proto == null) {
        Debug.LogError("no prototype for InstalledItem of type \"" + buildItem + "\"");

      } else {
        InstalledItem inst = InstalledItem.CreateInstance(this, proto, tile, spawn);
        if (inst != null) {
          if (cbInstalledItem != null) {
            cbInstalledItem(inst);

          }

          installedItems.Add(inst);
          if (inst.trash) {
            //Debug.Log("Added trash:" + inst.type);
            trashInstances.Add(inst);
          }
          if (inst.powerGenerated > 0) {
            AddPowerGen(inst);
          }

          GetNeighbours(inst.tile, true);

          if (inst.roomEnclosure) {
            DestroyRoomNodes();
            Room.CalculateRooms(inst);

          }
          if (inst.movementFactor < 1) {
            DestroyPathNodes();

          }

          return inst;
        } else {

          Debug.Log("failed to place item " + buildItem);
        }
      }
      return null;
    }

    private void DestroyRoomNodes() {
      //throw new NotImplementedException();
    }


    //--------------------------REGISTER CALLBACKS-----------------------------

    public void RegisterInstalledItemCB(Action<InstalledItem> cb) {
      cbInstalledItem += cb;

    }

    public void UnregisterInstalledItemCB(Action<InstalledItem> cb) {
      cbInstalledItem -= cb;

    }

    public void CBRegisterTileChanged(Action<Tile> cb) {
      cbTileChanged += cb;

    }

    public void CBUnregisterTileChanged(Action<Tile> cb) {
      cbTileChanged -= cb;

    }

    //public void CBRegisterInventoryItemPlacedOnTile(Action<Tile> cb) {
    //  cbTileInventoryItemPlacedOnTile += cb;

    //}

    //public void CBUnregisterInventoryItemPlacedOnTile(Action<Tile> cb) {
    //  cbTileInventoryItemPlacedOnTile -= cb;

    //}

    public void CBRegisterInventoryItemChangedOnTile(Action<Tile> cb) {
      cbTileInventoryItemChangedOnTile += cb;

    }

    public void CBUnregisterInventoryItemChangedOnTile(Action<Tile> cb) {
      cbTileInventoryItemChangedOnTile -= cb;

    }

    //public void CBRegisterInventoryItemRemovedFromTile(Action<Tile> cb) {
    //  cbTileInventoryItemRemovedFromTile += cb;

    //}

    //public void CBUnregisterInventoryItemRemovedFromTile(Action<Tile> cb) {
    //  cbTileInventoryItemRemovedFromTile -= cb;

    //}

    public void CBRegisterEntityChanged(Action<Entity> cb) {
      CBEntityChanged += cb;
    }

    public void CBUnregisterEntityChanged(Action<Entity> cb) {
      CBEntityChanged -= cb;
    }

    public void CBRegisterEntityCreated(Action<Entity> cb) {
      CBEntityCreated += cb;
    }

    public void CBUnregisterEntityCreated(Action<Entity> cb) {
      CBEntityCreated -= cb;
    }

    public void CBRegisterEntityRemoved(Action<Entity> cb) {
      CBEntityRemoved += cb;
    }

    public void CBUnregisterEntityRemoved(Action<Entity> cb) {
      CBEntityRemoved -= cb;
    }
    /*
    public void CBRegisterCharacterChanged(Action<Character> cb) {
      cbCharacterChanged += cb;

    }

    public void CBUnregisterCharacterChanged(Action<Character> cb) {
      cbCharacterChanged -= cb;

    }

    public void CBRegisterCharacterCreated(Action<Character> cb) {
      cbCharacterCreated += cb;

    }

    public void CBUnregisterCharacterCreated(Action<Character> cb) {
      cbCharacterCreated -= cb;

    }


    public void CBRegisterCharacterKilled(Action<Character> cb) {
      cbCharacterKilled += cb;

    }

    public void CBUnregisterCharacterKilled(Action<Character> cb) {
      cbCharacterKilled -= cb;

    }
    */

    public void CBRegisterPathNodesDestroyed(Action cb) {
      cbPathNodesDestroyed += cb;

    }

    public void CBUnregisterPathNodesDestroyed(Action cb) {
      cbPathNodesDestroyed -= cb;

    }

    public void CBRegisterChunkCreated(Action<TileChunk> cb) {
      cbChunkCreated += cb;

    }

    public void CBUnregisterChunkCreated(Action<TileChunk> cb) {
      cbChunkCreated -= cb;

    }



    private void DestroyPathNodes() {
      nodeMap = null;
      if (cbPathNodesDestroyed != null) {
        cbPathNodesDestroyed();
      }

    }

    public void OnTileChanged(Tile t) {
      if (cbTileChanged != null) {
        cbTileChanged(t);
        DestroyPathNodes();
        GetNeighbours(t, true);
      }

    }

    public bool isInstalledItemPositionValid(World world, string itemType, Tile t) {
      if (t == null) {
        Debug.LogError("isInstalledItemPositionValid: tile cannot be null");
      }

      if (itemType == null) {
        Debug.LogError("isInstalledItemPositionValid: itemType cannot be null");
      }


      return InstalledItem.prototypes[itemType].funcPositionValid(world, t.world_x, t.world_y);
    }

    public string GetPetName() {
      return petNames[UnityEngine.Random.Range(0, petNames.Length)];
    }

    public string GetSocialMediaName() {
      string name = "";
      int i = 0;
      bool hadNumber = false;
      int dummy = 0;
      while (i < 2) {
        string t = GetWord();
        if (t != null) {
          if (int.TryParse(t, out dummy)) {
            if (hadNumber) continue;
            hadNumber = true;
          }
          name += Funcs.TitleCase(t);
          i += 1;
        }
      }

      if (UnityEngine.Random.Range(0, 100) % 3 == 0) {
        name += GetNum();
      }


      return name;
    }

    public string GetNum() {

      if (Funcs.Chance100(50)) {
        return UnityEngine.Random.Range(1960, 2030).ToString();
      } else if (Funcs.Chance100(2)) {
        return 69.ToString();
      } else if (Funcs.Chance100(1)) {
        return 420.ToString();
      } else {
        return UnityEngine.Random.Range(1900, 5000).ToString();
      }
    }

    public string GetWord() {
      int r = UnityEngine.Random.Range(0, 6);

      switch (r) {
        case 0:
          return GetRandomString(words);//[UnityEngine.Random.Range(0, words.Length)];
        case 1:
          return GetRandomString(adjectives);//[UnityEngine.Random.Range(0, adjectives.Length)];
        case 2:
          return GetRandomString(animals);//[UnityEngine.Random.Range(0, animals.Length)];
        case 3:
          return GetNum();
        case 4:
          return GetRandomString(firstNames);
        case 5:
          return GetRandomString(petNames);
        default:
          return null;

      }

    }

    public string GetRandomString(string[] ar) {
      if (ar.Length == 0) {
        return "empty";
      }
      return ar[UnityEngine.Random.Range(0, ar.Length)];
    }

    public string GetName() {
      string name1 = firstNames[UnityEngine.Random.Range(0, firstNames.Length)];
      string name2 = lastNames[UnityEngine.Random.Range(0, lastNames.Length)];

      return Funcs.TitleCase(name1) + " " + Funcs.TitleCase(name2);

    }

    public string GetRobotName() {
      string name = GetRandomString(firstNames);

      name += " " + UnityEngine.Random.Range(1, int.MaxValue).ToString("X");

      return name;

    }

    private string[] LoadNames(string file) {
      string path = System.IO.Path.Combine(Application.streamingAssetsPath, "csv", file);
      return File.ReadAllLines(path);
    }

    private void LoadAllNames() {


      firstNames = LoadNames("firstnames.csv");
      lastNames = LoadNames("surnames.csv");
      adjectives = LoadNames("adjectives.csv");
      animals = LoadNames("animals.csv");
      words = LoadNames("words.csv");
      petNames = LoadNames("petnames.csv");

      //firstNames = File.ReadAllLines(Application.streamingAssetsPath + "/csv/firstnames.csv");
      //lastNames = File.ReadAllLines(Application.streamingAssetsPath + "/csv/surnames.csv");

      //adjectives = File.ReadAllLines(Application.streamingAssetsPath + "/csv/adjectives.txt");
      //animals = File.ReadAllLines(Application.streamingAssetsPath + "/csv/animals.txt");
      //words = File.ReadAllLines(Application.streamingAssetsPath + "/csv/words.txt");
      //petNames = File.ReadAllLines(Ap)

      //foreach (string line in lines)
      //  Console.WriteLine(line);
    }

    //utils

    public void informNeighbours(InstalledItem item) {
      //Dictionary<string, Tile> ngbrs = GetNeighbours(item);
      informNeighbour(item, item.tile.GetNeighbour(NORTH));
      informNeighbour(item, item.tile.GetNeighbour(EAST));
      informNeighbour(item, item.tile.GetNeighbour(SOUTH));
      informNeighbour(item, item.tile.GetNeighbour(WEST));

    }

    private static void informNeighbour(InstalledItem item, Tile t) {
      if (t != null && t.installedItem != null && t.installedItem.cbOnChanged != null && item.neighbourTypes.Contains(t.installedItem.type)) {
        t.installedItem.cbOnChanged(t.installedItem);
      }
    }

    //public List<Tile> GetNeighboursList(InstalledItem item, bool allowDiag = false) {
    //  return GetNeighboursList(item.tile);
    //}

    //public List<Tile> GetNeighboursList(Tile t, bool allowDiag = false) {
    //  Dictionary<string, Tile> tiles = GetNeighbours(t, allowDiag);

    //  List<Tile> rt = new List<Tile>();
    //  foreach (string s in tiles.Keys) {
    //    Tile tt = tiles[s];
    //    if (tt != null) {
    //      rt.Add(tt);
    //    }
    //  }

    //  return rt;
    //}

    public Dictionary<string, Tile> GetNeighbours(InstalledItem item, bool allowDiag = false) {
      return GetNeighbours(item.tile, allowDiag);
    }

    private Tile SetMeAndNeighbour(Tile me, string dir, string oppositeDir, int xd, int yd) {

      Tile n = GetTileIfChunkExists(xd, yd);
      me.neighbours[dir] = n; //set it, even if n is null
      if (n != null) {

        n.neighbours[oppositeDir] = me;
        n.edges[me] = GetMovementCost(n, me);


        me.edges[n] = GetMovementCost(me, n);

        //if (me.local_x == 0 && me.local_y == 0) {
        //  Debug.Log("me to n= " + me.edges[n] + " n to me=" + n.edges[me]);
        //}
      } else {

      }

      return n;

    }
    bool IsClippingCorner(Tile c, Tile n) {

      if (c == null || n == null) return false;
      int td = (int)Funcs.TaxiDistance(c, n);

      if (td == 2) {
        int dx = c.world_x - n.world_x;
        int dy = c.world_y - n.world_y;

        Tile t = GetTileIfChunkExists(c.world_x - dx, c.world_y);


        if (t == null || t.movementFactor <= 0.1) {
          return true;
        } else {
          Tile tt = GetTileIfChunkExists(c.world_x, c.world_y - dy);
          if (tt == null || tt.movementFactor <= 0.1) {
            return true;
          }
        }



      }
      return false;
    }

    public float GetMovementCost(Tile A, Tile B) {

      if (A.movementFactor > 0) {
        if (B != null && B.movementFactor > 0 && !IsClippingCorner(A, B)) {
          float mf = 1.0f / B.movementFactor;
          if (B.HasPendingWork) {
            mf *= 1.5f;
          }

          if (B.countOfOccupied > 0) {
            mf *= 1.5f;
          }

          return mf;



        }
      }



      return float.PositiveInfinity;
    }

    public Dictionary<string, Tile> GetNeighbours(Tile t, bool allowDiag = false) {
      Dictionary<string, Tile> dct; // = new Dictionary<string, Tile>();

      int x = t.world_x;
      int y = t.world_y;

      SetMeAndNeighbour(t, TheWorld.World.NORTH, TheWorld.World.SOUTH, x, y + 1);
      SetMeAndNeighbour(t, TheWorld.World.EAST, TheWorld.World.WEST, x + 1, y);
      SetMeAndNeighbour(t, TheWorld.World.SOUTH, TheWorld.World.NORTH, x, y - 1);
      SetMeAndNeighbour(t, TheWorld.World.WEST, TheWorld.World.EAST, x - 1, y);

      SetMeAndNeighbour(t, TheWorld.World.NORTHWEST, TheWorld.World.SOUTHEAST, x - 1, y + 1);
      SetMeAndNeighbour(t, TheWorld.World.NORTHEAST, TheWorld.World.SOUTHWEST, x + 1, y + 1);
      SetMeAndNeighbour(t, TheWorld.World.SOUTHWEST, TheWorld.World.NORTHEAST, x - 1, y - 1);
      SetMeAndNeighbour(t, TheWorld.World.SOUTHEAST, TheWorld.World.NORTHWEST, x + 1, y - 1);

      dct = t.neighbours;



      //dct[NORTH] = GetTileIfChunkExists(t.world_x, t.world_y + 1);
      //dct[EAST] = GetTileIfChunkExists(t.world_x + 1, t.world_y);
      //dct[SOUTH] = GetTileIfChunkExists(t.world_x, t.world_y - 1);
      //dct[WEST] = GetTileIfChunkExists(t.world_x - 1, t.world_y);

      //if (allowDiag) {
      //  dct[NORTHWEST] = GetTileAt(t.world_x - 1, t.world_y + 1);
      //  dct[NORTHEAST] = GetTileAt(t.world_x + 1, t.world_y + 1);
      //  dct[SOUTHWEST] = GetTileAt(t.world_x - 1, t.world_y - 1);
      //  dct[SOUTHEAST] = GetTileAt(t.world_x + 1, t.world_y - 1);
      //}


      return dct;
    }


    private bool hasMatchingNeighbour(Tile t, InstalledItem item) {
      if (t == null || t.installedItem == null || !t.installedItem.type.Equals(item.type)) {
        return false;
      } else {
        return true;
      }

    }




    //LOADING AND SAVING


    public XmlSchema GetSchema() {
      return null;
    }

    public void WriteXml(XmlWriter writer) {

      throw new NotImplementedException("write xml not implemented");

      //writer.WriteElementString("width", width.ToString());
      //writer.WriteElementString("height", height.ToString());
      //writer.WriteElementString("tileSize", tileSize.ToString());
      //writer.WriteElementString("xSeed", xSeed.ToString());
      //writer.WriteElementString("ySeed", ySeed.ToString());
      //writer.WriteElementString("noiseFactor", noiseFactor.ToString());

      //writer.WriteStartElement("characters");

      ////foreach (Character c in characters) {
      ////  c.WriteXml(writer);

      ////}
      //writer.WriteEndElement();

      //writer.WriteStartElement("tileTypes");

      //Dictionary<string, int> types = new Dictionary<string, int>();

      //foreach (string st in TileType.TYPES.Keys) {
      //  TileType type = TileType.TYPES[st];
      //  types[st] = type.id;

      //  writer.WriteStartElement("tileType");
      //  writer.WriteElementString("name", st);
      //  writer.WriteElementString("id", type.id.ToString());

      //  writer.WriteEndElement();

      //}

      //writer.WriteEndElement();

      //StringBuilder str = new StringBuilder();
      //StringBuilder str_installed = new StringBuilder();

      //for (int x = 0; x < width; x += 1) {
      //  for (int y = 0; y < height; y += 1) {
      //    Tile t = GetTileIfChunkExists(x, y);
      //    //str.Append(t.x + "_" + t.y + "_" + types[t.type.name] + ";");
      //    str.Append(types[t.type.name] + ";");
      //    if (t.installedItem != null) {
      //      //str.Append("I" + t.installedItem.prototypeId.ToString());
      //      str_installed.Append(t.installedItem.prototypeId.ToString() + ":" + t.world_x + ":" + t.world_y);
      //      StringBuilder installedData = new StringBuilder();
      //      installedData.Append(":(");
      //      Dictionary<string, string> items = t.installedItem.itemParameters.GetItems();
      //      int count = 0;
      //      foreach (string k in items.Keys) {
      //        if (count > 0) {
      //          installedData.Append(',');

      //        }
      //        installedData.Append("{" + k + "}={" + items[k].ToString() + "}");
      //        count += 1;
      //      }
      //      installedData.Append(")");
      //      str_installed.Append(installedData + ";");
      //    }


      //  }
      //}
      ////Debug.Log("original: " + str.Length + "\n" + str.ToString());

      //Debug.Log("installed: " + str_installed.ToString());

      //byte[] tileByes = Funcs.Zip(str.ToString());
      //byte[] installedBytes = Funcs.Zip(str_installed.ToString());
      //Debug.Log("bytes: " + tileByes.Length);
      //writer.WriteStartElement("tiles");
      //writer.WriteBase64(tileByes, 0, tileByes.Length);
      //writer.WriteEndElement();

      //writer.WriteStartElement("installed");
      //writer.WriteBase64(installedBytes, 0, installedBytes.Length);
      //writer.WriteEndElement();


      ////jobQueue.WriteXml(writer);


    }

    public void ReadXml(XmlReader reader) {
      Debug.Log("read xml");


      string tilesString = "empty tile string";
      //reader.MoveToContent();
      List<XElement> elements = new List<XElement>();
      //XElement xe = null;// = new XElement()

      while (reader.NodeType != XmlNodeType.Element)
        reader.Read();

      XElement xe = XElement.Load(reader);



      Debug.Log("XE = " + xe);

      this.width = int.Parse(xe.Elements().Where(e => e.Name.LocalName == "width").Single().Value);
      this.height = int.Parse(xe.Elements().Where(e => e.Name.LocalName == "height").Single().Value);
      this.xSeed = float.Parse(xe.Elements().Where(e => e.Name.LocalName == "xSeed").Single().Value);
      this.ySeed = float.Parse(xe.Elements().Where(e => e.Name.LocalName == "ySeed").Single().Value);
      this.noiseFactor = float.Parse(xe.Elements().Where(e => e.Name.LocalName == "noiseFactor").Single().Value);
      this.tileSize = int.Parse(xe.Elements().Where(e => e.Name.LocalName == "tileSize").Single().Value);


      string installed_string = xe.Elements().Where(e => e.Name.LocalName == "installed").Single().Value;
      byte[] ib = Funcs.Base64Decode(installed_string, true);
      installed_string = Funcs.Unzip(ib);
      char[] c = { ';' };
      savedInstalledItems = installed_string.Split(c, StringSplitOptions.RemoveEmptyEntries);

      Debug.Log("installed: " + installed_string);


      tilesString = xe.Elements().Where(e => e.Name.LocalName == "tiles").Single().Value;
      //Debug.Log("tiles string = " + tilesString);
      byte[] tb = Funcs.Base64Decode(tilesString, true);

      tilesString = Funcs.Unzip(tb);
      string[] tilesArray = tilesString.Split(';');




      Debug.Log("seeds: " + xSeed + " " + ySeed + " " + noiseFactor);
      //Debug.Log("Tiles: " + tilesString.Length + ":" + tilesString);




      Debug.Log("length of array " + tilesArray.Length + " width x height: " + (width * height));
      SetCollections();
      SetCallbacks();
      //CreateAllInstalledItemPrototypes();
      Entity.LoadFromFile();
      InstalledItem.LoadFromFile();
      InventoryItem.LoadFromFile();
      Recipe.LoadFromFile();

      CreateEmptyTiles();
      SetTilesFromArray(tilesArray);
      //SetInstalledFromArray(installedArray);


      //---------CHARACTERS
      foreach (XElement chrgroup in xe.Elements().Where(e => e.Name.LocalName == "characters")) {
        foreach (XElement chrxml in chrgroup.Elements().Where(e => e.Name.LocalName == "character")) {
          Debug.Log("characters: " + chrxml);
          int x = int.Parse(chrxml.Elements().Where(e => e.Name.LocalName == "xPos").Single().Value);
          int y = int.Parse(chrxml.Elements().Where(e => e.Name.LocalName == "yPos").Single().Value);
          string state = chrxml.Elements().Where(e => e.Name.LocalName == "state").Single().Value;
          string name = chrxml.Elements().Where(e => e.Name.LocalName == "name").Single().Value;
          Tile t = GetTileAt(x, y);
          if (t != null) {
            //Character chr = new Character(this, t, name, state);
            //if (chr != null) {
            //  CreateCharacter(chr);
            //}

          }

        }
      }
      //-------- END CHARACTERS




      LoadAllNames();

      //Init(width, height, 32, true, tilesArray);
    }

    public void SetInstalledFromArray() {
      throw new NotImplementedException("not implemented");
      //if (savedInstalledItems != null) {
      //  foreach (string s in savedInstalledItems) {
      //    string[] ss = s.Split(':');
      //    //Debug.Log(String.Format("0:\"{0}\" 1:\"{1}\" 2:\"{2}\"", ss[0], ss[1], ss[2]));
      //    int installedItemId = int.Parse(ss[0]);
      //    int x = int.Parse(ss[1]);
      //    int y = int.Parse(ss[2]);
      //    string prms = ss[3];
      //    Tile t = GetTileAt(x, y);
      //    if (installedItemId > 0 && x >= 0 && y >= 0 && t != null) {
      //      InstalledItem item = PlaceInstalledItem(getInstalledItemProtoById(installedItemId).type, t);//InstalledItem.CreateInstance(this, getInstalledItemProtoById(installedItemId), t);
      //      installedItems.Add(item);
      //      if (prms != null) {
      //        item.SetParameters(prms);
      //      }
      //    }
      //  }
      //}


    }

    private void SetTilesFromArray(string[] tilesArray) {
      //if (tilesArray != null) {
      //  for (int x = 0; x < width; x += 1) {
      //    for (int y = 0; y < height; y += 1) {
      //      int idx = y + x * width;
      //      //Debug.Log(tilesArray[idx]);

      //      string si = tilesArray[idx];
      //      int typeIndex = -1; // int.Parse(tilesArray[idx]);
      //                          //int installedItemId = -1;
      //                          //string installedItemName = null;

      //      if (int.TryParse(si, out typeIndex)) {
      //        TileType tt = TileType.TYPES_BY_ID[typeIndex];

      //        tiles[x, y].SetType(tt);
      //      }
      //    }
      //  }
      //}
    }



    //END LOADING

    public void Kill() {
      workManager = null;
      //installedItemProtos = null;
      //installedItemProtos_BY_ID = null;
      trashPrototypes = null;
      trashInstances = null;
      //tiles = null;
      nodeMap = null;
      chunks = null;

      //foreach (Character c in characters) {
      //  c.Kill();
      //  if (cbCharacterKilled != null) {
      //    cbCharacterKilled(c);
      //  }
      //}
      //characters = null;
    }

  }

}
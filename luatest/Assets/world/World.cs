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

  //testing
  public static readonly int NUMBER_OF_ROBOTS = 5;

  public static readonly int TEST_WIDTH = 50;
  public static readonly int TEST_HEIGHT = 50;

  public static readonly int M_M_MAXIMUM_TRASH = 25;



  //collections
  Dictionary<string, InstalledItem> installedItemProtos;
  Dictionary<int, string> installedItemProtos_BY_ID;
  public List<InstalledItem> trashPrototypes;
  public List<InstalledItem> trashInstances;
  private List<InstalledItem> installedItems;
  public List<Character> characters;
  public string[] names;
  private Tile[,] tiles;
  public List<Room> rooms;
  //public List<InventoryItem> inventoryItems;
  //properties
  private float xSeed, ySeed, noiseFactor;


  public int width { get; private set; }
  public int height { get; private set; }
  public int tileSize { get; private set; }
  bool allowDiagonalNeighbours = true;


  //objects
  public JobQueue jobQueue;
  public TileNodeMap nodeMap;
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
  Action<Character> cbCharacterChanged;
  Action<Character> cbCharacterCreated;
  Action<Character> cbCharacterKilled;
  Action cbPathNodesDestroyed;

  Action<InventoryItem> cbInventoryItemCreated;
  Action<InventoryItem> cbInventoryItemChanged;
  Action<InventoryItem> cbInventoryItemDestroyed;

  //collections
  string[] savedInstalledItems;






  void CreateEmptyTiles() {

    for (int x = 0; x < width; x += 1) {
      for (int y = 0; y < height; y += 1) {
        tiles[x, y] = new Tile(this, TileType.TYPES["empty"], x, y);
        tiles[x, y].cbRegisterOnChanged(OnTileChanged);
        //tiles[x, y].room = rooms[0];
        outside.AssignTile(tiles[x, y]);
      }


    }
  }

  private void SetCollections() {
    installedItemProtos = new Dictionary<string, InstalledItem>();
    installedItemProtos_BY_ID = new Dictionary<int, string>();
    characters = new List<Character>();
    jobQueue = new JobQueue();
    tiles = new Tile[width, height];
    installedItems = new List<InstalledItem>();
    trashPrototypes = new List<InstalledItem>();
    trashInstances = new List<InstalledItem>();
    rooms = new List<Room>();
    rooms.Add(new Room(this));
    //inventoryItems = new List<InventoryItem>();
    inventoryManager = new InventoryManager(this);
    //outside = rooms[0];
  }

  void InitNew(int width, int height, int tileSize) {

    xSeed = UnityEngine.Random.Range(-10f, 10f);
    ySeed = UnityEngine.Random.Range(-10f, 10f);
    noiseFactor = UnityEngine.Random.Range(0.01f, 0.1f);



    this.width = width;
    this.height = height;
    this.tileSize = tileSize;

    SetCollections();

    CreateAllInstalledItemPrototypes();
    InventoryItem.LoadFromFile();

    CreateEmptyTiles();

    loadNames();

 

  }

  public Tile GetRandomEmptyTile() {

    Tile t = null;
    int counter = 0;
    while(t == null && counter < 100) {
      counter += 1;

      t = GetRandomTile();

      if (!t.IsEmpty()) {
        t = null;

      }

    }


    return t;


  }

  public Tile GetRandomTile() {
    Tile t = getTileAt(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));

    

    return t;
  }

  public void SetJoinedSprites() {
    for (int x = 0; x < width; x += 1) {
      for (int y = 0; y < height; y += 1) {
        Tile t = getTileAt(x, y);
        if (t != null && t.installedItem != null) {
          informNeighbours(t.installedItem);

        }
      }
    }

  }

  //-----------------------------CONSTRUCTORS------------------------------
  public World() {

  }
  public World(int width = 50, int height = 50, int tileSize = 32) {

    InitNew(width, height, tileSize);
    //CreateCharacters();
  }

  public void SetAllNeighbours() {
    for (int x = 0; x < width; x += 1) {
      for (int y = 0; y < height; y += 1) {
        tiles[x, y].SetNeighbours(allowDiagonalNeighbours);

      }


    }
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
    foreach (Character c in characters) {
      c.Update(deltaTime);
    }

    foreach (InstalledItem item in installedItems) {
      item.Update(deltaTime);
    }
  }


  //-------------------------------------CHARACTERS-------------------------------------

  public void OnCharacterChanged(Character c) {
    if (cbCharacterChanged != null) {
      cbCharacterChanged(c);
    }
  }

  public void CreateCharacters() {
    int attempts = 0;
    int created = 0;
    while (created < NUMBER_OF_ROBOTS && attempts < 10000) {
      int x = UnityEngine.Random.Range(0, width);
      int y = UnityEngine.Random.Range(0, height);
      Tile t = getTileAt(x, y);

      if (t.type.movementFactor >= 0.5) {

        Character c = new Character(this, tiles[x, y]);

        if (CreateCharacter(c)) {
          created += 1;
        }
      }
    }

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



  //-========================================INSTALLED ITEMS---=========================================

  public Dictionary<string, InstalledItem> getProtoList() {
    return installedItemProtos;
  }

  public InstalledItem getInstalledItemProtoById(int id) {
    if (installedItemProtos_BY_ID.ContainsKey(id)) {
      return getInstalledItemProto(installedItemProtos_BY_ID[id]);

    } else {
      return null;
    }
  }

  public InstalledItem getInstalledItemProto(string item) {
    if (installedItemProtos.ContainsKey(item)) {
      return installedItemProtos[item];
    } else {
      return null;
    }
  }

  private void CreateAllInstalledItemPrototypes() {
    int unnamedCounter = 0;
    string path = Application.streamingAssetsPath + "/json/InstalledItems.json";

    string json = File.ReadAllText(path);
    JObject jo = JObject.Parse(json);

    JArray installedItemsArray = (JArray)jo["InstalledItems"];


    foreach (JObject installedItemJson in installedItemsArray) {
      string name = Funcs.jsonGetString(installedItemJson["name"], "unnamed_" + unnamedCounter);
      string niceName = Funcs.jsonGetString(installedItemJson["niceName"], "JSON MISSING");
      unnamedCounter += 1;
      string sprite = Funcs.jsonGetString(installedItemJson["sprite"], "");
      List<string> sprites = new List<string>();
      JArray spritesJsonArray = (JArray)installedItemJson["sprites"];
      foreach (string tempSpriteName in spritesJsonArray) {
        sprites.Add(tempSpriteName);
      }
      bool linked = Funcs.jsonGetBool(installedItemJson["linked"], false);
      List<string> neighbourTypeList = new List<string>();
      if (linked) {

        JArray neighbourTypes = Funcs.jsonGetArray(installedItemJson, "neighbourTypes");//(JArray)installedItemJson["neighbourTypes"];

        if (neighbourTypes != null) {
          foreach (string s in neighbourTypes) {
            neighbourTypeList.Add(s);
            Debug.Log("added [" + s + "]");
          }
        }


      }
      if (!neighbourTypeList.Contains(name)) {
        neighbourTypeList.Add(name);
      }


      float movement = Funcs.jsonGetFloat(installedItemJson["movementFactor"], 1);
      bool trash = Funcs.jsonGetBool(installedItemJson["trash"], false);
      bool build = Funcs.jsonGetBool(installedItemJson["build"], false);
      bool rotate = Funcs.jsonGetBool(installedItemJson["randomRotation"], false);
      int w = Funcs.jsonGetInt(installedItemJson["width"], 1);
      int h = Funcs.jsonGetInt(installedItemJson["height"], 1);
      int id = Funcs.jsonGetInt(installedItemJson["id"], -1);
      bool enclosesRoom = Funcs.jsonGetBool(installedItemJson["enclosesRoom"], false);

      InstalledItem proto = CreateOneInstalledItemPrototype(name, niceName, sprite, movement, w, h, linked, build, trash, rotate, id, enclosesRoom);
      proto.neighbourTypes = neighbourTypeList;
      //proto.roomEnclosure = enclosesRoom;

      Debug.Log(proto.ToString());

      if (linked) {
        string n_s = Funcs.jsonGetString(installedItemJson["neighbour_s"], "");
        string n_ns = Funcs.jsonGetString(installedItemJson["neighbour_ns"], "");
        string n_nsw = Funcs.jsonGetString(installedItemJson["neighbour_nsw"], "");
        string n_sw = Funcs.jsonGetString(installedItemJson["neighbour_sw"], "");
        string n_nesw = Funcs.jsonGetString(installedItemJson["neighbour_nesw"], "");
        proto.setLinkedSpriteNames(n_ns, n_nsw, n_s, n_sw, n_nesw);
      }

      if (sprites.Count > 0) {
        proto.setRandomSprites(sprites);
        //Debug.Log("proto " + proto.type + " has " + proto.randomSprites.Count + " random sprites");

      }

    }
    //InstalledItem proto = InstalledItemProto("installed::wall", "walls_none", 0, 1, 1,true);
    //

    foreach (string ss in installedItemProtos.Keys) {
      InstalledItem item = installedItemProtos[ss];
      if (item.trash) {
        trashPrototypes.Add(item);
      }
    }


  }


  private InstalledItem CreateOneInstalledItemPrototype(string name, string niceName, string spriteName, float movementFactor, int width, int height, bool linksToNeighbour, bool build, bool trash, bool rotate, int id, bool enclosesRoom) {
    InstalledItem proto = null;
    if (!installedItemProtos.ContainsKey(name)) {
      proto = InstalledItem.CreatePrototype(name, niceName, spriteName, movementFactor, width, height, linksToNeighbour, build, trash, rotate, id, enclosesRoom);
      installedItemProtos.Add(name, proto);
      installedItemProtos_BY_ID.Add(proto.prototypeId, name);

      if (name == "installed::door") {
        proto.itemParameters.SetFloat("openness", 0f);
        proto.itemParameters.SetFloat("opentime", 0.25f);
        proto.updateActions += InstalledItemActions.Door_UpdateActions;

        proto.enterRequested += InstalledItemActions.Door_EnterRequested;
      }
    }
    Debug.Log("prototype created: " + proto);

    return proto;
  }

  public Tile getTileAt(int x, int y) {
    if (x < 0 || x > width - 1 || y < 0 || y > height - 1) {
      //Debug.LogError("Tile (" + x + "," + y + ") is out of bounds");
      return null;
    }
    return tiles[x, y];
  }

  //------------------------------------TILES--------------------------------------

  public void RandomiseTiles() {
    Debug.Log("Randomise Tiles");
    float xo = xSeed;// UnityEngine.Random.Range(-10, 10);
    float yo = ySeed; // UnityEngine.Random.Range(-10, 10);
    float xx = 0;
    float yy = 0;
    for (int x = 0; x < width; x += 1) {
      xx = ((float)x) * noiseFactor;
      for (int y = 0; y < height; y += 1) {
        yy = ((float)y) * noiseFactor;
        Tile t = tiles[x, y];
        //if (UnityEngine.Random.Range(0,2) == 0)
        float f = Mathf.PerlinNoise(xx + xo, yy + yo);
        TileType tt = TileType.TYPES["empty"];
        foreach (string k in TileType.TYPES.Keys) {
          TileType tempT = TileType.TYPES[k];

          if (tempT.name != "empty") {
            if (f >= tempT.rangeLow && f < tempT.rangeHigh) {
              t.SetType(tempT);
              break;
            }
          }

        }
      }


    }
  }

  public void PlaceTrash() {
    //int countOfTrash = trashInstances.Count;
    if (trashPrototypes.Count > 0) {
      int attempts = 0;
      int countAdd = 0;
      while (countAdd < M_M_MAXIMUM_TRASH && attempts < 100) {
        attempts += 1;
        int x = UnityEngine.Random.Range(0, width);
        int y = UnityEngine.Random.Range(0, height);

        Tile tile = getTileAt(x, y);
        InstalledItem item = trashPrototypes[UnityEngine.Random.Range(0, trashPrototypes.Count)];
        if (isInstalledItemPositionValid(this, item.type, tile)) {
          PlaceInstalledItem(item.type, tile);
          countAdd += 1;

        }
      }
    }
  }

  public InventoryItem PlaceInventoryItem(string type, Tile tile, int qty) {
    InventoryItem proto = InventoryItem.GetPrototype(type);

    if (proto != null) {
      InventoryItem item = InventoryItem.CreateInventoyItemInstance(type);

      if (item != null) {
        
        item.currentStack = qty;
        if (tile.PlaceInventoryItem(item)) {
          if (tile.inventoryItem != null) {
            if (cbInventoryItemCreated != null) {
              cbInventoryItemCreated(tile.inventoryItem);
            }
          }

        }

      }


    }

    return null;
  }

  public InstalledItem PlaceInstalledItem(string buildItem, Tile tile) {
    ///TODO: Assumes 1x1 tiles
    ///with no rotation
    ///
    //Debug.Log("World.PlaceInstalledObject [" + buildItem + "] [" + tile + "]");
    InstalledItem proto = getInstalledItemProto(buildItem);

    if (proto == null) {
      //Debug.LogError("no prototype for InstalledItem of type \"" + buildItem + "\"");

    } else {
      InstalledItem inst = InstalledItem.CreateInstance(this, proto, tile);
      if (inst != null) {
        if (cbInstalledItem != null) {
          cbInstalledItem(inst);
          installedItems.Add(inst);
          if (inst.trash) {
            //Debug.Log("Added trash:" + inst.type);
            trashInstances.Add(inst);
          }
        }

        if (inst.roomEnclosure) {
          DestroyRoomNodes();
          Room.CalculateRooms(inst);

        }
        if (inst.movementFactor < 1) {
          DestroyPathNodes();

        }

        return inst;
      } else {
        //Debug.Log("failed to place item " + buildItem);
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

  public void CBRegisterInventoryItemCreated(Action<InventoryItem> cb) {
    cbInventoryItemCreated += cb;

  }

  public void CBUnregisterInventoryItemCreated(Action<InventoryItem> cb) {
    cbInventoryItemCreated -= cb;

  }

  public void CBRegisterInventoryItemChanged(Action<InventoryItem> cb) {
    cbInventoryItemChanged += cb;

  }

  public void CBUnregisterInventoryItemChanged(Action<InventoryItem> cb) {
    cbInventoryItemChanged -= cb;

  }

  public void CBRegisterInventoryItemDestroyed(Action<InventoryItem> cb) {
    cbInventoryItemDestroyed += cb;

  }

  public void CBUnregisterInventoryItemDestroyed(Action<InventoryItem> cb) {
    cbInventoryItemDestroyed -= cb;

  }

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

  public void CBRegisterPathNodesDestroyed(Action cb) {
    cbPathNodesDestroyed += cb;

  }

  public void CBUnregisterPathNodesDestroyed(Action cb) {
    cbPathNodesDestroyed -= cb;

  }



  private void DestroyPathNodes() {
    nodeMap = null;
    if (cbPathNodesDestroyed != null) {
      cbPathNodesDestroyed();
    }

  }

  void OnTileChanged(Tile t) {
    if (cbTileChanged != null) {
      cbTileChanged(t);
      DestroyPathNodes();
    }
  }

  public bool isInstalledItemPositionValid(World world, string itemType, Tile t) {
    if (t == null) {
      Debug.LogError("isInstalledItemPositionValid: tile cannot be null");
    }

    if (itemType == null) {
      Debug.LogError("isInstalledItemPositionValid: itemType cannot be null");
    }

    if (installedItemProtos == null) {
      Debug.LogError("isInstalledItemPositionValid: installedItemProtos cannot be null");
    }
    return installedItemProtos[itemType].funcPositionValid(world, t.x, t.y);
  }

  public string GetName() {
    string name1 = names[UnityEngine.Random.Range(0, names.Length)];
    string name2 = names[UnityEngine.Random.Range(0, names.Length)];

    return Funcs.TitleCase(name1) + " " + Funcs.TitleCase(name2);

  }

  private void loadNames() {
    names = File.ReadAllLines(Application.streamingAssetsPath + "/csv/surnames.csv");

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

  public List<Tile> GetNeighboursList(InstalledItem item, bool allowDiag = false) {
    return GetNeighboursList(item.tile);
  }

  public List<Tile> GetNeighboursList(Tile t, bool allowDiag = false) {
    Dictionary<string, Tile> tiles = GetNeighbours(t, allowDiag);

    List<Tile> rt = new List<Tile>();
    foreach (string s in tiles.Keys) {
      Tile tt = tiles[s];
      if (tt != null) {
        rt.Add(tt);
      }
    }

    return rt;
  }

  public Dictionary<string, Tile> GetNeighbours(InstalledItem item, bool allowDiag = false) {
    return GetNeighbours(item.tile, allowDiag);
  }

  public Dictionary<string, Tile> GetNeighbours(Tile t, bool allowDiag = false) {
    Dictionary<string, Tile> dct = new Dictionary<string, Tile>();

    dct[NORTH] = getTileAt(t.x, t.y + 1);
    dct[EAST] = getTileAt(t.x + 1, t.y);
    dct[SOUTH] = getTileAt(t.x, t.y - 1);
    dct[WEST] = getTileAt(t.x - 1, t.y);

    if (allowDiag) {
      dct[NORTHWEST] = getTileAt(t.x - 1, t.y + 1);
      dct[NORTHEAST] = getTileAt(t.x + 1, t.y + 1);
      dct[SOUTHWEST] = getTileAt(t.x - 1, t.y - 1);
      dct[SOUTHEAST] = getTileAt(t.x + 1, t.y - 1);
    }


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
    writer.WriteElementString("width", width.ToString());
    writer.WriteElementString("height", height.ToString());
    writer.WriteElementString("tileSize", tileSize.ToString());
    writer.WriteElementString("xSeed", xSeed.ToString());
    writer.WriteElementString("ySeed", ySeed.ToString());
    writer.WriteElementString("noiseFactor", noiseFactor.ToString());

    writer.WriteStartElement("characters");

    foreach (Character c in characters) {
      c.WriteXml(writer);

    }
    writer.WriteEndElement();

    writer.WriteStartElement("tileTypes");

    Dictionary<string, int> types = new Dictionary<string, int>();

    foreach (string st in TileType.TYPES.Keys) {
      TileType type = TileType.TYPES[st];
      types[st] = type.id;

      writer.WriteStartElement("tileType");
      writer.WriteElementString("name", st);
      writer.WriteElementString("id", type.id.ToString());

      writer.WriteEndElement();

    }

    writer.WriteEndElement();

    StringBuilder str = new StringBuilder();
    StringBuilder str_installed = new StringBuilder();

    for (int x = 0; x < width; x += 1) {
      for (int y = 0; y < height; y += 1) {
        Tile t = getTileAt(x, y);
        //str.Append(t.x + "_" + t.y + "_" + types[t.type.name] + ";");
        str.Append(types[t.type.name] + ";");
        if (t.installedItem != null) {
          //str.Append("I" + t.installedItem.prototypeId.ToString());
          str_installed.Append(t.installedItem.prototypeId.ToString() + ":" + t.x + ":" + t.y);
          StringBuilder installedData = new StringBuilder();
          installedData.Append(":(");
          Dictionary<string, string> items = t.installedItem.itemParameters.GetItems();
          int count = 0;
          foreach (string k in items.Keys) {
            if (count > 0) {
              installedData.Append(',');

            }
            installedData.Append("{" + k + "}={" + items[k].ToString() + "}");
            count += 1;
          }
          installedData.Append(")");
          str_installed.Append(installedData + ";");
        } else if (t.inventoryItem != null) {
          ///FIXME:
          ///
        }


      }
    }
    //Debug.Log("original: " + str.Length + "\n" + str.ToString());

    Debug.Log("installed: " + str_installed.ToString());

    byte[] tileByes = Funcs.Zip(str.ToString());
    byte[] installedBytes = Funcs.Zip(str_installed.ToString());
    Debug.Log("bytes: " + tileByes.Length);
    writer.WriteStartElement("tiles");
    writer.WriteBase64(tileByes, 0, tileByes.Length);
    writer.WriteEndElement();

    writer.WriteStartElement("installed");
    writer.WriteBase64(installedBytes, 0, installedBytes.Length);
    writer.WriteEndElement();


    jobQueue.WriteXml(writer);


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
    CreateAllInstalledItemPrototypes();
    InventoryItem.LoadFromFile();
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
        Tile t = getTileAt(x, y);
        if (t != null) {
          Character chr = new Character(this, t, name, state);
          if (chr != null) {
            CreateCharacter(chr);
          }

        }

      }
    }
    //-------- END CHARACTERS




    loadNames();

    //Init(width, height, 32, true, tilesArray);
  }

  public void SetInstalledFromArray() {

    if (savedInstalledItems != null) {
      foreach (string s in savedInstalledItems) {
        string[] ss = s.Split(':');
        //Debug.Log(String.Format("0:\"{0}\" 1:\"{1}\" 2:\"{2}\"", ss[0], ss[1], ss[2]));
        int installedItemId = int.Parse(ss[0]);
        int x = int.Parse(ss[1]);
        int y = int.Parse(ss[2]);
        string prms = ss[3];




        Tile t = getTileAt(x, y);
        if (installedItemId > 0 && x >= 0 && y >= 0 && t != null) {


          InstalledItem item = PlaceInstalledItem(getInstalledItemProtoById(installedItemId).type, t);//InstalledItem.CreateInstance(this, getInstalledItemProtoById(installedItemId), t);
          installedItems.Add(item);
          if (prms != null) {
            item.SetParameters(prms);
          }
        }
      }
    }


  }

  private void SetTilesFromArray(string[] tilesArray) {
    if (tilesArray != null) {
      for (int x = 0; x < width; x += 1) {
        for (int y = 0; y < height; y += 1) {
          int idx = y + x * width;
          //Debug.Log(tilesArray[idx]);

          string si = tilesArray[idx];
          int typeIndex = -1; // int.Parse(tilesArray[idx]);
                              //int installedItemId = -1;
                              //string installedItemName = null;

          if (int.TryParse(si, out typeIndex)) {
            TileType tt = TileType.TYPES_BY_ID[typeIndex];

            tiles[x, y].SetType(tt);
          }
        }
      }
    }
  }



  //END LOADING

  public void Kill() {
    jobQueue = null;
    installedItemProtos = null;
    installedItemProtos_BY_ID = null;
    trashPrototypes = null;
    trashInstances = null;
    tiles = null;
    nodeMap = null;

    foreach (Character c in characters) {
      c.Kill();
      if (cbCharacterKilled != null) {
        cbCharacterKilled(c);
      }
    }
    characters = null;
  }

}


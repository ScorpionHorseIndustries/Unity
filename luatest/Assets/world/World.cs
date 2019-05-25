﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text;
using System.Xml.Linq;

public class World : IXmlSerializable {



  Dictionary<string, InstalledItem> installedItemProtos;
  Dictionary<int, string> installedItemProtos_BY_ID;


  private float xSeed, ySeed, noiseFactor;
  private Tile[,] tiles;
  private List<InstalledItem> installedItems;
  public int width { get; private set; }
  public int height { get; private set; }
  public int tileSize { get; private set; }
  Action<InstalledItem> cbInstalledItem;
  Action<Tile> cbTileChanged;
  Action<Character> cbCharacterChanged;
  Action<Character> cbCharacterCreated;
  Action<Character> cbCharacterKilled;
  public JobQueue jobQueue;
  public List<Character> characters;
  bool allowDiagonalNeighbours = true;
  public string[] names;

  //private static readonly float NOISE_FACTOR = 0.05f;
  [NonSerialized]
  public TileNodeMap nodeMap;

  //STATIC
  public static readonly int NUMBER_OF_ROBOTS = 10;

  public static readonly int TEST_WIDTH = 100;
  public static readonly int TEST_HEIGHT = 100;





  void createEmptyTiles() {
    for (int x = 0; x < width; x += 1) {
      for (int y = 0; y < height; y += 1) {
        tiles[x, y] = new Tile(this, TileType.TYPES["empty"], x, y);
        tiles[x, y].cbRegisterOnChanged(OnTileChanged);
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
  }

  void InitNew(int width, int height, int tileSize) {

    xSeed = UnityEngine.Random.Range(-10f, 10f);
    ySeed = UnityEngine.Random.Range(-10f, 10f);
    noiseFactor = UnityEngine.Random.Range(0.01f, 0.1f);



    this.width = width;
    this.height = height;
    this.tileSize = tileSize;

    SetCollections();

    createInstalledItemProtos();

    createEmptyTiles();

    loadNames();

  }


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

  public void Update(float deltaTime) {
    foreach (Character c in characters) {
      c.Update(deltaTime);
    }
  }

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
      return true;
    }


    return false;
  }

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

  private void createInstalledItemProtos() {
    int unnamedCounter = 0;
    string path = Application.streamingAssetsPath + "/json/InstalledItems.json";

    string json = File.ReadAllText(path);
    JObject jo = JObject.Parse(json);

    JArray ja = (JArray)jo["InstalledItems"];

    foreach (JObject j in ja) {
      string name = Funcs.jsonGetString(j["name"], "unnamed_" + unnamedCounter);
      unnamedCounter += 1;
      string sprite = Funcs.jsonGetString(j["sprite"], "");
      List<string> sprites = new List<string>();
      JArray ja2 = (JArray)j["sprites"];
      foreach (string ja2s in ja2) {
        sprites.Add(ja2s);
      }
      bool linked = Funcs.jsonGetBool(j["linked"], false);
      if (linked) {

      }

      float movement = Funcs.jsonGetFloat(j["movementFactor"], 1);
      bool trash = Funcs.jsonGetBool(j["trash"], false);
      bool build = Funcs.jsonGetBool(j["build"], false);
      bool rotate = Funcs.jsonGetBool(j["randomRotation"], false);
      int w = Funcs.jsonGetInt(j["width"], 1);
      int h = Funcs.jsonGetInt(j["height"], 1);
      int id = Funcs.jsonGetInt(j["id"], -1);

      InstalledItem proto = InstalledItemProto(name, sprite, movement, w, h, linked, build, trash, rotate, id);

      if (linked) {
        string n_s = Funcs.jsonGetString(j["neighbour_s"], "");
        string n_ns = Funcs.jsonGetString(j["neighbour_ns"], "");
        string n_nsw = Funcs.jsonGetString(j["neighbour_nsw"], "");
        string n_sw = Funcs.jsonGetString(j["neighbour_sw"], "");
        string n_nesw = Funcs.jsonGetString(j["neighbour_nesw"], "");
        proto.setLinkedSpriteNames(n_ns, n_nsw, n_s, n_sw, n_nesw);
      }

      if (sprites.Count > 0) {
        proto.setRandomSprites(sprites);
        Debug.Log("proto " + proto.type + " has " + proto.randomSprites.Count + " random sprites");

      }

    }
    //InstalledItem proto = InstalledItemProto("installed::wall", "walls_none", 0, 1, 1,true);
    //


  }


  private InstalledItem InstalledItemProto(string name, string spriteName, float movementFactor, int width, int height, bool linksToNeighbour, bool build, bool trash, bool rotate, int id) {
    InstalledItem proto = null;
    if (!installedItemProtos.ContainsKey(name)) {
      proto = InstalledItem.CreatePrototype(name, spriteName, movementFactor, width, height, linksToNeighbour, build, trash, rotate, id);
      installedItemProtos.Add(name, proto);
      installedItemProtos_BY_ID.Add(proto.prototypeId, name);

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

  public void GenerateMap() {

    List<int[,]> maps = new List<int[,]>();

    for (int i = 0; i < TileType.countNatural - 1; i += 1) {
      maps.Add(MakeInitialMap(50 - (i * 3), Time.time + "_" + i.ToString()));
    }

    for (int i = 0; i < maps.Count; i += 1) {
      maps[i] = SmoothMap(maps[i], 5);
    }

    int[,] map = new int[width, height];

    for (int i = 0; i < maps.Count; i += 1) {
      for (int x = 0; x < width; x += 1) {
        for (int y = 0; y < height; y += 1) {
          map[x, y] += maps[i][x, y];
        }
      }
    }

    for (int x = 0; x < width; x += 1) {
      for (int y = 0; y < height; y += 1) {
        int j = map[x, y];
        Tile t = tiles[x, y];
        //if (UnityEngine.Random.Range(0,2) == 0)
        //float f = Mathf.PerlinNoise(xx + xo, yy + yo);
        TileType tt = TileType.TYPES["empty"];
        foreach (string k in TileType.TYPES.Keys) {
          TileType tempT = TileType.TYPES[k];

          if (tempT.name != "empty") {
            if (j == tempT.height) {
              t.SetType(tempT);
              break;
            }
          }

        }

      }
    }



  }

  private int[,] SmoothMap(int[,] map, int smoothCount, int threshhold = 4) {

    for (int i = 0; i < smoothCount; i += 1) {
      for (int x = 0; x < width; x += 1) {
        for (int y = 0; y < height; y += 1) {
          int walls = GetCountOfWalls(map, x, y);

          if (walls > threshhold) {
            map[x, y] = 1;
          } else if (walls < threshhold) {
            map[x, y] = 0;
          }

        }
      }
    }



    return map;
  }

  private int GetCountOfWalls(int[,] map, int x, int y) {
    int walls = 0;
    for (int nx = x - 1; nx <= x + 1; nx += 1) {
      for (int ny = y - 1; ny <= y + 1; ny += 1) {
        if (nx == x && ny == y) continue;
        if (nx < 0 || nx > width - 1 || ny < 0 || ny > height - 1) {
          walls += 1;
        } else {
          walls += map[nx, ny];
        }
      }
    }

    return walls;

  }

  private int[,] MakeInitialMap(int fillPercent, string seed) {

    UnityEngine.Random.InitState(seed.GetHashCode());

    int[,] map = new int[width, height];
    for (int x = 0; x < width; x += 1) {
      for (int y = 0; y < height; y += 1) {
        int r = UnityEngine.Random.Range(0, 100);
        map[x, y] = (r < fillPercent) ? 1 : 0;
      }
    }

    return map;

  }

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

  public void PlaceInstalledObject(string buildItem, Tile tile) {
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
        }
        DestroyPathNodes();
      } else {
        //Debug.Log("failed to place item " + buildItem);
      }
    }

  }

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

  private void DestroyPathNodes() {
    nodeMap = null;

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
    Dictionary<string, Tile> ngbrs = GetNeighbours(item);
    informNeighbour(item, ngbrs["north"]);
    informNeighbour(item, ngbrs["east"]);
    informNeighbour(item, ngbrs["south"]);
    informNeighbour(item, ngbrs["west"]);

  }

  private static void informNeighbour(InstalledItem item, Tile t) {
    if (t != null && t.installedItem != null && t.installedItem.cbOnChanged != null && item.type.Equals(t.installedItem.type)) {
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

    dct["north"] = getTileAt(t.x, t.y + 1);
    dct["south"] = getTileAt(t.x, t.y - 1);
    dct["east"] = getTileAt(t.x + 1, t.y);
    dct["west"] = getTileAt(t.x - 1, t.y);

    if (allowDiag) {
      dct["northwest"] = getTileAt(t.x - 1, t.y + 1);
      dct["northeast"] = getTileAt(t.x + 1, t.y + 1);
      dct["southwest"] = getTileAt(t.x - 1, t.y - 1);
      dct["southeast"] = getTileAt(t.x + 1, t.y - 1);
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
          str_installed.Append(t.installedItem.prototypeId.ToString() + ":" + t.x + ":" + t.y + ";");
        } else if (t.looseItem != null) {
          ///FIXME:
          ///
        }


      }
    }
    Debug.Log("original: " + str.Length + "\n" + str.ToString());

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
    string[] installedArray = installed_string.Split(c, StringSplitOptions.RemoveEmptyEntries);

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
    createInstalledItemProtos();
    createEmptyTiles();
    SetTilesFromArray(tilesArray);
    SetInstalledFromArray(installedArray);


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
    //--------CHARACTERS




    loadNames();

    //Init(width, height, 32, true, tilesArray);
  }

  private void SetInstalledFromArray(string[] installedArray) {

    if (installedArray != null) {
      foreach (string s in installedArray) {
        string[] ss = s.Split(':');
        Debug.Log(String.Format("0:\"{0}\" 1:\"{1}\" 2:\"{2}\"", ss[0], ss[1], ss[2]));
        int installedItemId = int.Parse(ss[0]);
        int x = int.Parse(ss[1]);
        int y = int.Parse(ss[2]);

        Tile t = getTileAt(x, y);
        if (installedItemId > 0 && x >= 0 && y >= 0 && t != null) {
          InstalledItem item = InstalledItem.CreateInstance(this, getInstalledItemProtoById(installedItemId), t);
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
          } else {

          }
        }
      }
    }
  }



  //END LOADING

  public void Kill() {
    jobQueue = null;
    installedItemProtos.Clear();
    installedItemProtos = null;
    installedItemProtos_BY_ID.Clear();
    installedItemProtos_BY_ID = null;
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


using System;
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

public class World : IXmlSerializable {

  //LOADING AND SAVING


  public XmlSchema GetSchema() {
    return null;
  }

  public void WriteXml(XmlWriter writer) {
    writer.WriteElementString("width", width.ToString());
    writer.WriteElementString("height", height.ToString());
    writer.WriteElementString("tileSize", tileSize.ToString());
    writer.WriteStartElement("characters");

    foreach (Character c in characters) {
      writer.WriteStartElement("character");
      writer.WriteElementString("name", c.name);
      writer.WriteElementString("xPos", ((int)c.X).ToString());
      writer.WriteElementString("yPos", ((int)c.Y).ToString());
      writer.WriteElementString("state", c.state.ToString());
      writer.WriteElementString("job", "....");

      writer.WriteEndElement();

    }
    writer.WriteEndElement();

    writer.WriteStartElement("tileTypes");

    Dictionary<string, int> types = new Dictionary<string, int>();
    int counter = 0;
    foreach (string st in TileType.TYPES.Keys) {
      TileType type = TileType.TYPES[st];
      types[st] = counter;
      
      writer.WriteStartElement("tileType");
      writer.WriteElementString("name", st);
      writer.WriteElementString("id", counter.ToString());

      writer.WriteEndElement();
      counter += 1;
    }

    writer.WriteEndElement();

    StringBuilder str = new StringBuilder();
    
    for (int x = 0; x < width; x += 1) {
      for (int y = 0; y < height; y += 1) {
        Tile t = getTileAt(x, y);
        //str.Append(t.x + "_" + t.y + "_" + types[t.type.name] + ";");
        str.Append(types[t.type.name] + ";");
        
      }
    }
    Debug.Log("original: " + str.Length + "\n" + str.ToString());

    byte[] tileByes = Funcs.Zip(str.ToString());
    Debug.Log("bytes: " + tileByes.Length);
    writer.WriteStartElement("tiles");
    writer.WriteBase64(tileByes, 0, tileByes.Length);
    writer.WriteEndElement();


    jobQueue.WriteXml(writer);

  }

  public void ReadXml(XmlReader reader) {

  }



  //END LOADING

  Dictionary<string, InstalledItem> installedItemProtos;


  private float xSeed, ySeed,noiseFactor;
  private Tile[,] tiles;
  public int width { get; private set; }
  public int height { get; private set; }
  public int tileSize { get; private set; }
  Action<InstalledItem> cbInstalledItem;
  Action<Tile> cbTileChanged;
  Action<Character> cbCharacterChanged;
  Action<Character> cbCharacterCreated;
  Action<Character> cbCharacterKilled;
  public JobQueue jobQueue;
  List<Character> characters = new List<Character>();
  bool allowDiagonalNeighbours = true;

  //private static readonly float NOISE_FACTOR = 0.05f;
  [NonSerialized]
  public TileNodeMap nodeMap;
  private readonly int NUMBER_OF_ROBOTS = 10;

  private readonly int TEST_WIDTH = 30;
  private readonly int TEST_HEIGHT = 30;



  public void Kill() {
    jobQueue = null;
    installedItemProtos.Clear();
    installedItemProtos = null;
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

  void Init(int width, int height, int tileSize) {
    xSeed = UnityEngine.Random.Range(-10f, 10f);
    ySeed = UnityEngine.Random.Range(-10f, 10f);
    noiseFactor = UnityEngine.Random.Range(0.01f, 0.1f);
    installedItemProtos = new Dictionary<string, InstalledItem>();
    jobQueue = new JobQueue();


    tiles = new Tile[width, height];
    this.width = width;
    this.height = height;
    this.tileSize = tileSize;

    for (int x = 0; x < width; x += 1) {
      for (int y = 0; y < height; y += 1) {
        tiles[x, y] = new Tile(this, TileType.TYPES["empty"], x, y);
        tiles[x, y].cbRegisterOnChanged(OnTileChanged);
      }


    }
    loadNames();
    createInstalledItemProtos();
  }


  public World() {
    Init(TEST_WIDTH, TEST_HEIGHT, 32);
  }
  public World(int width = 50, int height = 50, int tileSize = 32) {

    Init(width, height, tileSize);
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
        characters.Add(c);
        if (cbCharacterCreated != null) {
          cbCharacterCreated(c);
        }
        c.CBRegisterOnChanged(OnCharacterChanged);
        created += 1;
      }
    }

  }

  public Dictionary<string, InstalledItem> getProtoList() {
    return installedItemProtos;
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

      InstalledItem proto = InstalledItemProto(name, sprite, movement, w, h, linked, build, trash, rotate);
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


  private InstalledItem InstalledItemProto(string name, string spriteName, float movementFactor, int width, int height, bool linksToNeighbour, bool build, bool trash, bool rotate) {
    InstalledItem proto = null;
    if (!installedItemProtos.ContainsKey(name)) {
      proto = InstalledItem.CreatePrototype(name, spriteName, movementFactor, width, height, linksToNeighbour, build, trash, rotate);
      installedItemProtos.Add(name, proto);

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
      InstalledItem inst = InstalledItem.CreateInstance(proto, tile);
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

  public bool isInstalledItemPositionValid(string itemType, Tile t) {
    if (t == null ) {
      Debug.LogError("isInstalledItemPositionValid: tile cannot be null");
    }

    if (itemType == null) {
      Debug.LogError("isInstalledItemPositionValid: itemType cannot be null");
    }

    if (installedItemProtos == null) {
      Debug.LogError("isInstalledItemPositionValid: installedItemProtos cannot be null");
    }
    return installedItemProtos[itemType].funcPositionValid(t.x, t.y);
  }

  public string GetName() {
    string name1 = names[UnityEngine.Random.Range(0, names.Length)];
    string name2 = names[UnityEngine.Random.Range(0, names.Length)];

    return name1 + "_" + name2;

  }

  private void loadNames() {
    names = File.ReadAllLines(Application.streamingAssetsPath + "/csv/surnames.csv");

    //foreach (string line in lines)
    //  Console.WriteLine(line);
  }


  public string[] names;

}


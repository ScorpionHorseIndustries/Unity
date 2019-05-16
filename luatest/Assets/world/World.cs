using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;


public class World {

  Dictionary<string, InstalledItem> installedItemProtos = new Dictionary<string, InstalledItem>();

  private Tile[,] tiles;
  public int width { get; private set; }
  public int height { get; private set; }
  public int tileSize { get; private set; }
  Action<InstalledItem> cbInstalledItem;
  Action<Tile> cbTileChanged;
  Action<Character> cbCharacterChanged;
  Action<Character> cbCharacterCreated;
  public JobQueue jobQueue;
  List<Character> characters = new List<Character>();

  private static readonly float NOISE_FACTOR = 0.05f;
  public TileNodeMap nodeMap;




  public World(int width = 100, int height = 100, int tileSize = 32) {

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
    
    createInstalledItemProtos();

    //CreateCharacters();
  }

  public void SetAllNeighbours() {
    for (int x = 0; x < width; x += 1) {
      for (int y = 0; y < height; y += 1) {
        tiles[x, y].SetNeighbours(); 
        
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
    while(created < 20 && attempts < 10000) {
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
    float xo = UnityEngine.Random.Range(-10, 10);
    float yo = UnityEngine.Random.Range(-10, 10);
    float xx = 0;
    float yy = 0;
    for (int x = 0; x < width; x += 1) {
      xx = ((float)x) * NOISE_FACTOR;
      for (int y = 0; y < height; y += 1) {
        yy = ((float)y) * NOISE_FACTOR;
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

  public void RegisterTileChangedCB(Action<Tile> cb) {
    cbTileChanged += cb;

  }

  public void UnregisterTileChangedCB(Action<Tile> cb) {
    cbTileChanged -= cb;

  }

  public void RegisterCharacterChangedCB(Action<Character> cb) {
    cbCharacterChanged += cb;

  }

  public void UnregisterCharacterChangedCB(Action<Character> cb) {
    cbCharacterChanged -= cb;

  }

  public void RegisterCharacterCreatedCB(Action<Character> cb) {
    cbCharacterCreated += cb;

  }

  public void UnregisterCharacterCreatedCB(Action<Character> cb) {
    cbCharacterCreated -= cb;

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
    return installedItemProtos[itemType].funcPositionValid(t.x, t.y);
  }

}


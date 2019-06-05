using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using System.IO;

public class InstalledItem {
  


  //-----------------------------------PROPERTIES-----------------------------------

  public ItemParameters itemParameters;
  public Action<InstalledItem, float> updateActions;

  public Func<InstalledItem, Tile.CAN_ENTER> enterRequested;

  public string niceName { get; private set; }
  public int prototypeId { get; private set; }
  public Tile tile { get; private set; } //an object could be more than one tile... so...
  public string type { get; private set; }
  public float movementFactor { get; private set; } = 1;
  /// TODO: implement larger objects
  /// TODO: implement rotation
  public int width { get; private set; } = 1;
  public int height { get; private set; } = 1;
  public bool linksToNeighbour { get; private set; } = false;
  public string spriteName { get; private set; }
  public List<string> randomSprites = new List<string>();
  List<Job> jobs;
  public Inventory inventory;

  private InstalledItem prototype = null;



  public string sprite_ns { get; private set; }
  public string sprite_nsw { get; private set; }
  public string sprite_s { get; private set; }
  public string sprite_sw { get; private set; }
  public string sprite_nesw { get; private set; }
  public string recipeName { get; private set; }
  public bool build { get; private set; }
  public bool trash { get; private set; }
  public bool randomRotation { get; private set; }
  public List<string> neighbourTypes;
  public bool roomEnclosure { get; private set; } = true;
  public int inventorySlots { get; private set; } = 0;
  //public float open { get; private set; } = 0f; //0 = closed, 1 = open, see if you guess what intermediate values mean.
  //bool opening = false;
  //float openTime = 0.25f; //time taken to open/close


  //
  public Action<InstalledItem> cbOnChanged;
  public Action<InstalledItem> cbOnDestroyed;
  public Func<World, int, int, bool> funcPositionValid;


  //----------------------CONSTRUCTORS-------------------------

  private InstalledItem() {
    itemParameters = new ItemParameters();
    neighbourTypes = new List<string>();

    //updateActions = new List<Action>();

  }



  private InstalledItem(World world, InstalledItem proto) {

    //this.updateActions = new List<Action>();
    this.jobs = new List<Job>();
    this.prototypeId = proto.prototypeId;
    this.type = proto.type; // nice field name, doofus.
    this.niceName = proto.niceName;
    this.movementFactor = proto.movementFactor;
    this.width = proto.width;
    this.height = proto.height;

    this.linksToNeighbour = proto.linksToNeighbour;
    this.spriteName = proto.spriteName;
    this.setLinkedSpriteNames(proto);
    this.build = proto.build;
    this.trash = proto.trash;
    this.prototype = proto;
    this.randomRotation = proto.randomRotation;
    this.roomEnclosure = proto.roomEnclosure;
    this.recipeName = proto.recipeName;
    if (proto.updateActions != null) {
      this.updateActions = proto.updateActions;
    }
    this.itemParameters = new ItemParameters(proto.itemParameters);//.GetItems(); 
    this.enterRequested = proto.enterRequested;
    this.neighbourTypes = proto.neighbourTypes;
    this.inventory = new Inventory(world,proto.inventorySlots, INVENTORY_TYPE.INSTALLED_ITEM, this);

  }

  //----------------------------END OF CONSTRUCTORS--------------------------------



  //----------------------------------SPRITES----------------------------------


  public void setRandomSprites(List<string> list) {
    foreach (string s in list) {
      randomSprites.Add(s);
    }
  }

  public void setLinkedSpriteNames(string sprite_ns, string sprite_nsw, string sprite_s, string sprite_sw, string sprite_nesw) {
    this.sprite_ns = sprite_ns;
    this.sprite_nsw = sprite_nsw;
    this.sprite_s = sprite_s;
    this.sprite_sw = sprite_sw;
    this.sprite_nesw = sprite_nesw;

  }

  private void setLinkedSpriteNames(InstalledItem Item) {
    setLinkedSpriteNames(Item.sprite_ns, Item.sprite_nsw, Item.sprite_s, Item.sprite_sw, Item.sprite_nesw);
  }


  public void SetParameters(string prms) {
    string[] paramArray = prms.Split(',');
    foreach (string paramPair in paramArray) {
      string[] paramPairArray= paramPair.Split('=');
      if (paramPairArray.Length == 2) {
        string prop = paramPairArray[0].Replace('{', ' ').Replace('}', ' ').Replace('(', ' ').Replace(')', ' ').Trim();
        string val = paramPairArray[1].Replace('{', ' ').Replace('}', ' ').Replace('(', ' ').Replace(')', ' ').Trim();
        itemParameters.Set(prop, val);
        
      }

      
    }

  }




  public string getRandomSpriteName() {
    string s = spriteName;
    List<string> list = null;
    if (prototype == null) {
      list = randomSprites;
    } else {
      list = prototype.randomSprites;
    }
    if (list.Count > 0) {
      s = list[UnityEngine.Random.Range(0, list.Count)];

    }


    return s;

  }

  //-=-=-=-=-=-=-=-=-=-=- UPDATES =-=-=-=-=-=-=-=-=-=-=-

  public void Update(float deltaTime) {
    if (updateActions != null) {
      updateActions(this,deltaTime);
    }
    
  }




  //=-=-=-=-=-=-=-=-=-=- EVENTS =-=-=-=-=-=-=-=-=-=-=-=-=-


  public void Destroy() {
    if (cbOnDestroyed != null) {
      cbOnDestroyed(this);
    }
  }


  //----------------------------CALLBACK REGISTERS----------------------

  public void CBRegisterChanged(Action<InstalledItem> cb) {
    cbOnChanged += cb;
  }

  public void CBUnregisterChanged(Action<InstalledItem> cb) {
    cbOnChanged -= cb;
  }

  public void CBRegisterDestroyed(Action<InstalledItem> cb) {
    cbOnDestroyed += cb;
  }

  public void CBUnregisterDestroyed(Action<InstalledItem> cb) {
    cbOnDestroyed -= cb;
  }

  //-------------------------------POSITION METHODS----------------------------------

  public bool isPositionValid(World world, int x, int y) {
    Tile t = world.GetTileAt(x, y);
    //Debug.Log("Is Position Valid (" + x + "," + y + "): " + t);
    if (t == null || !t.type.build || t.installedItem != null || !t.IsInventoryEmpty()  || t.HasPendingJob) {
      return false;
    } else {
      return true;
    }
  }

  public bool isPositionValid_Door(int x, int y) {
    return false;
  }


  //---------------------------------OVERRIDES---------------------------

  public override string ToString() {
    return "installed item: " + this.type + " l:" + this.linksToNeighbour + " nl: " + neighbourTypes.ToString();
  }



  //-----------------------STATIC METHODS------------------

  public static InstalledItem CreateInstance(World world, InstalledItem proto, Tile tile) {
    if (!proto.funcPositionValid(world, tile.world_x, tile.world_y)) {
      return null;
    }
    //Debug.Log("InstalledItem.CreateInstance");
    InstalledItem o = new InstalledItem(world,proto);


    o.tile = tile;

    if (!o.tile.placeInstalledObject(o)) {
      return null;

    }


    if (o.linksToNeighbour) {
      //this type needs to tell it's neighbours about its creation

      world.informNeighbours(o);


    }
    return o;
  }


//-----------=----------------------STATIC PROPERTIES--------------------------------
  public static Dictionary<string, InstalledItem> prototypes;
  public static Dictionary<int, string> prototypesById;
  public static List<InstalledItem> trashPrototypes;
  public static Dictionary<string, string> prototypeRecipes;

  public static string GetRecipeName(string name) {

    if (PrototypeExists(name)) {
      return prototypes[name].recipeName;
    } else {
      return null;
    }

  }

  public static bool PrototypeExists(string name) {
    return prototypes.ContainsKey(name);
  }




  public static void LoadFromFile() {
    prototypes = new Dictionary<string, InstalledItem>();
    trashPrototypes = new List<InstalledItem>();
    prototypesById = new Dictionary<int, string>();
    prototypeRecipes = new Dictionary<string, string>();


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
      float movement = Funcs.jsonGetFloat(installedItemJson["movementFactor"], 1);
      bool trash = Funcs.jsonGetBool(installedItemJson["trash"], false);
      bool build = Funcs.jsonGetBool(installedItemJson["build"], false);
      bool rotate = Funcs.jsonGetBool(installedItemJson["randomRotation"], false);
      int w = Funcs.jsonGetInt(installedItemJson["width"], 1);
      int h = Funcs.jsonGetInt(installedItemJson["height"], 1);
      int id = Funcs.jsonGetInt(installedItemJson["id"], -1);
      bool enclosesRoom = Funcs.jsonGetBool(installedItemJson["enclosesRoom"], false);
      string recipeName = Funcs.jsonGetString(installedItemJson["recipe"], null);
      bool linked = Funcs.jsonGetBool(installedItemJson["linked"], false);
      int inventorySlots = Funcs.jsonGetInt(installedItemJson["inventorySlots"], 0);

      List<string> sprites = new List<string>();
      JArray spritesJsonArray = (JArray)installedItemJson["sprites"];
      foreach (string tempSpriteName in spritesJsonArray) {
        sprites.Add(tempSpriteName);
      }

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




      InstalledItem proto = new InstalledItem();//CreateOneInstalledItemPrototype(name, niceName, sprite, movement, w, h, linked, build, trash, rotate, id, enclosesRoom, recipeName);
      proto.neighbourTypes = neighbourTypeList;
      //proto.roomEnclosure = enclosesRoom;
      proto.niceName = niceName;
      proto.prototypeId = id;
      proto.type = name;
      proto.spriteName = sprite;
      proto.movementFactor = movement;
      proto.width = w;
      proto.height = h;
      proto.linksToNeighbour = linked;
      proto.funcPositionValid = proto.isPositionValid;
      proto.build = build;
      proto.trash = trash;
      proto.prototype = null;
      proto.randomRotation = rotate;
      proto.roomEnclosure = enclosesRoom;
      proto.recipeName = recipeName;
      proto.inventorySlots = inventorySlots;
      //proto.inventory = new Inventory(inventorySlots, INVENTORY_TYPE.INSTALLED_ITEM, proto);

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

      if (name == "installed::door") {
        proto.itemParameters.SetFloat("openness", 0f);
        proto.itemParameters.SetFloat("opentime", 0.25f);
        proto.updateActions += InstalledItemActions.Door_UpdateActions;

        proto.enterRequested += InstalledItemActions.Door_EnterRequested;
      } else if (name == "installed::stockpile") {
        proto.updateActions += InstalledItemActions.Stockpile_UpdateActions;
      }
      prototypes.Add(proto.type, proto);
      prototypesById.Add(proto.prototypeId, proto.type);
      prototypeRecipes.Add(proto.type, proto.recipeName);

    }
    //InstalledItem proto = InstalledItemProto("installed::wall", "walls_none", 0, 1, 1,true);
    //

    foreach (string ss in prototypes.Keys) {
      InstalledItem item = prototypes[ss];
      if (item.trash) {
        trashPrototypes.Add(item);
      }
    }


  }

  //private static InstalledItem CreateOneInstalledItemPrototype(string name, string niceName, string spriteName, float movementFactor, int width, int height, bool linksToNeighbour, bool build, bool trash, bool rotate, int id, bool enclosesRoom, string recipeName) {
  //  InstalledItem proto = null;
  //  if (!prototypes.ContainsKey(name)) {
  //    //proto = InstalledItem.CreatePrototype(name, niceName, spriteName, movementFactor, width, height, linksToNeighbour, build, trash, rotate, id, enclosesRoom, recipeName);
  //    //prototypes.Add(name, proto);
  //    //installedItemProtos_BY_ID.Add(proto.prototypeId, name);

  //    if (name == "installed::door") {
  //      proto.itemParameters.SetFloat("openness", 0f);
  //      proto.itemParameters.SetFloat("opentime", 0.25f);
  //      proto.updateActions += InstalledItemActions.Door_UpdateActions;

  //      proto.enterRequested += InstalledItemActions.Door_EnterRequested;
  //    }
  //  }
  //  Debug.Log("prototype created: " + proto);

  //  return proto;
  //}
}

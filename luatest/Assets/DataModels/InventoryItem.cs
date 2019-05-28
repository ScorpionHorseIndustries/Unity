using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class InventoryItem {


  private static Dictionary<string, InventoryItem> prototypes = new Dictionary<string, InventoryItem>();
  //properties
  public string type { get; private set; }
  public string niceName { get; private set; }
  public bool isPrototype { get; private set; }
  InventoryItem prototype = null;
  private int prototypeStackSize = 50;
  public string spriteName { get; private set; }
  public int maxStackSize {
    get {
      if (isPrototype) {
        return prototypeStackSize;
      } else {
        return prototype.prototypeStackSize;
      }

    }
  }
  public int currentStack = 1;

  public Tile tile { get; private set; }
  public Character character { get; private set; }

  //-----------------GET/SET----------------------

  public void SetTile(Tile tile) {
    character = null;
    this.tile = tile;

  }

  public void SetCharacter(Character character) {
    this.character = character;
    this.tile = null;
  }

  //----------------CONSTRUCTORS--------------------
  public InventoryItem() {

  }
  public InventoryItem(InventoryItem proto) {
    this.type = proto.type;
    this.isPrototype = false;
    this.niceName = proto.niceName;
    this.prototype = proto;
    this.spriteName = proto.spriteName;


  }

  public InventoryItem Copy() {
    InventoryItem item = new InventoryItem(this.prototype);
    item.currentStack = this.currentStack;

    return item;

  }

  //-----------------------------------------OVERRIDES-------------------------------

  public override string ToString() {
    return type + " isProto:" + isPrototype;
  }


  //--------------------------------------STATIC METHODS--------------------------------------

  public static InventoryItem GetPrototype(string type) {
    if (prototypes.ContainsKey(type)) {
      return prototypes[type];
    } else {
      return null;
    }

  }

  public static InventoryItem GetRandomPrototype() {
    int r = UnityEngine.Random.Range(0, prototypes.Count);
    int c = 0;
    foreach (string k in prototypes.Keys) {
      if (r == c) {
        return prototypes[k];
      }
      c += 1;
    }
    return null;
  }
  public static void CreateInventoryItemPrototypes(JArray InventoryPrototypesArray) {


    foreach (JObject jsonProto in InventoryPrototypesArray) {
      InventoryItem item = new InventoryItem();
      string type = Funcs.jsonGetString(jsonProto["type"], "none");
      string niceName = Funcs.jsonGetString(jsonProto["niceName"], "none");
      string spriteName = Funcs.jsonGetString(jsonProto["spriteName"], "");
      int stackSize = Funcs.jsonGetInt(jsonProto["stackSize"], 1);

      item.type = type;
      item.niceName = niceName;
      item.isPrototype = true;
      item.prototype = null;
      item.prototypeStackSize = stackSize;
      item.spriteName = spriteName;
      prototypes.Add(item.type, item);

      Debug.Log(item);
    }


  }

  public static void LoadFromFile() {
    prototypes.Clear();

    string path = Application.streamingAssetsPath + "/json/InventoryItems.json";

    string json = File.ReadAllText(path);

    JObject jo = JObject.Parse(json);

    JArray invItems = Funcs.jsonGetArray(jo, "InventoryItems");
    if (invItems != null) {
      CreateInventoryItemPrototypes(invItems);
    } else {
      Debug.LogError("could not find inventory items array in [" + path + "]");
    }

  }

  public static InventoryItem CreateInventoyItemInstance(string type) {


    InventoryItem item = null;

    if (type != null && prototypes.ContainsKey(type)) {
      item = new InventoryItem(prototypes[type]);


    }



    return item;
  }



}

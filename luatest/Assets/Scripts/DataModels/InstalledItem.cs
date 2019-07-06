using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using System.IO;

using NoYouDoIt.Utils;
using NoYouDoIt.TheWorld;
using NoYouDoIt.Controller;
using System.Linq;

namespace NoYouDoIt.DataModels {

  public class Growth {
    public int stage;
    public int length;
    public string sprite;
    public string deconstructRecipe;

    public Growth(int stage, int length, string sprite, string deconstructRecipe) {
      this.stage = stage;
      this.length = length;
      this.sprite = sprite;
      this.deconstructRecipe = deconstructRecipe;
    }
  }
  public class InstalledItem {

    public Dictionary<int, Growth> growthStages;
    public static readonly string NONE = "installed::none";

    //public struct ProductOfWork {
    //  string inv_name;
    //  int minQty;
    //  int maxQty;
    //  float chance;
    //}

    //-----------------------------------PROPERTIES-----------------------------------

    public int growthStage = -1;

    public ItemParameters itemParameters;
    //public Action<InstalledItem, float> updateActions;
    protected List<string> updateActions;

    //public Func<InstalledItem, Tile.CAN_ENTER> enterRequested;
    public string enterRequestedFunc;
    //public ProductOfWork[] products;

    public string niceName { get; private set; }
    public int prototypeId { get; private set; }
    public Tile tile { get; private set; } //an object could be more than one tile... so...
    public string type { get; private set; }
    public float movementFactor { get; private set; } = 1;
    /// TODO: implement larger objects
    /// TODO: implement rotation
    public int width { get; private set; } = 1;
    public int height { get; private set; } = 1;
    public int xClearance { get; private set; } = 0; //the distance to the east that this item must be away from other items
    public int yClearance { get; private set; } = 0; //the distance to the north that this item must be away from other items
    public bool linksToNeighbour { get; private set; } = false;
    public string spriteName { get; private set; }
    public string forcedSpriteName = null;
    public List<string> randomSprites = new List<string>();
    //List<Job> jobs;
    public Inventory inventory;
    public Recipe recipe { get; private set; }

    public bool paletteSwap { get; private set; }
    public float trashSpawnChance { get; private set; }
    public List<string> canSpawnOnTileTypeList { get; private set; }


    private InstalledItem prototype = null;
    public Vector2 workTileOffset { get; protected set; } = Vector2.zero; //relative to bottom left tile


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
    public float updateActionCountDown = 0;
    public float updateActionCountDownMax { get; private set; }
    public float updateActionCountDownRange { get; private set; }
    public string workRecipeName { get; private set; }
    public string deconRecipeName { get; private set; }
    public bool isWorkstation { get; private set; } = false;
    public string luaOnCreate { get; private set; }

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

    

    private InstalledItem(World world, InstalledItem proto, bool spawn) {

      //this.updateActions = new List<Action>();
      //this.jobs = new List<Job>();
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
      this.enterRequestedFunc = proto.enterRequestedFunc;
      this.neighbourTypes = proto.neighbourTypes;
      this.inventory = new Inventory(world, proto.inventorySlots, INVENTORY_TYPE.INSTALLED_ITEM, this);
      this.workTileOffset = new Vector2(proto.workTileOffset.x, proto.workTileOffset.y);
      //set the countdown timer
      this.updateActionCountDownMax = proto.updateActionCountDownMax;
      this.updateActionCountDownRange = proto.updateActionCountDownRange;
      this.updateActionCountDown = proto.updateActionCountDownMax;
      this.workRecipeName = proto.workRecipeName;
      this.isWorkstation = proto.isWorkstation;
      this.canSpawnOnTileTypeList = proto.canSpawnOnTileTypeList;
      this.growthStages = proto.growthStages;
      this.growthStage = proto.growthStage;
      this.deconRecipeName = proto.deconRecipeName;
      this.xClearance = proto.xClearance;
      this.yClearance = proto.yClearance;
      this.luaOnCreate = proto.luaOnCreate;

      if (spawn && this.growthStages.Count > 0) {
        this.growthStage = UnityEngine.Random.Range(0, itemParameters.GetInt("maxGrowthStage"));

      }

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
        string[] paramPairArray = paramPair.Split('=');
        if (paramPairArray.Length == 2) {
          string prop = paramPairArray[0].Replace('{', ' ').Replace('}', ' ').Replace('(', ' ').Replace(')', ' ').Trim();
          string val = paramPairArray[1].Replace('{', ' ').Replace('}', ' ').Replace('(', ' ').Replace(')', ' ').Trim();
          itemParameters.Set(prop, val);

        }


      }

    }

    public Growth GetGrowthStage() {
      if (growthStages.ContainsKey(growthStage)) {
        return growthStages[growthStage];
      }
      return null;
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
        if (updateActionCountDown > 0) {
          updateActionCountDown -= deltaTime;
          //Debug.Log(updateActionCountDown + " " + updateActionCountDownMax);
        } else {
          updateActionCountDown = updateActionCountDownMax + UnityEngine.Random.Range(-updateActionCountDownRange, updateActionCountDownRange);
          //updateActions(this, deltaTime);
          World.CallLuaFunctions(this.updateActions.ToArray(), this, deltaTime);
        }


      }

    }

    //=-=-=-=-=-=-=-=-=-=- HELPERS =-=-=-=-=-=-=-=-=-=-=-=-=-
    public Tile GetWorkSpot() {
      return World.current.GetTileAt((int)(tile.world_x + workTileOffset.x), (int)(tile.world_y + workTileOffset.y));
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

      for (int xx = x-this.xClearance; xx < x + this.width + this.xClearance; xx += 1) {
        for (int yy = y-this.yClearance; yy < y + this.height + this.yClearance; yy += 1) {
          Tile t = world.GetTileIfChunkExists(xx, yy);
          //Debug.Log("Is Position Valid (" + x + "," + y + "): " + t);
          if (t == null || !t.type.build || t.installedItem != null || !t.IsInventoryEmpty() || t.HasPendingWork) {
            return false;
          }
        }
      }

      return true;


    }

    public bool isPositionValid_Door(int x, int y) {
      return false;
    }

    public string GetDeconRecipeName() {
      if (growthStages.Count > 0) {
        string dr = growthStages[growthStage].deconstructRecipe;

        if (dr != null) {
          return dr;
        }

      }

      return deconRecipeName;
    }

    //---
    public void Deconstruct() {
      Inventory inventory = new Inventory(World.current, 99, INVENTORY_TYPE.NONE, this.tile);



      Recipe recipe = Recipe.GetRecipe(this.GetDeconRecipeName());//GetRecipe(this.type);
      List<RecipeProduct> prods = recipe.GetProducts();
      foreach (RecipeProduct rp in prods) {

        switch (rp.type) {
          case RECIPE_PRODUCT_TYPE.ROBOT:
            break;
          case RECIPE_PRODUCT_TYPE.INVENTORY_ITEM:
            inventory.AddItem(rp.name, (int)rp.outputQty);
            break;
          case RECIPE_PRODUCT_TYPE.MONEY:
            WorldController.Instance.AddCurrency(rp.outputQty);
            break;
          default:
            break;
        }

        

        //Debug.Log("Added: " +  + " of " + rr.name);
      }

      for (int xx = tile.world_x; xx < tile.world_x + width; xx += 1) {
        for (int yy = tile.world_y; yy < tile.world_y + height; yy += 1) {
          Tile nt = World.current.GetTileIfChunkExists(xx, yy);
          if (nt != null) {
            nt.RemoveInstalledItem();
          }
        }
      }


      Destroy();
      inventory.Explode();
      inventory.ClearAll();
      World.current.RemoveInstalledItem(this);
    }


    //---------------------------------OVERRIDES---------------------------

    public override string ToString() {
      return "installed item: " + type + " (" + width + "," + height + ") off(" + workTileOffset.x + "," + workTileOffset.y + ") inventory:" + this.inventorySlots + " links:" + this.linksToNeighbour;
    }



    //-----------------------STATIC METHODS------------------

    public static InstalledItem CreateInstance(World world, InstalledItem proto, Tile tile, bool spawn = false) {
      if (!proto.funcPositionValid(world, tile.world_x, tile.world_y)) {
        return null;
      }
      //Debug.Log("InstalledItem.CreateInstance");
      InstalledItem o = new InstalledItem(world, proto, spawn);


      o.tile = tile;

      if (!o.tile.placeInstalledItem(o)) {
        return null;

      }


      if (o.linksToNeighbour) {
        //this type needs to tell it's neighbours about its creation

        world.informNeighbours(o);


      }

      if (o.luaOnCreate != null) {
        World.CallLuaFunction(o.luaOnCreate, o);
      }
      return o;
    }


    //-----------=----------------------STATIC PROPERTIES--------------------------------
    public static Dictionary<string, InstalledItem> prototypes;
    public static Dictionary<int, string> prototypesById;
    public static List<InstalledItem> trashPrototypes;
    public static Dictionary<string, string> prototypeRecipes;
    public static readonly string DECONSTRUCT = "installeditem::action::deconstruct";
    private static List<string> spawnChanceList = new List<string>();

    public static string GetRandomTrashItemName() {
      if (spawnChanceList.Count == 0) {
        foreach (InstalledItem trashProto in trashPrototypes) {
          for (int i = 0; i < trashProto.trashSpawnChance * 100; i += 1) {
            spawnChanceList.Add(trashProto.type);
            //Debug.Log(i + " " + rp.name + " " + prlist.Count);
          }
        }
        while (spawnChanceList.Count < 100) {
          spawnChanceList.Add(null);
        }

        //string output = "";
        //int c = 0;
        //foreach (string s in spawnChanceList) {
        //  output += " " + c;
        //  if (s == null) {
        //    output += " null";
        //  } else {
        //    output += " " + s;
        //  }
        //  c += 1;
        //}
        //Debug.Log("Random Spawn Trahs List: " + output);
      }

      int r = UnityEngine.Random.Range(0, spawnChanceList.Count);
      return spawnChanceList[r];


    }


    public static InstalledItem GetPrototype(string name) {
      if (PrototypeExists(name)) {
        return prototypes[name];
      } else {
        return null;
      }
    }

    public static string GetRecipeName(string name) {

      if (PrototypeExists(name)) {
        return prototypes[name].recipeName;
      } else {
        return null;
      }

    }

    public static Recipe GetRecipe(string installedItemType) {
      string recipeName = GetRecipeName(installedItemType);
      if (recipeName != null) {
        return Recipe.GetRecipe(recipeName);
      }

      return null;
    }

    public static bool PrototypeExists(string name) {
      return prototypes.ContainsKey(name);
    }





    public static void LoadFromFile() {
      //LoadLua();
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
        int workTileOffsetX = Funcs.jsonGetInt(installedItemJson["workTileOffsetX"], 0);
        int workTileOffsetY = Funcs.jsonGetInt(installedItemJson["workTileOffsetY"], 0);
        float updateActionCD = Funcs.jsonGetFloat(installedItemJson["updateActionCountDown"], 0);
        float updateActionCDRange = Funcs.jsonGetFloat(installedItemJson["updateActionCountDownRange"], 0);
        bool IsWorkstation = Funcs.jsonGetBool(installedItemJson["workstation"], false);
        string workRecipeName = Funcs.jsonGetString(installedItemJson["workRecipe"], "");
        bool paletteSwap = Funcs.jsonGetBool(installedItemJson["paletteSwap"], false);
        float trashSpawnChance = Funcs.jsonGetFloat(installedItemJson["trashSpawnChance"], 0);
        string deconRecipeName = Funcs.jsonGetString(installedItemJson["deconstructRecipe"], null);
        int yClearance = Funcs.jsonGetInt(installedItemJson["y_clearance"], 0);
        int xClearance = Funcs.jsonGetInt(installedItemJson["x_clearance"], 0);

        string luaOnCreate = Funcs.jsonGetString(installedItemJson["onCreate"], null);

        JArray jsonCanSpawnOn = Funcs.jsonGetArray(installedItemJson, "canSpawnOn");
        List<string> canSpawnOn = new List<string>();
        if (jsonCanSpawnOn != null) {
          foreach (string tempName in jsonCanSpawnOn) {
            canSpawnOn.Add(tempName);
          }
        }


        List<string> sprites = new List<string>();
        JArray spritesJsonArray = Funcs.jsonGetArray(installedItemJson, "sprites");
        if (spritesJsonArray != null) {
          foreach (string tempSpriteName in spritesJsonArray) {
            sprites.Add(tempSpriteName);
          }
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
        proto.workTileOffset = new Vector2(workTileOffsetX, workTileOffsetY);
        proto.updateActionCountDownMax = updateActionCD;
        proto.updateActionCountDownRange = updateActionCDRange;
        proto.isWorkstation = IsWorkstation;
        proto.workRecipeName = workRecipeName;
        proto.paletteSwap = paletteSwap;
        proto.trashSpawnChance = trashSpawnChance;
        proto.canSpawnOnTileTypeList = canSpawnOn;
        proto.deconRecipeName = deconRecipeName;
        proto.xClearance = xClearance;
        proto.yClearance = yClearance;
        proto.luaOnCreate = luaOnCreate;
        //proto.inventory = new Inventory(inventorySlots, INVENTORY_TYPE.INSTALLED_ITEM, proto);

        //Debug.Log(proto.ToString() + "\n" + workTileOffsetX + "," + workTileOffsetY);

        if (linked) {
          string n_s = Funcs.jsonGetString(installedItemJson["neighbour_s"], "");
          string n_ns = Funcs.jsonGetString(installedItemJson["neighbour_ns"], "");
          string n_nsw = Funcs.jsonGetString(installedItemJson["neighbour_nsw"], "");
          string n_sw = Funcs.jsonGetString(installedItemJson["neighbour_sw"], "");
          string n_nesw = Funcs.jsonGetString(installedItemJson["neighbour_nesw"], "");
          proto.setLinkedSpriteNames(n_ns, n_nsw, n_s, n_sw, n_nesw);
        }

        if (proto.paletteSwap) {
          List<string> randoSprites = NYDISpriteManager.Instance.GetSpritesStarting(proto.spriteName);
          //foreach (string s in randoSprites) {
          //  Debug.Log(s);
          //}
          proto.setRandomSprites(randoSprites);
        } else if (sprites.Count > 0) {
          proto.setRandomSprites(sprites);
          //Debug.Log("proto " + proto.type + " has " + proto.randomSprites.Count + " random sprites");

        }

        JArray growth = Funcs.jsonGetArray(installedItemJson, "growth");
        proto.growthStages = new Dictionary<int, Growth>();

        if (growth != null) {
          foreach (JObject growthStage in growth) {
            int stage = Funcs.jsonGetInt(growthStage["stage"], -1);
            int length = Funcs.jsonGetInt(growthStage["length"], -1);
            string gSprite = Funcs.jsonGetString(growthStage["sprite"], null);
            string gdecon = Funcs.jsonGetString(growthStage["deconstructRecipe"], deconRecipeName);

            if (stage >= 0 && length > 0 && gSprite != null) {
              proto.growthStages[stage] = new Growth(stage, length, gSprite, gdecon);
            }



          }
        }
        if (proto.growthStages.Count > 0) {
          proto.growthStage = 0;
          proto.itemParameters.SetInt("maxGrowthStage", proto.growthStages.Values.Max(e => e.stage));
        } else {
          proto.growthStage = -1;
        }
        

        JArray parameters = Funcs.jsonGetArray(installedItemJson, "parameters");
        if (parameters != null) {
          foreach (JObject prm in parameters) {
            string prmName = Funcs.jsonGetString(prm["name"], null);
            string prmType = Funcs.jsonGetString(prm["type"], null);

            if (prmName != null && prmType != null) {
              switch (prmType) {
                case "float":
                  float prmValueFloat = Funcs.jsonGetFloat(prm["value"], 0);
                  proto.itemParameters.SetFloat(prmName, prmValueFloat);
                  break;
                case "int":
                  int prmValueInt = Funcs.jsonGetInt(prm["value"], 0);
                  proto.itemParameters.SetInt(prmName, prmValueInt);
                  break;
                case "string":
                  string prmValueString = Funcs.jsonGetString(prm["value"], "");
                  proto.itemParameters.Set(prmName, prmValueString);
                  break;
                case "bool":
                  bool prmValueBool = Funcs.jsonGetBool(prm["value"], false);
                  proto.itemParameters.SetBool(prmName, prmValueBool);
                  break;
              }
            }
          }
        }

        string updateActionName = Funcs.jsonGetString(installedItemJson["updateAction"], null);
        if (updateActionName != null) {
          proto.updateActions = new List<string>();
          proto.updateActions.Add(updateActionName);
        }

        string enterRequestionName = Funcs.jsonGetString(installedItemJson["onEnterRequested"], null);

        if (enterRequestionName != null) {
          proto.enterRequestedFunc = enterRequestionName;
        }

        //if (name == "installed::door") {
        //  proto.enterRequested += InstalledItemActions.Door_EnterRequested;
        //}
        //  proto.updateActions += InstalledItemActions.Door_UpdateActions;


        //} else if (name == "installed::stockpile") {
        //  proto.updateActions += InstalledItemActions.Stockpile_UpdateActions;
        //} else if (IsWorkstation) {
        //  proto.updateActions += InstalledItemActions.Workstation_UpdateActions;
        //}
        prototypes.Add(proto.type, proto);
        prototypesById.Add(proto.prototypeId, proto.type);
        prototypeRecipes.Add(proto.type, proto.recipeName);
        proto.recipe = GetRecipe(proto.type);

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
}
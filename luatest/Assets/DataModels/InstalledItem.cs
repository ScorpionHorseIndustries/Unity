using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class InstalledItem {

  //-----------------------------------PROPERTIES-----------------------------------

  public ItemParameters itemParameters;
  public Action<InstalledItem, float> updateActions;

  public Func<InstalledItem, Tile.CAN_ENTER> enterRequested;

  public int prototypeId { get; private set; }
  public Tile tile { get; private set; } //an object could be more than one tile
  public string type { get; private set; }
  public float movementFactor { get; private set; } = 1;
  /// TODO: implement larger objects
  /// TODO: implement rotation
  public int width { get; private set; } = 1;
  public int height { get; private set; } = 1;
  public bool linksToNeighbour { get; private set; } = false;
  public string spriteName { get; private set; }
  public List<string> randomSprites = new List<string>();

  private InstalledItem prototype = null;
  public string sprite_ns { get; private set; }
  public string sprite_nsw { get; private set; }
  public string sprite_s { get; private set; }
  public string sprite_sw { get; private set; }
  public string sprite_nesw { get; private set; }
  public bool build { get; private set; }
  public bool trash { get; private set; }
  public bool randomRotation { get; private set; }
  public List<string> neighbourTypes;
  public bool roomEnclosure { get; private set; } = true;
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

  public static InstalledItem CreatePrototype(string type, string spriteName, float movementFactor, int width, int height, bool linksToNeighbour, bool build, bool trash, bool rotate, int id, bool enclosesRoom) {
    InstalledItem o = new InstalledItem();
    o.prototypeId = id;
    o.type = type;
    o.spriteName = spriteName;
    o.movementFactor = movementFactor;
    o.width = width;
    o.height = height;
    o.linksToNeighbour = linksToNeighbour;
    o.funcPositionValid = o.isPositionValid;
    o.build = build;
    o.trash = trash;
    o.prototype = null;
    o.randomRotation = rotate;
    o.roomEnclosure = enclosesRoom;
    //string json = JsonUtility.ToJson(o);
    //Debug.Log(o.type + " to json : " + json);
    return o;

  }

  private InstalledItem(InstalledItem proto) {
    
    //this.updateActions = new List<Action>();
    this.prototypeId = proto.prototypeId;
    this.type = proto.type; // nice field name, doofus.
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
    if (proto.updateActions != null) {
      this.updateActions = proto.updateActions;
    }
    this.itemParameters = new ItemParameters(proto.itemParameters);//.GetItems(); 
    this.enterRequested = proto.enterRequested;
    this.neighbourTypes = proto.neighbourTypes;

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
    Tile t = world.getTileAt(x, y);
    //Debug.Log("Is Position Valid (" + x + "," + y + "): " + t);
    if (t == null || !t.type.build || t.installedItem != null || t.looseItem != null || t.pendingJob) {
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
    if (!proto.funcPositionValid(world, tile.x, tile.y)) {
      return null;
    }
    //Debug.Log("InstalledItem.CreateInstance");
    InstalledItem o = new InstalledItem(proto);


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
}

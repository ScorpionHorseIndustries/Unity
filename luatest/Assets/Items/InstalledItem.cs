using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class InstalledItem {

  public Tile tile { get; private set; } //an object could be more than one tile
  public string type { get; private set; }
  public float movementCost { get; private set; } = 1;
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


  //
  Action<InstalledItem> cbOnChanged;
  public Func<int, int, bool> funcPositionValid;



  private InstalledItem() {

  }

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

  public static InstalledItem CreatePrototype(string type, string spriteName, float movementCost, int width, int height, bool linksToNeighbour, bool build, bool trash, bool rotate) {
    InstalledItem o = new InstalledItem();
    o.type = type;
    o.spriteName = spriteName;
    o.movementCost = movementCost;
    o.width = width;
    o.height = height;
    o.linksToNeighbour = linksToNeighbour;
    o.funcPositionValid = o.isPositionValid;
    o.build = build;
    o.trash = trash;
    o.prototype = null;
    o.randomRotation = rotate;
    //string json = JsonUtility.ToJson(o);
    //Debug.Log(o.type + " to json : " + json);
    return o;

  }

  public static InstalledItem CreateInstance(InstalledItem proto, Tile tile) {
    if (!proto.funcPositionValid(tile.x, tile.y)) {
      return null;
    }
    //Debug.Log("InstalledItem.CreateInstance");
    InstalledItem o = new InstalledItem();
    o.type = proto.type;
    o.movementCost = proto.movementCost;
    o.width = proto.width;
    o.height = proto.height;
    o.tile = tile;
    o.linksToNeighbour = proto.linksToNeighbour;
    o.spriteName = proto.spriteName;
    o.setLinkedSpriteNames(proto);
    o.build = proto.build;
    o.trash = proto.trash;
    o.prototype = proto;
    o.randomRotation = proto.randomRotation;

    if (!o.tile.placeInstalledObject(o)) {
      return null;

    }


    if (o.linksToNeighbour) {
      //this type needs to tell it's neighbours about its creation

      InstalledItem.informNeighbours(o);


    }
    return o;
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

  private static void informNeighbours(InstalledItem item) {
    Dictionary<string, Tile> ngbrs = WorldController.Instance.GetNeighbours(item);
    informNeighbour(item, ngbrs["north"]);
    informNeighbour(item, ngbrs["east"]);
    informNeighbour(item, ngbrs["south"]);
    informNeighbour(item, ngbrs["west"]);

  }

  private static void informNeighbour(InstalledItem item, Tile t) {
    if (t != null && t.installedItem != null && item.type.Equals(t.installedItem.type)) {
      t.installedItem.cbOnChanged(t.installedItem);
    }
  }



  public void RegisterCB(Action<InstalledItem> cb) {
    cbOnChanged += cb;
  }

  public void UnregisterCB(Action<InstalledItem> cb) {
    cbOnChanged -= cb;
  }



  public bool isPositionValid(int x, int y) {
    Tile t = WorldController.Instance.world.getTileAt(x, y);
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
}


using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour {


  private Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
  


  public static SpriteController Instance { get; private set; }
  public WorldController wcon;
  public World world;


  private void Awake() {
    if (Instance != null) {
      Debug.LogError("THERE SHOULD ONLY BE ONE SPRITE CONTROLLER YOU DING DONG");
    }
    Instance = this;
  }

  void Start() {

    LoadSprites();

  }


  // Update is called once per frame
  void Update() {


  }






  private void LoadSprites() {
    Sprite[] loaded = Resources.LoadAll<Sprite>("sprites");
    foreach (Sprite s in loaded) {
      //Debug.Log(s);
      string sn = s.name;
      int counter = 1;
      while (true) {
        if (!sprites.ContainsKey(sn)) {
          sprites.Add(sn, s);
          break;

        } else {
          sn = s.name + "_" + counter;
          counter += 1;
        }
      }

    }
  }

  public void SetTileSprite(Tile t) {
    SpriteRenderer sr = WorldController.Instance.GetGameObjectFromTile(t).GetComponent<SpriteRenderer>();
    //Debug.Log("give me sprite: " + t.type.spriteName);
    sr.sprite = GetSprite(t.type.spriteName);

  }

  public Sprite GetSprite(string name) {
    if (sprites.ContainsKey(name)) {
      return sprites[name];
    } else {
      return sprites["sprite_error"];
    }
  }

  public void JobCreated(Job j) {
    /*
    InstalledItem item = wcon.world.get
    Sprite s = j.jobType
    */
  }


  public SpriteHolder GetSprite(InstalledItem item) {
    SpriteHolder sp = new SpriteHolder();
    sp.s = sprites[item.getRandomSpriteName()];
    if (item.randomRotation) {
      int r = Random.Range(0, 4);
      sp.r = 90 * r;
    }

    

    if (item.linksToNeighbour) {
      int x = item.tile.x;
      int y = item.tile.y;
      bool n, e, s, w;
      int nc = 0;
      Dictionary<string, Tile> ngbrs = WorldController.Instance.getNeighbours(item);
      Tile north = ngbrs["north"];
      Tile east = ngbrs["east"];
      Tile south = ngbrs["south"];
      Tile west = ngbrs["west"];

      n = hasMatchingNeighbour(north, item);
      if (n) nc += 1;
      e = hasMatchingNeighbour(east, item);
      if (e) nc += 1;
      s = hasMatchingNeighbour(south, item);
      if (s) nc += 1;
      w = hasMatchingNeighbour(west, item);
      if (w) nc += 1;

      switch (nc) {
        case 0:

          break;
        case 1:
          sp.s = sprites[item.sprite_s];
          if (n) {
            sp.r = 180;
          } else if (e) {
            sp.r = 90;
          } else if (w) {
            sp.r = -90;
          }
          break;
        case 2:
          sp.s = sprites[item.sprite_ns];
          if (e && w) {
            sp.r = 90;
          } else if (n && s) {
            sp.r = 0;
          } else {
            sp.s = sprites[item.sprite_sw];
            if (s && w) {

            } else if (n && e) {
              sp.r = -180;
            } else if (n && w) {
              sp.r = -90;
            } else if (e && s) {
              sp.r = 90;
            }
          }
          break;
        case 3:
          sp.s = sprites[item.sprite_nsw];

          if (n && s && w) {

          } else if (n && e && w) {
            sp.r = -90;
          } else if (e && s && w) {
            sp.r = 90;
          } else if (n && e && s) {
            sp.r = 180;
          }

          break;
        case 4:
          sp.s = sprites[item.sprite_nesw];
          break;

      }


    } else {

    }
    return sp;
  }

  private bool hasMatchingNeighbour(Tile t, InstalledItem item) {
    if (t == null || t.installedItem == null || !t.installedItem.type.Equals(item.type)) {
      return false;
    } else {
      return true;
    }

  }

}

public class SpriteHolder {
  public Sprite s = null;
  public float r = 0;
  public SpriteHolder() { }
}

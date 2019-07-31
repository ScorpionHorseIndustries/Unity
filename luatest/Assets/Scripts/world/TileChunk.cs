using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

namespace NoYouDoIt.TheWorld {
  using NoYouDoIt.Utils;
  using NoYouDoIt.DataModels;
  using NoYouDoIt.Controller;
  using NoYouDoIt.Utils.Maps.CityMap;
  
  public class TileChunk : IXmlSerializable {
    public static readonly int CHUNK_WIDTH = Funcs.GetSettingInt("chunk_width");
    public static readonly int CHUNK_HEIGHT = Funcs.GetSettingInt("chunk_height");
    public Tile[,] tiles;
    protected World world;
    public int x { get; private set; }
    public int y { get; private set; }
    public int world_x {
      get {
        return x * CHUNK_WIDTH;
      }
    }

    public Texture2D texture;
    public Sprite sprite;

    public int world_y {
      get {
        return y * CHUNK_HEIGHT;
      }

    }

    Dictionary<string, TileChunk> neighbours;



    public void Init() {
      Rect rect = new Rect(0, 0, CHUNK_WIDTH * 32, CHUNK_HEIGHT * 32);
      Vector2 pivot = new Vector2(0, 0);
      texture = new Texture2D(CHUNK_WIDTH * 32, CHUNK_HEIGHT * 32, NYDISpriteManager.DEFAULT_TEXTURE_FORMAT,false);
      texture.filterMode = FilterMode.Point;

      sprite = Sprite.Create(texture, rect, pivot,32);
      sprite.name = "sprite::chunk_" + x + "_" + y;

      for (int xx = 0; xx < CHUNK_WIDTH; xx += 1) {
        for (int yy = 0; yy < CHUNK_HEIGHT; yy += 1) {
          Tile t = new Tile(world, this, TileType.TYPES["empty"], xx, yy);
          tiles[xx, yy] = t;
          t.cbRegisterOnChanged(OnTileChanged);
          t.cbRegisterOnChanged(world.OnTileChanged);
          t.cbRegisterOnItemChanged(world.OnInventoryItemChangedOnTile);
          t.cbRegisterOnItemChanged(OnInventoryItemChangedOnTile);
          //tiles[x, y].room = rooms[0];
          world.outside.AssignTile(t);

          //float xf = ((float)t.world_x + world.xSeed) * world.noiseFactor;
          //float yf = ((float)t.world_y + world.ySeed) * world.noiseFactor;
          RMTile rm = World.current.citymap.GetTile(t.world_x, t.world_y);
          int n = rm.h;
          //float n2 = World.current.SimplexNoise(((float)t.world_x), ((float)t.world_y),World.current.oreZ);
          //float n = (n1 + n2) / 2;
          //int j = Mathf.FloorToInt(n * (float)TileType.countNatural);

          if (rm.showsAs == TYPE.BUILDING) {
            t.SetType(TileType.TYPES["concrete"]);
            if (rm.btype == BUILDING_TYPE.EDGE) {
              t.SetType(TileType.TYPES["building_edge"]);
            } else if (rm.btype == BUILDING_TYPE.INSIDE) {
              t.SetType(TileType.TYPES["building_interior"]);
            }
            
          } else if (rm.showsAs == TYPE.ROAD) {
            t.SetType(TileType.TYPES["concrete"]);
          } else if (rm.showsAs == TYPE.WATER) {
            t.SetType(TileType.TYPES["water"]);
          } else {


            foreach (string k in TileType.TYPES.Keys) {
              TileType tempT = TileType.TYPES[k];



              if (tempT.name != "empty") {
                if (n == tempT.heightIndex) {

                  t.SetType(tempT);

                  if (tempT.varieties.Count == 0) {

                  } else {
                    foreach (string variety in tempT.varieties.Keys) {
                      TileType vt = TileType.TYPES[variety];
                      float A = tempT.varieties[variety].Item1;
                      float B = tempT.varieties[variety].Item2;
                      float na = World.current.SimplexNoise(((float)t.world_x), ((float)t.world_y), A);
                      float nb = World.current.SimplexNoise(((float)t.world_x), ((float)t.world_y), B);
                      float navg = (na + nb) / 2f;
                      if ((navg >= 0.95f && navg <= 1f) || (navg >= 0.05f && navg <= 0.1f)) {
                        t.SetType(vt);
                        break;
                      }
                    }
                  }



                  break;
                }
              }
            }
          }


        }


      }

      for (int xx = 0; xx < CHUNK_WIDTH; xx += 1) {
        for (int yy = 0; yy < CHUNK_HEIGHT; yy += 1) {
          tiles[xx, yy].SetNeighbours(true);

          Tile t = tiles[xx,yy];
          if (t.type.name != "empty" && t.type.name != "water") {
            if (Funcs.Chance100(1)) {
              if (InstalledItem.trashPrototypes.Count > 0) {
                string trashItemName = InstalledItem.GetRandomTrashItemName();
                InstalledItem proto = InstalledItem.GetPrototype(trashItemName);

                if (proto != null) {
                  if (proto.canSpawnOnTileTypeList.Count > 0) {
                    if (!proto.canSpawnOnTileTypeList.Contains(t.type.name)) continue;
                  }

                  //Debug.Log("I will spawn a " + (trashItemName == null ? "null" : trashItemName));
                  world.PlaceInstalledItem(trashItemName, t, true);
                }

              }
            } else if (Funcs.fChance(0.08f)) {
              InventoryItem item = InventoryItem.GetRandomSpawnItem();
              if (item != null) {
                string type = item.type;
                int qty = UnityEngine.Random.Range(1, InventoryItem.GetStackSize(type) + 1);
                if (Funcs.fChance(item.spawnsOnChunkGenchance * 100)) {
                  world.PlaceTileInventoryItem(type, t, qty);
                }
              }
            } else if (Funcs.fChance(0.05f)) {
              World.current.CreateEntityAtTile(t, "entities::chicken");
            }
          }


        }
      }
      SetNeighbourChunks();
    }

    private void OnInventoryItemChangedOnTile(Tile obj) {

    }

    public TileChunk(World world, int x, int y) {
      //Debug.Log("Chunk created at (" + x + "," + y + ")");
      neighbours = new Dictionary<string, TileChunk>();
      tiles = new Tile[CHUNK_WIDTH, CHUNK_HEIGHT];
      this.world = world;
      this.x = x;
      this.y = y;


    }

    public Tile GetTileAtWorldCoord(int wx, int wy) {
      int chunk_x = wx / CHUNK_WIDTH;
      int chunk_y = wy / CHUNK_HEIGHT;

      if (chunk_x == x && chunk_y == y) {
        //Debug.Log("Chunk = " + chunk_x + "," + chunk_y + " world:" + wx + "," + wy);
        return GetTileAt(wx % CHUNK_WIDTH, wy % CHUNK_HEIGHT);
      } else {
        Debug.LogError("This is not my beautiful coordinates: " + wx + ", " + wy + ": mine:" + x + "," + y);
        return null;
      }
    }

    public Tile GetTileAt(int xt, int yt) {
      //if(xt < 0) {
      //  xt = CHUNK_WIDTH - xt;
      //}

      //if (yt < 0) {
      //  yt = CHUNK_HEIGHT - yt;
      //}

      if (xt < 0 || xt > CHUNK_WIDTH - 1 || yt < 0 || yt > CHUNK_HEIGHT - 1) {
        return null;
      } else {
        return tiles[xt, yt];
      }
    }

    private void SetMeAndNeighbour(string dir, string oppositeDir, int xd, int yd) {

      TileChunk n = world.GetChunkIfExists(xd, yd);
      neighbours[dir] = n; //set it, even if it's null
      if (n != null) {
        if (n.neighbours[oppositeDir] == null) {
          n.neighbours[oppositeDir] = this;
        }


      }

    }


    public void SetNeighbourChunks() {

      SetMeAndNeighbour(World.NORTH, World.SOUTH, x, y + 1);
      SetMeAndNeighbour(World.EAST, World.WEST, x + 1, y);
      SetMeAndNeighbour(World.SOUTH, World.NORTH, x, y - 1);
      SetMeAndNeighbour(World.WEST, World.EAST, x - 1, y);

      SetMeAndNeighbour(World.NORTHWEST, World.SOUTHEAST, x - 1, y + 1);
      SetMeAndNeighbour(World.NORTHEAST, World.SOUTHWEST, x + 1, y + 1);
      SetMeAndNeighbour(World.SOUTHWEST, World.NORTHEAST, x - 1, y - 1);
      SetMeAndNeighbour(World.SOUTHEAST, World.NORTHWEST, x + 1, y - 1);




      //neighbours[World.NORTH] = world.GetChunkIfExists(x, y + 1);
      //neighbours[World.EAST] = world.GetChunkIfExists(x + 1, y);
      //neighbours[World.SOUTH] = world.GetChunkIfExists(x, y - 1);
      //neighbours[World.WEST] = world.GetChunkIfExists(x - 1, y);


      //neighbours[World.NORTHWEST] = world.GetChunkIfExists(x - 1, y + 1);
      //neighbours[World.NORTHEAST] = world.GetChunkIfExists(x + 1, y + 1);
      //neighbours[World.SOUTHWEST] = world.GetChunkIfExists(x - 1, y - 1);
      //neighbours[World.SOUTHEAST] = world.GetChunkIfExists(x + 1, y - 1);


      //for (int xx = x - 1; xx <= x + 1; xx += 1) {
      //  for (int yy = y - 1; yy <= y + 1; yy += 1) {
      //    if (xx == x && yy == y) continue;

      //    TileChunk n = world.GetChunk(xx, yy);

      //  }
      //}
    }




    public void OnTileChanged(Tile tile) {

    }

    public XmlSchema GetSchema() {
      throw new NotImplementedException();
    }

    public void ReadXml(XmlReader reader) {
      throw new NotImplementedException();
    }

    public void WriteXml(XmlWriter writer) {
      throw new NotImplementedException();
    }
  }


}
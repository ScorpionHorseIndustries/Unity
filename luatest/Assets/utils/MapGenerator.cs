using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class MapGenerator {

  List<int[,]> maps = new List<int[,]>();
  int width, height;
  World world;

  public static void MakeNewMap(World world, int width, int height) {
    new MapGenerator(world, width, height);

  }
  private MapGenerator(World world, int width, int height) {
    this.width = width;
    this.height = height;
    this.world = world;
    GenerateMap();

  }

  private void GenerateMap() {
    //DateTime date = new DateTime();

    

    

    for (int i = 0; i < TileType.countNatural - 1; i += 1) {
      String seed = DateTime.Now.ToString() + "_" + i + "_" + Guid.NewGuid().ToString();
      maps.Add(MakeInitialMap(55 - (i * 3), seed));
      //Debug.Log(seed);
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
        Tile t = world.getTileAt(x, y);
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

    for (int x = 0; x < width; x += 1) {
      Tile tt = world.getTileAt(x, 0);
      Tile tb = world.getTileAt(x, height-1);

      TileType type = TileType.TYPES["bedrock"];

      tt.SetType(type);
      tb.SetType(type);
    }
    for (int y = 0; y < height; y += 1) {
      Tile tt = world.getTileAt(0, y);
      Tile tb = world.getTileAt(width-1, y);

      TileType type = TileType.TYPES["bedrock"];

      tt.SetType(type);
      tb.SetType(type);
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
}


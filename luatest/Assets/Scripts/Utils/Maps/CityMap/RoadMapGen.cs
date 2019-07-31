using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NoYouDoIt.Utils.Maps.CityMap {
  using NoYouDoIt.Utils.Maps.CityMap.Rivers;
  public enum TYPE {
    GROUND,
    ROAD,
    BUILDING,
    WATER,
    BOUNDARY
  }

  public enum BUILDING_TYPE {
    NONE,
    EDGE,
    INSIDE
  }


  public class RoadMapGen {

    //static properties
    public static int ROAD_WIDTH = 2;
    public static int BLOCK_WIDTH = 32;

    public static int BUILDING_SIZE_MIN = 4;
    public static int BUILDING_SIZE_MAX = 10;

    public static float ROAD_Y_CHANCE = 0.3f;
    public static float ROAD_Y_ON_CHANCE = 0.6f;
    public static float ROAD_X_CHANCE = 0.3f;
    public static float ROAD_X_ON_CHANCE = 0.6f;
    public List<RMGBuilding> buildings = new List<RMGBuilding>();

    System.Random rng;



    //properties
    public int width { get; private set; }
    public int height { get; private set; }

    RMTile[,] tiles;

    private float GetXChance(int x) {
      return 1.0f - ((float)(width - x) / (float)width);
    }

    private bool Chance(float f) {
      return rng.NextDouble() < f;
    }

    private bool XChance(int x) {
      return (rng.NextDouble() < GetXChance(x));
    }
    private RoadMapGen() {

    }

    public int numGroundTileTypes { get; private set; }

    public static RoadMapGen Generate(int widthInTiles, int heightInTiles, int numGroundTileTypes) {
      RoadMapGen roadmap = new RoadMapGen();
      roadmap.rng = new System.Random();
      roadmap.width = widthInTiles;
      roadmap.height = heightInTiles;
      roadmap.numGroundTileTypes = numGroundTileTypes;

      roadmap.tiles = new RMTile[roadmap.width, roadmap.height];

      for (int x = 0; x < roadmap.width; x += 1) {
        for (int y = 0; y < roadmap.height; y += 1) {
          roadmap.tiles[x, y] = new RMTile(roadmap, x, y);
        }
      }

      roadmap.MakeRoads();
      MakeBuildings(roadmap);
      roadmap.UpdateTiles();

      for (int i = 0; i < 10; i += 1) {
        //roadmap.TryWave(TYPE.GROUND_SAND);
        //roadmap.TryWave(TYPE.GROUND_GRASS);
        for (int j = 0; j < roadmap.numGroundTileTypes; j += 1) {
          roadmap.TryWave(UnityEngine.Random.Range(0, roadmap.numGroundTileTypes));
          roadmap.SmoothMap(2, 4);
        }
        roadmap.SmoothMap(6, 4);





      }
      
      roadmap.TryWater();
      //roadmap.SmoothMap(3, 4);




      roadmap.SaveToPng();


      return roadmap;

    }

    //private void SmoothBlur(int iterations) {

    //  float[,] newValues = new float[width, height];
    //  for (int i = 0; i < iterations; i += 1) {
    //    for (int x = 0; x < width; x += 1) {
    //      for (int y = 0; y < height; y += 1) {
    //        RMTile tile = GetTile(x, y);

    //        float sum = 0;
    //        float count = 0;
    //        for (int xx = x - 1; xx < x + 2; xx += 1) {
    //          int ix = Mathf.Clamp(xx, 0, width - 1);
    //          for (int yy = y - 1; yy < y + 2; yy += 1) {
    //            if (xx == x && yy == y) continue;
    //            int iy = Mathf.Clamp(yy, 0, width - 1);
    //            RMTile nt = GetTile(ix, iy);

    //            sum += nt.h;
    //            count += 1;

    //          }
    //        }
    //        newValues[x, y] = sum / count;
    //      }
    //    }
    //  }

    //  for (int x = 0; x < width; x += 1) {
    //    for (int y = 0; y < height; y += 1) {
    //      tiles[x, y].h = newValues[x, y];
    //    }
    //  }
    //}

    private void SmoothMap(int iterations, int threshhold) {
      for (int i = 0; i < iterations; i += 1) {
        for (int x = 0; x < width; x += 1) {
          for (int y = 0; y < height; y += 1) {
            RMTile tile = GetTile(x, y);
            if (tile.markedAs != TYPE.ROAD && tile.markedAs != TYPE.BUILDING) {
              Dictionary<int, int> dct = Neighbours(x, y, tile.h);
              var sorted = from entry in dct orderby entry.Value descending select entry;
              foreach (var t in sorted) {
                if (t.Value > threshhold) {
                  tile.h = t.Key;

                  break;
                }

              }

            }

          }
        }
      }

    }

    Dictionary<int, int> Neighbours(int x, int y, int myH) {
      Dictionary<int, int> dct = new Dictionary<int, int>();

      for (int xx = x - 1; xx < x + 2; xx += 1) {
        for (int yy = y - 1; yy < y + 2; yy += 1) {
          if (xx == yy) continue;
          if (OOB(xx, yy)) {
            if (dct.ContainsKey(myH)) {
              dct[myH] += 1;
            } else {
              dct[myH] = 1;
            }
          } else {
            int hh = GetTile(xx, yy).h;
            if (dct.ContainsKey(hh)) {
              dct[hh] += 1;
            } else {
              dct[hh] = 1;
            }

          }
        }

      }

      return dct;

    }

    private void TryWater() {
      for (int i = 0; i < 2; i += 1) {
        int x = 0;
        int y = 0;
        float facing = 0;
        int r = (int)(Funcs.Random(10, 100));

        switch (r % 4) {
          case 0:
            y = (int)(Funcs.Random(0, height));

            break;
          case 1:
            x = width - 1;
            y = (int)(Funcs.Random(0, height));
            facing = Mathf.PI;
            break;
          case 2:
            x = (int)(Funcs.Random(0, width));
            facing = Mathf.PI / 2f;
            break;
          case 3:
            x = (int)(Funcs.Random(0, width));
            y = height - 1;
            facing = -(Mathf.PI / 2f);
            break;
        }


        RMGRiver river = new RMGRiver(this, x, y, width, height, 10, facing);
        while (!river.finished) {
          river.Update();
        }
        river.Draw();

        river = null;
      }
      

    }

    //private void TryWater() {
    //  UpdateTiles();
    //  int countSuccess = 0;
    //  for (int i = 0; i < 20; i += 1) {
    //    if (countSuccess > 3) break;
    //    int xs = 0, ys = 0, xe = 0, ye = 0;
    //    int r = UnityEngine.Random.Range(0, 10) % 4;
    //    int r2 = UnityEngine.Random.Range(0, 10) % 2;
    //    switch (r) {
    //      case 0:
    //        xs = 0;
    //        ys = UnityEngine.Random.Range(0, height);
    //        xe = width - 1;
    //        ye = UnityEngine.Random.Range(0, height);
    //        break;
    //      case 1:
    //        xs = UnityEngine.Random.Range(0, width);
    //        ys = 0;
    //        xe = UnityEngine.Random.Range(0, width);
    //        ye = height - 1;
    //        break;
    //      case 2:
    //        xs = UnityEngine.Random.Range(0, width);
    //        ys = 0;
    //        if (r2 == 0) {
    //          xe = 0;
    //          ys = UnityEngine.Random.Range(0, height);
    //        } else {
    //          xe = width - 1;
    //          ys = UnityEngine.Random.Range(0, height);

    //        }
    //        break;
    //      case 3:
    //        xs = UnityEngine.Random.Range(0, width);
    //        ys = height - 1;
    //        if (r2 == 0) {
    //          xe = 0;
    //          ys = UnityEngine.Random.Range(0, height);
    //        } else {
    //          xe = width - 1;
    //          ys = UnityEngine.Random.Range(0, height);

    //        }
    //        break;
    //      default:
    //        break;
    //    }


    //    RMTile start = GetTile(xs, ys);
    //    RMTile end = GetTile(xe, ye);

    //    if (start != null && end != null && start.markedAs != TYPE.BUILDING && end.markedAs != TYPE.BUILDING) {

    //      RMGPath path = new RMGPath(this, start, end);

    //      if (path.findPath()) {

    //        List<RMTile> riverPath = new List<RMTile>();
    //        riverPath.Add(path.path[0]);
    //        float angle = UnityEngine.Random.Range(0, Mathf.PI * 2);
    //        float angle2 = UnityEngine.Random.Range(0, Mathf.PI * 2);
    //        float rad1 = UnityEngine.Random.Range(3, 6);
    //        float rad2 = UnityEngine.Random.Range(3, 6);
    //        for (int j = 1; j < path.path.Count - 1; j += 1) {
    //          angle += 0.1f;
    //          RMTile pos = path.path[j];
    //          riverPath.Add(pos);
    //          RMTile nextPos = path.path[j + 1];

    //          Vector2 p = new Vector2(pos.x, pos.y);
    //          Vector2 np = new Vector2(nextPos.x, nextPos.y);
    //          Vector2 dir = np - p;
    //          dir.Normalize();

    //          Vector2 dirRA = new Vector2(-dir.y, dir.x);
    //          Vector2 dirRA2 = new Vector2(dirRA.x, dirRA.y);
    //          dirRA *= Mathf.Sin(angle) * rad1;
    //          dirRA2 *= Mathf.Cos(angle2) * rad2;
    //          dirRA += p;
    //          dirRA2 += p;

    //          for (float tt = 0; tt < 1; tt += 0.1f) {
    //            Vector2 v = (dirRA - p) * tt;
    //            Vector2 v2 = (dirRA2 - p) * tt;
    //            v += p;
    //            v2 += p;

    //            int x1 = Mathf.FloorToInt(v.x);
    //            int y1 = Mathf.FloorToInt(v.y);

    //            int x2 = Mathf.FloorToInt(v2.x);
    //            int y2 = Mathf.FloorToInt(v2.y);


    //            RMTile t = GetTile(x1, y1);
    //            if (t != null) {
    //              riverPath.Add(t);
    //            }
    //            t = GetTile(x2, y2);
    //            if (t != null) {
    //              riverPath.Add(t);
    //            }


    //          }



    //        }
    //        countSuccess += 1;
    //        foreach (var t in riverPath) {
    //          if (t.showsAs != TYPE.ROAD && t.markedAs != TYPE.BUILDING) {
    //            t.markedAs = TYPE.WATER;
    //            t.showsAs = TYPE.WATER;
    //          }
    //        }
    //        //Debug.Log("found path");
    //      } else {
    //        //Debug.Log("could not find path");
    //      }
    //    }
    //  }
    //}

    private void TryWave(int newH) {
      UpdateTiles();
      for (int i = 0; i < 20; i += 1) {
        int xs = UnityEngine.Random.Range(0, width);
        int ys = UnityEngine.Random.Range(0, height);
        int xe = UnityEngine.Random.Range(0, width);
        int ye = UnityEngine.Random.Range(0, height);
        if (MakeWave(xs, ys, xe, ye, newH)) {
          break;

        }
      }
    }



    private bool MakeWave(int startX, int startY, int endX, int endY, int newH) {




      RMTile start = GetTile(startX, startY);
      RMTile end = GetTile(endX, endY);

      if (IsObstacle(startX, startY) || IsObstacle(endX, endY)) return false;

      List<Tuple<int, int, int>> nodes = new List<Tuple<int, int, int>>();


      nodes.Add(MT(endX, endY, 1));

      while (nodes.Count > 0) {
        List<Tuple<int, int, int>> newNodes = new List<Tuple<int, int, int>>();

        foreach (var node in nodes) {
          int x = node.Item1;
          int y = node.Item2;
          int d = node.Item3;

          GetTile(x, y).d = d;
          //check east
          if (!OOB(x + 1, y) && GetTile(x + 1, y).d == 0) {
            newNodes.Add(MT(x + 1, y, d + 1));
          }
          //check west
          if (!OOB(x - 1, y) && GetTile(x - 1, y).d == 0) {
            newNodes.Add(MT(x - 1, y, d + 1));
          }

          //check north
          if (!OOB(x, y - 1) && GetTile(x, y - 1).d == 0) {
            newNodes.Add(MT(x, y - 1, d + 1));
          }
          //check south
          if (!OOB(x, y + 1) && GetTile(x, y + 1).d == 0) {
            newNodes.Add(MT(x, y + 1, d + 1));
          }

        }

        newNodes.Sort((e1, e2) => p(e1.Item1, e1.Item2).CompareTo(p(e1.Item1, e2.Item2)));
        newNodes = newNodes.GroupBy(e => p(e.Item1, e.Item2)).Select(e => e.First()).ToList();
        nodes.Clear();
        nodes.AddRange(newNodes);

      }
      List<RMTile> tilesToUpdate = new List<RMTile>();

      int maxD = 0;
      for (int x = 0; x < width; x += 1) {
        for (int y = 0; y < height; y += 1) {
          RMTile t = tiles[x, y];
          if (t.d > maxD) {
            maxD = t.d;
          }

        }
      }

      for (int x = 0; x < width; x += 1) {
        for (int y = 0; y < height; y += 1) {
          RMTile t = tiles[x, y];
          t.fd = (float)t.d / (float)maxD;

        }
      }




      tilesToUpdate.Add(start);

      int max = UnityEngine.Random.Range(1, maxD);



      for (int x = 0; x < width; x += 1) {
        for (int y = 0; y < height; y += 1) {
          RMTile t = tiles[x, y];
          float c = (float)t.d / (float)max;
          if (Chance(c / 2.0f)) {
            if (t.markedAs != TYPE.BUILDING && t.showsAs != TYPE.ROAD) {
              if (t.d <= max) {
                tilesToUpdate.Add(t);
              }
            }
          }

        }
      }


      foreach (RMTile t in tilesToUpdate) {

        t.h = newH;
      }

      return true;





    }

    Tuple<int, int, int> GetTuple(int x, int y) {
      if (OOB(x, y)) return null;
      RMTile tile = GetTile(x, y);
      return MT(tile.x, tile.y, tile.d);
    }

    private Tuple<int, int> KVP(int x, int y) {
      return new Tuple<int, int>(x, y);
    }

    private int p(int x, int y) {
      return y * width + x;
    }

    public static Tuple<int, int, int> MT(int x, int y, int z) {
      return new Tuple<int, int, int>(x, y, z);
    }

    private void UpdateTiles() {
      for (int x = 0; x < width; x += 1) {
        for (int y = 0; y < height; y += 1) {

          tiles[x, y].Update();
          tiles[x, y].SetNeighbours();
        }
      }
    }

    public bool IsObstacle(int x, int y) {
      if (OOB(x, y)) {
        return true;
      }

      RMTile tile = GetTile(x, y);
      if (tile.markedAs == TYPE.BUILDING) {
        return true;
      }

      return false;
    }

    private static void MakeBuildings(RoadMapGen roadmap) {
      for (int y = 2; y < roadmap.height; y += BLOCK_WIDTH) {
        roadmap.SearchForBuildings(y, 1);
        roadmap.SearchForBuildings(y + (BLOCK_WIDTH - 3), -1);
      }

      for (int x = 2; x < roadmap.width; x += BLOCK_WIDTH) {
        roadmap.SearchForBuildingsX(x, 1);
        roadmap.SearchForBuildingsX(x + (BLOCK_WIDTH - 3), -1);
      }
      roadmap.ProcBuildings();

      //roadmap.SetBuildingEdges();
    }

    private void SaveToPng() {
      Texture2D tex = new Texture2D(width * 4, height * 4, TextureFormat.ARGB32, false);
      for (int x = 0; x < width; x += 1) {
        for (int y = 0; y < height; y += 1) {
          RMTile t = GetTile(x, y);
          Color c = Color.white;


          switch (t.showsAs) {
            case TYPE.GROUND:
              float v = (((float)t.h) / ((float)numGroundTileTypes));
              c = new Color(v, v, v);

              break;
            case TYPE.ROAD:
              c = Color.gray;
              break;
            case TYPE.BUILDING:
              c = Color.red;
              break;
            case TYPE.WATER:
              c = Color.blue;
              break;
            default:
              break;
          }

          for (int xx = x * 4; xx < (x * 4) + 4; xx += 1) {
            for (int yy = y * 4; yy < (y * 4) + 4; yy += 1) {
              tex.SetPixel(xx, yy, c);
            }
          }
        }
      }

      tex.Apply();
      SaveTexToPng("map",tex);

    }

    public static void SaveTexToPng(string n, Texture2D tex) {
      byte[] bytes = tex.EncodeToPNG();

      DateTime dt = DateTime.Now;

      string fname = string.Format("{0}map{1:yyyyMMddHHmmssffff}.png", n,dt);


      File.WriteAllBytes(Path.Combine(Application.streamingAssetsPath, "output", fname), bytes);
    }

    public RMTile GetTile(int x, int y) {
      if (!OOB(x, y)) {
        return tiles[x, y];
      } else {
        return null;
      }
    }

    public bool OOB(int x, int y) {
      return (x < 0 || x > width - 1 || y < 0 || y > height - 1);
    }

    private void MakeRoads() {

      for (int x = 0; x < width; x += RoadMapGen.BLOCK_WIDTH) {
        bool goY = true;

        for (int y = 0; y < height; y += 1) {
          if (y % RoadMapGen.BLOCK_WIDTH == 1) {
            if (Chance(ROAD_Y_CHANCE)) {
              goY = Chance(ROAD_Y_ON_CHANCE);
            }
          }

          if (goY) {
            RMTile t = GetTile(x, y);

            if (XChance(x)) {
              t.showsAs = TYPE.ROAD;
            }
            t.markedAs = TYPE.ROAD;
            t = GetTile(x + 1, y);
            if (t != null) {
              t.markedAs = TYPE.ROAD;
              if (XChance(x)) {
                t.showsAs = TYPE.ROAD;
              }
            }

          }
        }

      }


      for (int y = 0; y < width; y += BLOCK_WIDTH) {
        bool goX = true;
        for (int x = 0; x < width; x += 1) {
          float c1 = 1 - ((float)(width - x) / (float)(width));
          if (x % BLOCK_WIDTH == 1) {
            if (Chance(ROAD_X_CHANCE)) {

              goX = XChance(x);
            }
          }
          if (goX) {


            RMTile t = GetTile(x, y);
            t.markedAs = TYPE.ROAD;
            if (XChance(x)) {
              t.showsAs = TYPE.ROAD;
            }
            t = GetTile(x, y + 1);
            if (t != null) {
              t.markedAs = TYPE.ROAD;
              if (XChance(x)) {
                t.showsAs = TYPE.ROAD;
              }
            }
          }
        }
      }
    }

    enum MODE {
      SEARCH, PLACE, DONTPLACE
    }


    void ProcBuildings() {
      foreach (RMGBuilding b in buildings) {

        for (int xx = b.tlx; xx <= b.brx; xx += 1) {
          for (int yy = b.tly; yy <= b.bry; yy += 1) {
            b.tiles.Add(GetTile(xx, yy));
          }
        }

        for (int xx = b.tlx + 1; xx < b.brx; xx += 1) {
          for (int yy = b.tly + 1; yy < b.bry; yy += 1) {

            RMTile t = GetTile(xx, yy);
            if (t != null) {
              t.btype = BUILDING_TYPE.INSIDE;
            } else {
              Debug.LogError(b + " is invalid");
            }
          }

        }

      }


      for (int i = buildings.Count - 1; i >= 0; i -= 1) {
        RMGBuilding A = buildings[i];
        for (int j = buildings.Count - 1; j >= 0; j -= 1) {
          if (i == j) continue;
          RMGBuilding B = buildings[j];

          if (A.yDir != 0 && B.yDir != 0) {
            RemoveEastWestWalls(A, B);

          } else if (A.xDir != 0 && B.xDir != 0) {
            RemoveNorthSouthWalls(A, B);
          }

        }
      }


    }

    private void RemoveNorthSouthWalls(RMGBuilding A, RMGBuilding B) {
      bool isNorthNeighbour = false;
      bool isSouthNeighbour = false;

      for (int x = A.tlx + 1; x < A.brx; x += 1) {
        RMTile north = GetTile(x, A.tly - 1);
        RMTile east = GetTile(x, A.bry + 1);

        if (north != null) {
          if (B.tiles.Contains(north)) {
            isNorthNeighbour = true;
            break;

          }
        }
        if (east != null) {
          if (B.tiles.Contains(east)) {
            isSouthNeighbour = true;
            break;
          }
        }
      }

      if (isNorthNeighbour || isSouthNeighbour) {
        RMGBuilding removeNorthWall = null;
        RMGBuilding removeSouthWall = null;

        if (A.area < B.area) {
          if (isNorthNeighbour) {
            removeNorthWall = A;
          } else {
            removeSouthWall = A;
          }
        } else if (A.area >= B.area) {
          if (isNorthNeighbour) {
            removeSouthWall = B;
          } else {
            removeNorthWall = B;
          }
        }

        if (removeNorthWall != null) {
          for (int x = removeNorthWall.tlx + 1; x < removeNorthWall.brx; x += 1) {
            RMTile t = GetTile(x, removeNorthWall.tly);
            t.btype = BUILDING_TYPE.INSIDE;
          }
        }

        if (removeSouthWall != null) {
          for (int x = removeSouthWall.tlx + 1; x < removeSouthWall.brx; x += 1) {
            RMTile t = GetTile(x, removeSouthWall.bry);
            t.btype = BUILDING_TYPE.INSIDE;
          }
        }

      }
    }

    private void RemoveEastWestWalls(RMGBuilding A, RMGBuilding B) {
      bool isWestNeighbour = false;
      bool isEastNeighbour = false;

      for (int y = A.tly + 1; y < A.bry; y += 1) {
        RMTile west = GetTile(A.tlx - 1, y);
        RMTile east = GetTile(A.brx + 1, y);

        if (west != null) {
          if (B.tiles.Contains(west)) {
            isWestNeighbour = true;
            break;

          }
        }
        if (east != null) {
          if (B.tiles.Contains(east)) {
            isEastNeighbour = true;
            break;
          }
        }
      }

      if (isWestNeighbour || isEastNeighbour) {
        RMGBuilding removeWestWall = null;
        RMGBuilding removeEastWall = null;

        if (A.area < B.area) {
          if (isWestNeighbour) {
            removeWestWall = A;
          } else {
            removeEastWall = A;
          }
        } else if (A.area >= B.area) {
          if (isWestNeighbour) {
            removeEastWall = B;
          } else {
            removeWestWall = B;
          }
        }
        List<RMTile> toRemove = new List<RMTile>();
        if (removeWestWall != null) {

          for (int y = removeWestWall.tly + 1; y < removeWestWall.bry; y += 1) {
            RMTile t = GetTile(removeWestWall.tlx, y);
            int countWalls = 0;
            for (int xx = removeWestWall.tlx-1; xx <= removeWestWall.tlx; xx += 1) {
              for (int yy = y-1; yy <= y +1; yy += 1) {
                if (xx == removeWestWall.tlx && yy == y) continue;
                if (t.btype == BUILDING_TYPE.EDGE) {
                  countWalls += 1;
                }
              }
            }

            if (countWalls >= 5) {
              toRemove.Add(t);
            }
            
          }
        }

        if (removeEastWall != null) {
          for (int y = removeEastWall.tly + 1; y < removeEastWall.bry; y += 1) {
            RMTile t = GetTile(removeEastWall.brx, y);
            int countWalls = 0;
            for (int xx = removeEastWall.tlx; xx <= removeEastWall.tlx+1; xx += 1) {
              for (int yy = y - 1; yy <= y + 1; yy += 1) {
                if (xx == removeEastWall.tlx && yy == y) continue;
                if (t.btype == BUILDING_TYPE.EDGE) {
                  countWalls += 1;
                }
              }
            }

            if (countWalls >= 5) {
              toRemove.Add(t);
            }
          }
        }

        foreach(var t in toRemove) {
          t.btype = BUILDING_TYPE.INSIDE;
        }

      }
    }

    void SearchForBuildings(int y, int yDir) {

      MODE mode = MODE.SEARCH;
      for (int x = 0; x < width; x += 1) {
        RMTile t = GetTile(x, y);
        RMTile above = GetTile(x, y - yDir);
        if (t == null) continue;

        if (above == null || above.markedAs != TYPE.ROAD) continue;

        if (t.markedAs == TYPE.GROUND) {
          if (mode == MODE.SEARCH) { //can begin

            if (XChance(x)) {
              mode = MODE.PLACE;
            } else {
              mode = MODE.DONTPLACE;
            }
          }
        } else if (t.markedAs == TYPE.ROAD) {
          if (mode == MODE.PLACE || mode == MODE.DONTPLACE) {
            mode = MODE.SEARCH;
          }
        }

        if (mode == MODE.PLACE) {
          if (XChance(x) && Chance(0.1f)) {
            mode = MODE.DONTPLACE;
          }
        } else if (mode == MODE.DONTPLACE) {
          if (XChance(x) && Chance(0.1f)) {
            mode = MODE.PLACE;
          }
        }

        if (mode == MODE.PLACE) {
          int w = BUILDING_SIZE_MIN;
          int h = BUILDING_SIZE_MIN * yDir;
          int tlx = int.MaxValue, tly = int.MaxValue, brx = int.MinValue, bry = int.MinValue;
          bool valid = false;

          while (true) {

            bool doesItFit = true;
            for (int xx = x; xx < x + w; xx += 1) {
              for (int yy = y; ; yy += yDir) {
                if (yDir > 0) {
                  if (yy >= y + h) break;
                } else if (yDir < 0) {
                  if (yy <= y + h) break;
                }

                RMTile tt = GetTile(xx, yy);
                if (tt == null || tt.markedAs == TYPE.ROAD) {
                  doesItFit = false;
                }
              }
              if (!doesItFit) break;
            }

            if (doesItFit) {
              for (int xx = x; xx < x + w; xx += 1) {
                for (int yy = y; ; yy += yDir) {
                  if (yDir > 0) {
                    if (yy >= y + h) break;
                  } else if (yDir < 0) {
                    if (yy <= y + h) break;
                  }
                  RMTile tt = GetTile(xx, yy);

                  tt.showsAs = TYPE.BUILDING;
                  tt.markedAs = TYPE.BUILDING;
                  if (xx < tlx) {
                    tlx = xx;
                  }

                  if (xx > brx) {
                    brx = xx;
                  }

                  if (yy < tly) {
                    tly = yy;
                  }

                  if (yy > bry) {
                    bry = yy;
                  }
                  valid = true;
                }


              }

              w += (Chance(0.4f)) ? 1 : 0;
              h += (Chance(0.4f)) ? 1 * yDir : 0;


              if (Mathf.Abs(w) > BUILDING_SIZE_MAX || Mathf.Abs(h) > BUILDING_SIZE_MAX) break;
              if (Chance(0.1f)) break;
            } else {
              break;
            }
          }
          if (valid) {
            for (int xx = tlx; xx <= brx; xx += 1) {
              RMTile tle = GetTile(xx, tly);
              tle.btype = BUILDING_TYPE.EDGE;
              tle = GetTile(xx, bry);
              tle.btype = BUILDING_TYPE.EDGE;
            }

            for (int yy = tly; yy <= bry; yy += 1) {
              RMTile tle = GetTile(tlx, yy);
              tle.btype = BUILDING_TYPE.EDGE;
              tle = GetTile(brx, yy);
              tle.btype = BUILDING_TYPE.EDGE;
            }

            buildings.Add(new RMGBuilding(tlx, tly, brx, bry, 0, yDir));

          }


          x += w - 1;

        }



      }
    }



    void SearchForBuildingsX(int x, int xDir) {

      MODE mode = MODE.SEARCH;
      for (int y = 0; y < width; y += 1) {
        RMTile t = GetTile(x, y);
        RMTile above = GetTile(x - xDir, y);
        if (t == null) continue;

        if (above == null || above.markedAs != TYPE.ROAD) continue;


        if (t.showsAs == TYPE.BUILDING) continue;
        if (t.markedAs == TYPE.GROUND) {
          if (mode == MODE.SEARCH) { //can begin

            if (XChance(x)) {
              mode = MODE.PLACE;
            } else {
              mode = MODE.DONTPLACE;
            }
          }
        } else if (t.markedAs == TYPE.ROAD) {
          if (mode == MODE.PLACE || mode == MODE.DONTPLACE) {
            mode = MODE.SEARCH;
          }
        }

        if (mode == MODE.PLACE) {
          int w = BUILDING_SIZE_MIN * xDir;
          int h = BUILDING_SIZE_MIN;
          int tlx = int.MaxValue, tly = int.MaxValue, brx = int.MinValue, bry = int.MinValue;
          bool valid = false;
          while (true) {

            bool doesItFit = true;
            for (int yy = y; yy < y + h; yy += 1) {
              for (int xx = x; ; xx += xDir) {

                if (xDir > 0) {
                  if (xx >= x + w) break;
                } else if (xDir < 0) {
                  if (xx <= x + w) break;
                }

                RMTile tt = GetTile(xx, yy);
                if (tt == null || tt.markedAs == TYPE.ROAD) {
                  doesItFit = false;
                }
              }
              if (!doesItFit) break;
            }

            if (doesItFit) {
              for (int yy = y; yy < y + h; yy += 1) {
                for (int xx = x; ; xx += xDir) {

                  if (xDir > 0) {
                    if (xx >= x + w) break;
                  } else if (xDir < 0) {
                    if (xx <= x + w) break;
                  }
                  RMTile tt = GetTile(xx, yy);

                  tt.showsAs = TYPE.BUILDING;
                  tt.markedAs = TYPE.BUILDING;
                  if (xx < tlx) {
                    tlx = xx;
                  }

                  if (xx > brx) {
                    brx = xx;
                  }

                  if (yy < tly) {
                    tly = yy;
                  }

                  if (yy > bry) {
                    bry = yy;
                  }
                  valid = true;
                }
              }

              w += (Chance(0.4f)) ? 1 * xDir : 0;
              h += (Chance(0.4f)) ? 1 : 0;


              if (Math.Abs(w) > BUILDING_SIZE_MAX || Math.Abs(h) > BUILDING_SIZE_MAX) break;
              if (Chance(0.1f)) break;
            } else {
              break;
            }
          }
          if (valid) {
            for (int xx = tlx; xx <= brx; xx += 1) {
              RMTile tle = GetTile(xx, tly);
              tle.btype = BUILDING_TYPE.EDGE;
              tle = GetTile(xx, bry);
              tle.btype = BUILDING_TYPE.EDGE;
            }

            for (int yy = tly; yy <= bry; yy += 1) {
              RMTile tle = GetTile(tlx, yy);
              tle.btype = BUILDING_TYPE.EDGE;
              tle = GetTile(brx, yy);
              tle.btype = BUILDING_TYPE.EDGE;
            }

            buildings.Add(new RMGBuilding(tlx, tly, brx, bry, xDir, 0));
          }
          y += h - 1;
        }



      }
    }

  }





}

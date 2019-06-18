using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;

using System;
using System.Linq;





public class NYDISpriteManager : ScriptableObject {

  public static TextureFormat DEFAULT_TEXTURE_FORMAT = TextureFormat.RGBA32;
  private static NYDISpriteManager _instance;
  public static NYDISpriteManager Instance {
    get {
      if (_instance == null) {
        _instance = CreateInstance<NYDISpriteManager>();
      }
      return _instance;
    }
  }

  static NYDISpriteManager() {
    NYDISpriteManager n = NYDISpriteManager.Instance;
  }

  private Dictionary<string, Sprite> sprites;

  private void Awake() {
    sprites = new Dictionary<string, Sprite>();
    LoadSprites();
  }
  private void OnEnable() {

  }

  private void LoadSprites() {




    string path = Application.streamingAssetsPath;
    path = Path.Combine(path, "images");
    List<string> folders = new List<string>(Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories));
    foreach (string s in folders) {
      List<string> files = new List<string>(Directory.GetFiles(s, "*.png"));
      foreach (string f in files) {
        CreateSprite(f);
        //Debug.Log(f + " = " + Path.GetFileName(Path.GetDirectoryName(f)));

      }
    }

    //foreach (string name in sprites.Keys) {
    //  Debug.Log(sprites[name]);
    //}



  }

  public List<string> GetSpritesStarting(string starting) {

    return sprites.Keys.Where(e => e.Length >= starting.Length && e.Substring(0, starting.Length) == starting).ToList<string>();

  }

  private void CreateSprite(string fullPath) {


    MemoryStream stream = new MemoryStream();
    System.Drawing.Image image = System.Drawing.Bitmap.FromFile(fullPath);
    image.Save(stream, image.RawFormat);
    byte[] rawBytes = stream.ToArray();
    byte[] spriteBytes = File.ReadAllBytes(fullPath);

    //Debug.Log("raw bytes: " + rawBytes.Length + " sb: " + spriteBytes.Length);
    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(fullPath);
    bitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);


    byte[] rgbBytes = new byte[bitmap.Width * bitmap.Height * 4];
    for (int x = 0; x < bitmap.Width; x += 1) {
      for (int y = 0; y < bitmap.Height; y += 1) {
        System.Drawing.Color pixel = bitmap.GetPixel(x, y);
        //Color c = new Color32(pixel.R, pixel.G, pixel.B, pixel.A);

        int offset = y * bitmap.Width * 4 + x * 4;
        rgbBytes[offset + 0] = pixel.R;
        rgbBytes[offset + 1] = pixel.G;
        rgbBytes[offset + 2] = pixel.B;
        rgbBytes[offset + 3] = pixel.A;
      }
    }
    //Array.Reverse(rgbBytes);

    Texture2D temp = new Texture2D(2, 2, TextureFormat.RGBA32, false);
    if (!temp.LoadImage(spriteBytes)) {
      Debug.LogError("image file:[" + fullPath + "] could not be read.");
      return;
    }

    Texture2D spriteSheetTexture = new Texture2D(temp.width, temp.height, TextureFormat.RGBA32, false, false);

    spriteSheetTexture.LoadRawTextureData(rgbBytes);
    if (true) {
      File.WriteAllBytes(Application.streamingAssetsPath + "/output/" + Path.GetFileNameWithoutExtension(fullPath) + ".png", spriteSheetTexture.EncodeToPNG());
      string jsonFile = Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath) + ".json");

      List<SpriteLoader> loaders = new List<SpriteLoader>();


      if (File.Exists(jsonFile)) {
        JObject json = JObject.Parse(File.ReadAllText(jsonFile));

        float ppu = Funcs.jsonGetFloat(json["ppu"], 32f);
        int eachWidth = Funcs.jsonGetInt(json["width"], 32);
        int eachHeight = Funcs.jsonGetInt(json["height"], 32);
        List<PaletteSwap> swaps = new List<PaletteSwap>();

        JArray jsonPaletteSwapArray = Funcs.jsonGetArray(json, "paletteSwaps");
        if (jsonPaletteSwapArray != null) {
          foreach (JObject swap in jsonPaletteSwapArray) {
            float csInput = (float)(Funcs.jsonGetInt(swap["input"], 0) % 255);


            PaletteSwap ps = new PaletteSwap();
            ps.input = csInput / 255.0f;

            JArray csOutputs = Funcs.jsonGetArray(swap, "output");
            if (csOutputs != null) {
              foreach (JToken csOutputColour in csOutputs) {

                int colour = Convert.ToInt32(csOutputColour.ToString(), 16);

                byte r = (byte)((colour >> 16) & 255);
                byte g = (byte)((colour >> 8) & 255);
                byte b = (byte)((colour >> 0) & 255);
                Debug.Log(csOutputColour.ToString() + " r:" + r + " g:" + g + " b:" + b);
                Color c = new Color32(r, g, b,255);
                ps.output.Add(c);
              }
              swaps.Add(ps);


            }
          }
        }


        JArray jsonArray = Funcs.jsonGetArray(json, "sprites");

        if (jsonArray != null) {
          foreach (JObject jsonSprite in jsonArray) {
            string sName = Funcs.jsonGetString(jsonSprite["name"], null);
            if (sName == null) continue;

            SpriteLoader sl = new SpriteLoader(sName);
            if (swaps.Count > 0) {
              sl.paletteSwaps = swaps;
            }
            sl.x = Funcs.jsonGetInt(jsonSprite["x"], sl.x);
            sl.y = Funcs.jsonGetInt(jsonSprite["y"], sl.y);
            sl.pivotX = Funcs.jsonGetFloat(jsonSprite["px"], sl.pivotX);
            sl.pivotY = Funcs.jsonGetFloat(jsonSprite["px"], sl.pivotY);
            sl.flipX = Funcs.jsonGetBool(jsonSprite["flipX"], sl.flipX);
            sl.width = eachWidth;
            sl.height = eachHeight;
            sl.ppu = ppu;
            sl.spriteSheetTex = new Texture2D(spriteSheetTexture.width, spriteSheetTexture.height, DEFAULT_TEXTURE_FORMAT, false);
            sl.spriteSheetTex.SetPixels(spriteSheetTexture.GetPixels());
            sl.spriteSheetTex.Apply();
            loaders.Add(sl);


          }
        }

      } else {
        SpriteLoader sl = new SpriteLoader(Path.GetFileNameWithoutExtension(fullPath));
        sl.spriteSheetTex = new Texture2D(spriteSheetTexture.width, spriteSheetTexture.height, DEFAULT_TEXTURE_FORMAT, false);
        sl.spriteSheetTex.SetPixels(spriteSheetTexture.GetPixels());
        sl.spriteSheetTex.Apply();
        sl.width = spriteSheetTexture.width;
        sl.height = spriteSheetTexture.height;

        loaders.Add(sl);
      }



      foreach (SpriteLoader loader in loaders) {
        Vector2 pivot = new Vector2(loader.pivotX, loader.pivotY);

        ////Debug.Log("loading " + loader.ToString());
        //Color32[] col32 = loader.spriteSheetTex.GetPixels32();
        //int xs = loader.x * loader.width;
        //int ys = loader.y * loader.height;
        //int xe = xs + loader.width;
        //int ye = ys + loader.height;
        //Color[] c = new Color32[loader.width * loader.height];
        //for (int y = 0; y < loader.height; y += 1) {
        //  for (int x = 0; x < loader.width; x += 1) {

        //    int srcIndex = ((ys + y) * loader.spriteSheetTex.width) + (xs + x);

        //    int dstIndex= y * loader.width + x;
        //    Color32 ccc = col32[srcIndex];
        //    c[dstIndex] = ccc;
        //  }

        //}
        Color[] c = loader.spriteSheetTex.GetPixels(loader.x * loader.width, loader.y * loader.height, loader.width, loader.height);


        loader.spriteTex = new Texture2D(loader.width, loader.height, DEFAULT_TEXTURE_FORMAT, false);
        if (loader.flipX) {
          for (int x = 0; x < loader.width; x += 1) {
            int xx = loader.width - 1 - x;
            for (int y = 0; y < loader.height; y += 1) {
              loader.spriteTex.SetPixel(x, y, c[y * loader.width + xx]);

            }

          }


        } else {
          loader.spriteTex.SetPixels(c);

        }
        loader.spriteTex.filterMode = loader.filter;
        loader.spriteTex.Apply();

        //Color32[] c32 = tex.GetPixels32();
        ////for(int ii = 0; ii < c32.Length; ii += 1) {

        ////  c32[ii] = new Color32(c32[ii].r, c32[ii].g, c32[ii].b, 255);
        ////}
        //tex.SetPixels32(c32);



        //tex.Apply();
        if (loader.paletteSwaps == null) {

          Rect rect = new Rect(0, 0, loader.width, loader.height);
          string properName = Path.GetFileName(Path.GetDirectoryName(fullPath)) + "::" + loader.name;
          Sprite sprite = Sprite.Create(loader.spriteTex, rect, pivot, loader.ppu);
          sprite.name = "sprite::" + properName;

          sprites[properName] = sprite;
        } else {
          for (int j = 0; j < 1; j += 1) {
            Texture2D newTex = new Texture2D(loader.spriteTex.width, loader.spriteTex.height, DEFAULT_TEXTURE_FORMAT, false);
            newTex.SetPixels(loader.spriteTex.GetPixels());

            foreach (PaletteSwap ps in loader.paletteSwaps) {

              Color[] c32 = loader.spriteTex.GetPixels();


              for (int y = 0; y < newTex.height; y += 1) {
                for (int x = 0; x < newTex.width; x += 1) {

                  Color oc = c32[y * newTex.width + x];


                  //if (x == y) Debug.Log(loader.name + "(" + x + "," + y + ") oc = " + oc.ToString() + " " + " input=" + ps.input);                  
                  if (oc.a > 0 && oc.r == ps.input) {

                    Color nc = ps.output[UnityEngine.Random.Range(0, ps.output.Count)];

                    newTex.SetPixel(x, y, nc);
                  } else {

                  }


                }
              }

            }
            newTex.filterMode = FilterMode.Point;
            newTex.Apply();
            Rect rect = new Rect(0, 0, loader.width, loader.height);
            string properName = Path.GetFileName(Path.GetDirectoryName(fullPath)) + "::" + loader.name + "_" + j;
            Sprite sprite = Sprite.Create(newTex, rect, pivot, loader.ppu);
            sprite.name = "sprite::" + properName;
            sprites[properName] = sprite;
          }
        }


        //string output = sprite.name + " [" + properName + "]"+
        //  "\n(" + sprite.texture.width + "x" + sprite.texture.height + "@"+sprite.pixelsPerUnit+ ") (g:" + sprite.texture.graphicsFormat + ") (format:" + sprite.texture.format + ")";
        //Debug.Log("sprite:" + output);
        //Debug.Log("loader:" + loader);
      }
    }
  }

  public Sprite GetSprite(string name) {
    if (!sprites.ContainsKey(name)) {
      //Debug.LogError("sprite name " + name + " could not be found");
      return null;
    } else {
      //Debug.Log("sprite found: " + name);
      return sprites[name];
    }

  }

  public static string TexToString(Texture2D tex) {
    return tex.width + "x" + tex.height + ") (g:" + tex.graphicsFormat + ") (format:" + tex.format + ")";
  }
  private class PaletteSwap {
    public float input;
    public List<Color> output;
    public PaletteSwap() {
      output = new List<Color>();
    }

  }

  private class SpriteLoader {
    public List<PaletteSwap> paletteSwaps;// = new List<PaletteSwap>();
    public int width = 32;
    public int height = 32;
    public float ppu = 32;
    public int x = 0, y = 0;
    public float pivotX = 0.5f, pivotY = 0.5f;
    public Texture2D spriteSheetTex;
    public Texture2D spriteTex;
    public bool flipX = false;
    public string name;
    public FilterMode filter = FilterMode.Point;
    public SpriteLoader(string name) {
      this.name = name;
    }

    public override string ToString() {
      string output = name + " xy(" + x + "," + y + ") wh(" + width + "," + height + ") ppu:" + ppu + " fx:" + flipX.ToString();
      if (spriteSheetTex != null) {
        output += "sheet: " + TexToString(spriteSheetTex);
      }

      if (spriteTex != null) {
        output += "tex:" + TexToString(spriteTex);
      }

      return output;
    }


  }
}



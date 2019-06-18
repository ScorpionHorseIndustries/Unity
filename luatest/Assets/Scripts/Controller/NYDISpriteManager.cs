using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine.Experimental.Rendering;

public class NYDISpriteManager : ScriptableObject {


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

  private void CreateSprite(string fullPath) {
    //string spriteName = (Path.GetFileName(Path.GetDirectoryName(fullPath)) + "::" + Path.GetFileNameWithoutExtension(fullPath)).ToLower();

    byte[] spriteBytes = File.ReadAllBytes(fullPath);

    Texture2D temp = new Texture2D(1, 1);
    if (!temp.LoadImage(spriteBytes)) {
      Debug.LogError("image file:[" + fullPath + "] could not be read.");
      return;
    }


    Texture2D spriteSheetTexture = new Texture2D(temp.width, temp.height, TextureFormat.RGBA32, false);
    if (spriteSheetTexture.LoadImage(spriteBytes)) {
      string jsonFile = Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath) + ".json");

      List<SpriteLoader> loaders = new List<SpriteLoader>();


      if (File.Exists(jsonFile)) {
        JObject json = JObject.Parse(File.ReadAllText(jsonFile));

        float ppu = Funcs.jsonGetFloat(json["ppu"], 32f);
        int eachWidth = Funcs.jsonGetInt(json["width"], 32);
        int eachHeight = Funcs.jsonGetInt(json["height"], 32);

        JArray jsonArray = Funcs.jsonGetArray(json, "sprites");

        if (jsonArray != null) {
          foreach (JObject jsonSprite in jsonArray) {
            string sName = Funcs.jsonGetString(jsonSprite["name"], null);
            if (sName == null) continue;

            SpriteLoader sl = new SpriteLoader(sName);
            sl.x = Funcs.jsonGetInt(jsonSprite["x"], sl.x);
            sl.y = Funcs.jsonGetInt(jsonSprite["y"], sl.y);
            sl.pivotX = Funcs.jsonGetFloat(jsonSprite["px"], sl.pivotX);
            sl.pivotY = Funcs.jsonGetFloat(jsonSprite["px"], sl.pivotY);
            sl.flipX = Funcs.jsonGetBool(jsonSprite["flipX"], sl.flipX);
            sl.width = eachWidth;
            sl.height = eachHeight;
            sl.ppu = ppu;
            sl.spriteSheetTex = new Texture2D(spriteSheetTexture.width, spriteSheetTexture.height,TextureFormat.RGBA32,false);
            sl.spriteSheetTex.SetPixels(spriteSheetTexture.GetPixels());
            sl.spriteSheetTex.Apply();
            loaders.Add(sl);


          }
        }

      } else {
        SpriteLoader sl = new SpriteLoader(Path.GetFileNameWithoutExtension(fullPath));
        sl.spriteSheetTex = new Texture2D(spriteSheetTexture.width, spriteSheetTexture.height,TextureFormat.RGBA32,false);
        sl.spriteSheetTex.SetPixels(spriteSheetTexture.GetPixels());
        sl.spriteSheetTex.Apply();
        sl.width = spriteSheetTexture.width;
        sl.height = spriteSheetTexture.height;
        loaders.Add(sl);
      }



      foreach (SpriteLoader loader in loaders) {
        Vector2 pivot = new Vector2(loader.pivotX, loader.pivotY);

        Debug.Log("loading " + loader.ToString());
        Color[] c = loader.spriteSheetTex.GetPixels(loader.x * loader.width, loader.y * loader.height, loader.width, loader.height);
        loader.spriteTex = new Texture2D(loader.width, loader.height,TextureFormat.RGBA32,false);
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

        Rect rect = new Rect(0, 0, loader.width, loader.height);
        string properName = Path.GetFileName(Path.GetDirectoryName(fullPath)) + "::" + loader.name;
        Sprite sprite = Sprite.Create(loader.spriteTex, rect, pivot, loader.ppu);
        sprite.name = "sprite::" + properName;
        
        sprites[properName] = sprite;
        string output = sprite.name + " [" + properName + "]"+
          "\n(" + sprite.texture.width + "x" + sprite.texture.height + "@"+sprite.pixelsPerUnit+ ") (g:" + sprite.texture.graphicsFormat + ") (format:" + sprite.texture.format + ")";
        Debug.Log("sprite:" + output);
        Debug.Log("loader:" + loader);
      }
    }
  }

  public Sprite GetSprite(string name) {
    if (!sprites.ContainsKey(name)) {
      Debug.LogError("sprite name " + name + " could not be found");
      return null;
    } else {
      //Debug.Log("sprite found: " + name);
      return sprites[name];
    }

  }

  public static string TexToString(Texture2D tex) {
    return tex.width + "x" + tex.height + ") (g:" + tex.graphicsFormat + ") (format:" + tex.format + ")";
  }

  private class SpriteLoader {
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
      string output =  name + " xy(" + x + "," + y + ") wh(" + width + "," + height + ") ppu:" + ppu + " fx:" + flipX.ToString();
      if (spriteSheetTex != null) {
        output += "sheet: " + TexToString(spriteSheetTex);
      }

      if (spriteTex != null) {
        output += "tex:"+TexToString(spriteTex);
      }

      return output;
    }


  }
}



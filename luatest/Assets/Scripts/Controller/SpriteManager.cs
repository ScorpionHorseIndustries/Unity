using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;

public class SpriteManager : ScriptableObject {


  private static SpriteManager _instance;
  public static SpriteManager Instance {
    get {
      if (_instance == null) {
        _instance = CreateInstance<SpriteManager>();
      }
      return _instance;
    }
  }

  private Dictionary<string, Sprite> sprites;

  private void OnEnable() {
    sprites = new Dictionary<string, Sprite>();
    LoadSprites();
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

    foreach (string name in sprites.Keys) {
      Debug.Log(name);
    }



  }

  private void CreateSprite(string fullPath) {
    //string spriteName = (Path.GetFileName(Path.GetDirectoryName(fullPath)) + "::" + Path.GetFileNameWithoutExtension(fullPath)).ToLower();

    byte[] spriteBytes = File.ReadAllBytes(fullPath);

    Texture2D spriteTex = Texture2D.whiteTexture;
    if (spriteTex.LoadImage(spriteBytes)) {
      string jsonFile = Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath) + ".json");

      List<SpriteLoader> loaders = new List<SpriteLoader>();


      if (File.Exists(jsonFile)) {
        JObject json = JObject.Parse(File.ReadAllText(jsonFile));

        float ppu = Funcs.jsonGetFloat(json["ppu"], 32f);
        int eachWidth = Funcs.jsonGetInt(json["eachWidth"], 32);
        int eachHeight = Funcs.jsonGetInt(json["eachHeight"], 32);

        JArray jsonArray = Funcs.jsonGetArray(json, "sprites");

        if (jsonArray != null) {
          foreach (JObject jsonSprite in jsonArray) {
            string sName = Funcs.jsonGetString(jsonSprite["name"], null);
            if (sName == null) continue;

            SpriteLoader sl = new SpriteLoader(sName);
            sl.x = Funcs.jsonGetInt(jsonSprite["x"], sl.x);
            sl.y = Funcs.jsonGetInt(jsonSprite["y"], sl.y);
            sl.px = Funcs.jsonGetFloat(jsonSprite["px"], sl.px);
            sl.py = Funcs.jsonGetFloat(jsonSprite["px"], sl.py);
            sl.flipX = Funcs.jsonGetBool(jsonSprite["flipX"], sl.flipX);
            sl.width = eachWidth;
            sl.height = eachHeight;
            sl.ppu = ppu;

            loaders.Add(sl);


          }
        }

      } else {
        SpriteLoader sl = new SpriteLoader(Path.GetFileNameWithoutExtension(fullPath));
        sl.tex = spriteTex;
        sl.width = spriteTex.width;
        sl.height = spriteTex.height;
        loaders.Add(sl);
      }



      foreach (SpriteLoader loader in loaders) {
        Vector2 pivot = new Vector2(loader.px, loader.py);


        Color[] c = spriteTex.GetPixels(loader.x * loader.width, loader.y * loader.height, loader.width, loader.height);
        Texture2D tex = new Texture2D(loader.width, loader.height);
        tex.SetPixels(c);



        Rect rect = new Rect(0, 0, loader.width, loader.height);


        string properName = Path.GetFileName(Path.GetDirectoryName(fullPath)) + "::" + loader.name;

        Sprite sprite = Sprite.Create(tex, rect, pivot, loader.ppu);
        sprites[properName] = sprite;
        Debug.Log("sprite:" + properName);
      }
    }
  }

  public Sprite GetSprite(string name) {
    if (sprites.ContainsKey(name)) {
      return null;
    } else {
      return sprites[name];
    }

  }

  private class SpriteLoader {
    public int width = 32;
    public int height = 32;
    public float ppu = 32;
    public int x = 0, y = 0;
    public float px = 0.5f, py = 0.5f;
    public Texture2D tex;
    public bool flipX = false;
    public string name;
    public SpriteLoader(string name) {
      this.name = name;
    }


  }
}



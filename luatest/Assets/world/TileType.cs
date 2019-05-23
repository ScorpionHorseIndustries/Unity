﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;


public class TileType {

  public static Dictionary<string, TileType> TYPES = new Dictionary<string, TileType>();
  public static Dictionary<int, TileType> TYPES_BY_ID = new Dictionary<int, TileType>();

  public int id { get; private set; }
  //private static bool loaded = false;
  public string name { get; private set; }
  public string spriteName { get; private set; }
  public string[] sprites { get; private set; }
  public float rangeLow, rangeHigh;
  public bool build { get; private set; } = true; //can be built on
  public float movementFactor { get; private set; }
  public int height { get; private set; }
  public static int countNatural { get; private set; }
	private TileType(string name, string spriteName)  {
		this.name = name;
		this.spriteName = spriteName;
	}
	private TileType() {

	}

	private TileType(string name, string[] sprites) {
		this.name = name;
		this.sprites = sprites;
		this.spriteName = sprites[0];
	}

	public override string ToString() {
    return name;
		//return name + " " + spriteName + " (" + rangeLow + "->" + rangeHigh + ") build?:" + build;
	}

	

	public static void LoadFromFile() {

    TYPES.Clear();
    TYPES_BY_ID.Clear();
    countNatural = 0;

    string path = Application.streamingAssetsPath + "/json/TileTypes.json";
		string json = File.ReadAllText(path);

		JObject job = JObject.Parse(json);
		//Debug.Log(job);


		JArray ja = (JArray)job["tileTypes"];
		foreach (JObject j in ja) {
			string n = (string)j["name"];
      string prefix = Funcs.jsonGetString(j["spritePrefix"], "");
      int num = Funcs.jsonGetInt(j["sprites"], 0);

      int offset = Funcs.jsonGetInt(j["spritesOffset"], 0);
      


      int id = Funcs.jsonGetInt(j["id"], 0);
			List<string> sprites = new List<string>();

      for (int i = offset; i < offset + num; i += 1) {
        sprites.Add(prefix + i);
      }

			//Debug.Log(n + " " + sprites[0] + " " + sprites.Count);
			TileType t = new TileType(n, sprites.ToArray<string>());
      t.id = id;
			t.rangeLow = (float)j["rangeLow"];
			t.rangeHigh = (float)j["rangeHigh"];
			t.build = (bool)j["build"];
      t.movementFactor = Funcs.jsonGetFloat(j["movementFactor"], 0.5f);
      t.height = Funcs.jsonGetInt(j["height"], -1);
			
			TYPES.Add(n, t);
      TYPES_BY_ID.Add(t.id, t);

      if (t.height >= 0) {
        countNatural += 1;
      }
		}
		//loaded = true;


		

	}


	//public static TileType DIRT = new TileType("DIRT", "tile_dirt");
	//public static TileType GRASS = new TileType("GRASS", "tile_grass");
	//public static TileType WATER = new TileType("WATER", "tile_water");
	//public static TileType EMPTY = new TileType("EMPTY", "tile_empty");

}


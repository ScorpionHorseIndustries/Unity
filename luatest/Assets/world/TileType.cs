using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;


public class TileType {

	public static Dictionary<string, TileType> TYPES = new Dictionary<string, TileType>();
	
	private static bool loaded = false;
	public string name { get; private set; }
	public string spriteName { get; private set; }
	public string[] sprites { get; private set; }
	public float rangeLow, rangeHigh;
	public bool build { get; private set; } = true;
	private TileType(string name, string spriteName)  {
		this.name = name;
		this.spriteName = spriteName;
	}
	private TileType() {

	}

	private TileType(string name, string[] sprites) {
		this.name = name;
		this.sprites = sprites;
		this.spriteName = sprites[Random.Range(0, sprites.Length)];
	}

	public override string ToString() {
		return name + " " + spriteName + " (" + rangeLow + "->" + rangeHigh + ") build?:" + build;
	}

	

	public static void LoadFromFile() {
		if (loaded) return;
		string path = Application.streamingAssetsPath + "/json/TileTypes.json";
		string json = File.ReadAllText(path);

		JObject job = JObject.Parse(json);
		//Debug.Log(job);


		JArray ja = (JArray)job["tileTypes"];
		foreach (JObject j in ja) {
			string n = (string)j["name"];
			JArray nja = (JArray)j["sprites"];
			List<string> sprites = new List<string>();
			foreach (string njas in nja) {
				sprites.Add(njas);
			}
			//Debug.Log(n + " " + sprites[0] + " " + sprites.Count);
			TileType t = new TileType(n, sprites.ToArray<string>());
			t.rangeLow = (float)j["rangeLow"];
			t.rangeHigh = (float)j["rangeHigh"];
			t.build = (bool)j["build"];
			Debug.Log(t);
			TYPES.Add(n, t);
		}
		loaded = true;


		

	}


	//public static TileType DIRT = new TileType("DIRT", "tile_dirt");
	//public static TileType GRASS = new TileType("GRASS", "tile_grass");
	//public static TileType WATER = new TileType("WATER", "tile_water");
	//public static TileType EMPTY = new TileType("EMPTY", "tile_empty");

}


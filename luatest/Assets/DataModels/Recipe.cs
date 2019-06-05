using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Recipe {
  public string name { get; private set; }
  public int id { get; private set; }
  public float buildTime { get; private set; }


  public class RecipeResource {
    public string name { get; private set; }
    public int qtyRequired { get; private set; }
    public int qtyRemaining { get; private set; }
    public RecipeResource(string name, int qty) {
      this.name = name;
      this.qtyRequired = qty;
      this.qtyRemaining = qty;
    }

    public RecipeResource(RecipeResource o) {
      this.name = o.name;
      this.qtyRequired = o.qtyRequired;
      this.qtyRemaining = o.qtyRequired;
    }

    public void Add(int qty) {
      qty = Mathf.Abs(qty);
      if (qty > qtyRemaining) {
        qtyRemaining = 0;
      } else {
        qtyRemaining -= qty;
      }

    }
  }

  public override string ToString() {
    string items = "";
    foreach (RecipeResource rr in resources.Values) {
      items += string.Format("[{0}:{1}/{2}]", rr.name, rr.qtyRemaining, rr.qtyRequired);
    }
    return "recipe: \"" + name + "\": " + items;
    
  }



  public Dictionary<string, RecipeResource> resources;


  private Recipe(Recipe proto) {
    this.name = proto.name;
    this.id = proto.id;
    resources = new Dictionary<string, RecipeResource>();
    this.buildTime = proto.buildTime;
    
    foreach (RecipeResource rp in proto.resources.Values) {
      resources[rp.name] = new RecipeResource(rp);
    }



  }

  private Recipe() {

  }

  public void Add(string name, int qty) {
    if(resources.ContainsKey(name)) {
      resources[name].Add(qty);
    }

  }

  //----------------------------STATIC ---------------------------------

  public static Recipe GetRecipe(string name) {
    if (recipes.ContainsKey(name)) {
      return new Recipe(recipes[name]);
    }

    return null;
  }

  private static Dictionary<string, Recipe> recipes; 

  public static void LoadFromFile() {
    recipes = new Dictionary<string, Recipe>();

    string path = Application.streamingAssetsPath + "/json/Recipes.json";

    string json = File.ReadAllText(path);

    JObject jo = JObject.Parse(json);

    JArray invItems = Funcs.jsonGetArray(jo, "Recipes");
    if (invItems != null) {
      CreateRecipes(invItems);
    } else {
      Debug.LogError("could not find recipes items array in [" + path + "]");
    }

  }


  private static void CreateRecipes(JArray recipeArray) {
    foreach (JObject jRecipe in recipeArray) {
      Recipe recipe = new Recipe();

      recipe.name = Funcs.jsonGetString(jRecipe["name"], "");
      recipe.id = Funcs.jsonGetInt(jRecipe["id"], -1);
      recipe.buildTime = Funcs.jsonGetFloat(jRecipe["buildTime"], 1);
      JArray jaResources = Funcs.jsonGetArray(jRecipe, "resources");

      if (jaResources != null) {
        recipe.resources = new Dictionary<string, RecipeResource>();
        foreach (JObject jResource in jaResources) {
          string rname = Funcs.jsonGetString(jResource["name"], null);
          int rqty = Funcs.jsonGetInt(jResource["qty"], -1);

          

          if (rname != null && rqty > 0) {
            RecipeResource r = new RecipeResource(rname, rqty);

            recipe.resources[rname] = r;
            
          }


        }
      }

      recipes[recipe.name] = recipe;
      Debug.Log("recipe Added: " + recipe + " " + recipe.buildTime);

    }

  }

}

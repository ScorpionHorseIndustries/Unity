using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Recipe {
  public string name { get; private set; }
  public int id { get; private set; }
  public float buildTime { get; private set; }

  private List<string> productChanceList;

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
  public class RecipeProduct{
    public string name { get; private set; }
    public int qtyMin { get; private set; }
    public int qtyMax{ get; private set; }
    public float chance { get; private set; }

    public RecipeProduct(string name, int qtyMin, int qtyMax, float chance) {
      this.name = name;
      this.qtyMin = qtyMin;
      this.qtyMax = qtyMax;
      this.chance = chance;
      
    }

    public RecipeProduct(RecipeProduct o) {
      this.name = o.name;
      this.qtyMin = o.qtyMin;
      this.qtyMax = o.qtyMax;
      this.chance = o.chance;
    }

  }

  public override string ToString() {
    string items = "";
    foreach (RecipeResource rr in resources.Values) {
      items += string.Format("[{0}:{1}/{2}]", rr.name, rr.qtyRemaining, rr.qtyRequired);
    }
    return "recipe: \"" + name + "\": " + items;
    
  }

  public string ToString(bool includeProducts) {
    string output = ToString();
    string items = "";
    if (includeProducts) {
      foreach(RecipeProduct rp in products.Values) {
        items += string.Format("[{0}:{1}-{2}]", rp.name, rp.qtyMin, rp.qtyMax);
      }
    }

    return output + "\n" + items;
  }



  public Dictionary<string, RecipeResource> resources;
  public Dictionary<string, RecipeProduct> products;


  private Recipe(Recipe proto) {
    this.name = proto.name;
    this.id = proto.id;
    resources = new Dictionary<string, RecipeResource>();
    products = new Dictionary<string, RecipeProduct>();
    this.buildTime = proto.buildTime;
    
    foreach (RecipeResource rp in proto.resources.Values) {
      resources[rp.name] = new RecipeResource(rp);
    }

    foreach(RecipeProduct rp in proto.products.Values) {
      products[rp.name] = new RecipeProduct(rp);
    }



  }

  public RecipeProduct GetProduct() {
    //List<string> prlist = new List<string>();

    if (productChanceList == null) {
      productChanceList = new List<string>();
    }

    if (productChanceList.Count == 0) {
      foreach (RecipeProduct rp in products.Values) {
        for (int i = 0; i < rp.chance * 100; i += 1) {
          productChanceList.Add(rp.name);
          //Debug.Log(i + " " + rp.name + " " + prlist.Count);
        }
      }
    }

    if (productChanceList.Count > 0) {

      string rpname = productChanceList[UnityEngine.Random.Range(0, productChanceList.Count)];

      return products[rpname];
    } else {
      return null;
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

      recipe.products = new Dictionary<string, RecipeProduct>();
      recipe.resources = new Dictionary<string, RecipeResource>();

      JArray jaResources = Funcs.jsonGetArray(jRecipe, "resources");

      if (jaResources != null) {
        
        foreach (JObject jResource in jaResources) {
          string rname = Funcs.jsonGetString(jResource["name"], null);
          int rqty = Funcs.jsonGetInt(jResource["qty"], -1);

          

          if (rname != null && rqty > 0) {
            RecipeResource r = new RecipeResource(rname, rqty);

            recipe.resources[rname] = r;
            
          }


        }
      }


      JArray jaProducts = Funcs.jsonGetArray(jRecipe, "products");
      if (jaProducts != null) {
        
        foreach (JObject jProduct in jaProducts) {
          string rname = Funcs.jsonGetString(jProduct["name"], null);
          int rminQty = Funcs.jsonGetInt(jProduct["qtyMin"], -1);
          int rmaxQty = Funcs.jsonGetInt(jProduct["qtyMax"], -1);
          float rchance = Funcs.jsonGetFloat(jProduct["chance"], 0);



          if (rname != null && rminQty >= 0 && rmaxQty > 0) {
            RecipeProduct r = new RecipeProduct(rname, rminQty, rmaxQty, rchance);

            recipe.products[rname] = r;

          }


        }
      }

      recipes[recipe.name] = recipe;
      Debug.Log("recipe Added: " + recipe.ToString(true) + " " + recipe.buildTime);

    }

  }

}

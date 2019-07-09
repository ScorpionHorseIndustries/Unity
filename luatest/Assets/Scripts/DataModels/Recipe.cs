using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
namespace NoYouDoIt.DataModels {
  using NoYouDoIt.Utils;

  public enum RECIPE_PRODUCT_TYPE {
    INVENTORY_ITEM,
    ENTITY,
    MONEY
  }
  public class RecipeProduct {
    public string name { get; private set; }
    public int qtyMin { get; private set; }
    public int qtyMax { get; private set; }
    public float chance { get; private set; }
    public float outputQty { get; private set; }
    public RECIPE_PRODUCT_TYPE type { get; private set; }

    public RecipeProduct(string name, int qtyMin, int qtyMax, float chance) {
      this.name = name;
      this.qtyMin = qtyMin;
      this.qtyMax = qtyMax;
      this.chance = chance;
      if (name.StartsWith("inv::")) {
        this.type = RECIPE_PRODUCT_TYPE.INVENTORY_ITEM;
      } else if (name.StartsWith("entity::")) {
        this.type = RECIPE_PRODUCT_TYPE.ENTITY;
      } else if (name.StartsWith("money")) {
        this.type = RECIPE_PRODUCT_TYPE.MONEY;
      }
      //Debug.Log("product added: " + this.ToString());
    }

    private void SetOutput() {
      if (UnityEngine.Random.Range(0,1) < chance) {
        switch (type) {
          case RECIPE_PRODUCT_TYPE.ENTITY:
          case RECIPE_PRODUCT_TYPE.INVENTORY_ITEM:
            outputQty = Mathf.FloorToInt(UnityEngine.Random.Range((int)qtyMin, (int)qtyMax + 1));          
            break;
          case RECIPE_PRODUCT_TYPE.MONEY:
            outputQty = UnityEngine.Random.Range(qtyMin, qtyMax);
            break;
          default:
            break;
        }
      }
    }


    public override string ToString() {
      return string.Format("{0}:t{1}:q{2}-{3}@{4}%", name, type.ToString(), qtyMin, qtyMax, chance * 100);
    }

    public RecipeProduct(RecipeProduct o) {
      this.name = o.name;
      this.qtyMin = o.qtyMin;
      this.qtyMax = o.qtyMax;
      this.chance = o.chance;
      this.type = o.type;
      SetOutput();
    }

  }

  public class RecipeResource {
    public string name { get; private set; }
    public int qtyRequired { get; private set; }
    public int qtyRemaining { get; private set; }

    public override string ToString() {
      return "resource: " + name + " " + qtyRemaining + "/" + qtyRequired;
    }

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

  public class Recipe {
    public string name { get; private set; }
    public int id { get; private set; }
    public float buildTime { get; private set; }
    public float cost { get; private set; }
    public bool onDemand { get; private set; }
    public string btnText { get; private set; }

    private List<string> productChanceList;







    public override string ToString() {
      string items = "";
      foreach (RecipeResource rr in resources.Values) {
        items += string.Format("[{0}:{1}/{2}]", rr.name, rr.qtyRemaining, rr.qtyRequired);
      }
      return "recipe: [" + name + "], " + cost + ": " + items;

    }

    public string ToString(bool includeProducts) {
      string output = ToString();
      string items = "";
      if (includeProducts) {
        foreach (RecipeProduct rp in products.Values) {
          items += string.Format("[{0}:{1}-{2}]", rp.name, rp.qtyMin, rp.qtyMax);
        }
      }

      return output + "\n" + items;
    }



    public Dictionary<string, RecipeResource> resources;

    public bool givesCash { get; private set; }
    public float minCash { get; private set; }
    public float maxCash { get; private set; }

    public Dictionary<string, RecipeProduct> products;


    private Recipe(Recipe proto) {

      this.name = proto.name;
      this.id = proto.id;
      resources = new Dictionary<string, RecipeResource>();
      products = new Dictionary<string, RecipeProduct>();
      this.givesCash = proto.givesCash;
      this.minCash = proto.minCash;
      this.maxCash = proto.maxCash;
      this.buildTime = proto.buildTime;
      this.cost = proto.cost;
      this.onDemand = proto.onDemand;
      this.btnText = proto.btnText;

      foreach (RecipeResource rp in proto.resources.Values) {
        resources[rp.name] = new RecipeResource(rp);
      }

      foreach (RecipeProduct rp in proto.products.Values) {
        products[rp.name] = new RecipeProduct(rp);
      }



    }

    public List<RecipeProduct> GetProducts() {
      //List<string> prlist = new List<string>();
      List<RecipeProduct> rpList = new List<RecipeProduct>();

      foreach (RecipeProduct rp in products.Values) {
        RecipeProduct nrp = new RecipeProduct(rp);
        
        rpList.Add(nrp);

      }

      return rpList;//.ToArray();




    }

    private Recipe() {

    }

    public void Add(string name, int qty) {
      if (resources.ContainsKey(name)) {
        resources[name].Add(qty);
      }

    }

    //----------------------------STATIC ---------------------------------
    public static IEnumerable<string> GetRecipeNames() {
      return recipes.Keys;
    }


    public static Recipe GetRecipe(string name) {
      if (recipes.ContainsKey(name)) {
        return new Recipe(recipes[name]);
      }

      return null;
    }

    private static Dictionary<string, Recipe> recipes;

    public static void LoadFromFile() {
      recipes = new Dictionary<string, Recipe>();

      string path = Path.Combine(Application.streamingAssetsPath, "data","Recipes");

      string[] files = Directory.GetFiles(path, "*.json",SearchOption.AllDirectories);

      foreach (string file in files) {
        
        string json = File.ReadAllText(file);
        //Debug.Log(file + "\n" +json);
        JObject jo = JObject.Parse(json);

        CreateRecipePrototype(jo);
      }

      

      

      //JArray invItems = Funcs.jsonGetArray(jo, "Recipes");
      //if (invItems != null) {
      //  CreateRecipes(invItems);
      //} else {
      //  Debug.LogError("could not find recipes items array in [" + path + "]");
      //}

    }


    //private static void CreateRecipes(JArray recipeArray) {
    //  foreach (JObject jRecipe in recipeArray) {
    //    CreateRecipePrototype(jRecipe);

    //  }

    //}

    private static void CreateRecipePrototype(JObject jRecipe) {
      Recipe recipe = new Recipe();

      recipe.name = Funcs.jsonGetString(jRecipe["name"], "");
      recipe.id = Funcs.jsonGetInt(jRecipe["id"], -1);
      recipe.buildTime = Funcs.jsonGetFloat(jRecipe["buildTime"], 1);

      recipe.products = new Dictionary<string, RecipeProduct>();
      recipe.resources = new Dictionary<string, RecipeResource>();
      recipe.cost = Funcs.jsonGetFloat(jRecipe["cost"], 0);
      recipe.onDemand = Funcs.jsonGetBool(jRecipe["onDemand"], false);
      recipe.btnText = Funcs.jsonGetString(jRecipe["btnText"], "");

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
      //Debug.Log("recipe Added: " + recipe.ToString(true) + " " + recipe.buildTime);
    }
  }
}
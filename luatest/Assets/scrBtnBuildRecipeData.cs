using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NoYouDoIt.DataModels;
using NoYouDoIt.Utils;

public class scrBtnBuildRecipeData : MonoBehaviour
{
  public List<string> recipeData = new List<string>();

  public void AddKeyValuePair(string a, string b) {
    recipeData.Add(Funcs.PadPair(28, a, b,'.'));
  }
  
  public void AddString(string s) {
    recipeData.Add(s);
  }
  public void AddRecipe(Recipe recipe) {

    foreach(RecipeResource rr in recipe.resources.Values) {
      string nName = InventoryItem.GetPrototype(rr.name).niceName;
      //string line = nName + ":" + rr.qtyRequired;
      
      AddKeyValuePair(nName, rr.qtyRequired.ToString());

    }

    if (recipe.cost > 0) {
     AddKeyValuePair("cost",string.Format("{0:00.00}", recipe.cost));
    }

  }

  public override string ToString() {
    string output = "";
    foreach(string s in recipeData) {
      output += s + "\n";
    }
    return output;
  }
}

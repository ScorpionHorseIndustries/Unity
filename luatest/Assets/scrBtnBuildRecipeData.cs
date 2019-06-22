using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NoYouDoIt.DataModels;

public class scrBtnBuildRecipeData : MonoBehaviour
{
  public List<string> recipeData = new List<string>();
  public void AddRecipe(Recipe recipe) {

    foreach(RecipeResource rr in recipe.resources.Values) {
      string nName = InventoryItem.GetPrototype(rr.name).niceName;
      string line = nName + ":" + rr.qtyRequired;
      recipeData.Add(line);

    }

    if (recipe.cost > 0) {
      recipeData.Add(string.Format("cost:{0:00.00}", recipe.cost));
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;

using NoYouDoIt.Controller;
using NoYouDoIt.DataModels;
using NoYouDoIt.TheWorld;

public class scrUIBuildMenu : MonoBehaviour {
  // Start is called before the first frame update
  public Button btnBuildPrefab;

  public static void DoSomething(PointerEventData o) {
    if (o.pointerEnter.GetComponent<Button>() != null ) {
      scrBtnBuildRecipeData rec = o.pointerEnter.GetComponent<scrBtnBuildRecipeData>();

      WorldController.Instance.UpdateRecipe(rec.ToString());
    }
    
  }

  void Start() {




    //add a button for building each type of installed item
    foreach (InstalledItem proto in
      InstalledItem.prototypes.Values.
        Where(e => e.build).
        OrderBy(e => e.niceName)) {
      Button btn = Instantiate(btnBuildPrefab, Vector2.zero, Quaternion.identity);
      btn.transform.SetParent(this.transform);

      btn.GetComponentInChildren<Text>().text = proto.niceName;
      btn.name = "btnBuild::" + proto.type;
      btn.onClick.AddListener(delegate { WorldController.Instance.SetBuildType_InstalledItem(proto.type); });
      btn.transform.localScale = Vector2.one;

      SetButtonSprite(btn, proto.spriteName + NYDISpriteManager.ICON_SUFFIX);


      EventTrigger trig = btn.GetComponent<EventTrigger>();
      EventTrigger.Entry entry = new EventTrigger.Entry();
      entry.eventID = EventTriggerType.PointerEnter;
      entry.callback.AddListener(data => { scrUIBuildMenu.DoSomething((PointerEventData)data); });
      trig.triggers.Add(entry);

      scrBtnBuildRecipeData recipeData = btn.GetComponent<scrBtnBuildRecipeData>();
      if (recipeData != null) {
        recipeData.AddRecipe(proto.recipe);
      }





      Recipe r = Recipe.GetRecipe(proto.workRecipeName);
      if (r != null && r.onDemand) {
        btn = Instantiate(btnBuildPrefab, Vector2.zero, Quaternion.identity);
        btn.transform.SetParent(this.transform);

        btn.GetComponentInChildren<Text>().text = r.btnText;
        btn.name = "btnBuild::" + r.name + "::ondemand";
        btn.onClick.AddListener(delegate { World.current.InstalledItems_AssignOnDemandJobs(proto.type); });
        btn.transform.localScale = Vector2.one;
        SetButtonSprite(btn, proto.spriteName + NYDISpriteManager.ICON_SUFFIX);

      }
       
    }

    //foreach (string recipeName in Recipe.GetRecipeNames()) {
    //  Recipe recipe = Recipe.GetRecipe(recipeName);

    //  if (recipe != null) {
    //    if (recipe.onDemand) {

    //    }
    //  }

    //}
 
  }

  private static void SetButtonSprite(Button btn, string spriteName) {
    Sprite sprite = WorldController.Instance.spriteController.GetSprite(spriteName);
    Image img = btn.GetComponentsInChildren<Image>().Where(e => e.transform.parent != btn.transform.parent).First(); //get the image from a child, not the current object
    if (img != null) {
      img.sprite = sprite;
    }
  }


}

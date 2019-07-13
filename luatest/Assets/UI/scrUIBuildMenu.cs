using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;
using System;

using NoYouDoIt.Controller;
using NoYouDoIt.DataModels;
using NoYouDoIt.TheWorld;


public class scrUIBuildMenu : MonoBehaviour {
  // Start is called before the first frame update
  public Button btnBuildPrefab;

  public static void UpdateRecipe(PointerEventData o) {
    if (o.pointerEnter.GetComponent<Button>() != null) {
      scrBtnBuildRecipeData rec = o.pointerEnter.GetComponent<scrBtnBuildRecipeData>();



      WorldController.Instance.UpdateRecipe(rec.ToString());
    }

  }


  void AddButtonEvent(Button btn, UnityEngine.Events.UnityAction<BaseEventData> call) {
    EventTrigger trig = btn.GetComponent<EventTrigger>();
    EventTrigger.Entry entry = new EventTrigger.Entry();
    entry.eventID = EventTriggerType.PointerEnter;
    entry.callback.AddListener(call);
    trig.triggers.Add(entry);

  }

  void Start() {

    Button btnConcrete = MakeButton("btnBuild::Concrete", "Concrete", null);
    btnConcrete.onClick.AddListener(delegate { WorldController.Instance.SetBuildType_Tile("concrete"); });

    Button btnRemove = MakeButton("btnBuild::Deconstruct", "Remove", null);
    btnRemove.onClick.AddListener(delegate { WorldController.Instance.SetBuildType_Deconstruct(); });


    //add a button for building each type of installed item
    foreach (InstalledItem proto in
      InstalledItem.prototypes.Values.
        Where(e => e.build).
        OrderBy(e => e.niceName)) {


      Button btn = MakeButton(proto);
      AddButtonEvent(btn,
        data => { scrUIBuildMenu.UpdateRecipe((PointerEventData)data); }
        );
      //EventTrigger trig = btn.GetComponent<EventTrigger>();
      //EventTrigger.Entry entry = new EventTrigger.Entry();
      //entry.eventID = EventTriggerType.PointerEnter;
      //entry.callback.AddListener(data => { scrUIBuildMenu.UpdateRecipe((PointerEventData)data); });
      //trig.triggers.Add(entry);

      scrBtnBuildRecipeData recipeData = btn.GetComponent<scrBtnBuildRecipeData>();
      if (recipeData != null) {
        recipeData.AddKeyValuePair("name", proto.niceName);
        recipeData.AddRecipe(proto.recipe);
      }





      if (proto.workRecipeName != null && proto.workRecipeName != "") {
        Recipe r = Recipe.GetRecipe(proto.workRecipeName);
        if (r != null && r.onDemand) {
          btn = Instantiate(btnBuildPrefab, Vector2.zero, Quaternion.identity);
          btn.transform.SetParent(this.transform);

          btn.GetComponentInChildren<Text>().text = r.btnText;
          btn.name = "btnBuild::" + r.name + "::ondemand";
          btn.onClick.AddListener(delegate { World.current.InstalledItems_AssignOnDemandJobs(proto.type); });
          btn.transform.localScale = Vector2.one;
          SetButtonSprite(btn, proto.spriteName + NYDISpriteManager.ICON_SUFFIX);

          AddButtonEvent(btn,
                data => { scrUIBuildMenu.UpdateRecipe((PointerEventData)data); }
              );

          recipeData = btn.GetComponent<scrBtnBuildRecipeData>();
          if (recipeData != null) {
            recipeData.AddKeyValuePair("name", r.btnText);
            recipeData.AddRecipe(r);

          }
        }
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

  private Button MakeButton(string name, string displayName, string spriteName) {
    Button btn = Instantiate(btnBuildPrefab, Vector2.zero, Quaternion.identity);
    btn.transform.SetParent(this.transform);

    btn.GetComponentInChildren<Text>().text = displayName;
    btn.name = "btnBuild::" + name;
    btn.transform.localScale = Vector2.one;
    SetButtonSprite(btn, spriteName);
    return btn;
  }

  private Button MakeButton(InstalledItem proto) {
    Button btn = MakeButton("btnBuild::" + proto.type, proto.niceName, proto.spriteName + NYDISpriteManager.ICON_SUFFIX);
    btn.onClick.AddListener(delegate { WorldController.Instance.SetBuildType_InstalledItem(proto.type); });

    return btn;
  }

  private static void SetButtonSprite(Button btn, string spriteName) {
    Sprite sprite = WorldController.Instance.spriteController.GetSprite(spriteName);
    Image img = btn.GetComponentsInChildren<Image>().Where(e => e.transform.parent != btn.transform.parent).First(); //get the image from a child, not the current object
    if (img != null) {
      if (sprite == null || spriteName == null) {
        img.color = Color.clear;
      } else {
        img.sprite = sprite;
      }
    } else {

    }
  }


}

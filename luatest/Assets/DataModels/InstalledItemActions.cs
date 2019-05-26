using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class InstalledItemActions {

  public static void Door_UpdateActions(InstalledItem item, float deltaTime) {

    float opentime = item.itemParameters.GetFloat("opentime", 0.25f);

    if (item.itemParameters.GetBool("opening")) {
      item.itemParameters.IncFloat("openness", deltaTime / opentime);
    } else {
      item.itemParameters.IncFloat("openness", -(deltaTime / opentime));
    }
    float f = item.itemParameters.GetFloat("openness");
    f = Mathf.Clamp01(f);
    item.itemParameters.SetFloat("openness", f);

    if (item.itemParameters.GetFloat("openness") >= 1) {
      item.itemParameters.SetBool("opening", false);
    }
    //Debug.Log("Update door " + item.itemParameters.ToString());
    if (item.cbOnChanged != null) {
      item.cbOnChanged(item);
    }

  }

  public static Tile.CAN_ENTER Door_EnterRequested(InstalledItem item) {
    item.itemParameters.SetBool("opening", true);
    if (item.itemParameters.GetFloat("openness") >= 1) {
      return Tile.CAN_ENTER.YES;
    } else {
      return Tile.CAN_ENTER.SOON;
    }


    
    
  }
}


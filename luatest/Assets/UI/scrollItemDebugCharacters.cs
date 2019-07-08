using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using NoYouDoIt.DataModels;

public class scrollItemDebugCharacters : MonoBehaviour {

  public Text txtName;
  public Text txtJob;
  public Text txtPos;
  public Text txtInventory;
  // Start is called before the first frame update
  void Start() {

  }


  public void Set(Entity chr) {
    //txtName.text = chr.name + " " + chr.state.ToString();
    //txtJob.text = "";
    //if (chr.myJob != null) {
    //  txtJob.text = chr.myJob.description + " " + (chr.myJob.inventory != null ? chr.myJob.inventory.ToString() : "") + "\n" + chr.myJob.ToString();

    //}

    //txtInventory.text = chr.inventory.ToString();
    //txtPos.text = (int)chr.X + "," + (int)chr.Y;
    
  }
}

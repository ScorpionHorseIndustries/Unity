using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using NoYouDoIt.DataModels;

public class scrJobScrollItem : MonoBehaviour {

  public Button btnCancel;
  public Text nameText;
  public Text typeText;
   //job;
  // Start is called before the first frame update
  void Start() {

  }
  public void Setup(WorkItem job) {
    //this.job = job;
    //nameText.text = job.jobType.ToString() + " " + job.age + " p:" + job.priority + "(" + job.minPriority + "," + job.maxPriority + ")";
    //switch (job.jobType) {
    //  case JOB_TYPE.NONE:
    //    break;
    //  case JOB_TYPE.BUILD:
    //    typeText.text = job.recipe.name + " (" + job.tile.world_x + "," + job.tile.world_y + ")";
    //    break;
    //  case JOB_TYPE.HAUL:
    //    typeText.text = job.recipeResouceQty + " of " + job.recipeResourceName + " to (" + job.tile.world_x + "," + job.tile.world_y + ")";
    //    break;
    //  default:
    //    break;
    //}
  

  }
}

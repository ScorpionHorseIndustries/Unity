using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NoYouDoIt.Controller {
  using NoYouDoIt.Controller;
  using NoYouDoIt.DataModels;
  using NoYouDoIt.TheWorld;
  using NoYouDoIt.Utils;
  public class WorkItemManager {

    Action<WorkItem> CBWorkCreated;


    private List<WorkItem> workItems = new List<WorkItem>();

    public WorkItemManager() {



    }

    public void Update() {

    }

    public void AddWorkItem(WorkItem item) {
      Debug.Log(item);
      workItems.Add(item);

    }

    public WorkItem GetNearestWork(Tile posTile) {
      WorkItem item = null;
      float shortestDist = 0;
      for (int i = workItems.Count-1; i >= 0; i -= 1) {
        WorkItem w = workItems[i];
        float dist = Funcs.TaxiDistance(w.workTile, posTile);
        if (item == null || dist < shortestDist) {
          item = w;
          shortestDist = dist;

        }


      }

      return item;

    }


    public void CBRegisterOnCreated(Action<WorkItem> cb) {
      CBWorkCreated -= cb;
      CBWorkCreated += cb;
    }

    public void CBUnregisterOnCreated(Action<WorkItem> cb) {
      CBWorkCreated -= cb;
    }


  }


}

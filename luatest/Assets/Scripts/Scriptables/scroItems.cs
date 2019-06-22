using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace NoYouDoIt.Scripts {
  public class scroItems : ScriptableObject {

    private List<scrInventory> Inventories = new List<scrInventory>();
    private List<string> Items = new List<string>();
    private static scroItems _inst;
    public static scroItems Instance {
      get {
        if (!_inst) {
          _inst = FindObjectOfType<scroItems>();
        }

        if (!_inst) {
          _inst = CreateInstance<scroItems>();
        }
        return _inst;
      }
    }

    public void registerInventory(scrInventory inv) {
      if (!Inventories.Contains(inv)) {
        Inventories.Add(inv);
      }
    }

    public void deregisterInventory(scrInventory inv) {
      if (Inventories.Contains(inv)) {
        Inventories.Remove(inv);
      }
    }

    private void Awake() {
      Items.Add("item::iron_plate");
      Items.Add("item::iron_sheet");
      Items.Add("item::iron_screw");
      Items.Add("item::wooden_log");
      Items.Add("item::wooden_plank");
      Items.Add("item::wooden_dowel");

    }

    public string GetItem(string item) {
      if (Items.Contains(item)) {
        return item;
      } else {
        return null;
      }

    }

    public List<scrInventory> FindItem(string item, int qty) {
      List<scrInventory> list = new List<scrInventory>();
      foreach (scrInventory inv in Inventories) {
        if (inv.searchable) {
          int rq = CheckItem(inv, item, qty);
          if (rq > 0) {
            list.Add(inv);
          }
        }
      }


      return list;
    }

    public int CheckItem(scrInventory inv, string item, int qty) {

      int rq = 0;


      if (inv.contents.ContainsKey(item)) {
        int q = inv.contents[item];
        if (q >= qty) {
          rq = qty;
        } else {
          rq = q;
        }
      }

      return rq;

    }




    public void GetRandomItemList(int HowMany, Dictionary<string, int> dct) {
      string[] ItemsArr = Items.ToArray();
      for (int i = 0; i < HowMany; i += 1) {
        int qty = (int)Random.Range(1, 5);
        int j = (int)Random.Range(0, ItemsArr.Length);
        string item = ItemsArr[j];
        if (dct.ContainsKey(item)) {
          int q = dct[item];
          q += qty;
          dct[item] = q;
        } else {
          dct.Add(item, qty);
        }


      }

    }

  }

}

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class ScriptableParent : ScriptableObject
{
	private static ScriptableParent _inst;
	public static ScriptableParent Instance {
		get {
			if (!_inst)
			{
				_inst = FindObjectOfType<ScriptableParent>();
			}

			if (!_inst)
			{
				_inst = CreateInstance<ScriptableParent>();
			}
			return _inst;
		}
	}
}
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoYouDoIt.Utils {
  public class ItemParameters {
    private Dictionary<string, string> items;
    //private Dictionary<string, List<string>> itemArrays;


    public ItemParameters(ItemParameters o) {
      this.items = o.GetItems();
    }

    public ItemParameters() {
      items = new Dictionary<string, string>();
    }

    public override string ToString() {
      string s = "";
      foreach (string k in items.Keys) {
        s += k + "=" + items[k] + ",";
      }

      return s;
    }

    public bool HasProperty(string propertyName) {
      return items.ContainsKey(propertyName);
    }


    public Dictionary<string, string> GetItems() {
      Dictionary<string, string> nitems = new Dictionary<string, string>();

      foreach (string s in items.Keys) {
        nitems[s] = items[s];
      }

      return nitems;
    }

    public void Remove(string name) {
      if (items.ContainsKey(name)) {
        items.Remove(name);
      }
    }


    public void SetBool(string name, bool value) {
      items[name] = value.ToString();
    }

    public void SetFloat(string name, float value) {
      items[name] = value.ToString();
    }

    public void SetInt(string name, int value) {
      items[name] = value.ToString();
    }


    public void SetString(string name, string value) {
      Set(name, value);
    }
    public void Set(string name, string value) {
      items[name] = value;
    }

    public void IncFloat(string name, float value) {
      if (!items.ContainsKey(name)) {
        SetFloat(name, value);
      } else {
        SetFloat(name, value + GetFloat(name));
      }

    }

    public void IncInt(string name, int value) {
      if (!items.ContainsKey(name)) {
        SetInt(name, value);
      } else {
        SetInt(name, value + GetInt(name));
      }

    }

    public float GetFloat(string name) {
      return GetFloat(name, 0.0f);
    }

    public float GetFloat(string name, float df) {
      float f = df;
      if (items.ContainsKey(name)) {
        if (float.TryParse(items[name], out f)) {
          return f;
        }
      } else {
        SetFloat(name, df);
      }

      return f;

    }

    public int GetInt(string name) {
      return GetInt(name, 0);

    }

    public int GetInt(string name, int df) {
      int f = df;
      if (items.ContainsKey(name)) {
        if (int.TryParse(items[name], out f)) {
          return f;
        }
      } else {
        SetInt(name, df);
      }

      return f;

    }

    public bool GetBool(string name) {
      return GetBool(name, false);
    }

    public bool GetBool(string name, bool df) {
      bool f = df;
      if (items.ContainsKey(name)) {
        if (bool.TryParse(items[name], out f)) {
          return f;
        }
      } else {
        SetBool(name, df);
      }

      return f;

    }

    public string GetString(string name) {
      return GetString(name, null);
    }

    public string GetString(string name, string df ) {

      if (items.ContainsKey(name)) {

        return items[name];

      } else {
        if (df != null) {
          Set(name, df);
        }
      }

      return df;
    }
  }
}
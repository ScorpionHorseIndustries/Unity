using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoYouDoIt.Utils {
  public class LuaFunctionList {
    private List<string> functions = new List<string>();

    public void Add(string s) {
      if (!functions.Contains(s)) {
        functions.Add(s);
      }

    }

    public override string ToString() {
      string output = "";

      foreach(string s in functions) {
        output += " " + s;
      }


      return output;
    }

    public void Remove(string s) {
      if (functions.Contains(s)) {
        functions.Remove(s);
      }
    }

    public string[] ToArray() {
      return functions.ToArray();
    }

    



  }
}

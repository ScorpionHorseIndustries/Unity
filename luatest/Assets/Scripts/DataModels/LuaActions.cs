using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NLua;

public class LuaActions {

  private static Lua lua; 
  
  public static void Init() {
    lua = new Lua();
    lua.LoadCLRPackage();
    
    
  }

  public static string LuaString(string code) {
    Debug.Log("lua code: " + code);
    System.Object[] result = lua.DoString(code);

    
    foreach (System.Object o in result) {
      Debug.Log(o.ToString());
    }

    return result[0].ToString();
  }


    
  
}

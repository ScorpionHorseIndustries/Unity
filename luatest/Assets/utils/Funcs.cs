using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
public static class Funcs 
{
	public static string getSpriteName(string name) {
		name = name.Replace(':', '_');



		return name;
	}

  public static string jsonGetString(JToken token, string df) {
    
    if (token == null) {
      return df; 
    } else {
      return (string)token;
    }
  }

  public static bool jsonGetBool(JToken token, bool df) {
    if (token == null) {
      return df;
    } else {
      return (bool)token;
    }
  }

  public static int jsonGetInt(JToken token, int df) {
    if (token == null) {
      return df;
    } else {
      return (int)token;
    }
  }

  public static float jsonGetFloat(JToken token, float df) {
    if (token == null) {
      return df;
    } else {
      return (float)token;
    }
  }


}

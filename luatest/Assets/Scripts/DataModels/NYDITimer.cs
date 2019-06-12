using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class NYDITimer {
  float time;
  float initialTime;
  string name;
  Action func;

  public NYDITimer(string name, float initialTime, Action func) {
    this.func += func;
    this.name = name;
    this.initialTime = initialTime;
    this.time = initialTime;
  }

  public void update(float deltaTime) {
    time -= deltaTime;
    if (time < 0) {
      time += initialTime;
      if (func != null) {
        func();
      }
    }

  }



}


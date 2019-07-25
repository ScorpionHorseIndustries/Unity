using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoYouDoIt.Utils {
  public class NYDIList<T> {
    private List<T> list;
    private int incIndex = -1;
    public bool overflow { get; private set; } = false;
    private T current;
    public NYDIList() {
      list = new List<T>();
    }

    public void Add(T t) {
      if (!list.Contains(t)) {
        list.Add(t);
      }

    }

    public void Remove(T t) {
      if (list.Contains(t)) {
        list.Remove(t);
      }
    }

    public T Get(int index) {
      return list[index];
    }

    public T GetCurrent() {
      return current;
    }
    public T GetNext() {
      if (list.Count > 0) {
        overflow = false;
        incIndex += 1;
        if (incIndex >= list.Count) {
          overflow = true;
          incIndex = 0;
        }
        current = Get(incIndex);
        return current;

      } else {
        return default(T);
      }



    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoYouDoIt.Utils {
  class Heap<T> where T : IHeapItem<T> {
    T[] items;
    int currentItemCount;

    public Heap(int maxHeapSize) {
      items = new T[maxHeapSize];


    }

    public T RemoveFirst() {
      T firstItem = items[0];
      currentItemCount -= 1;

      items[0] = items[currentItemCount];
      items[0].HeapIndex = 0;
      SortDown(items[0]);
      return firstItem;
    }

    public void Add(T item) {
      item.HeapIndex = currentItemCount;
      items[currentItemCount] = item;
      SortUp(item);
      currentItemCount += 1;
    }

    void SortDown(T item) {
      while(true) {
        int childLeft = item.HeapIndex * 2 + 1;
        int childRight = item.HeapIndex * 2 + 2;

        int swapIndex = 0;

        if (childLeft < currentItemCount) {
          swapIndex = childLeft;

          if (childRight < currentItemCount) {
            if (items[childLeft].CompareTo(items[childRight]) < 0) {
              swapIndex = childRight;

            }
          }

          if (item.CompareTo(items[swapIndex]) < 0) {
            Swap(item, items[swapIndex]);
          } else {
            return;
          }


        } else {
          return;
        }
      }

    }


    public bool Contains(T item) {
      return Equals(items[item.HeapIndex], item);
    }

    public int Count {
      get {
        return currentItemCount;
      }
    }

    public void UpdateItem(T item) {
      SortUp(item);
    }

    void SortUp(T item) {

      int parentIndex = PI(item);
      while(true) {
        T parentItem = items[parentIndex];

        if (item.CompareTo(parentItem) > 0) {
          Swap(item, parentItem);
        } else {
          break;
        }
        parentIndex = PI(item);
      }

    }

    void Swap(T A, T B) {
      items[A.HeapIndex] = B;
      items[B.HeapIndex] = A;

      int tempA = A.HeapIndex;
      A.HeapIndex = B.HeapIndex;
      B.HeapIndex = tempA;
    }

    int PI(T item) {
      return (item.HeapIndex - 1) / 2;

    }
  }

  public interface IHeapItem<T> : IComparable<T> {
    int HeapIndex { get; set; }


  }
}

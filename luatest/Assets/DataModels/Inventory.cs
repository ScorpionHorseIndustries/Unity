using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using UnityEngine;


public enum INVENTORY_TYPE {
  NONE, TILE, JOB, CHARACTER, INSTALLED_ITEM, INVENTORY_ITEM
}

public class Inventory {

  public Job job;
  public Tile tile;
  public Character character;
  public System.Object parent;
  private World world;


  public INVENTORY_TYPE type { get; private set; } = INVENTORY_TYPE.NONE;
  public int numSlots { get; protected set; }
  List<InventorySlot> slots;
  private Dictionary<string, int> restrictions;
  public Inventory(World world, int numSlots, INVENTORY_TYPE type, System.Object parent) {
    this.world = world;
    this.type = type;
    this.numSlots = numSlots;
    slots = new List<InventorySlot>();
    restrictions = new Dictionary<string, int>();
    this.parent = parent;

    if (parent.GetType() ==  typeof(Job)) {
      this.job = (Job)parent;
    } else if (parent.GetType() == typeof(Tile)) {
      this.tile = (Tile)parent;
    } else if (parent.GetType() == typeof(Character)) {
      this.character= (Character)parent;
    }
    world.inventoryManager.RegisterInventory(this);

  }
  public string SpriteName() {
    if (slots.Count > 0) {
      return InventoryItem.GetPrototype(slots[0].type).spriteName;
    }

    return "none";
  }

  public override string ToString() {
    string output = "";
    int c = 0;
    foreach (InventorySlot slot in slots) {
      if (c > 0) {
        output += ",";
      }
      output += slot.type + "(" + slot.qty + ")";
      c += 1;
    }

    return output;
  }

  public void AddRestriction(string type, int qty) {
    restrictions[type] = qty;
  }

  public int AddItem(string type, int qtyOffered) {

    if (restrictions.Count > 0) {
      if (!restrictions.ContainsKey(type)) {
        return 0;
      } else {
        int currentQty = HowMany(type);
        int allowedQty = restrictions[type];
        if (currentQty + qtyOffered > allowedQty) {
          qtyOffered = allowedQty - currentQty;
        }
      }

    }


    int qtyAccepted = 0;
    int qtyRemaining = qtyOffered;
    foreach (InventorySlot slot in slots.Where(e => e.type == type)) {
      if (qtyRemaining == 0) break;
      int tempQ = slot.Add(type, qtyRemaining);
      qtyRemaining -= tempQ;

    }

    while (qtyRemaining > 0 && slots.Count < numSlots) {
      int tempQ;
      slots.Add(InventorySlot.NewSlot(type, qtyRemaining, out tempQ));
      qtyRemaining -= tempQ;



    }
    qtyAccepted = qtyOffered - qtyRemaining;

    return qtyAccepted;


  }

  private void ClearEmpty() {
    for (int i = slots.Count - 1; i >= 0; i -= 1) {
      InventorySlot slot = slots[i];
      if (slot.qty == 0) {
        slots.RemoveAt(i);
      }
    }

  }


  public int TotalQty() {
    int total = 0;
    foreach (InventorySlot slot in slots) {
      total += slot.qty;
    }

    return total;
  }

  public bool IsEmpty() {
    ClearEmpty();

    return slots.Count == 0;
  }

  public int HowMany(string type) {
    int qty = 0;
    foreach (InventorySlot slot in slots.Where(e => e.type == type)) {
      qty += slot.qty;
    }

    return qty;
  }

  public string GetFirst() {
    if (slots.Count > 0) {
      return slots[0].type;
    } else {
      return null;
    }
  }

  public int RemoveItem(string type, int qtyRequested) {
    int qtyGiven = 0;
    int qtyRemaining = qtyRequested;
    foreach (InventorySlot slot in slots.Where(e => e.type == type)) {
      if (qtyRemaining == 0) break;
      int tempQ = slot.Remove(type, qtyRemaining);
      qtyRemaining -= tempQ;
    }
    ClearEmpty();


    qtyGiven = qtyRequested - qtyRemaining;


    return qtyGiven;
  }
}

class InventorySlot {
  public string type { get; private set; }
  public int qty { get; private set; }
  public int qtyCap { get; private set; }

  private InventorySlot() {

  }

  

  public int Add(string type, int qtyOffered) {
    if (this.type != type) return 0;

    int qtyAccepted = Mathf.Min(qtyCap - qty, qtyOffered);
    qty += qtyAccepted;
    return qtyAccepted;
  }

  public int Remove(string type, int qtyRequested) {
    int qtyGiven = 0;

    if (qtyRequested > qty) {
      qtyGiven = qty;
      qty = 0;
    } else {
      qtyGiven = qtyRequested;
      qty -= qtyRequested;
    }


    return qtyGiven;
  }

  public static InventorySlot NewSlot(string type, int qty, out int qtyAccepted) {
    InventoryItem proto = InventoryItem.GetPrototype(type);
    qtyAccepted = 0;
    if (proto == null) return null;
    InventorySlot slot = new InventorySlot();
    slot.type = proto.type;
    slot.qtyCap = proto.stackSize;//proto.maxStackSize;
    if (qty > slot.qtyCap) {
      slot.qty = slot.qtyCap;
      qtyAccepted = slot.qty;
    } else {
      slot.qty = qty;
      qtyAccepted = qty;
    }




    return slot;

  }

}


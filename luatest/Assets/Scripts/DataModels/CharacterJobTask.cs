using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NoYouDoIt.TheWorld;


namespace NoYouDoIt.DataModels {
  class CharacterJobTask {

    public Tile tile { get; private set; }
    public string itemName { get; private set; }
    public int qty { get; private set; }
    public float order { get; set; }
    public int state = 0;

    private CharacterJobTask() {

    }
    public static CharacterJobTask PICK_UP_FROM_TILE(Tile tile, string itemName, int qty) {
      CharacterJobTask cjt = new CharacterJobTask();
      cjt.tile = tile;
      cjt.itemName = itemName;
      cjt.qty = qty;

      return cjt;
    }

    public static CharacterJobTask DROP_AT_TILE(Tile tile, string itemName, int qty) {
      CharacterJobTask cjt = new CharacterJobTask();
      cjt.tile = tile;
      cjt.itemName = itemName;
      cjt.qty = qty;

      return cjt;
    }

    public static CharacterJobTask DROP_AT_JOB(Tile tile, string itemName, int qty) {
      CharacterJobTask cjt = new CharacterJobTask();
      cjt.tile = tile;
      cjt.itemName = itemName;
      cjt.qty = qty;

      return cjt;
    }

    public static CharacterJobTask WORK_TILE(Tile tile) {
      CharacterJobTask cjt = new CharacterJobTask();
      cjt.tile = tile;
      return cjt;
    }

    public static CharacterJobTask GO_TO(Tile tile) {
      CharacterJobTask cjt = new CharacterJobTask();
      cjt.tile = tile;
      return cjt;
    }

    public static CharacterJobTask FIND_INVENTORY_ITEM() {
      return null;

    }

  }

}
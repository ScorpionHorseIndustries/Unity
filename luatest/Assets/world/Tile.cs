using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Tile {
	//public enum TYPE {DIRT,GRASS,EMPTY,WALL }
	Action<Tile> cbChanged;

	//public Tile.TYPE type { get { return type; } set { type = value; if (cbTypeChanged != null) cbTypeChanged(this); } }
	public TileType type { get; private set; }
	public LooseItem looseItem { get; private set; }
	public InstalledItem installedItem { get; private set; }

	public int x { get; private set; }
	public int y { get; private set; }
	public World world { get; private set; }

	public bool pendingJob = false;

	public Tile(World world, TileType type, int x, int y)
	{
		this.type = type;
		this.x = x;
		this.y = y;
		this.world = world;

	}

	public override String ToString()
	{
    return "tile: " + type + " (" + x + "," + y + ") pendingJob:" + pendingJob + " hasInstalled: " + (installedItem != null) + " hasLoose: " + (looseItem != null);
	}

	public void cbRegisterOnChanged(Action<Tile> cb)
	{
		cbChanged += cb;
	}

	public void cbUnregisterOnChanged(Action<Tile> cb) {
		cbChanged -= cb;
	}

	public TileType getType ()
	{
		return type;
	}

	public void SetType(TileType t)
	{
		type = t;
		if (cbChanged != null)
		{
			cbChanged(this);
		}
	}

	public bool placeInstalledObject(InstalledItem instobj) {
		if (instobj == null) {
			this.installedItem = null;
			return true;
		}
		if (this.installedItem == null) {
			this.installedItem = instobj;
			return true;
		} else {
			return false;
		}
	}


    
}

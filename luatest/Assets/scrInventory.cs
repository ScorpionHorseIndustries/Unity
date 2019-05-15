using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scrInventory : MonoBehaviour
{

	scroItems Items;
	[SerializeField]
	public int capacity = 1;
	public bool input = false; //if false only the owner can add items
	public bool output = false; //if false on the owner can remove items
	public bool searchable = false;

	private int ownerId;
	
	
	[SerializeField]
	public Dictionary<string, int> contents = new Dictionary<string, int>();
    // Start is called before the first frame update
    void Start()
    {
		Items = scroItems.Instance;
		Items.registerInventory(this);
    }

	private void OnEnable()
	{
		
	}

	private void OnDestroy()
	{
		Items.deregisterInventory(this);
	}

	public void SetOwnerId(int ownerId)
	{
		this.ownerId = ownerId;
	}

	// Update is called once per frame
	void Update()
    {
        
    }

	public int CheckItem(int requesterId, string item, int qty)
	{
		
		int rq = 0;
		if (!searchable && ownerId != requesterId) return 0;

		if (contents.ContainsKey(item))
		{
			int q = contents[item]; 
			if (q >= qty)
			{
				rq = qty;
			} else
			{
				rq = q;
			}
		}

		return rq;

	}

	int RemoveItem(int requesterId, string item, int qty)
	{
		if (!output && ownerId != requesterId) return 0;
		int qtyRemoved = 0;

		if (contents.ContainsKey(item))
		{
			int q = contents[item];
			if (q >= qty)
			{
				qtyRemoved = qty;
			} else
			{
				qtyRemoved = q;
			}
			if (q - qtyRemoved == 0)
			{
				contents.Remove(item);
			} else
			{
				contents[item] = q - qtyRemoved;
			}
		}


		return qtyRemoved;

	}


	bool AddItem(int requesterId, string item, int qty)
	{

		if (!input && ownerId != requesterId) return false;

		if (contents.ContainsKey(item))
		{
			int q = contents[item];
			q += qty;
			contents[item] = q;
		} else
		{
			if (contents.Count >= capacity)
			{
				return false;
			} else
			{
				contents.Add(item, qty);
			}
		}
		return true;

	}
}

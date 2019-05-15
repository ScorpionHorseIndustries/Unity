using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NLua;

public class scrRobot : MonoBehaviour
{
	Lua lua;
	[SerializeField]
	scrInventory inventory;
	Vector2 targetPos;
	List<string> instructions = new List<string>();
	string currentInstruction;
	int index = -1;
    // Start is called before the first frame update
    void Start()
    {
		lua = new Lua();
		lua.LoadCLRPackage();

		inventory = GetComponent<scrInventory>();
		if (inventory != null)
		{
			inventory.SetOwnerId(GetInstanceID());
		}
        
    }

	// Update is called once per frame
	void Update()
	{
		if (index == -1)
		{
			if (instructions.Count > 0)
			{
				index = 0;
				
			}
		}

		if (currentInstruction == "")
		{
			if (index >= 0 && index <= instructions.Count - 1)
			{

				currentInstruction = instructions[index];


			}
		}

		
        
    }

	void FindPathToPos()
	{


	}

	void FindItem()
	{

	}

	void GrabFromInventory()
	{

	}

	void PutInInventory()
	{

	}

	




}

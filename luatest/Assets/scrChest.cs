using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NoYouDoIt.Scripts;
public class scrChest : MonoBehaviour
{
	scrInventory inventory;
	scroItems Items;
    // Start is called before the first frame update
    void Start()
    {
		inventory = GetComponent<scrInventory>();
		inventory.SetOwnerId(GetInstanceID());
		Items = scroItems.Instance;
		Items.GetRandomItemList(10, inventory.contents);
		
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

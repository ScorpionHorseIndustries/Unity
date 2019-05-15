using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolTip : MonoBehaviour
{
	[SerializeField]
	public string Name = "Tooltip";
	[SerializeField]
	public int Width = 1;
	[SerializeField]
	public int Height = 1;
	public TextAsset Text;
	



	// Start is called before the first frame update
	void Start()
	{


	}

	// Update is called once per frame
	void Update()
	{
		/*
		Vector2 mp = Camera.current.ScreenToWorldPoint(Input.mousePosition);

		float tx = transform.position.x;
		float ty = transform.position.y;

		if (mp.x >= tx && mp.x < tx + Width && mp.y >= ty && mp.y < ty + Height)
		{

		}
		*/

	}
}

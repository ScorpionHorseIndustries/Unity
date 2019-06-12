using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AutomaticResize : MonoBehaviour
{

	public float childHeight = 35;
	RectTransform rect;
    // Start is called before the first frame update
    void Start()
    {
		rect = this.GetComponent<RectTransform>();
		AdjustSize();
        
    }

    // Update is called once per frame
    void Update()
    {
		//AdjustSize();
        
    }


	public void AdjustSize()
	{
		Vector2 size = rect.sizeDelta;
		size.y = this.transform.childCount * childHeight;
		rect.sizeDelta = size;
	}
}

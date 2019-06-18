using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
public class PaletteSwapLookup : MonoBehaviour
{
	public Texture LookupTexture;

	Material _mat;

	void OnEnable()
	{
		Shader shader = Shader.Find("2D/PaletteSwapLookup");
		if (_mat == null)
			_mat = new Material(shader);
    }

	void OnDisable()
	{
		if (_mat != null)
			DestroyImmediate(_mat);
	}

	void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
    Debug.Log("hammers");
		_mat.SetTexture("_PaletteTex", LookupTexture);
		Graphics.Blit(src, dst,  _mat);
	}


}
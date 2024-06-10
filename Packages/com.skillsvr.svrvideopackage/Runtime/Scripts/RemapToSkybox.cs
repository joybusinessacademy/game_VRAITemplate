using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemapToSkybox : MonoBehaviour
{
	public Material avproMaterial;
	public Material skyboxMaterial;

	private void Update()
	{
		 skyboxMaterial.mainTexture = avproMaterial.mainTexture; 
	}
}

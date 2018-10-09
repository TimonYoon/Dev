using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SceneLightSetting : MonoBehaviour {

	public Color ambientLight = Color.black;
	public float ambientIntensity = 1f;


	#if UNITY_EDITOR
	void Update () 
	{
		RenderSettings.ambientLight = ambientLight;
		RenderSettings.ambientIntensity = ambientIntensity;
	}
	#endif


}

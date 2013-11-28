using UnityEngine;
using System.Collections;

public class DynamicIBL : MonoBehaviour {

	public Camera[] cameras;

	public Color ambientColor;
	public RenderTexture ambientMap;
	[Range(0.1f,3.0f)]
	public float ambientPower = 1;

	public Color rimColor;
	[Range(0.1f,3.0f)]
	public float rimPower = 0;

	public Cubemap reflectionMap;
	public Color reflectionTint;

	public Color specularTint;

	public Texture2D rampTex;
	public Color rampTint;

	void Start () {
		ambientMap.isCubemap = true;
	}
	
	void LateUpdate () {

		RenderSettings.ambientLight = Color.white;

		Shader.SetGlobalTexture("_RampTex", rampTex);
		Shader.SetGlobalColor("_RampTint", rampTint);

		Shader.SetGlobalColor("_SpecularTint", specularTint);

		Shader.SetGlobalColor("_ReflectionTint", reflectionTint);
		Shader.SetGlobalTexture("_ReflectionMap", reflectionMap);

		Shader.SetGlobalColor("_RimColor", rimColor);
		Shader.SetGlobalFloat("_RimPower", rimPower);

		Shader.SetGlobalColor("_AmbientColor", ambientColor);
		Shader.SetGlobalFloat("_AmbientPower", ambientPower);

		for (var i=0; i<6; ++i) {
			cameras[i].RenderToCubemap(ambientMap, 1 << i);
		}
		ambientMap.SetGlobalShaderProperty("_AmbientMap");
	}
}


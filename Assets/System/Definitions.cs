using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Definitions : MonoBehaviour {

	static Definitions instance;
	public static Definitions Get () {
		return instance;
	}

	public string path = "http://dl.dropboxusercontent.com/u/10592653/eve";

	public List<string> loadedDefs = new List<string>();

	public Dictionary<string,object> definitions = new Dictionary<string,object>();

	void Awake () {
		instance = this;
	}

	void Start () {
	}

	public IEnumerator LoadDefinitions () {

		string defName = string.Empty;

		defName = "WallStructure";
		yield return StartCoroutine( Requestor.ForJSON( path + "/" + defName + ".json", ( json ) => {
			var obj = MiniJSON.Json.Deserialize( json );
			definitions[defName] = obj;
			loadedDefs.Add( defName );
		} ) );

		yield return null;
	}
	
}

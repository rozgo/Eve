using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Definitions : MonoBehaviour {

	static Definitions instance;
	public static Definitions Get () {
		if ( instance == null ) {
			var defObj = new GameObject( "Definitions" );
			defObj.AddComponent<Definitions>();
			Object.DontDestroyOnLoad( defObj );
		}
		return instance;
	}

	public string path = "http://dl.dropboxusercontent.com/u/10592653/eve/Definitions";

	public List<string> loadedDefs = new List<string>();

	public Dictionary<string,object> defsByName = new Dictionary<string,object>();
	public Dictionary<int,object> defsById = new Dictionary<int,object>();

	void Awake () {
		instance = this;
	}

	void Start () {
	}

	public IEnumerator LoadDefinitions () {
		foreach ( var defFile in defFiles ) {
			yield return StartCoroutine( Requestor.ForJSON( path + "/" + defFile + ".json", ( json ) => {
				try {
					var defs = ( List<object> )MiniJSON.Json.Deserialize( json );
					foreach ( Dictionary<string,object> def in defs ) {
						var defName = def["NameInternal"] as string;
						var defId = ( int )System.Convert.ChangeType( def["Id"], typeof( int ) );
						defsByName[defName] = def;
						defsById[defId] = def;
						loadedDefs.Add( defId.ToString( "00000" ) + " " + defFile + "/" + defName );
					}
				}
				catch ( System.Exception e ) {
					Debug.LogError( defFile );
					Debug.LogError( e );
				}
			}));
		}
	}

	string[] defFiles = {
		//"AdsConfiguration",
		"AmmoDefinitions",
		//"AnimationSetDefinitions",
		//"AwaitDropDefinitions",
		"BuildingDefinitions",
		//"CameraCustomizationConfig",
		//"CameraOrthoConfig",
		//"CameraPerspectiveConfig",
		//"DefenseBuildingDefinitions",
		"HeroDefinitions",
		//"HeroSlotDefinitions",
		//"HeroTree",
		//"InAppProductDefinitions",
		"MissionDefinitions",
		//"QuestDefinitions",
		//"RepairPurchaseDefinition",
		//"SupplyDropDefinitions",
		//"TrapDefinitions",
		//"UnitDefinitions",
		//"UpgradeDefinitions",
		//"UpgradeTree",
		//"UserDefinition",
		"WallDefinitions",
		//"WarCacheDefinitions",
		//"WeaponDefinitions",
		//"WorldMapObjectDefinitions",
	};
	
}

public class Def {

	Dictionary<string,object> defsByName;

	public Def () {
		defsByName = Definitions.Get().defsByName;
	}

	public Def ( Dictionary<string,object> defs ) {
		defsByName = defs;
	}

	public IEnumerable<T> Values< T > ( string defName, string defKey ) {
		try {
			var def = ( Dictionary<string,object> )( defsByName[defName] );
			var objects = ( ( List<object> )( def[defKey] ) );
			var values = objects.Select( obj => (T)System.Convert.ChangeType( obj, typeof( T ) ) );
			values.FirstOrDefault();
			return values;
		}
		catch ( System.Exception e ) {
			Debug.LogError( defName );
			Debug.LogError( defKey );
		}
		return new List<T>();
	}

	public T Value< T > ( string defName, string defKey, T defValue ) {
		try {
			var def = ( Dictionary<string,object> )( defsByName[defName] );
			var value = (T)System.Convert.ChangeType( def[defKey], typeof( T ) );;
			return value;
		}
		catch ( System.Exception e ) {
			Debug.LogError( defName );
			Debug.LogError( defKey );
		}
		return defValue;
	}

}


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

    public Dictionary<string, object> defsByName = new Dictionary<string, object>();
    public Dictionary<int, object> defsById = new Dictionary<int, object>();

    void Awake () {
        instance = this;
    }

    void Start () {
    }

    public IEnumerator LoadDefinitions ( System.Action onDone ) {
        foreach ( var defFile in defFiles ) {
            yield return StartCoroutine( Requestor.ForJSON( path + "/" + defFile + ".json", ( json ) => {
                try {
                    Debug.Log( path + "/" + defFile + ".json" );
                    var defs = ( List<object> )MiniJSON.Json.Deserialize( json );
                    foreach ( Dictionary<string, object> def in defs ) {
                        var defName = def["NameInternal"] as string;
                        var defId = ( int )System.Convert.ChangeType( def["Id"], typeof( int ) );
                        defsByName[defName] = def;
                        defsById[defId] = def;
                        loadedDefs.Add( defId.ToString( "00000" ) + " " + defFile + "/" + defName );
                    }
                } catch ( System.Exception e ) {
                    Debug.LogError( defFile );
                    Debug.LogError( e );
                }
            } ) );
        }
        onDone();
        //StartCoroutine( LoadBlocks( "http://dl.dropboxusercontent.com/u/10592653/eve/UnitTestBlocks.json" ) );
        //StartCoroutine( LoadMission( "Normandy, Alaska" ) );

        // Normandy, Alaska
    }

    string[] defFiles = {
        //"StructureDefinitions",
        //"AdsConfiguration",
        //"AmmoDefinitions",
        //"AnimationSetDefinitions",
        //"AwaitDropDefinitions",
        "BuildingDefinitions",
        //"CameraCustomizationConfig",
        //"CameraOrthoConfig",
        //"CameraPerspectiveConfig",
        "DefenseBuildingDefinitions",
        //"HeroDefinitions",
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


    public IEnumerator LoadBlocks ( string url ) {
        yield return StartCoroutine( Requestor.ForJSON( url, ( json ) => {
            var obj = MiniJSON.Json.Deserialize( json );
            Dynamic.For<Dictionary<string, object>>( obj, blockTypes => {
                var createdBlocks = new List<Blocks.Block>();
                foreach ( var blockType in blockTypes.Keys ) {
                    Dynamic.For<Dictionary<string, object>>( blockTypes[blockType], blocks => {
                        foreach ( var blockId in blocks.Keys ) {
                            Dynamic.For<Dictionary<string, object>>( blocks[blockId], record => {
                                var entityId = record["entity"] as string;
                                var entity = GameObject.Find( entityId ) as GameObject;
                                if ( entity == null ) {
                                    entity = new GameObject( entityId );
                                }
                                var block = entity.AddComponent( blockType as string ) as Blocks.Block;
                                createdBlocks.Add( block );
                                block.OnDataBind();
                                block.OnSource( blockId as string, record );
                            } );
                        }
                    } );
                }
                foreach ( var createdBlock in createdBlocks ) {
                    createdBlock.OnSetup();
                }
            } );
        } ) );
    }

    public IEnumerator LoadMission ( string mission, System.Action onDone ) {

        var missionDef  = ( Dictionary<string, object> )defsByName[mission];

        var instancePath = missionDef["WallInstancePath"] as string;
        //Debug.Log( wallsPath );

        string baseUrl = "http://dl.dropboxusercontent.com/u/10592653/eve";

        yield return StartCoroutine( Requestor.ForJSON( baseUrl + "/" + instancePath + ".json", ( json ) => {

            var defs = MiniJSON.Json.Deserialize( json );
            foreach ( Dictionary<string, object> def in ( List<object> )defs ) {
                Debug.Log( Def.Value<int>( def["Level"], 0 ) );

                var entityObject = new GameObject( Def.Value<string>( def["InstanceId"], "BadInstance" ) );
                var healthBlock = entityObject.AddComponent<Blocks.Health>();
                healthBlock.OnDataBind();
                var levelBlock = entityObject.AddComponent<Blocks.Level>();
                levelBlock.OnDataBind();
                var wallBlock = entityObject.AddComponent<Blocks.WallStructure>();
                wallBlock.OnDataBind();

                wallBlock.xPos.Set( Def.Value<int>( def["X"], 0 ) );
                wallBlock.yPos.Set( Def.Value<int>( def["Y"], 0 ) );
                wallBlock.zPos.Set( Def.Value<int>( def["Z"], 0 ) );

                wallBlock.typeId.Set( Def.Value<int>( def["TypeId"], 0 ) );

                levelBlock.level.Set( Def.Value<int>( def["Level"], 1 ) );

                healthBlock.OnSetup();
                levelBlock.OnSetup();
                wallBlock.OnSetup();

                wallBlock.OnLoad();
            }

        } ) );

        instancePath = missionDef["BuildingInstancePath"] as string;

        yield return StartCoroutine( Requestor.ForJSON( baseUrl + "/" + instancePath + ".json", ( json ) => {

            var defs = MiniJSON.Json.Deserialize( json );
            foreach ( Dictionary<string, object> def in ( List<object> )defs ) {
                Debug.Log( Def.Value<int>( def["Level"], 0 ) );

                var entityObject = new GameObject( Def.Value<string>( def["InstanceId"], "BadInstance" ) );
                var healthBlock = entityObject.AddComponent<Blocks.Health>();
                healthBlock.OnDataBind();
                var levelBlock = entityObject.AddComponent<Blocks.Level>();
                levelBlock.OnDataBind();

                var buildingBlock = entityObject.AddComponent<Blocks.BuildingStructure>();
                buildingBlock.OnDataBind();

                buildingBlock.xPos.Set( Def.Value<int>( def["X"], 0 ) );
                buildingBlock.yPos.Set( Def.Value<int>( def["Y"], 0 ) );
                buildingBlock.zPos.Set( Def.Value<int>( def["Z"], 0 ) );

                buildingBlock.typeId.Set( Def.Value<int>( def["TypeId"], 0 ) );

                levelBlock.level.Set( Def.Value<int>( def["Level"], 1 ) );

                healthBlock.OnSetup();
                levelBlock.OnSetup();
                buildingBlock.OnSetup();

                buildingBlock.OnLoad();
            }

        } ) );



        onDone();
    }

}

public class Def {

    Dictionary<string, object> defsByName;

    public Def () {
        defsByName = Definitions.Get().defsByName;
    }

    public Def ( Dictionary<string, object> defs ) {
        defsByName = defs;
    }

    public IEnumerable<T> Values<T> ( string defName, string defKey ) {
        try {
            var def = ( Dictionary<string, object> )( defsByName[defName] );
            var objects = ( ( List<object> )( def[defKey] ) );
            var values = objects.Select( obj => ( T )System.Convert.ChangeType( obj, typeof( T ) ) );
            values.FirstOrDefault();
            return values;
        } catch ( System.Exception e ) {
            Debug.LogError( defName );
            Debug.LogError( defKey );
            Debug.LogError( e );
        }
        return new List<T>();
    }

    public T Value<T> ( string defName, string defKey, T defValue ) {
        try {
            var def = ( Dictionary<string, object> )( defsByName[defName] );
            var value = ( T )System.Convert.ChangeType( def[defKey], typeof( T ) );
            return value;
        } catch ( System.Exception e ) {
            Debug.LogError( defName );
            Debug.LogError( defKey );
            Debug.LogError( e );
        }
        return defValue;
    }

    public static T Value<T> ( object value, T defValue ) {
        try {
            return ( T )System.Convert.ChangeType( value, typeof( T ) );
        } catch ( System.Exception e ) {
            Debug.LogError( e );
        }
        return defValue;
    }

}


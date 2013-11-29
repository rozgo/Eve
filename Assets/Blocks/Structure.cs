using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Blocks {

public class Structure : Block {

    public Field.Number xPos;
    public Field.Number yPos;
    public Field.Number zPos;

    public Field.Number typeId;

    public int width;
    public int length;

    public Level level;
    public string prefab;

    public override void OnDataBind () {
        base.OnDataBind();
        xPos = record.Add<Field.Number>( "X" );
        yPos = record.Add<Field.Number>( "Y" );
        zPos = record.Add<Field.Number>( "Z" );
        typeId = record.Add<Field.Number>( "TypeId" );
        Debug.Log( "Structure OnDataBind" );
    }

    public override void OnSetup () {
        base.OnSetup();
        level = GetComponent<Level>();

        try {

            var def = ( Dictionary<string, object> )Definitions.Get().defsById[typeId.Get<int>()];

            width = ( int )System.Convert.ChangeType( def["Width"], typeof( int ) );
            length = ( int )System.Convert.ChangeType( def["Length"], typeof( int ) );

            var prefabs = ( ( List<object> )def["Prefab"] ).Cast<string>();
            prefab = prefabs.ElementAt( level.level.Get<int>() );

        } catch ( System.Exception e ) {
            Debug.LogError( e );
        }
    }

    public override void OnLoad () {

        Debug.Log( "OnLoad" );
        Debug.Log( prefab );

        if ( prefab == null ) {
            return;
        }

        var building = Instantiator.Instantiate ( prefab, "nuclear stuff", null, transform, null );

        var buildingPathfindingNode = gameObject.AddComponent<PathFindingNode>();
        buildingPathfindingNode.Width = ( uint )width;
        buildingPathfindingNode.Length = ( uint )length;

        Debug.Log( xPos.Get<float>() );

        var pos = new Vector3( xPos.Get<float>(), yPos.Get<float>(), zPos.Get<float>() );
        buildingPathfindingNode.Position = pos;
        transform.position = pos + new Vector3( width * 0.5f * SpaceConversion.MapTileSize,
                                                0, length * 0.5f * SpaceConversion.MapTileSize );

        NavigationSystem.Get().AddGameModel( buildingPathfindingNode );
    }

    public virtual void OnWillMoveTo ( int x, int y ) {
    }

    public virtual void OnDidMoveTo ( int x, int y ) {
    }
}
}

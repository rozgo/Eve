using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Blocks {

public class BuildingStructure : Structure {

    public override void OnDataBind () {
        base.OnDataBind();
    }

    public override void OnSetup () {
        base.OnSetup();

        var def = new Def();

        width = def.Value<int>( "Wall", "Width", 1 );
        length = def.Value<int>( "Wall", "Length", 1 );

        //foreach (var cost in def.Values<int>( "Wall", "Cost" ) ) {
        //  Debug.Log( cost );
        //}

        Debug.Log( typeId.Get<int>() );

        var buildingDef = ( Dictionary<string, object> )Definitions.Get().defsById[typeId.Get<int>()];
        var prefabs = ( ( List<object> )buildingDef["Prefab"] ).Cast<string>();
        prefab = prefabs.ElementAt( level.level.Get<int>() );
        //Debug.Log( prefab );
    }

    void Start () {
    }

    public override void OnWillMoveTo ( int x, int y ) {
    }

    public override void OnDidMoveTo ( int x, int y ) {
    }
}

}

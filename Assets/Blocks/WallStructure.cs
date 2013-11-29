using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Blocks {

    public class WallStructure : Structure {

        public override void OnDataBind () {
            base.OnDataBind();
        }

        public override void OnSetup () {
            base.OnSetup();

            width = Def.Value<int>( "Wall", "Width", 1 );
            length = Def.Value<int>( "Wall", "Length", 1 );

            //foreach (var cost in Def.Values<int>( "Wall", "Cost" ) ) {
            //	Debug.Log( cost );
            //}

            prefab = Def.Values<string>( "Wall", "Prefab" ).ElementAt( level.level.Get<int>() );
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

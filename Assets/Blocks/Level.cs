using UnityEngine;
using System.Collections;

namespace Blocks {

    public class Level : Block {

        public Field.Number level;

        public override void OnDataBind () {
            base.OnDataBind();
            level = record.Add<Field.Number>( "level" );
        }

    }
}

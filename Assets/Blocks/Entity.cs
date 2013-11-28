using UnityEngine;
using System.Collections;

namespace Blocks {

    public class Entity : Block {
        //		public Record<Block>.FieldDef<float> Weight;
        public override void OnStart () {
            base.OnStart();

//            float weight = 0;
//            Weight = record.Field<float>( "weight", () => weight, (v ) => {
//                weight = v;
//            } );
//            Weight.Set( 2.3f );
//            Debug.Log( Weight.Get() );

        }
    }
}

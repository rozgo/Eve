using UnityEngine;
using System.Collections;

namespace Blocks {

    public class Resource : Block {
    	
        public Field.String product;
        public Field.Number amount;
        public Field.Number rate;

        public override void OnDataBind () {
            base.OnDataBind();
            product = record.Add<Field.String>( "product" );
            amount = record.Add<Field.Number>( "amount" );
            rate = record.Add<Field.Number>( "rate" );
        }

        public void Start () {
            //Debug.Log("Resource" + this.GetHashCode());
            //Debug.Log( rate.Get() );
        }

        public float Produce ( float elapsedTime ) {
            var delta = elapsedTime * rate.Get();
            amount.Set( amount.Get() + delta );
            return delta;
        }

        IEnumerator Producing () {
            while ( true ) {
                Produce( 3 );
                yield return new WaitForSeconds( 3 );
            }
        }
    }
}

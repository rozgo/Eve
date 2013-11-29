using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Blocks {

    public class Block : MonoBehaviour {

        public Record record = new Record();

        public virtual void OnDataBind () {
            record.table = Table.Get( this.GetType().ToString() );
        }

        public void OnSource ( string id, Dictionary<string,object> source ) {
            record.id = id;
            record.table.records[id] = record;
            foreach ( var field in record.fields ) {
                object value;
                if ( source.TryGetValue( field.name, out value ) ) {
                    field.Decode( value );
                }
            }
        }

        public virtual void OnSetup () {
        }
    }
}

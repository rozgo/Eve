using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Blocks {

    public class Block : MonoBehaviour {

        public bool inspect = false;
        public Record record = new Record();
        public Dictionary<string,object> definition = new Dictionary<string,object>();

        public virtual void OnDataBind () {
            record.table = Table.Get( this.GetType().ToString() );
            var defName = GetType().ToString().Split('.')[1];
            object defObject;
            if ( Definitions.Get().definitions.TryGetValue( defName, out defObject ) ) {
                Dynamic.For<Dictionary<string,object>>( defObject, def => {
                    definition = def;
                } );
            }
        }

        public void OnSource ( string id, Dictionary<string,object> source ) {
            record.id = id;
            record.table.records[id] = record;
            foreach (var field in record.fields ) {
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

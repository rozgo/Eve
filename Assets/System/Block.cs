using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Blocks {

    public class Block : MonoBehaviour {
        public bool inspect = false;
        public Record record = new Record();

        void Awake () {
            OnSetup();
        }

        public virtual void OnSetup () {
            record.table = Table.Get( this.GetType().ToString() );
        }

        public virtual void OnStart () {
        }

        public virtual void OnUpdate () {
        }

        void Start () {
            OnStart();
        }

        void Update () {
            OnUpdate();
        }

    }
}

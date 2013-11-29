using UnityEngine;
using System.Collections;

namespace Blocks {
    public class Weapon : Block {
        public float MinRange;
        public float MaxRange;
        public int DPS;
        public Health Target;
        public AIFlags TargetFlags;
        public float Range;
        public bool IsReady;
        public bool IsAimed;

        public void Fire()
        {
        }

        public void Fire(Vector3 to)
        {
        }

        public void StopFiring()
        {
        }

    }
}

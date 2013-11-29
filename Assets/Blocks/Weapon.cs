using UnityEngine;
using System.Collections;

namespace Blocks {
    public class Weapon : Block {
        public float MinRange;
        public float MaxRange;
        public float DPS;
        public PathFindingNode Target;
    }
}

using UnityEngine;
using System.Collections;

namespace Blocks {

    public class Hero : Unit {

        public uint TileLength;
        public uint TileWidth;

        public Vector3 GetGotoTargetPosition () {
            //TODO:Set goto target??
            return Vector3.zero;
        }

        public void RemoveFromPlatoon(uint unitId)
        {
            //TODO:Remove unit from platoon
        }
    }
}

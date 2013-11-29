using UnityEngine;
using System.Collections;

namespace Blocks {
    public class PathFindingNode : Block {
        public uint TeamId;
        public float Health;
        public uint TileLength;
        public uint TileWidth;
        public bool AnchoredInScene;
        public int InstanceId;
        public AIFlags AITypeFlags;
        public Vector3 Position;
        public uint Width;
        public uint Length;
        public Vector3 Center;
        public float DamageDoneToMePerSecond;
    }
}
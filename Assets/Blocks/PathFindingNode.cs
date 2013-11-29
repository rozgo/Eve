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
        public float DamageDoneToMePerSecond;
        public Vector3 Center {
            get {
                return new Vector3(
                    Position.x + Width * SpaceConversion.MapTileSize * 0.5f,
                    0,
                    Position.z + Length * SpaceConversion.MapTileSize * 0.5f
                );
            }
        }
    }
}
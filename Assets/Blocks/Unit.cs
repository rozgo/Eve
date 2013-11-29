using UnityEngine;
using System.Collections;

namespace Blocks {

    public class Unit : Block {
        public Vector3 Position;

        public enum Mind {
            Conscious,
            Unconscious,
            Semiconscious,
            Dead
        }

        public Mind MindState { get; private set; }
    }
}


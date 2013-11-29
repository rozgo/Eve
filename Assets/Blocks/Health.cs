using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Blocks {

    public class Health : Block {
        public static List<Health> Targets = new List<Health>();
        public float hitPoints;

        public static Health FindNearestObject ( Vector3 position, uint teamId, AIFlags flags ) {
            //TODO:Return best target
            return Targets.FirstOrDefault();
        }

        public void OnAttacked ( int DPS ) {
        }

        public void OnAttackedEnd ( int DPS ) {
        }

        public void DoDamage ( int amount, int Id ) {
        }
    }
}
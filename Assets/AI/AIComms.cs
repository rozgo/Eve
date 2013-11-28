//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;

//---------------------------------------------------------------------------------------------------------------------
[Flags]
public enum AIFlags {
    None = 0,
    Ground = 1 << 0,
    Air = 1 << 1,
    Sea = 1 << 2,
    Walls = 1 << 3,
    Resources = 1 << 4,
    Defenses = 1 << 5,
    Builder = 1 << 6,
    BuilderHome = 1 << 7,
    TroopEncampment = 1 << 8,
    Heroes = 1 << 9,
    Units = 1 << 10,
    All = ~0
}
//---------------------------------------------------------------------------------------------------------------------
public class AIComms {
    public enum AnimationState {
        Idle,
        Attack,
        Death,
        Victory,
        Jump,
        Salute,
        Revived,
        Locomote,
    }
    //---------------------------------------------------------------------------------------------------------------------
    public AIComms () :
        this( Vector3.zero, Quaternion.identity ) {
    }
    //---------------------------------------------------------------------------------------------------------------------
    public AIComms ( Vector3 position, Quaternion rotation ) { 
        Position = position;
        Rotation = rotation;
        Animation = AnimationState.Idle;
        Speed = 0;
    }
    //---------------------------------------------------------------------------------------------------------------------
    public Vector3 Position { get; set; }

    public Quaternion Rotation { get; set; }

    public AnimationState Animation { get; set; }

    public float Speed { get; set; }
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------


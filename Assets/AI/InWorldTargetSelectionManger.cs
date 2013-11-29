//---------------------------------------------------------------------------------------------------------------------   
using System;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

//---------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------
public class InWorldTargetSelectionManager {
    //---------------------------------------------------------------------------------------------------------------------
    public InWorldTargetSelectionManager () {
        m_attackerTargetMap = new Dictionary<int, Health>();
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void RegisterAttackTarget ( int attacker, int DPS, Health target ) {
        UnRegisterAttackTarget( attacker, DPS );

        if ( target == null ) {
            return;
        }

        if ( m_attackerTargetMap.ContainsValue( target ) == false ) {
            target.OnAttacked( DPS );
        }

        m_attackerTargetMap.Add( attacker, target );
    }
    //---------------------------------------------------------------------------------------------------------------------
    public void UnRegisterAttackTarget ( int attacker, int DPS ) {
        if ( m_attackerTargetMap.ContainsKey( attacker ) ) {
            m_attackerTargetMap[ attacker ].OnAttackedEnd( DPS );
        }

        m_attackerTargetMap.Remove( attacker );
    }
    //---------------------------------------------------------------------------------------------------------------------
    private Dictionary<int,Health > m_attackerTargetMap;
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------------------------------------------------------------------------


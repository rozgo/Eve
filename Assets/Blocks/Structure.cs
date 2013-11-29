﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Blocks {

    public class Structure : Block {

        public Field.Number xPos;
        public Field.Number yPos;

        public int width;
        public int length;

        public Level level;
        public string prefab;

        public override void OnDataBind () {
            base.OnDataBind();
            xPos = record.Add<Field.Number>( "x" );
            yPos = record.Add<Field.Number>( "y" );
        }

        public override void OnSetup () {
            base.OnSetup();
            level = GetComponent<Level>();
        }

        public virtual void OnWillMoveTo ( int x, int y ) {
        }

        public virtual void OnDidMoveTo ( int x, int y ) {
        }
    }
}

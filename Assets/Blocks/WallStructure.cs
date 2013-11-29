using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Blocks {

public class WallStructure : Structure {

    public override void OnDataBind () {
        base.OnDataBind();
    }

    public override void OnSetup () {
        base.OnSetup();

    }

    void Start () {
    }

    public override void OnWillMoveTo ( int x, int y ) {
    }

    public override void OnDidMoveTo ( int x, int y ) {
    }
}

}

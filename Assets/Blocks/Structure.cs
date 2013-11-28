using UnityEngine;
using System.Collections;

namespace Blocks {

public class Structure : Block {

	public override void OnStart () {
		base.OnStart();
	}
	
	public override void OnUpdate () {
		base.OnUpdate();
	}
	
	public virtual void OnWillMoveTo (int x, int y) {
	}
	
	public virtual void OnDidMoveTo (int x, int y) {
	}
}
	
}

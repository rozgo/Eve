using UnityEngine;
using System.Collections;

namespace Blocks {

	public class Resource : Block {
		
		Field.String product;
		Field.Number amount;
		Field.Number rate;
		
		public float Produce (float elapsedTime) {
			var delta = elapsedTime * rate.Get();
			amount.Set(amount.Get() + delta);
			return delta;
		}
		
		public override void OnSetup () {
			base.OnSetup();
			product = record.Add<Field.String>("product");
			amount  = record.Add<Field.Number>("amount");
			rate    = record.Add<Field.Number>("rate");
		}
		
		public override void OnStart () {
			base.OnStart();
			StartCoroutine(Producing());
		}
		
		public override void OnUpdate () {
			base.OnUpdate();
		}
		
		IEnumerator Producing () {
			while (true) {
				Produce(3);
				yield return new WaitForSeconds(3);
			}
		}
	}
	
}

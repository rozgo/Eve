using UnityEngine;
using System.Collections;

public class MuzzleFlashFX : MonoBehaviour {
    public ParticleEmitter smokeFlash;
    public ParticleEmitter smokeTrail;
    public ParticleEmitter smokeProjectile;
    public ParticleEmitter smokeAfterEffects;
    public Transform muzzlePlane;

    IEnumerator Start () {
        yield return new WaitForSeconds( 0.07f );
        smokeFlash.emit = false;
        muzzlePlane.gameObject.SetActive( false );
        smokeTrail.emit = false;
		
        //yield return new WaitForSeconds(1);
		
        smokeAfterEffects.emit = false;
        //yield return new WaitForSeconds(0.2f);
        smokeProjectile.emit = false;
        yield return new WaitForSeconds( 5 );
        Instantiate( gameObject, transform.position, transform.rotation );
        yield return new WaitForSeconds( 1 );
        Destroy( gameObject );
    }
}

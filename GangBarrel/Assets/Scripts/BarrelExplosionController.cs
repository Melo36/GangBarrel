using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelExplosionController : MonoBehaviour
{
    public ParticleSystem explosion;
    public GameObject explosionRadius;

    /// <summary>
    /// 
    /// This is called in the time of explosion.
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <param name="fuseTrigger"></param>
    public void ExplosionTrigger(Collider other, bool fuseTrigger)
    {
        if (other == null && !fuseTrigger)
            Debug.LogError("Either you need a shot, or activate a fuse.");
        if (fuseTrigger || other.gameObject.CompareTag("Bullet") || other.gameObject.CompareTag("ExplosionTrigger"))
        {
            ParticleSystem particle = Instantiate(explosion, transform.position, Quaternion.identity);
            
            // Instantiate with the correct rotation based on player position
            //Quaternion explosionRotation = directionHandler.GetExplosionRotation();
            GameObject expl = Instantiate(explosionRadius, transform.position, Quaternion.identity);
            //Debug.Log($"explosionRotation = {explosionRotation}");
            
            // Destroy Objects
            Destroy(gameObject);
            if (other != null)
            {
                Destroy(other.gameObject);
            }
                
            Destroy(expl, 1);
            Destroy(particle.gameObject, particle.main.duration);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        ExplosionTrigger(other, false);
    }
}

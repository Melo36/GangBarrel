using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelExplosionController : MonoBehaviour
{
    public ParticleSystem explosion;
    public GameObject explosionRadius;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bullet") || other.gameObject.CompareTag("ExplosionTrigger"))
        {
            ParticleSystem particle = Instantiate(explosion, transform.position, Quaternion.identity);
            
            // Instantiate with the correct rotation based on player position
            //Quaternion explosionRotation = directionHandler.GetExplosionRotation();
            GameObject expl = Instantiate(explosionRadius, transform.position, Quaternion.identity);
            //Debug.Log($"explosionRotation = {explosionRotation}");
            
            // Destroy Objects
            Destroy(gameObject);
            Destroy(other.gameObject);
            //Destroy(expl, 1);
            Destroy(particle, particle.main.duration);
        }
    }
}

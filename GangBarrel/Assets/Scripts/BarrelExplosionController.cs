using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelExplosionController : MonoBehaviour
{
    public ParticleSystem explosion;
    public GameObject explosionRadius;
    private void OnCollisionEnter(Collision other)
    {
        // Only a barrel gets destroyed by a bullet
        if (other.gameObject.CompareTag("Bullet") || other.gameObject.CompareTag("ExplosionTrigger"))
        {
            ParticleSystem particle = Instantiate(explosion, transform.position, Quaternion.identity);
            GameObject expl = Instantiate(explosionRadius, transform.position, Quaternion.identity);
            
            // Destroy Objects
            Destroy(gameObject);
            Destroy(other.gameObject);
            Destroy(expl,1);
            Destroy(particle, particle.main.duration);
        }
    }
}

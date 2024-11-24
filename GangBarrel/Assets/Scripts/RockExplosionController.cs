using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockExplosionController : MonoBehaviour
{
    public ParticleSystem explosion;
    public GameObject explosionRadius;
    private void OnCollisionEnter(Collision other)
    {
        // Only a barrel gets destroyed by a bullet
        if (other.gameObject.CompareTag("ExplosionTrigger"))
        {
            ParticleSystem particle = Instantiate(explosion, transform.position, Quaternion.identity);
            Vector3 explosionDirection =
                (other.gameObject.transform.position - gameObject.transform.position).normalized;
            GameObject expl = Instantiate(explosionRadius, transform.position, Quaternion.LookRotation(explosionDirection * -1));
            
            // Destroy Objects
            Destroy(gameObject);
            Destroy(other.gameObject);
            Destroy(expl,1);
            Destroy(particle, particle.main.duration);
        }
    }
}
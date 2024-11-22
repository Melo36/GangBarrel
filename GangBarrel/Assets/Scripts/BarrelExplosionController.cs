using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelExplosionController : MonoBehaviour
{
    public ParticleSystem explosion;
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
            Destroy(other.gameObject);
        }
    }
}

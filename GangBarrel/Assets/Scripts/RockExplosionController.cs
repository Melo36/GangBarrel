using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class RockExplosionController : MonoBehaviour
{
    public ParticleSystem explosion;
    public GameObject explosionRadius;
    private BarrelDirectionHandler directionHandler;
    private ScanManager scanManager;
    
    private void OnEnable()
    {
        directionHandler = GetComponent<BarrelDirectionHandler>();
        scanManager = FindObjectOfType<ScanManager>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Explosion triggered by {other.gameObject.name}");

        
        // Only a barrel gets destroyed by a bullet
        if (other.gameObject.CompareTag("ExplosionTrigger"))
        {
            ParticleSystem particle = Instantiate(explosion, transform.position, Quaternion.identity);
            //Quaternion explosionRotation = directionHandler.GetExplosionRotation();
            Vector3 explosionDirection;
            Transform otherTransform = other.gameObject.transform;
            if (other.gameObject.transform.parent)
            {
                explosionDirection =
                    (otherTransform.parent.position - gameObject.transform.position).normalized;
            }
            else
            {
                explosionDirection =
                    (otherTransform.position - gameObject.transform.position).normalized;
            }
            
            var q = Quaternion.Euler(0, 90, 0);
            GameObject expl = Instantiate(explosionRadius, transform.position, Quaternion.LookRotation(explosionDirection * -1));
            
            // Scan the map after each explosion occurence.
            // Schedule the scan

            ScanManager.Instance.ScheduleScan(1.5f, gameObject.name);
            
            // Destroy Objects
            Destroy(gameObject);
            Destroy(other.gameObject);
            Destroy(expl,1);
            Destroy(particle.gameObject, particle.main.duration);
        }
    }
    
}
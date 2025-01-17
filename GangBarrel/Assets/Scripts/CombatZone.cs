using System;
using UnityEngine;

public class CombatZone : MonoBehaviour
{
    [SerializeField] private BoxCollider outerTrigger; // Combat disengage zone
    [SerializeField] private BoxCollider innerTrigger; // Combat engage zone
    [SerializeField] private bool showDebugGizmos = true;

    public RoundManager roundManager;
    public EnemyController enemyController;

    private bool isInCombat = false;

    private void Awake()
    {
        roundManager = FindObjectOfType<RoundManager>();
        
        // Ensure both triggers are set correctly
        outerTrigger.isTrigger = true;
        innerTrigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Start combat only when player enters the inner zone
        if (!isInCombat && other.bounds.Intersects(innerTrigger.bounds))
        {
            StartCombat();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // End combat only when player completely exits the outer zone
        if (isInCombat && !other.bounds.Intersects(outerTrigger.bounds))
        {
            EndCombat();
        }
    }

    private void StartCombat()
    {
        if (isInCombat) return;
        
        isInCombat = true;
        roundManager.AddEnemyToCombat(enemyController);
        roundManager.StartCombat();
    }

    private void EndCombat()
    {
        if (!isInCombat) return;
        
        isInCombat = false;
        Debug.Log("Combat Zone: Disengaging combat!");
        roundManager.RemoveEnemyFromCombat(enemyController);
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw inner combat zone (red)
        if (innerTrigger != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.matrix = innerTrigger.transform.localToWorldMatrix;
            Gizmos.DrawCube(innerTrigger.center, innerTrigger.size);
        }

        // Draw outer combat zone (yellow)
        if (outerTrigger != null)
        {
            Gizmos.color = new Color(1, 1, 0, 0.2f);
            Gizmos.matrix = outerTrigger.transform.localToWorldMatrix;
            Gizmos.DrawCube(outerTrigger.center, outerTrigger.size);
        }
    }
    #endif
}
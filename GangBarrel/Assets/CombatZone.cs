using UnityEngine;

public class CombatZone : MonoBehaviour
{
    public RoundManager roundManager; // Reference to the RoundManager
    public EnemyController enemyController; // Associated enemy for this combat zone

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Combat Zone has been entered!");
        // If the player enters the combat zone, add the enemy to the RoundManager's list
        if (other.CompareTag("Player"))
        {
            roundManager.AddEnemyToCombat(enemyController);
            roundManager.StartCombat();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Combat Zone has been exited!");
        // Optional: Remove the enemy if the player leaves the zone
        if (other.CompareTag("Player"))
        {
            roundManager.RemoveEnemyFromCombat(enemyController);
        }
    }
}
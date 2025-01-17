using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class RoundManager : MonoBehaviour
{
    public PlayerController playerController;
    public List<EnemyController> activeEnemies = new List<EnemyController>();
    public CameraFollow cameraFollow;

    public GameObject turnTextBackground;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI remainingActionsText;

    public Transform playersGoal;
    
    // This button can be pressed to end a turn more early.
    public Button endTurnEarly;
    
    public int currentTurnIndex = -1; // -1 for player, 0+ for enemies

    public int remainingActions = 0;
    //private const int MaxActionsPerTurn = 5; // Max actions for both player and enemies
    public bool isCombatActive = false;

    private void Start()
    {
        turnTextBackground.gameObject.SetActive(false);
        endTurnEarly.onClick.AddListener(EndCurrentTurn);
    }

    public void StartCombat()
    {
        turnTextBackground.SetActive(true);

        if (!isCombatActive)
        {
            isCombatActive = true;
            StartPlayerTurn();
        }
    }

    public void StartPlayerTurn()
    {
        currentTurnIndex = -1; // Player's turn
        
        remainingActions = playerController.maxActions;
        remainingActionsText.text = "Remaining Actions: " + remainingActions;
        
        turnText.text = "Player's Turn";
        turnText.color = Color.green;
        playerController.isInTurn = true;

        cameraFollow.SetTarget(playerController.transform);
        playerController.StopMovement();

    }

    // (player, -1), (enemy, 0)
    public void StartEnemyTurn()
    {
        if (!isCombatActive)
            return;
        
        if (activeEnemies.Count == 0)
        {
            if (isCombatActive)
            {
                EndCombat();
                return;
            }
        }
        
        var currentEnemy = activeEnemies[currentTurnIndex];

        // Set remaining actions for this enemy
        remainingActions = currentEnemy.maxActions;
        remainingActionsText.text = "Remaining Actions: " + remainingActions;

        turnText.text = $"Enemy {currentTurnIndex + 1}'s Turn";
        turnText.color = Color.red;

        cameraFollow.SetTarget(currentEnemy.transform);
        
        // Tell the enemy to start their logic
        currentEnemy.isInTurn = true;
        currentEnemy.StartEnemyTurn(); // Pass the RoundManager for communication
    }
    
    public void DecrementActions(int amount)
    {
        var change = amount;
        if (amount > remainingActions)
        {
            Debug.LogError("You can not decrement more actions, than you have!");
            change = remainingActions;
        }
        remainingActions-=change;
        
        remainingActionsText.text = "Remaining Actions: " + remainingActions;
    }

    public void EndCurrentTurn()
    {
        if (currentTurnIndex == -1)
        {
            // Player's turn is ending, switching to first enemy
            playerController.isInTurn = false;
            currentTurnIndex = 0;
            StartEnemyTurn();
        }
        else
        {
            // Current enemy's turn is ending
            if (activeEnemies.Count > 0)
            {
                activeEnemies[currentTurnIndex].isInTurn = false;
            }

            if (currentTurnIndex >= activeEnemies.Count - 1)
            {
                // Last enemy finished, back to player
                currentTurnIndex = -1;
                StartPlayerTurn();
            }
            else
            {
                // More enemies to go
                currentTurnIndex++;
                StartEnemyTurn();
            }
        }
    }


    public void EndCombat()
    {
        if(!isCombatActive) return;
        
        isCombatActive = false;
        currentTurnIndex = -1;
        turnText.text = "Combat Ended!";
        turnText.color = Color.gray;

        StartCoroutine(HideTurnTextBackgroundAfterDelay(4f));
    }

    private IEnumerator HideTurnTextBackgroundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        turnTextBackground.SetActive(false);
    }

    public void AddEnemyToCombat(EnemyController enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }

    public void RemoveEnemyFromCombat(EnemyController enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);

            if (activeEnemies.Count == 0 && isCombatActive)
            {
                EndCombat();
            }
        }
    }
}

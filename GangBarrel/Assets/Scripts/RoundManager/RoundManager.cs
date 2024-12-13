using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    
    private int currentTurnIndex = -1; // -1 for player, 0+ for enemies

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
        Debug.Log("Start Combat");
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

        Debug.Log($"Player turn started with {remainingActions} actions.");
    }

    public void StartEnemyTurn()
    {
        if (activeEnemies.Count == 0)
        {
            if (isCombatActive)
            {
                EndCombat();
                return;
            }
        }

        currentTurnIndex++;
        if (currentTurnIndex >= activeEnemies.Count)
        {
            StartPlayerTurn(); // Reset to player's turn
            return;
        }

        var currentEnemy = activeEnemies[currentTurnIndex];

        // Set remaining actions for this enemy
        remainingActions = currentEnemy.movementRange;
        remainingActionsText.text = "Remaining Actions: " + remainingActions;

        turnText.text = $"Enemy {currentTurnIndex + 1}'s Turn";
        turnText.color = Color.red;

        cameraFollow.SetTarget(currentEnemy.transform);

        Debug.Log($"Enemy {currentTurnIndex + 1} turn started with {remainingActions} actions.");

        // Tell the enemy to start their logic
        currentEnemy.isInTurn = true;
        currentEnemy.StartEnemyTurn(); // Pass the RoundManager for communication
    }
    
    public void DecrementActions(int amount)
    {
        if(amount > remainingActions)
            Debug.LogError("You can not decrement more actions, than you have!");
        remainingActions-=amount;
        
        remainingActionsText.text = "Remaining Actions: " + remainingActions;
        
        if (remainingActions <= 0)
        {
            Debug.Log("No actions left. Ending turn.");
            EndCurrentTurn();
        }
    }

    public void EndCurrentTurn()
    {
        if (currentTurnIndex == -1)
        {
            // Player's turn ended
            StartEnemyTurn();
        }
        else
        {
            // Enemy's turn ended
            StartEnemyTurn();
        }
    }

    public void EndCombat()
    {
        if(!isCombatActive) return;
        
        Debug.Log("EndCombat!");
        isCombatActive = false;
        currentTurnIndex = -1;
        turnText.text = "Combat Ended!";
        turnText.color = Color.gray;

        StartCoroutine(HideTurnTextBackgroundAfterDelay(4f));
        Debug.Log("Combat has ended.");
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

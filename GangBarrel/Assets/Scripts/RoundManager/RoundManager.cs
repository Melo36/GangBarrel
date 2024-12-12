using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public PlayerController playerController;
    public EnemyController enemyController;
    public Camera playerCamera;
    public Camera enemyCamera;

    private bool isPlayerTurn = true;

    public void StartRoundMode()
    {
        playerController.EnableMovement(5f); // 5 meters range
        enemyController.EnableMovement(3f); // 3 meters range
        SwitchToPlayerTurn();
    }

    private void SwitchToPlayerTurn()
    {
        isPlayerTurn = true;
        playerCamera.enabled = true;
        enemyCamera.enabled = false;
        playerController.StartTurn();
    }

    public void EndPlayerTurn()
    {
        isPlayerTurn = false;
        playerController.EndTurn();
        SwitchToEnemyTurn();
    }

    private void SwitchToEnemyTurn()
    {
        enemyCamera.enabled = true;
        playerCamera.enabled = false;
        enemyController.StartTurn();
    }

    public void EndEnemyTurn()
    {
        isPlayerTurn = true;
        enemyController.EndTurn();
        SwitchToPlayerTurn();
    }
}

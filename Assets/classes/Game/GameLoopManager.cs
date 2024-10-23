using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;

public class GameLoopManager : MonoBehaviour
{
    private float timer; // Timer variable
    private const float gameDuration = 15f; // Duration for the game timer

    public static Vector3[] NodePositions;
    private static Queue<Enemy> EnemiesToRemove;
    private static Queue<int> EnemyIDsToSummon;

    public Transform NodeParent;
    public bool LoopShouldEnd;

    public GameObject gameOverUI;  // Declare the Game Over UI field
    private int enemiesRemovedCount = 0; // Counter for removed enemies
    private const int maxEnemiesAllowed = 5; // Max number of enemies before game over

    void Start()
    {
        Debug.Log("GameLoopManager Start() called");
        EnemyIDsToSummon = new Queue<int>();
        EnemiesToRemove = new Queue<Enemy>();
        EntitySummoner.Init();

        NodePositions = new Vector3[NodeParent.childCount];
        for(int i = 0; i < NodePositions.Length; i++)
        {
            NodePositions[i] = NodeParent.GetChild(i).position;
        }

        StartCoroutine(GameLoop());
        InvokeRepeating("SummonTest", 0f, 1f);
        gameOverUI.SetActive(false);
    }

    void SummonTest ()
    {
        Debug.Log("SummonTest: Attempting to enqueue enemy ID 1");
        EnqueueEnemyIDToSummon(1);
    }

    IEnumerator GameLoop () 
    {   
        timer = gameDuration; // Initialize the timer

        while (LoopShouldEnd == false) 
        {   
            // Countdown timer
            timer -= Time.deltaTime;
        
            // Check if the timer has run out
            if (timer <= 0)
            {
                TriggerGameWon(); // Call Game Won method
                yield break; // Exit the game loop
            }

            // Spawn Enemies
            if (EnemyIDsToSummon.Count > 0)
            {
                for(int i = 0; i < EnemyIDsToSummon.Count; i++) 
                {
                    EntitySummoner.SummonEnemy(EnemyIDsToSummon.Dequeue());
                }
            }

            // Move Enemies
            if (EntitySummoner.EnemiesInGame.Count > 0)
            {
                // Allocate NativeArrays
                NativeArray<Vector3> NodesToUse = new NativeArray<Vector3>(NodePositions, Allocator.TempJob);
                NativeArray<float> EnemySpeeds = new NativeArray<float>(EntitySummoner.EnemiesInGame.Count, Allocator.TempJob);
                NativeArray<int> NodeIndices = new NativeArray<int>(EntitySummoner.EnemiesInGame.Count, Allocator.TempJob);
                TransformAccessArray EnemyAccess = new TransformAccessArray(EntitySummoner.EnemiesInGameTransform.ToArray(), 2);

                for (int i = 0; i < EntitySummoner.EnemiesInGame.Count; i++)
                {
                    EnemySpeeds[i] = EntitySummoner.EnemiesInGame[i].Speed;
                    NodeIndices[i] = EntitySummoner.EnemiesInGame[i].NodeIndex;
                }

                // Schedule the job
                MoveEnemiesJob MoveJob = new MoveEnemiesJob 
                {
                    NodePositions = NodesToUse,
                    EnemySpeed = EnemySpeeds,
                    NodeIndex = NodeIndices,
                    deltaTime = Time.deltaTime
                };

                JobHandle MoveJobHandle = MoveJob.Schedule(EnemyAccess);
                MoveJobHandle.Complete();

                // Update enemies' NodeIndex based on the job's results
                for (int i = 0; i < EntitySummoner.EnemiesInGame.Count; i++)
                {
                    EntitySummoner.EnemiesInGame[i].NodeIndex = NodeIndices[i];

                    if (EntitySummoner.EnemiesInGame[i].NodeIndex == NodePositions.Length)
                    {
                        EnqueueEnemyToRemove(EntitySummoner.EnemiesInGame[i]);
                    }
                }

                // Clean up NativeArrays after the job is complete
                EnemySpeeds.Dispose();
                NodeIndices.Dispose();
                EnemyAccess.Dispose();
                NodesToUse.Dispose();
            }

            // Remove Enemies
            // if (EnemiesToRemove.Count > 0)
            // {
            //     for(int i = 0; i < EnemiesToRemove.Count; i++) 
            //     {
            //         EntitySummoner.RemoveEnemy(EnemiesToRemove.Dequeue());
            //     }
            // }

            if (EnemiesToRemove.Count > 0)
            {
                for (int i = 0; i < EnemiesToRemove.Count; i++)
                {
                    EntitySummoner.RemoveEnemy(EnemiesToRemove.Dequeue());

                    // Increment the counter for removed enemies
                    enemiesRemovedCount++;
                    Debug.Log("Enemy Removed! Count: " + enemiesRemovedCount);

                    // Check if we've reached the limit of allowed enemies removed
                    if (enemiesRemovedCount >= maxEnemiesAllowed)
                    {
                        TriggerGameOver();
                        yield break; // Exit the game loop once Game Over is triggered
                    }
                }
            }

            yield return null; // Wait for the next frame before repeating
        }
    }

    void TriggerGameOver()
    {
        Debug.Log("Game Over! " + maxEnemiesAllowed + " enemies have been removed.");
        gameOverUI.SetActive(true);
        LoopShouldEnd = true; // Stop the game loop
    }

    void TriggerGameWon()
    {
        Debug.Log("Game Won! Time has run out without reaching " + maxEnemiesAllowed + " enemies removed.");
        gameOverUI.SetActive(true);  // Show the same UI
        // Change the text to "Game Won"
        Text uiText = gameOverUI.GetComponentInChildren<Text>(); // Assuming you are using UnityEngine.UI
        if (uiText != null)
        {
            uiText.text = "GAME WON";
            uiText.color = Color.green;
        }
        LoopShouldEnd = true; // Stop the game loop
    }

    public static void EnqueueEnemyIDToSummon(int ID)
    {
        EnemyIDsToSummon.Enqueue(ID);
    }

    public static void EnqueueEnemyToRemove (Enemy EnemyToRemove)
    {
        EnemiesToRemove.Enqueue(EnemyToRemove);
    }
}

public struct MoveEnemiesJob : IJobParallelForTransform
{
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> NodePositions;

    [NativeDisableParallelForRestriction]
    public NativeArray<float> EnemySpeed;

    [NativeDisableParallelForRestriction]
    public NativeArray<int> NodeIndex;

    public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        if (NodeIndex[index] < NodePositions.Length)
        {
            Vector3 PositionToMoveTo = NodePositions[NodeIndex[index]];
            transform.position = Vector3.MoveTowards(transform.position, PositionToMoveTo, EnemySpeed[index] * deltaTime);

            if (transform.position == PositionToMoveTo)
            {
                NodeIndex[index]++;
            }
        }
    }
}





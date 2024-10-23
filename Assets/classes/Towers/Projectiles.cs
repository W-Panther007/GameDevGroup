using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float Speed = 10f; // How fast the projectile moves
    private Enemy target;      // The enemy the projectile is targeting

    // Method to set the target for the projectile
    public void SetTarget(Enemy enemy)
    {
        target = enemy;
    }

    void Update()
    {
        // If there is no target, destroy the projectile
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Calculate the direction to move the projectile towards the enemy
        Vector3 direction = (target.transform.position - transform.position).normalized;
        float distanceThisFrame = Speed * Time.deltaTime;

        // Check if the projectile is close enough to hit the target
        if (Vector3.Distance(transform.position, target.transform.position) <= distanceThisFrame)
        {
            HitTarget();  // Call the method to handle hitting the enemy
            return;
        }

        // Move the projectile towards the enemy
        transform.Translate(direction * distanceThisFrame, Space.World);
    }

    void HitTarget()
    {
        // Remove the enemy when the projectile hits it
        if (target != null)
        {
            EntitySummoner.RemoveEnemy(target);
        }

        // Destroy the projectile after it hits the enemy
        Destroy(gameObject);
    }
}

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class TowerBehaviour : MonoBehaviour
// {
//     // Start is called before the first frame update
//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }
// }


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBehaviour : MonoBehaviour
{
    public GameObject ProjectilePrefab;  // Prefab of the projectile the tower shoots
    public float FireRate = 1f;          // How often the tower fires
    public float FireRange = 10f;        // The range within which the tower can shoot
    private float fireCountdown = 0f;    // Countdown for the next shot

    void Update()
    {
        // Reduce the fire countdown timer
        fireCountdown -= Time.deltaTime;

        // If the countdown reaches zero, check for enemies to fire at
        if (fireCountdown <= 0f)
        {
            Enemy target = GetNearestEnemy();
            if (target != null)
            {
                Shoot(target);  // Shoot at the nearest enemy
            }
            fireCountdown = 1f / FireRate;  // Reset the fire countdown
        }
    }

    Enemy GetNearestEnemy()
    {
        Enemy nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Enemy enemy in EntitySummoner.EnemiesInGame)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance && distanceToEnemy <= FireRange)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    void Shoot(Enemy target)
    {
        // Instantiate the projectile and set its target
        GameObject projectileGO = Instantiate(ProjectilePrefab, transform.position, Quaternion.identity);
        Projectile projectile = projectileGO.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.SetTarget(target);
        }
    }

    // Optional: draw the range of the tower in the scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, FireRange);
    }
}

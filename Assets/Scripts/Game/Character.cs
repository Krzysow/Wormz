using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    float deathYLevel = -50f;
    protected int health = 100;
    [SerializeField] protected float speed = 20f;
    [SerializeField] protected float maxJumpForce = 30f;
    [SerializeField] protected float maxShootForce = 60f;
    protected float forceMultiplier = 10f;
    protected DynamicBody dynamicBody;
    protected PhysicsCheck physics;
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected GameObject rocketPrefab;
    [SerializeField] protected bool hasTurn = false;
    [SerializeField] protected bool isOnPlayerTeam = false;

    // Start is called before the first frame update
    virtual protected void Start()
    {
        dynamicBody = GetComponent<DynamicBody>();
        physics = FindObjectOfType<PhysicsCheck>();
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        if (transform.position.y < deathYLevel)
            Die();
    }

    protected void Die()
    {
        FindObjectOfType<TurnManager>().RemoveTeammate(this);
        physics.Despawn(gameObject);
    }

    protected void Jump(Vector2 direction)
    {
        Vector2 normal = direction.normalized;
        float magnitude = direction.magnitude;
        Vector2 force = normal * Mathf.Clamp(magnitude * forceMultiplier, 0f, maxJumpForce);
        if (force.sqrMagnitude > 1)
            dynamicBody.SetVelocity(force);
    }

    public virtual void Learn(Vector2 hitPoint) {}

    protected void Shoot(Vector2 direction)
    {
        Vector2 normal = direction.normalized;
        float magnitude = direction.magnitude;
        Vector2 force = normal * Mathf.Clamp(magnitude * forceMultiplier, 0f, maxShootForce);
        Vector3 velocityStep = dynamicBody.GetVelocity() * Time.deltaTime;
        Vector3 spawnPoint = transform.position + new Vector3(2 * normal.x * transform.localScale.x, 2 * normal.y * transform.localScale.y) + velocityStep;
        RayResult rayResult = physics.RayCast(transform.position, spawnPoint);
        if (rayResult.wasHit)
            spawnPoint = rayResult.hitPoints[0];
        GameObject bullet = physics.Spawn(bulletPrefab, spawnPoint, Quaternion.identity);
        bullet.GetComponent<DynamicBody>().AddVelocity(force);
        bullet.GetComponent<Bullet>().owner = this;
    }

    protected void Rocket(Vector2 direction)
    {
        Vector2 normal = direction.normalized;
        FindObjectOfType<PhysicsCheck>().Spawn(rocketPrefab, transform.position + new Vector3(2 * normal.x * transform.localScale.x, 2 * normal.y * transform.localScale.y));
    }

    public void Damage(int damage)
    {
        health -= damage;
        Debug.Log(damage);
    }

    public int GetHealth()
    {
        return health;
    }

    public bool GetHasTurn()
    {
        return hasTurn;
    }

    virtual public void SetHasTurn(bool turn)
    {
        hasTurn = turn;
    }

    public bool GetIsOnPlayerTeam()
    {
        return isOnPlayerTeam;
    }
}

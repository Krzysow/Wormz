using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PhysicsResult
{
    public bool hasCollided;
    public Vector2 collisionNormal;
    public float penetration;
    public List<Vector2> contactPoints;
}

public class DynamicBody : MonoBehaviour
{
    protected Vector2 position;
    protected Vector2 velocity;
    [SerializeField] protected float mass = 10.0f;
    protected float massInverse;
    [SerializeField] protected float bounciness = 0.3f;
    protected float orientation = 0.0f;
    protected float angularVelocity = 0.0f;
    [SerializeField] protected float momentOfInertia = -0.3f;
    [SerializeField] protected float staticFriction = 5f;
    [SerializeField] protected float dynamicFriction = 3f;
    protected bool isColliding = false;
    public float minimumSqrVelocity = 0.000001f;
    public float minimumAngularVelocity = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        InitializeDynamicBody();
    }

    // Update is called once per frame
    void Update()
    {
        if (velocity.sqrMagnitude < minimumSqrVelocity)
            velocity = Vector2.zero;
        if (Mathf.Abs(angularVelocity) * Mathf.Deg2Rad < minimumAngularVelocity)
            angularVelocity = 0;

        orientation += angularVelocity * Time.deltaTime;
        Quaternion rotate = Quaternion.Euler(0, 0, orientation);
        transform.rotation = rotate;
        position += velocity * Time.deltaTime;
        transform.position = position;

        // Debug
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.red;
        if (isColliding)
            spriteRenderer.color = Color.green;
    }

    public void InitializeDynamicBody()
    {
        massInverse = 1f / mass;
        position = transform.position;
        orientation = transform.rotation.eulerAngles.z;
    }

    public Vector2 GetPosition()
    {
        return position;
    }

    public void SetPosition(Vector2 newPosition)
    {
        position = newPosition;
    }

    public Vector2 GetVelocity()
    {
        return velocity;
    }

    public void SetVelocity(Vector2 newVelocity)
    {
        velocity = newVelocity;
    }

    public void AddVelocity(Vector2 newVelocity)
    {
        velocity += newVelocity;
    }

    public float GetAngularVelocity()
    {
        return angularVelocity;
    }

    public void SetAngularVelocity(float newAngularVelocity)
    {
        angularVelocity = newAngularVelocity;
    }

    public void AddAngularVelocity(float newAngularVelocity)
    {
        angularVelocity += newAngularVelocity;
    }

    public float GetMass()
    {
        return mass;
    }

    public float GetMassInverse()
    {
        return massInverse;
    }

    public float GetBounciness()
    {
        return bounciness;
    }

    public float GetStaticFriction()
    {
        return staticFriction;
    }

    public float GetDynamicFriction()
    {
        return dynamicFriction;
    }

    public float GetOrientation()
    {
        return orientation;
    }

    public void SetIsColliding(bool newIsColliding)
    {
        isColliding = newIsColliding;
    }

    public bool GetIsColliding()
    {
        return isColliding;
    }

    public void ApplyPointForce(Vector2 location, Vector2 force)
    {
        //check with the slides
        Vector3 torque = Vector3.Cross(position - location, force);
        angularVelocity += torque.z / momentOfInertia;
    }
}

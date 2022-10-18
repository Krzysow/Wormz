using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    float radius;
    DynamicBody dynamicBody;

    // Start is called before the first frame update
    void Start()
    {
        radius = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y) * 0.5f;
        dynamicBody = GetComponent<DynamicBody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public DynamicBody GetDynamicBody()
    {
        return dynamicBody;
    }

    public PhysicsResult IsCollidingWithCircle(Circle circle)
    {
        PhysicsResult physicsResult = new PhysicsResult();

        if (radius + circle.radius > (dynamicBody.GetPosition() - circle.dynamicBody.GetPosition()).magnitude)
        {
            physicsResult.hasCollided = true;

            Vector2 collisionDirection = circle.dynamicBody.GetPosition() - dynamicBody.GetPosition();
            physicsResult.collisionNormal = collisionDirection.normalized;
            physicsResult.penetration = (radius + circle.radius) - collisionDirection.magnitude;
        }
        else
        {
            physicsResult.hasCollided = false;
        }

        return physicsResult;
    }
}

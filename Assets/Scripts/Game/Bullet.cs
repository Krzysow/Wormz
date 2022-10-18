using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] protected int destructionStep = 4;
    [SerializeField] protected int damage = 40;
    [SerializeField] protected float explosionRadius = 5f;
    [SerializeField] protected float explosionPower = 5f;
    [SerializeField] [Tooltip("Bullet only, not rockect")] protected float maxAliveTime = 10f;
    public Character owner;

    DynamicBody dynamicBody;
    float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        dynamicBody = GetComponent<DynamicBody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (dynamicBody.GetIsColliding())
        {
            Explode();
        }

        if (timer >= maxAliveTime)
        {
            Explode();
        }

        timer += Time.deltaTime;
    }

    protected void Explode()
    {
        if (owner)
            owner.Learn(transform.position);

        PhysicsCheck physics = FindObjectOfType<PhysicsCheck>();
        foreach (Character ai in FindObjectsOfType<Character>())
        {
            Vector2 explosionVector = ai.transform.position - transform.position;
            float explosionDistance = explosionVector.magnitude;
            if (explosionDistance < explosionRadius)
            {
                int finalDamage = (int)(damage * (1f - explosionDistance / explosionRadius));
                ai.Damage(finalDamage);
                    
                if (explosionRadius - explosionDistance > 0)
                    ai.GetComponent<DynamicBody>().AddVelocity(explosionVector.normalized * (explosionRadius - explosionDistance) * explosionPower);
            }
        }

        FindObjectOfType<MapGrid>().UpdateGrid(transform.position, explosionRadius);

        List<Shape> destructibleShapes = new List<Shape>();
        if (PhysicsCheck.CheckSphere(transform.position, explosionRadius, physics.GetPhysicsShapes(), ref destructibleShapes))
        {
            foreach (Square square in destructibleShapes)
            {
                if (!square.GetDynamicBody())
                    square.Destructable(transform.position, explosionRadius, destructionStep);
            }
        }

        Debug.Log("Boom");
        physics.Despawn(gameObject);
    }
}

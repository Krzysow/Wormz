using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RayResult
{
    public bool wasHit;
    public List<Shape> shapesHit;
    public List<Vector2> hitPoints;
}

public class PhysicsCheck : MonoBehaviour
{
    List<Circle> physicsCircles = new List<Circle>();
    List<Shape> physicsShapes = new List<Shape>();
    List<DynamicBody> dynamicBodies = new List<DynamicBody>();
    [SerializeField] float gravity = 9.8f;
    [SerializeField] float drag = 0.7f;
    [SerializeField] float angularDrag = 0.3f;
    [SerializeField] float defaultBounciness = 0.5f;
    [SerializeField] float defaultStaticFriction = 5f;
    [SerializeField] float defaultDynamicFriction = 3f;

    // Start is called before the first frame update
    void Awake()
    {
        dynamicBodies.AddRange(FindObjectsOfType<DynamicBody>());
        physicsCircles.AddRange(FindObjectsOfType<Circle>());
        physicsShapes.AddRange(FindObjectsOfType<Shape>());
    }

    private void Start()
    {
        foreach (DynamicBody dynamicBody in dynamicBodies)
        {
            Shape shape = dynamicBody.GetComponent<Shape>();
            if (shape)
            {
                foreach (Shape shapeChild in shape.childShapes)
                {
                    RemoveShape(shapeChild);
                }
            }
        }
    }

    void RemoveShape(Shape shape)
    {
        foreach (Shape childShape in shape.childShapes)
        {
            RemoveShape(childShape);
        }
        physicsShapes.Remove(shape);
    }

    // Update is called once per frame
    void Update()
    {
        Dynamics();
        CircleCollisions();
        SATCollisions();
    }

    private void SATCollisions()
    {
        foreach (Shape physicsShape in physicsShapes)
        {
            DynamicBody dynamicBody = physicsShape.GetDynamicBody();
            if (dynamicBody)
                dynamicBody.SetIsColliding(false);
        }

        for (int i = 0; i < physicsShapes.Count - 1; i++)
        {
            for (int j = i + 1; j < physicsShapes.Count; j++)
            {
                DynamicBody dynamicBodyA = physicsShapes[i].GetDynamicBody();
                DynamicBody dynamicBodyB = physicsShapes[j].GetDynamicBody();

                if (dynamicBodyA || dynamicBodyB)
                {
                    // broad phase
                    if (physicsShapes[i].AABB(physicsShapes[j]))
                    {
                        // narrow phase
                        PhysicsResult physicsResult = physicsShapes[i].IsCollidingSAT(physicsShapes[j]);
                        if (physicsResult.hasCollided)
                        {
                            float massInverseA = 0;
                            Vector2 velocityA = Vector2.zero;
                            float bouncinessA = defaultBounciness;
                            float staticFrictionA = defaultStaticFriction;
                            float dynamicFrictionA = defaultDynamicFriction;

                            float massInverseB = 0;
                            Vector2 velocityB = Vector2.zero;
                            float bouncinessB = defaultBounciness;
                            float staticFrictionB = defaultStaticFriction;
                            float dynamicFrictionB = defaultDynamicFriction;


                            if (dynamicBodyA)
                            {
                                massInverseA = dynamicBodyA.GetMassInverse();
                                velocityA = dynamicBodyA.GetVelocity();
                                bouncinessA = dynamicBodyA.GetBounciness();
                                staticFrictionA = dynamicBodyA.GetStaticFriction();
                                dynamicFrictionA = dynamicBodyA.GetDynamicFriction();
                                dynamicBodyA.SetIsColliding(true);
                            }

                            if (dynamicBodyB)
                            {
                                massInverseB = dynamicBodyB.GetMassInverse();
                                velocityB = dynamicBodyB.GetVelocity();
                                bouncinessB = dynamicBodyB.GetBounciness();
                                staticFrictionB = dynamicBodyB.GetStaticFriction();
                                dynamicFrictionB = dynamicBodyB.GetDynamicFriction();
                                dynamicBodyB.SetIsColliding(true);
                            }

                            if (dynamicBodyA || dynamicBodyB)
                            {
                                CollisionResolution(physicsResult,
                                           dynamicBodyA, dynamicBodyB,
                                           massInverseA, velocityA, bouncinessA, staticFrictionA, dynamicFrictionA,
                                           massInverseB, velocityB, bouncinessB, staticFrictionB, dynamicFrictionB);
                            }
                        }
                    }
                }
            }
        }
    }

    private static void CollisionResolution(PhysicsResult physicsResult,
                                   DynamicBody dynamicBodyA, DynamicBody dynamicBodyB,
                                   float massInverseA, Vector2 velocityA, float bouncinessA, float staticFrictionA, float dynamicFrictionA,
                                   float massInverseB, Vector2 velocityB, float bouncinessB, float staticFrictionB, float dynamicFrictionB)
    {
        Vector2 relativeVelocity = velocityB - velocityA;
        float velocityAlongNormal = Vector2.Dot(relativeVelocity, physicsResult.collisionNormal);
        if (velocityAlongNormal <= 0)
        {
            float bounciness = Mathf.Min(bouncinessA, bouncinessB);
            float impulseScale = -(1 + bounciness) * velocityAlongNormal;
            impulseScale /= massInverseA + massInverseB;
            Vector2 impulseTotal = impulseScale * physicsResult.collisionNormal;

            Vector2 tangent = (relativeVelocity - Vector2.Dot(relativeVelocity, physicsResult.collisionNormal) * physicsResult.collisionNormal).normalized;
            float frictionMagnitude = -Vector2.Dot(relativeVelocity, tangent) / (massInverseA + massInverseB);
            float mu = Mathf.Sqrt(staticFrictionA * staticFrictionA + staticFrictionB * staticFrictionB);

            if (Mathf.Abs(frictionMagnitude) < impulseScale * mu)
            {
                impulseTotal += frictionMagnitude * tangent;
            }
            else
            {
                float df = Mathf.Sqrt(dynamicFrictionA * dynamicFrictionA + dynamicFrictionB * dynamicFrictionB);
                impulseTotal += -impulseScale * tangent * df;
            }

            if (dynamicBodyA)
                dynamicBodyA.AddVelocity(-(massInverseA * impulseTotal)); // order of operation
            if (dynamicBodyB)
                dynamicBodyB.AddVelocity(massInverseB * impulseTotal);

            //foreach (Vector2 contactPoint in )
            {
                if (dynamicBodyA)
                    dynamicBodyA.ApplyPointForce(physicsResult.contactPoints[0], -impulseTotal);
                if (dynamicBodyB)
                    dynamicBodyB.ApplyPointForce(physicsResult.contactPoints[0], impulseTotal);
            }

            const float percent = 0.8f; // usually 20% to 80%
            Vector2 correction = physicsResult.penetration / (massInverseA + massInverseB) * percent * physicsResult.collisionNormal;
            if (dynamicBodyA)
                dynamicBodyA.SetPosition(dynamicBodyA.GetPosition() - massInverseA * correction);
            if (dynamicBodyB)
                dynamicBodyB.SetPosition(dynamicBodyB.GetPosition() + massInverseB * correction);
        }
    }

    private void CircleCollisions()
    {
        for (int i = 0; i < physicsCircles.Count; i++)
        {
            SpriteRenderer spriteRenderer = physicsCircles[i].GetComponent<SpriteRenderer>();
            spriteRenderer.color = Color.red;

            for (int j = i + 1; j < physicsCircles.Count; j++)
            {
                PhysicsResult physicsResult = physicsCircles[i].IsCollidingWithCircle(physicsCircles[j]);
                if (physicsResult.hasCollided)
                {
                    spriteRenderer.color = Color.green;

                    DynamicBody dynamicBodyA = physicsCircles[i].GetDynamicBody();
                    DynamicBody dynamicBodyB = physicsCircles[j].GetDynamicBody();

                    Vector2 relativeVelocity = dynamicBodyB.GetVelocity() - dynamicBodyA.GetVelocity();
                    float velocityAlongNormal = Vector2.Dot(relativeVelocity, physicsResult.collisionNormal);
                    if (velocityAlongNormal <= 0)
                    {
                        float bounciness = Mathf.Min(physicsCircles[j].GetDynamicBody().GetBounciness());
                        float impulseScale = -(1 - bounciness) * velocityAlongNormal;
                        impulseScale /= (1.0f / dynamicBodyA.GetMass()) + (1.0f / dynamicBodyB.GetMass());
                        Vector2 impulseTotal = impulseScale * physicsResult.collisionNormal;
                        dynamicBodyA.AddVelocity(-1.0f * (1.0f / dynamicBodyA.GetMass() * impulseTotal));
                        dynamicBodyB.AddVelocity(1.0f / dynamicBodyB.GetMass() * impulseTotal);
                    }
                }
            }
        }
    }

    public RayResult RayCast(Vector2 start, Vector2 end)
    {
        RayResult rayResult = new RayResult();
        rayResult.shapesHit = new List<Shape>();
        rayResult.hitPoints = new List<Vector2>();
        
        foreach (Shape shape in physicsShapes)
        {
            shape.CalculateNormals();

            List<Vector2> vertices = shape.GetVertices();
            List<Vector2> normals = shape.GetNormals();

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 vertex0 = vertices[i];
                Vector2 vertex1 = vertices[(i + 1) % vertices.Count];
                Vector2 edge = vertex1 - vertex0;
                Vector2 ray = end - start;

                if (Vector2.Dot(normals[i], ray) <= 0)
                {
                    var d = edge.x * ray.y - edge.y * ray.x;
                    var u = ((start.x - vertex0.x) * edge.y - (start.y - vertex0.y) * edge.x) / d;
                    var t = ((start.x - vertex0.x) * ray.y - (start.y - vertex0.y) * ray.x) / d;

                    if (0 <= u && u <= 1 && 0 <= t && t <= 1)
                    {
                        rayResult.wasHit = true;
                        rayResult.shapesHit.Add(shape);
                        rayResult.hitPoints.Add(vertex0 + t * edge);
                    }
                }
            }
        }

        return rayResult;
    }

    static public bool CheckSphere(Vector2 center, float radius, List<Shape> shapes)
    {
        foreach (Shape shape in shapes)
        {
            if (IntersectsShapeCircle(shape, center, radius))
                return true;
        }

        return false;
    }

    static public bool CheckSphere(Vector2 center, float radius, List<Shape> shapes, ref List<Shape> overlapingShapes)
    {
        overlapingShapes.Clear();
        foreach (Shape shape in shapes)
        {
            if (IntersectsShapeCircle(shape, center, radius))
                overlapingShapes.Add(shape);
        }

        if (overlapingShapes.Count > 0)
            return true;

        return false;
    }


    // POLYGON vs CIRCLE
    static public bool IntersectsShapeCircle(Shape shape, Vector2 center, float radius)
    {
        if (IntersectsShapePoint(shape, center))
            return true;

        // go through each of the vertices, plus
        // the next vertex in the list
        int next = 0;
        List<Vector2> vertices = shape.GetVertices();
        for (int current = 0; current < vertices.Count; current++)
        {
            // get next vertex in list
            // if we've hit the end, wrap around to 0
            next = (current + 1) % vertices.Count;

            Vector2 currentVertex = vertices[current];
            Vector2 nextVertex = vertices[next];

            // check for collision between the circle and
            // a line formed between the two vertices
            if (IntersectsLineCircle(currentVertex, nextVertex, center, radius))
                return true;
        }

        return false;
    }


    // LINE vs CIRCLE
    static public bool IntersectsLineCircle(Vector2 start, Vector2 end, Vector2 center, float radius)
    {
        // is either end INSIDE the circle?
        // if so, return true immediately
        bool inside1 = IntersectsPointCircle(start, center, radius);
        bool inside2 = IntersectsPointCircle(end, center, radius);
        if (inside1 || inside2)
            return true;

        // get length of the line
        //float distX = start.x - end.x;
        //float distY = start.y - end.y;
        //float len = Mathf.Sqrt((distX * distX) + (distY * distY));

        // get dot product of the line and circle
        float dot = Vector2.Dot(center - start, end - start) / (start - end).sqrMagnitude; // why len?
        //float dot = ((center.x - start.x) * (end.x - start.x) + (center.y - start.y) * (end.y - start.y)) / (len * len);

        // find the closest point on the line
        Vector2 closestPoint = new Vector2(start.x + dot * (end.x - start.x), start.y + dot * (end.y - start.y));

        // is this point actually on the line segment?
        // if so keep going, but if not, return false
        if (!IntersectsLinePoint(start, end, closestPoint))
            return false;

        //// get distance to closest point
        //distX = closestX - cx;
        //distY = closestY - cy;
        float distanceSqr = (closestPoint - center).sqrMagnitude;// sqrt((distX * distX) + (distY * distY));

        // is the circle on the line?
        if (distanceSqr <= radius * radius)
            return true;

        return false;
    }


    // LINE vs POINT
    static public bool IntersectsLinePoint(Vector2 start, Vector2 end, Vector2 point)
    {
        // get distance from the point to the two ends of the line
        float d1 = (start - point).magnitude;// dist(px, py, x1, y1);
        float d2 = (end - point).magnitude;// dist(px, py, x2, y2);

        // get the length of the line
        float lineLen = (end - start).magnitude;// dist(x1, y1, x2, y2);

        // since floats are so minutely accurate, add
        // a little buffer zone that will give collision
        float buffer = 0.01f;    // higher # = less accurate

        // if the two distances are equal to the line's
        // length, the point is on the line!
        // note we use the buffer here to give a range, rather
        // than one #
        if (d1 + d2 >= lineLen - buffer && d1 + d2 <= lineLen + buffer)
        {
            return true;
        }
        return false;
    }


    // POINT vs CIRCLE
    static public bool IntersectsPointCircle(Vector2 point, Vector2 center, float radius)
    {
        // get distance between the point and circle's center
        // using the Pythagorean Theorem
        //float distX = px - cx;
        //float distY = py - cy;
        //float distance = sqrt((distX * distX) + (distY * distY));

        // if the distance is less than the circle's 
        // radius the point is inside!
        if ((point - center).sqrMagnitude <= radius * radius)
            return true;

        return false;
    }


    // POLYGON vs POINT
    static public bool IntersectsShapePoint(Shape shape, Vector2 point)
    {
        bool collision = false;

        // go through each of the vertices, plus the next
        // vertex in the list
        int next = 0;
        List<Vector2> vertices = shape.GetVertices();
        for (int current = 0; current < vertices.Count; current++)
        {

            // get next vertex in list
            // if we've hit the end, wrap around to 0
            next = current + 1;
            if (next == vertices.Count) next = 0;

            // get the PVectors at our current position
            // this makes our if statement a little cleaner
            Vector2 currentVertex = vertices[current];    // c for "current"
            Vector2 nextVertex = vertices[next];       // n for "next"

            // compare position, flip 'collision' variable
            // back and forth
            if (((currentVertex.y > point.y && nextVertex.y < point.y) || (currentVertex.y < point.y && nextVertex.y > point.y)) &&
                 (point.x < (nextVertex.x - currentVertex.x) * (point.y - currentVertex.y) / (nextVertex.y - currentVertex.y) + currentVertex.x))
            {
                collision = !collision;
            }
        }
        return collision;
    }

    private void Dynamics()
    {
        foreach (DynamicBody dynamicBody in dynamicBodies)
        {
            dynamicBody.AddVelocity(Vector2.down * gravity * Time.deltaTime);

            Vector2 velocity = dynamicBody.GetVelocity();
            dynamicBody.SetVelocity(velocity *= 1 - (drag * Time.deltaTime));

            float angularVelocity = dynamicBody.GetAngularVelocity();
            dynamicBody.SetAngularVelocity(angularVelocity *= 1 - (angularDrag * Time.deltaTime));
        }
    }

    public List<Shape> GetPhysicsShapes()
    {
        return physicsShapes;
    }

    private void AddPhysicsObject(Shape newShape)
    {
        physicsShapes.Add(newShape);
        DynamicBody dynamicBody = newShape.GetDynamicBody();
        if (dynamicBody)
        {
            dynamicBody.InitializeDynamicBody();
            dynamicBodies.Add(dynamicBody);
        }
    }

    private void RemovePhysicsObject(Shape shape)
    {
        physicsShapes.Remove(shape);
        DynamicBody dynamicBody = shape.GetDynamicBody();
        if (dynamicBody)
            dynamicBodies.Remove(dynamicBody);
    }

    public GameObject Spawn(GameObject gameObject)
    {
        return Spawn(gameObject, Vector2.zero);
    }

    public GameObject Spawn(GameObject gameObject, Vector2 position)
    {
        return Spawn(gameObject, position, Quaternion.identity);
    }

    public GameObject Spawn(GameObject gameObject, Vector2 position, Quaternion rotation)
    {
        return Spawn(gameObject, position, rotation, null);
    }

    public GameObject Spawn(GameObject gameObject, Vector2 position, Quaternion rotation, Transform parent)
    {
        GameObject gameObjectRef = Instantiate(gameObject, position, rotation, parent);
        Shape collider = gameObjectRef.GetComponent<Shape>();
        if (collider)
        {
            collider.InitializeShape();
            AddPhysicsObject(collider);
        }
        return gameObjectRef;
    }

    public void Despawn(GameObject gameObject)
    {
        Shape collider = gameObject.GetComponent<Shape>();
        if (collider)
            RemovePhysicsObject(collider);
        Destroy(gameObject);
    }
}

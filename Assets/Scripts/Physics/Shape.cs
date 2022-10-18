using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Shape : MonoBehaviour
{
    [SerializeField] protected List<Vector2> vertices = new List<Vector2>();
    [SerializeField] protected List<Vector2> normals = new List<Vector2>();
    public List<Shape> childShapes = new List<Shape>();
    DynamicBody dynamicBody;

    void Awake()
    {
        InitializeShape();
    }

    virtual public void InitializeShape()
    {
        int childCount = transform.childCount;
        for (int childIdx = 0; childIdx < childCount; childIdx += 1)
        {
            Shape shape = transform.GetChild(childIdx).GetComponent<Shape>();
            if (shape)
            {
                childShapes.Add(shape);
            }
        }

        dynamicBody = GetComponent<DynamicBody>();
        if (!dynamicBody)
        {
            UpdateVertices();
        }
    }

    public DynamicBody GetDynamicBody()
    {
        return dynamicBody;
    }

    public List<Vector2> GetVertices()
    {
        return vertices;
    }

    public List<Vector2> GetNormals()
    {
        return normals;
    }

    public bool AABB(Shape otherShape)
    {
        if (dynamicBody)
        {
            UpdateVertices();
            foreach (Shape childShape in childShapes)
                childShape.UpdateVertices();
        }

        if (otherShape.dynamicBody)
        {
            otherShape.UpdateVertices();
            foreach (Shape otherChildShape in otherShape.childShapes)
                otherChildShape.UpdateVertices();
        }

        Vector2 minA = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 maxA = new Vector2(float.MinValue, float.MinValue);
        Vector2 minB = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 maxB = new Vector2(float.MinValue, float.MinValue);

        List<Vector2> allVertices = new List<Vector2>();
        allVertices.AddRange(GetChildVertices());
        foreach (Vector2 vertex in allVertices)
        {
            if (vertex.x < minA.x)
                minA.x = vertex.x;
            if (vertex.y < minA.y)
                minA.y = vertex.y;
            if (vertex.x > maxA.x)
                maxA.x = vertex.x;
            if (vertex.y > maxA.y)
                maxA.y = vertex.y;
        }

        List<Vector2> allOtherVertices = new List<Vector2>();
        allOtherVertices.AddRange(otherShape.GetChildVertices());
        foreach (Vector2 vertex in allOtherVertices)
        {
            if (vertex.x < minB.x)
                minB.x = vertex.x;
            if (vertex.y < minB.y)
                minB.y = vertex.y;
            if (vertex.x > maxB.x)
                maxB.x = vertex.x;
            if (vertex.y > maxB.y)
                maxB.y = vertex.y;
        }

        if (minA.x < maxB.x && maxA.x > minB.x && minA.y < maxB.y && maxA.y > minB.y)
        {
            return true;
        }

        return false;
    }

    List<Vector2> GetChildVertices()
    {
        List<Vector2> allVertices = vertices;
        foreach (Shape childShape in childShapes)
        {
            allVertices.AddRange(childShape.GetChildVertices());
        }
        return allVertices;
    }

    public PhysicsResult IsCollidingSAT(Shape shape)
    {
        PhysicsResult physicsResult = new PhysicsResult();
        physicsResult.penetration = float.MaxValue;
        physicsResult.contactPoints = new List<Vector2>();

        foreach (Shape childShape in childShapes)
        {
            foreach (Shape otherChildShape in shape.childShapes)
            {
                //if (this != childShape && shape != otherChildShape)
                {
                    physicsResult = ComparePhysicsResults(physicsResult, childShape.IsCollidingSAT(otherChildShape));
                }
            }
        }

        foreach (Shape childShape in childShapes)
        {
            physicsResult = ComparePhysicsResults(physicsResult, childShape.IsCollidingSAT(shape));
        }

        foreach (Shape otherChildShape in shape.childShapes)
        {
            physicsResult = ComparePhysicsResults(physicsResult, IsCollidingSAT(otherChildShape));
        }

        CalculateNormals();
        shape.CalculateNormals();

        float minA = 0;
        float maxA = 0;
        float minB = 0;
        float maxB = 0;

        List<Vector2> allNormals = new List<Vector2>(normals);
        allNormals.AddRange(shape.normals);

        foreach (Vector2 normal in allNormals)
        {
            CalculateProjectionMinMax(normal, ref minA, ref maxA);
            shape.CalculateProjectionMinMax(normal, ref minB, ref maxB);

            float overlap = Mathf.Min(maxA, maxB) - Mathf.Max(minA, minB);
            if (overlap > 0)
            {
                float potentialPenetration = overlap;
                if (physicsResult.penetration > potentialPenetration)
                {
                    physicsResult.penetration = potentialPenetration;
                    physicsResult.collisionNormal = normal;

                    Vector2 positionA = dynamicBody ? dynamicBody.GetPosition() : new Vector2(transform.position.x, transform.position.y);
                    Vector2 positionB = shape.GetDynamicBody() ? shape.GetDynamicBody().GetPosition() : new Vector2(shape.transform.position.x, shape.transform.position.y);
                    if (Vector2.Dot(positionB - positionA, normal) < 0)
                        physicsResult.collisionNormal = -physicsResult.collisionNormal;
                }
            }
            else
            {
                return physicsResult;
            }
        }

        physicsResult.hasCollided = true;
        physicsResult.contactPoints.AddRange(GetContactPoints(shape, physicsResult.collisionNormal));

        return physicsResult;
    }

    private static PhysicsResult ComparePhysicsResults(PhysicsResult physicsResult, PhysicsResult otherPhysicsResult)
    {
        if (otherPhysicsResult.hasCollided)
        {
            physicsResult.hasCollided = true;
            physicsResult.contactPoints.AddRange(otherPhysicsResult.contactPoints);

            if (otherPhysicsResult.penetration < physicsResult.penetration)
            {
                physicsResult.penetration = otherPhysicsResult.penetration;
                physicsResult.collisionNormal = otherPhysicsResult.collisionNormal;
            }
        }

        return physicsResult;
    }

    private void CalculateProjectionMinMax(Vector2 normal, ref float projectionMin, ref float projectionMax)
    {
        float min = float.MaxValue;
        float max = float.MinValue;

        foreach (Vector2 vertex in vertices)
        {
            float dot = Vector2.Dot(normal, vertex);
            if (dot < min)
                min = dot;
            if (dot > max)
                max = dot;
        }

        projectionMin = min;
        projectionMax = max;
    }

    public void CalculateNormals()
    {
        UpdateVertices();
        normals.Clear();

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 face = vertices[(i + 1) % vertices.Count] - vertices[i];
            normals.Add(new Vector2(face.y * -1, face.x).normalized);
        }
    }

    protected Vector2 RotateVectorAroundPoint(Vector2 vector, Vector2 point, float angle)
    {
        return Quaternion.Euler(0, 0, angle) * (vector - point) + new Vector3(point.x, point.y);
    }

    List<Vector2> GetContactPoints(Shape otherShape, Vector2 normal)
    {
        List<Vector2> referenceEdge;
        List<Vector2> incidentEdge;
        List<Vector2> collisionEdgeA = GetCollisionEdge(normal);
        List<Vector2> collisionEdgeB = otherShape.GetCollisionEdge(-normal);

        if (Mathf.Abs(Vector2.Dot(collisionEdgeA[1] - collisionEdgeA[0], normal)) > Mathf.Abs(Vector2.Dot(collisionEdgeB[1] - collisionEdgeB[0], normal)))
        {
            referenceEdge = collisionEdgeB;
            incidentEdge = collisionEdgeA;
        }
        else
        {
            referenceEdge = collisionEdgeA;
            incidentEdge = collisionEdgeB;
        }

        // clip the incident edge from one side
        for (int i = 0; i < incidentEdge.Count; i++)
        {
            Vector2 referenceVector = (referenceEdge[1] - referenceEdge[0]).normalized;
            float referenceDot = Vector2.Dot(referenceVector, referenceEdge[0]);
            float dot = Vector2.Dot(referenceVector, incidentEdge[i]);

            if (dot < referenceDot)
                incidentEdge[i] += referenceVector * (referenceDot - dot);
        }

        // and then the other
        for (int i = 0; i < incidentEdge.Count; i++)
        {
            Vector2 referenceVector = (referenceEdge[0] - referenceEdge[1]).normalized;
            float referenceDot = Vector2.Dot(referenceVector, referenceEdge[1]);
            float dot = Vector2.Dot(referenceVector, incidentEdge[i]);

            if (dot < referenceDot)
                incidentEdge[i] += referenceVector * (referenceDot - dot);
        }

        // remove points outside the shape
        for (int i = 0; i < incidentEdge.Count; i++)
        {
            Vector2 face = referenceEdge[1] - referenceEdge[0];
            Vector2 referenceNormal = new Vector2(face.y * -1, face.x).normalized;

            float referenceDot = Vector2.Dot(referenceNormal, referenceEdge[0]);
            float dot = Vector2.Dot(referenceNormal, incidentEdge[i]);

            if (dot > referenceDot)
                incidentEdge.Remove(incidentEdge[i]);
        }

        return incidentEdge;
    }

    List<Vector2> GetCollisionEdge(Vector2 normal)
    {
        int index = 0;
        float max = float.MinValue;

        for (int i = 0; i < vertices.Count; i++)
        {
            float dot = Vector2.Dot(normal, vertices[i]);
            if (dot > max)
            {
                max = dot;
                index = i;
            }
        }

        List<Vector2> edgeA = new List<Vector2> { vertices[index], vertices[(index + 1) % vertices.Count] };
        List<Vector2> edgeB = new List<Vector2> { vertices[index == 0 ? vertices.Count - 1 : index - 1], vertices[index] };

        if (Mathf.Abs(Vector2.Dot(edgeA[1] - edgeA[0], normal)) > Mathf.Abs(Vector2.Dot(edgeB[1] - edgeB[0], normal)))
            return edgeB;
        else
            return edgeA;
    }

    public bool CoordinateIsInside(Vector2 coordinate)
    {
        foreach (Shape childShape in childShapes)
        {
            childShape.CoordinateIsInside(coordinate);
        }

        CalculateNormals();

        float min = 0;
        float max = 0;

        foreach (Vector2 normal in normals)
        {
            CalculateProjectionMinMax(normal, ref min, ref max);
            float coordinateProjection = Vector2.Dot(normal, coordinate);

            float overlap = Mathf.Min(max, coordinateProjection) - Mathf.Max(min, coordinateProjection);
            if (overlap <= 0)
            {
                return false;
            }
        }

        return true;
    }

    protected abstract void UpdateVertices();
}

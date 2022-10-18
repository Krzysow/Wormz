using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : Shape
{
    float width;
    float height;
    PhysicsCheck physics;

    // Start is called before the first frame update
    void Awake()
    {
        InitializeShape();
    }

    // Update is called once per frame
    void Update()
    {

    }

    override public void InitializeShape()
    {
        width = transform.lossyScale.x;
        height = transform.lossyScale.y;
        physics = FindObjectOfType<PhysicsCheck>();
        base.InitializeShape();
    }

    public bool IsCollidingWithSquare(Square square)
    {
        if (transform.position.x - width * 0.5f < square.transform.position.x + square.width * 0.5f
            && transform.position.x + width * 0.5f > square.transform.position.x - square.width * 0.5f
            && transform.position.y - height * 0.5f < square.transform.position.y + square.height * 0.5f
            && transform.position.y + height * 0.5f > square.transform.position.y - square.height * 0.5f)
            return true;

        return false;
    }

    protected override void UpdateVertices()
    {
        vertices.Clear();
        Vector2 position = new Vector2(transform.position.x, transform.position.y);

        vertices.Add(position + Vector2.left * width * 0.5f + Vector2.up * height * 0.5f);
        vertices.Add(position + Vector2.right * width * 0.5f + Vector2.up * height * 0.5f);
        vertices.Add(position + Vector2.right * width * 0.5f + Vector2.down * height * 0.5f);
        vertices.Add(position + Vector2.left * width * 0.5f + Vector2.down * height * 0.5f);

        for (int i = 0; i < vertices.Count; i++)
        {
            DynamicBody dynamicBody = GetComponent<DynamicBody>();
            float angle = dynamicBody ? dynamicBody.GetOrientation() : transform.eulerAngles.z;
            vertices[i] = RotateVectorAroundPoint(vertices[i], position, angle);
        }
    }

    public void Destructable(Vector2 explosionCenter, float explosionRadius, int destructionStep)
    {
        if (destructionStep > 0 && width > 2 && height > 2)
        {
            List<Square> allNewSquares = new List<Square>();

            Vector2 newPosition = new Vector2(transform.position.x - width * 0.25f, transform.position.y + height * 0.25f);
            newPosition = RotateVectorAroundPoint(newPosition, transform.position, transform.rotation.eulerAngles.z);
            GameObject newSquare = physics.Spawn(gameObject, newPosition, transform.rotation, transform.root);
            newSquare.transform.localScale = new Vector2(width * 0.5f, height * 0.5f);
            newSquare.GetComponent<Square>().InitializeShape();
            allNewSquares.Add(newSquare.GetComponent<Square>());

            newPosition = new Vector2(transform.position.x + width * 0.25f, transform.position.y + height * 0.25f);
            newPosition = RotateVectorAroundPoint(newPosition, transform.position, transform.rotation.eulerAngles.z);
            newSquare = physics.Spawn(gameObject, newPosition, transform.rotation, transform.root);
            newSquare.transform.localScale = new Vector2(width * 0.5f, height * 0.5f);
            newSquare.GetComponent<Square>().InitializeShape();
            allNewSquares.Add(newSquare.GetComponent<Square>());

            newPosition = new Vector2(transform.position.x - width * 0.25f, transform.position.y - height * 0.25f);
            newPosition = RotateVectorAroundPoint(newPosition, transform.position, transform.rotation.eulerAngles.z);
            newSquare = physics.Spawn(gameObject, newPosition, transform.rotation, transform.root);
            newSquare.transform.localScale = new Vector2(width * 0.5f, height * 0.5f);
            newSquare.GetComponent<Square>().InitializeShape();
            allNewSquares.Add(newSquare.GetComponent<Square>());

            newPosition = new Vector2(transform.position.x + width * 0.25f, transform.position.y - height * 0.25f);
            newPosition = RotateVectorAroundPoint(newPosition, transform.position, transform.rotation.eulerAngles.z);
            newSquare = physics.Spawn(gameObject, newPosition, transform.rotation, transform.root);
            newSquare.transform.localScale = new Vector2(width * 0.5f, height * 0.5f);
            newSquare.GetComponent<Square>().InitializeShape();
            allNewSquares.Add(newSquare.GetComponent<Square>());

            foreach (Square square in allNewSquares)
            {
                if (PhysicsCheck.IntersectsShapeCircle(square, explosionCenter, explosionRadius))
                {
                    square.Destructable(explosionCenter, explosionRadius, destructionStep - 1);
                }
            }
        }
        physics.Despawn(gameObject);
    }
}

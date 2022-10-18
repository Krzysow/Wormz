using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGrid : MonoBehaviour
{
    public Vector2 worldSize = new Vector2(256, 128);
    public float nodeSpacing = 1f;
    
    public Node[,] nodes;
    int sizeX, sizeY;
    float nodeHalfSize;

    public bool drawGrid;

    public void Start()
    {
        nodeHalfSize = nodeSpacing * 0.5f;
        sizeX = Mathf.RoundToInt(worldSize.x / nodeSpacing);
        sizeY = Mathf.RoundToInt(worldSize.y / nodeSpacing);
        CreateGrid();
    }

    void CreateGrid()
    {
        nodes = new Node[sizeX, sizeY];
        Vector3 worldCorner = transform.position - Vector3.right * worldSize.x / 2 - Vector3.up * worldSize.y / 2;

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                Vector3 worldPoint = worldCorner + Vector3.right * (x * nodeSpacing + nodeHalfSize) + Vector3.up * (y * nodeSpacing + nodeHalfSize);
                bool walkable = !PhysicsCheck.CheckSphere(worldPoint, nodeHalfSize, FindObjectOfType<PhysicsCheck>().GetPhysicsShapes());
                nodes[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    public void UpdateGrid(Vector2 center, float radius)
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if ((nodes[x, y].worldPosition - center).sqrMagnitude <= radius * radius)
                    nodes[x, y].walkable = true;
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.x + x;
                int checkY = node.y + y;

                if (checkX >= 0 && checkX < sizeX && checkY >= 0 && checkY < sizeY)
                {
                    neighbours.Add(nodes[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    public Node NodeRetriever(Vector2 coordinate)
    {
        float xFraction = Mathf.Clamp01((coordinate.x + worldSize.x * 0.5f) / worldSize.x);
        float yFraction = Mathf.Clamp01((coordinate.y + worldSize.y * 0.5f) / worldSize.y);

        int x = Mathf.RoundToInt((sizeX - 1) * xFraction);
        int y = Mathf.RoundToInt((sizeY - 1) * yFraction);
        return nodes[x, y];
    }

    public List<Node> path = new List<Node>();
    void OnDrawGizmos()
    {
        if (drawGrid)
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(worldSize.x, worldSize.y, 1));

            if (nodes != null)
            {
                foreach (Node n in nodes)
                {
                    Gizmos.color = (n.walkable) ? Color.white : Color.red;
                    if (path != null)
                        if (path.Contains(n))
                            Gizmos.color = Color.black;
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeSpacing - .1f));
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : Bullet
{
    PathFinding pathFinding;
    MapGrid grid;
    List<Node> path = new List<Node>();
    float speed = 10f;

    // Start is called before the first frame update
    void Start()
    {
        float farthest = 0;
        Vector2 target = Vector2.zero;
        grid = FindObjectOfType<MapGrid>();
        pathFinding = FindObjectOfType<PathFinding>();
        foreach (Character character in FindObjectsOfType<Character>())
        {
            float possibleFarthest = (character.transform.position - transform.position).sqrMagnitude;
            if (possibleFarthest > farthest)
            {
                farthest = possibleFarthest;
                target = character.transform.position;
            }
        }

        Node targetNode = grid.NodeRetriever(target);
        if (!targetNode.walkable)
        {
            target = Vector2.zero;
            foreach (Node node in grid.GetNeighbours(targetNode))
            {
                if (node.walkable)
                {
                    target = node.worldPosition;
                }
            }
        }

        path = pathFinding.Star(transform.position, target);
    }

    // Update is called once per frame
    void Update()
    {
        if (path.Count > 0)
        {
            Node targetNode = path[0];
            transform.position += (new Vector3(targetNode.worldPosition.x, targetNode.worldPosition.y) - transform.position).normalized * Time.deltaTime * speed;
            if (grid.NodeRetriever(transform.position) == targetNode)
                path.RemoveAt(0);
        }
        else
            Explode();
    }
}

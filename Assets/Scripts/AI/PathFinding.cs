using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    MapGrid grid;

    private void Awake()
    {
        grid = GetComponent<MapGrid>();
    }

    public List<Node> Star(Vector2 start, Vector2 end)
    {
        Node startNode = grid.NodeRetriever(start);
        Node endNode = grid.NodeRetriever(end);

        List<Node> openSetForwards = new List<Node>();
        List<Node> openSetBackwards = new List<Node>();
        List<Node> closedSetForwards = new List<Node>();
        List<Node> closedSetBackwards = new List<Node>();
        openSetForwards.Add(startNode);
        openSetBackwards.Add(endNode);

        while (openSetForwards.Count > 0 && openSetBackwards.Count > 0)
        {
            StarStep(ref openSetForwards, ref closedSetForwards, endNode, true);
            StarStep(ref openSetBackwards, ref closedSetBackwards, startNode, false);

            foreach (Node node in closedSetForwards)
            {
                if (closedSetBackwards.Contains(node))
                {
                    List<Node> path = RetracePath(startNode, node, true);
                    path.AddRange(RetracePath(endNode, node, false));
                    grid.path = path;
                    return path;
                }
            }
        }
        return new List<Node>();
    }

    void StarStep(ref List<Node> openSet, ref List<Node> closedSet, Node endNode, bool isForwards)
    {
        Node node = openSet[0];
        for (int i = 1; i < openSet.Count; i++)
        {
            if (openSet[i].fCost < node.fCost || openSet[i].fCost == node.fCost && openSet[i].hCost < node.hCost)
            {
                node = openSet[i];
            }
        }

        openSet.Remove(node);
        closedSet.Add(node);

        foreach (Node neighbour in grid.GetNeighbours(node))
        {
            if (!neighbour.walkable || closedSet.Contains(neighbour))
            {
                continue;
            }

            int newCostToNeighbour = node.gCost + CalculateCost(node, neighbour);
            if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
            {
                neighbour.gCost = newCostToNeighbour;
                neighbour.hCost = CalculateCost(neighbour, endNode);
                if (isForwards)
                    neighbour.parentToStart = node;
                else
                    neighbour.parentToEnd = node;

                if (!openSet.Contains(neighbour))
                {
                    openSet.Add(neighbour);
                }
            }
        }
    }

    static int CalculateCost(Node nodeA, Node nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.x - nodeB.x);
        int distanceY = Mathf.Abs(nodeA.y - nodeB.y);

        if (distanceX > distanceY)
        {
            return 14 * distanceY + 10 * (distanceX - distanceY);
        }
        return 14 * distanceX + 10 * (distanceY - distanceX);
    }

    static List<Node> RetracePath(Node startNode, Node endNode, bool isForwards)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            if (isForwards)
                currentNode = currentNode.parentToStart;
            else
                currentNode = currentNode.parentToEnd;
        }

        if (isForwards)
            path.Reverse();

        return path;
    }
}

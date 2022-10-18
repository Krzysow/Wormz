using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool walkable;
    public Vector2 worldPosition;
    public int x;
    public int y;

    public int gCost;
    public int hCost;
    public Node parentToStart;
    public Node parentToEnd;

    public Node(bool _Walkable, Vector2 _WorldPosition, int _x, int _y)
    {
        walkable = _Walkable;
        worldPosition = _WorldPosition;
        x = _x;
        y = _y;
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
}

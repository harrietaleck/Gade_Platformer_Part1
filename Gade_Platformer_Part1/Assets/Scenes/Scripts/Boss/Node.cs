
using UnityEngine;

public class Node
{
    //Declare variables
    public string ID;
    public Transform Waypoint;
    private Node[] paths;
    private int pathCount;

    //Create a constant to store the maximum number of paths
    public Node(string id, Transform waypoint)
    {
        ID = id;
        Waypoint = waypoint;

        paths = new Node[8];
        pathCount = 0;
    }
    //Create a method to get the paths
    public Node GetPaths(int index)
    {
        return paths[index];
    }
    //Add the paths to the node
    public void AddPaths(Node nodee)
    {
        paths[pathCount] = nodee;
        pathCount++;
    }
    //Create a property to get the path count
    public int PathCount
    {
        get 
        {
            return pathCount; 
        }
    }
}

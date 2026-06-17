using System.Collections.Generic;
using UnityEngine;

public class GraphADT
{
    //Declare variables
    private Node[] nodes;
    private int nodeCount;

    //Create a construct method
    public GraphADT()
    {
        nodes = new Node[100];
        nodeCount = 0;
    }
    //Create a method to addthe node
    public void AddNode(Node nodee)
    {
        nodes[nodeCount] = nodee;
        nodeCount++;
    }
    //Create a method to get id of the node
    public Node GetNode(string id)
    {
        for (int i = 0; i < nodeCount; i++)
        {
            if (nodes[i].ID == id)
            {
                return nodes[i];
            }
        }

        return null;
    }
    //Create a method to add the location and destination to the node
    public void AddEdge(string location, string destination)
    {
        Node a = GetNode(location);
        Node b = GetNode(destination);

        if (a == null || b == null)
        {
            return;
        }
        a.AddPaths(b);
        b.AddPaths(a);
    }
}

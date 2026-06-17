
using UnityEngine;

public class GraphSetup : MonoBehaviour
{
    //Declare Variables
    public WaypointsBOSS[] waypoints;
    public static GraphADT Graphs;

    private void Awake()
    {
        Graphs = new GraphADT();
        //Create a for loop to add the nodes to the graph
        for (int i = 0; i < waypoints.Length; i++)
        {
            Graphs.AddNode(new Node(waypoints[i].nodeID,waypoints[i].transform));
        }
        //Create the edges between the nodes
        Graphs.AddEdge("PathA", "PathB");
        Graphs.AddEdge("PathB", "PathC");
        Graphs.AddEdge("PathC", "PathD");
        Graphs.AddEdge("PathD", "PathE");

        Graphs.AddEdge("PathB", "PathF");
        Graphs.AddEdge("PathF", "PathH");
        Graphs.AddEdge("PathH", "PathI");
        Graphs.AddEdge("PathI", "PathG");
        Graphs.AddEdge("PathG", "PathC");
    }
}

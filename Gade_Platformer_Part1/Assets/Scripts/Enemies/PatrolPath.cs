using UnityEngine;

public class PatrolPath : MonoBehaviour
{
    /*public Patrollnode First;
    public Patrollnode Last;

    public void AddLast(Transform value)
    {
        var node = new Patrollnode(value);
        if (First == null)
        {
            First = node;
            Last = node;
        }
        else
        {
            Last.Next = node;
            Last = node;
        }
    }

    public Patrollnode GetNextNode(Patrollnode current)
    {
        if (current == null)
            return First;

        return current.Next ?? First;
    }

    public void BuildFromList(System.Collections.Generic.List<Transform> points)
    {
        First = null;
        Last = null;

        if (points == null)
            return;

        for (int i = 0; i < points.Count; i++)
        {
            if (points[i] != null)
                AddLast(points[i]);
        }
    }*/
    public Patrollnode First;
    public Patrollnode Last;

    public void AddLast(Transform point)
    {
        Patrollnode node =
            new Patrollnode(point);

        if (First == null)
        {
            First = node;
            Last = node;
        }
        else
        {
            Last.Next = node;
            Last = node;
        }
    }

    public Patrollnode GetNextNode(Patrollnode current)
    {
        if (current == null)
            return First;

        if (current.Next == null)
            return First;

        return current.Next;
    }
}

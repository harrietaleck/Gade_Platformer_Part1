using UnityEngine;

public class Patrollnode
{
    public Transform Value;
    public Patrollnode Next;

    public Patrollnode(Transform value)
    {
        Value = value;
        Next = null;
    }
}

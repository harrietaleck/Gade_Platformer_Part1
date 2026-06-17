using UnityEngine;

public class MOVEplatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;

    public float speed = 2f;
    private bool activated = false;
    private bool movingToB = true;

    void Update()
    {
        //Move the platform only if activated
        if (!activated) return;

        Transform target = movingToB ? pointB: pointA;
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        //Switch direction when the platform reaches the target point
        if (Vector3.Distance(transform.position, target.position) < 0.01f)
        {
            movingToB = !movingToB; // Switch direction
        }
    }
    //Set the boolean as true to move the platform
    public void Activate()
    {
        activated = true;
    }
}

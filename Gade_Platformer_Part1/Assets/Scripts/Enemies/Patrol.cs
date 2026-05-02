using UnityEngine;

public class Patrol : MonoBehaviour
{
    public Transform[] patrolPoints;
    private int currentPointIndex = 0;

    public Transform GetNextPoint()
    {
        //Return the nect postion point and increment the index
        if (patrolPoints.Length == 0) return null;
        Transform nextPoint = patrolPoints[currentPointIndex];
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length; //Loop back to the first point
        return nextPoint;
    }
    public Transform GetCurrentPoint()
    {
        //Return the current position point without incrementing the index
        if (patrolPoints.Length == 0) return null;
        return patrolPoints[currentPointIndex];
    }

}

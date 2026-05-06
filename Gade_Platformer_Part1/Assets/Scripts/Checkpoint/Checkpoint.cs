using System.Collections.Generic;
using UnityEngine;

// Pure Stack ADT class (not a MonoBehaviour component)
public class Checkpoint
{
    //Use the standardize stat data structure to store checkpoints
    private Stack<CheckpointData> stack = new Stack<CheckpointData>();

    //Add a new checkpoint using the PUSH function
    public void Push(CheckpointData data)
    {
        stack.Push(data);
    }

    //Remove the latest checkpoint using the POP function
    public CheckpointData Pop()
    {
        if (stack.Count == 0)
        {
            Debug.LogWarning("Stack is empty!");
            return new CheckpointData(Vector3.zero, 0, 0);
        }

        return stack.Pop();
    }

    //Get the latest checkpoint data without removing it using the PEEK function
    public CheckpointData Peek()
    {
        if (IsEmpty())
        {
            Debug.LogWarning("Stack is empty!");
            return new CheckpointData(Vector3.zero, 0, 0);
        }

        return stack.Peek();
    }
    //Chcek if the stack list is empty
    public bool IsEmpty()
    {
        if (stack.Count == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
//Store the checkpoints data
[System.Serializable]
public class CheckpointData
{
    //Declare the variables
    public Vector3 position;
    public int lives;
    public int score;

    //Assign the varibales using the constructor
    public CheckpointData(Vector3 positionPLY, int livesPLY, int scorePLY)
    {
        this.position = positionPLY;
        this.lives = livesPLY;
        this.score = scorePLY;
    }
}

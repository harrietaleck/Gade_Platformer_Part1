using UnityEngine;

public class DecreaseBoosts : MonoBehaviour
{
    GameManager gameManager;
    Player Player;
    private float timer;

    void OnTriggerEnter(Collider other)
    {
        Player = other.GetComponent<Player>();

        if (Player != null)
        {
            if (other.gameObject.CompareTag("Mud/waterSPEDDOWN"))
            {
                Player.moveSpeed = 1f;
                timerCountDown();
            }
            if (other.gameObject.CompareTag("FreezePoint"))
            {
                Player.moveSpeed = 0f;
                timerCountDown();
            }
        }
    }
    void timerCountDown()
    {
        //Increment the timer
        timer = Time.time;
        //When the timer reaches 5 then reset the speed and timer
        if (timer >= 5f)
        {
            Player.moveSpeed = 6f;
            timer = 0f;
        }
    }
}

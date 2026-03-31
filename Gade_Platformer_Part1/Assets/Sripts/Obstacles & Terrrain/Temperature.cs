using System.Threading;
using UnityEngine;

public class Temperature : MonoBehaviour
{
    /*public int temperatureChangeDOWN = 10;
    public int temperatureCount = 30;
    private float timerCOUNTER;
    Player player;
    GameManager gameManager;
    public GameObject temperatureIncreaseBTN;
    PlayerCheckpointDatat playerCheckpointDatat;

    private void Awake()
    {
        //Assign the player varable to this script
        player = GetComponent<Player>();
        playerCheckpointDatat = GetComponent<PlayerCheckpointDatat>();
        temperatureIncreaseBTN.SetActive(false);
    }
    void Update()
    {
       if (playerCheckpointDatat == null) return;
        if (player == null) return;
        
        //Decreate the temperature every 5 seconds
        timerCOUNTER += Time.deltaTime;
        Debug.Log("Timer for temp: " + timerCOUNTER);
        if (timerCOUNTER >= 5f)
        {
            //GameManager.Instance.AddScore(temperatureChange);
            temperatureCount -= temperatureChangeDOWN;
            Debug.Log("Temperature decreased! Current temperature: " + temperatureCount);
            timerCOUNTER = 0f;
        }

        /*if (temperatureCount <= 20)
            player.moveSpeed = 1f;
        temperatureIncreaseBTN.SetActive(true);
        if (temperatureCount == 0)
            player.moveSpeed = 0f;
        temperatureIncreaseBTN.SetActive(false);
        playerCheckpointDatat.Death();
    }
    public void IncreaseeTemp()
    {
        if (gameManager == null) return;

        if (gameManager.thermalStones > 0)
            temperatureCount += 20;
        gameManager.DecreaseThermalStone(1);
        Debug.Log("Temperature increased! Current temperature: " + temperatureCount);
    }*/

}

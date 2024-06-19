using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

/// <summary>
/// Manage game state, scores, and respawns during gameplay. Only one GamePlayManager should be in the scene
/// reference this script to access gameplay objects like UI
/// </summary>
public class GamePlayManager : MonoBehaviour, IOnEventCallback
{
    #region Variables
    //store gameplay states
    public enum State { Intro, Gameplay, Pause, Ending }

    [Header("Game Settings")]
    public State gameState = State.Intro;
    //store score data
    public int player1Score, player2Score;
    public int player1Ammo, player2Ammo;

    public int maxScore;
    public float gameDuration;
    public bool isOnline = false;

    //store respawns positions
    public Transform[] respawnPositions;
    private int lastSpawnP1, lastSpawnP2;

    [Header("Player Settings")]
    public GameObject player1Prefab;
    public GameObject player2Prefab;
    //store player object references
    public GameObject player1;
    public GameObject player2;

    

    //store player data
    public string player1Name, player2Name;

    [Header("UI Settings")]
    //store UI references
    public TMP_Text player1ScoreText;
    public TMP_Text player2ScoreText;
    public TMP_Text player1AmmoText;
    public TMP_Text player2AmmoText;
    public TMP_Text timerText;
    public TMP_Text messageText;

    //Define Photon events
    private const byte TIMER_UPDATE = 1;
    private const byte UPDATE_NAMES = 2;
    private const byte UPDATE_MAXSCORE = 3;


    [Header("Other Components")]
    //store a timer
    public Timer gameTimer;
    public PhotonView view;


    #endregion

    #region Raise Event Code
    //enable or disable the ability to listen to events
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    //listen to events, and respond locally
    public void OnEvent(EventData data)
    {
        if(data.Code == TIMER_UPDATE) //check what even you recieved
        {
            object[] localData = (object[])data.CustomData; //get the data from the event and make it local
            timerText.text = (string)localData[0]; //using the data, remebering to cast the data as the proper type
        }
        if(data.Code == UPDATE_NAMES) //check what even you recieved
        {
            object[] localData = (object[])data.CustomData; //get the data from the event and make it local
            player1Name = (string)localData[0];
            player2Name = (string)localData[1];
            //update the score UI
            player1ScoreText.text = player1Name + " : " + player1Score;
            player2ScoreText.text = player2Name + " : " + player2Score;
        }
        if(data.Code == UPDATE_MAXSCORE)
        {
            object[] localData = (object[])data.CustomData;
            maxScore = (int)localData[0];
        }
    }
    #endregion

    #region Singleton
    public static GamePlayManager instance; //create a static reference to ourselves

    //assign this object to the reference
    private void Awake()
    {
        //if (instance != null) Destroy(gameObject); //optional for if you only ever want 1 singleton
        instance = this;
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        //if a Gamemaster exists, then use it's data
        if(GameMaster.instance != null)
        {
            maxScore = GameMaster.instance.saveData.maxKills; //remember in your game, you may change maxKills to something else
            gameDuration = GameMaster.instance.saveData.maxRoundTime;
            if(!isOnline)
            {
                player1Name = GameMaster.instance.currentPlayer1.playerName;
                player2Name = GameMaster.instance.currentPlayer2.playerName;
            }
            if (PhotonNetwork.IsMasterClient)
            {
                //update or create your data locallly
                player1Name = PhotonNetwork.CurrentRoom.GetPlayer(1).NickName;
                player2Name = PhotonNetwork.CurrentRoom.GetPlayer(2).NickName;
                //format your data into an array of objects
                object[] data = new object[] { player1Name, player2Name };
                PhotonNetwork.RaiseEvent(UPDATE_NAMES, data, RaiseEventOptions.Default, SendOptions.SendUnreliable);

                maxScore = 30;
                object[] scoreData = new object[] { maxScore };
                PhotonNetwork.RaiseEvent(UPDATE_MAXSCORE, scoreData, RaiseEventOptions.Default, SendOptions.SendReliable);
            }
        }
        view = GetComponent<PhotonView>();
        SetupGame();
    }

    // Update is called once per frame
    void Update()
    {
        if(gameState == State.Gameplay)
        {
            DisplayTimer();
        }
    }
    [PunRPC]
    void SyncGameSettings(int maxScore, float gameDuration)
    {
        this.maxScore = maxScore;
        this.gameDuration = gameDuration;
    }

    [PunRPC]
    public void UpdateAmmo(int playerNumber, int amount)
    {
        if (gameState != State.Gameplay) return;

        if (playerNumber == 1)
        {
            if (GameMaster.instance != null) GameMaster.instance.currentPlayer1.ammoCount += amount;
            player1Score += amount;
        }
        else
        {
            if (GameMaster.instance != null) GameMaster.instance.currentPlayer2.ammoCount += amount;
            player2Score += amount;
        }
        player1ScoreText.text = player1Name + " : " + player1Score;
        player2ScoreText.text = player2Name + " : " + player2Score;

        if (player1Score >= maxScore || player2Score >= maxScore && gameState != State.Ending)
        {
            EndGame();
            gameState = State.Ending;
            StartCoroutine(SceneDelay());
        }
    }

    private IEnumerator SceneDelay()
    {
        if(player1Score >= maxScore || player2Score >= maxScore && gameState != State.Ending)
        {
            EndGame();
            gameState = State.Ending;
            yield return new WaitForSeconds(5);
            SceneManager.LoadScene("MainMenu");
        }
    }


    //respawns - find player to respawn, deactivate controls, decrease lives*, 
    //disable player, move to spawn point, reset data, reactivate player, check for end of game if relevant
    [PunRPC]
    public void SpawnPlayer(int playerNumber)
    {
        Debug.Log("Attempt to spawn");

        Debug.Log("Attempt to spawn as Master Client");
        //Pick which player to spawn
        GameObject currentPlayer;
        if (playerNumber == 1) currentPlayer = player1;
        else currentPlayer = player2;

        //disable any scripts
        currentPlayer.GetComponent<CCMovement>().enabled = false;
        //add all the scripts you want to disable here.

        //call the reactivation of the player
        StartCoroutine(FinishSpawn(currentPlayer));

    }
    private IEnumerator FinishSpawn(GameObject currentPlayer)
    {
        //wait incase animations need to play
        yield return new WaitForSeconds(2f);

        //pick a random number to spawn from
        int spawnIndex = Random.Range(0, respawnPositions.Length);

        //check if the spawn index matches either previous spawn, if so, reroll
        while(spawnIndex == lastSpawnP1 || spawnIndex == lastSpawnP2)
        {
            spawnIndex = Random.Range(0, respawnPositions.Length);
        }

        //move player to spawn point
        currentPlayer.transform.position = respawnPositions[spawnIndex].position;
        //store new last position
        if (currentPlayer == player1) lastSpawnP1 = spawnIndex;
        else lastSpawnP2 = spawnIndex;

        yield return new WaitForSeconds(0.5f);
        //reactivate all scripts and reset all data
        currentPlayer.GetComponent<CCMovement>().enabled = true;
        //add all the other scripts you need to activate or reset e.g.
        //health reset, animation state changed, weapons reactivated etc
        currentPlayer.GetComponentInChildren<Animator>().SetBool("Dead", false);
        PlayerHealth healthScript = currentPlayer.GetComponent<PlayerHealth>();
        healthScript.health = healthScript.maxHealth;
        healthScript.dead = false;
    }

    //Display the timer
    void DisplayTimer()
    {
        if (!isOnline)
        {
            timerText.text = gameTimer.GetFormattedTime();
        }
        else if (PhotonNetwork.IsMasterClient)
        {
            string formattedTime = gameTimer.GetFormattedTime();
            object[] data = new object[] { formattedTime };
            PhotonNetwork.RaiseEvent(TIMER_UPDATE, data, RaiseEventOptions.Default, SendOptions.SendUnreliable);
            timerText.text = formattedTime;
        }
    }

    //run intro sequence
    void SetupGame()
    {
        //set game state
        gameState = State.Intro;

        if (PhotonNetwork.IsMasterClient)
        {
            view.RPC("SyncGameSettings", RpcTarget.All, maxScore, gameDuration);
        }

        if (!isOnline)
        {
            //Spawn players for the first time
            int spawnIndex = Random.Range(0, respawnPositions.Length);
            player1 = Instantiate(player1Prefab, respawnPositions[0].position, respawnPositions[0].rotation);
            lastSpawnP1 = spawnIndex;

            //repeat for player 2
            spawnIndex = Random.Range(0, respawnPositions.Length);

            int attempts = 0;
            while (spawnIndex == lastSpawnP1 && attempts < 3)
            {
                Random.Range(0, respawnPositions.Length);
                attempts++;
            }

            player2 = Instantiate(player2Prefab, respawnPositions[1].position, respawnPositions[1].rotation);
            lastSpawnP2 = spawnIndex;
        }
        else if(PhotonNetwork.LocalPlayer.ActorNumber == 1)
        {
            PhotonNetwork.Instantiate(player1Prefab.name, respawnPositions[0].position, Quaternion.identity);
        }
        else if (PhotonNetwork.LocalPlayer.ActorNumber == 2) 
        {
            PhotonNetwork.Instantiate(player2Prefab.name, respawnPositions[1].position, Quaternion.identity);
        }

        //player names displayed in UI
        player1ScoreText.text = player1Name + " : " + 0;
        player2ScoreText.text = player2Name + " : " + 0;

        //hide timer
        timerText.text = "";

        //display start message
        messageText.text = "Get Ready";
        //run the intro sequence coroutine
        StartCoroutine(IntroSequence());

    }
    //intro coroutine
    private IEnumerator IntroSequence()
    {
        //show the timer message
        int timer = 4;
        
        //count down from three (displaying time as we go)
        while(timer > 0)
        {
            yield return new WaitForSeconds(1);
            timer -= 1;
            messageText.text = "Starting in " + timer;
        }
        //Activate game play
        messageText.text = "Go!";
        yield return new WaitForSeconds(1);
        //hide messages
        messageText.text = "";

        //run gameplay timer
        gameTimer.countDown = true;
        gameTimer.maxTime = gameDuration;
        gameTimer.StartTimer();

        gameState = State.Gameplay;
    }



    //End game - freeze players, tally scores, display results or move to next scene
    [PunRPC]
    public void EndGame()
    {
        //FreezePlayers
        player1.GetComponent<CCMovement>().enabled = false;
        player2.GetComponent<CCMovement>().enabled = false;
        //deactivate other scripts when ready

        //change state
        gameState = State.Ending;       

        //update UI;
        player1ScoreText.text = "";
        player2ScoreText.text = "";
        timerText.text = "";

        //display winner
        string winningPlayer;
        if (player1Score > player2Score) winningPlayer = player1Name;
        else winningPlayer = player2Name;

        messageText.text = winningPlayer + " Wins!" + "\n" + player1Name + " : " + player1Score + "\n" + player2Name + " : " + player2Score;

        if (GameMaster.instance != null)  GameMaster.instance.SortTempList(GameMaster.instance.tempPlayers, true);
        if (GameMaster.instance != null)  GameMaster.instance.SaveGame();

        StartCoroutine(SceneDelayTimer());

        player1.GetComponent<CCMovement>().enabled = true;
        player2.GetComponent<CCMovement>().enabled = true;

    }
    private IEnumerator SceneDelayTimer()
    {
        {
            yield return new WaitForSeconds(5);

            //Disconnect from Photon server
            PhotonNetwork.Disconnect();

            //Wait for disconnection to complete
            while (PhotonNetwork.IsConnected)
            {
                yield return null;
            }

            //Load Main menu scene if disconnected successfully
            SceneManager.LoadScene("MainMenu");
        }
    }
}

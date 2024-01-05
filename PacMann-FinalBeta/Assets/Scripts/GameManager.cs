using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    [Header("Player")]
    public GameObject pacman;

    [Header("Teleport Nodes")]
    public GameObject leftTeleportNode;
    public GameObject rightTeleportNode;

    [Header("Ghost Nodes")]
    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeStart;
    public GameObject ghostNodeCenter;

    [Header("Ghosts")]
    public GameObject redGhost;
    public GameObject pinkGhost;
    public GameObject blueGhost;
    public GameObject orangeGhost;

    [Header("Audios")]
    public AudioSource siren;
    public AudioSource eat1;
    public AudioSource eat2;
    public AudioSource startGame;
    public AudioSource death;
    public AudioSource powerPelletAudio;
    public AudioSource respawningAudio;
    public AudioSource ghostEatenAudio;

    [Header("Integer Values")]
    public int lives;
    public int currentLevel;
    public int currentEat = 0;
    public int score;
    public int totalPellets;
    public int pelletsLeft;
    public int pellectedCollectedOnThisLife;

    [Header("Texts")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameoverText;
    public TextMeshProUGUI livesText;

    [Header("Enemy Controllers")]
    public EnemyController redGhostController;
    public EnemyController pinkGhostController;
    public EnemyController blueGhostController;
    public EnemyController orangeGhostController;
    public Image blackBackground;

    [Header("Boolean Values")]
    public bool hadDeathOnThisLevel = false;
    public bool gameIsRunning;
    public bool newGame;
    public bool clearedLevel;
    public bool isPowerPelletRunning = false;

    [Header("Float Values")]
    public float currentPowerPelletTime = 0;
    public float powerPelletTimer = 8f;

    [Header("Integer Values")]
    public int powerPelletMultiplyer = 1;

    [Header("Ghost Mode")]
    public int[] ghostModeTimers = new[] { 7, 20, 7, 20, 5, 20, 5}; 
    public int ghostModeTimerIndex = 0;
    public float ghostModeTimer = 0;
    public bool isRunningTimer;
    public bool isComplatedTimer;
    public List<NodeController> nodeControllers = new List<NodeController>();

    [Header("High Score")]
    public int highScore;
    public TextMeshProUGUI highScoreText;
    public enum GhostMode
    {
        chase, scatter
    }

    public GhostMode currentGhostMode;

    

    void Awake()
    {
        newGame = true;
        clearedLevel = false;
        blackBackground.enabled = false;
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        #region Ghost Controllers
        redGhostController = redGhost.GetComponent<EnemyController>();
        pinkGhostController = pinkGhost.GetComponent<EnemyController>();
        blueGhostController = blueGhost.GetComponent<EnemyController>();
        orangeGhostController = orangeGhost.GetComponent<EnemyController>();
        #endregion

        ghostNodeStart.GetComponent<NodeController>().isGhostStartingNode = true;

        pacman = GameObject.Find("Player");

        StartCoroutine(Setup());
    }

    void Update()
    {
        if (!gameIsRunning)
        {
            return;
        }

        if(redGhostController.ghostNodeState == EnemyController.GhostNodeStatesEnum.respawning
            || pinkGhostController.ghostNodeState == EnemyController.GhostNodeStatesEnum.respawning
            || blueGhostController.ghostNodeState == EnemyController.GhostNodeStatesEnum.respawning
            || orangeGhostController.ghostNodeState == EnemyController.GhostNodeStatesEnum.respawning)
        {
            if (!respawningAudio.isPlaying)
            {
                respawningAudio.Play();
            }
        }
        else
        {
            if (respawningAudio.isPlaying)
            {
                respawningAudio.Stop();
            }
        }

        // Ghost mode Scatter ve Chase arasýnda geçiþ yapma
        if (!isComplatedTimer && isRunningTimer)
        {
            ghostModeTimer += Time.deltaTime;
            if (ghostModeTimer>= ghostModeTimers[ghostModeTimerIndex])
            {
                ghostModeTimer = 0;
                ghostModeTimerIndex++;
                if (currentGhostMode==GhostMode.chase)
                {
                    currentGhostMode = GhostMode.scatter;
                }
                else
                {
                    currentGhostMode = GhostMode.chase;
                }
                if (ghostModeTimerIndex==ghostModeTimers.Length)
                {
                    isComplatedTimer = true;
                    isRunningTimer = false;
                    currentGhostMode = GhostMode.chase;
                }
            }
        }

        if (isPowerPelletRunning)
        {
            currentPowerPelletTime += Time.deltaTime;
            if(currentPowerPelletTime >= powerPelletTimer)
            {
                isPowerPelletRunning = false;
                currentPowerPelletTime = 0;
                powerPelletAudio.Stop();
                siren.Play();
                powerPelletMultiplyer = 1;
            }
        }
    }

    public IEnumerator Setup()
    {
        ghostModeTimerIndex = 0;
        ghostModeTimer = 0;
        isComplatedTimer = false;
        isRunningTimer = true;

        gameoverText.enabled = false;
        // Eðer pacman leveli temizlerse ekraný bir görsel kaplayacak ve oyun 0.1 saniyeliðine duracak
        if (clearedLevel)
        {
            blackBackground.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }
        blackBackground.enabled = false;

        pellectedCollectedOnThisLife = 0;
        currentGhostMode = GhostMode.chase;
        currentGhostMode = GhostMode.scatter;
        gameIsRunning = false;
        currentEat = 0;


        float waitTimer = 1f;

        yield return new WaitForSeconds(1);
        if (clearedLevel||newGame)
        {
            pelletsLeft = totalPellets;
            waitTimer = 4f;
            // Oyuncu bütün noktalarý topladýysa veya yeni oyuna baþladýysa bütün noktalarý tekrar oluþtur
            for 
                (
                int i = 0;
                i < nodeControllers.Count;
                i++
                )
            {
                nodeControllers[i].RespawnPellet();
            }
        }

        if (newGame)
        {
            startGame.Play();
            score = 0;
            scoreText.text = "Score: " + score.ToString();
            SetLives(3);
            currentLevel = 1;
        }

        pacman.GetComponent<PlayerController>().Setup();

        redGhostController.Setup();
        pinkGhostController.Setup();
        blueGhostController.Setup();
        orangeGhostController.Setup();

        newGame = false;
        clearedLevel = false;

        yield return new WaitForSeconds(waitTimer);

        StartGame();
    }


    void SetLives(int newLives)
    {
        lives = newLives;
        livesText.text = "Lives: " + lives;
    }

    void StartGame()
    {
        gameIsRunning = true;
        siren.Play();
    }

    void StopGame()
    {
        gameIsRunning = false;
        siren.Stop();
        powerPelletAudio.Stop();
        respawningAudio.Stop();
        pacman.GetComponent<PlayerController>().Stop();
    }

    public void GotPelletFromNodeController(NodeController nodeController)
    {
        nodeControllers.Add(nodeController);
        totalPellets++;
        pelletsLeft++;
    }

    public void AddToScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score.ToString();

        if (score < highScore)
        {
            highScore = score;
            highScoreText.enabled = true;
            highScoreText.text = "High Score: " + highScore.ToString();

            // HighScore'u kaydet
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
    }

    public IEnumerator CollectedDot(NodeController nodeController)
    {
        if (currentEat ==0)
        {
            eat1.Play();
            currentEat = 1;
        }
        else if(currentEat == 1)
        {
            eat2.Play();
            currentEat = 0;
        }
        pelletsLeft--;
        pellectedCollectedOnThisLife++;
        int requiredBluePellets = 0;
        int requiredOrangePellets = 0;

        if(hadDeathOnThisLevel)
        {
            requiredBluePellets = 12;
            requiredOrangePellets = 32;
        }
        else
        {
            requiredBluePellets = 30;
            requiredOrangePellets = 60;
        }

        if (pellectedCollectedOnThisLife >= requiredBluePellets && !blueGhost.GetComponent<EnemyController>().leftHomeBefore)
        {
            blueGhost.GetComponent<EnemyController>().readyToLeaveHome = true;
        }

        if (pellectedCollectedOnThisLife >= requiredOrangePellets && !orangeGhost.GetComponent<EnemyController>().leftHomeBefore)
        {
            orangeGhost.GetComponent<EnemyController>().readyToLeaveHome = true;
        }

        // Skor sistemi
        AddToScore(10);

        // Noktalarýn bittiðini kontrol etme
        if(pelletsLeft == 0)
        {
            currentLevel++;
            clearedLevel = true;
            StopGame();
            yield return new WaitForSeconds(1);
            StartCoroutine(Setup());
        }


        // Power dot mu normal dot mu
        if (nodeController.isPowerPellet)
        {
            siren.Stop();
            powerPelletAudio.Play();
            isPowerPelletRunning = true;
<<<<<<< HEAD:PacMann-FinalBeta/Assets/Scripts/GameManager.cs
            currentPowerPelletTime = 0;
=======
            currentPowerPelletTime = 0;            
>>>>>>> 4eb5eb8e52e0259e2326d6d03000dbffaf76dbea:PacMann-main/Assets/Scripts/GameManager.cs

            redGhostController.SetFrightened(true);
            pinkGhostController.SetFrightened(true);
            blueGhostController.SetFrightened(true);
            orangeGhostController.SetFrightened(true);
<<<<<<< HEAD:PacMann-FinalBeta/Assets/Scripts/GameManager.cs

=======
>>>>>>> 4eb5eb8e52e0259e2326d6d03000dbffaf76dbea:PacMann-main/Assets/Scripts/GameManager.cs
        }
    }

    public IEnumerator PauseGame(float timeToPause)
    {
        gameIsRunning = false;
        yield return new WaitForSeconds(timeToPause);
        gameIsRunning = true;
    }

    public void GhostEaten()
    {
        ghostEatenAudio.Play();
        AddToScore(400 * powerPelletMultiplyer);
        powerPelletMultiplyer++;
        StartCoroutine(PauseGame(1));
    }

    public IEnumerator PlayerEaten()
    {
        hadDeathOnThisLevel = true;
        StopGame();
        yield return new WaitForSeconds(1);

        redGhostController.SetVisible(false);
        pinkGhostController.SetVisible(false);
        blueGhostController.SetVisible(false);
        orangeGhostController.SetVisible(false);

        pacman.GetComponent<PlayerController>().Death();
        death.Play();
        yield return new WaitForSeconds(3);
        SetLives(lives - 1);
        if (lives <= 0)
        {
            newGame = true;
            gameoverText.enabled = true;
            yield return new WaitForSeconds(3);
        }
        StartCoroutine(Setup());
    }
}

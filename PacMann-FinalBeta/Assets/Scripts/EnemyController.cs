using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum GhostNodeStatesEnum
    {
        respawning,
        leftNode,
        rightNode,
        centerNode,
        startNode,
        movingInNodes
    }
    public GhostNodeStatesEnum ghostNodeState;
    public GhostNodeStatesEnum respawnState;
    public GhostNodeStatesEnum startGhostNodeState;

    [Header("Ghost Type")]
    public GhostType ghostType;    
    public enum GhostType
    {
        red,
        blue,
        pink,
        orange
    }
    [Header("Game Objects")]
    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeStart;
    public GameObject ghostNodeCenter;
    public GameObject startingNode;
    public GameObject[] scatterNodes;

    [Header("Boolean Values")]
    public bool readyToLeaveHome = false;
    public bool testRespawn = false;
    public bool isFrightened = false;
    public bool leftHomeBefore = false;
    public bool isVisible =true;

    [Header("Sprite Renderers")]
    public SpriteRenderer ghostSprite;

    [Header("Integer Values")]
    public int scatterNodeIndex;

    [Header("Animator")]
    public Animator animator;

    public GameManager gameManager;
    public MovementController movementController;
    public Color color;

    void Awake()
    {
        animator = GetComponent<Animator>();
        ghostSprite = GetComponentInChildren<SpriteRenderer>();

        scatterNodeIndex = 0;

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        movementController = GetComponent<MovementController>();

        switch (ghostType)
        {
            case GhostType.red:
                startGhostNodeState = GhostNodeStatesEnum.startNode;
                respawnState = GhostNodeStatesEnum.centerNode;
                startingNode = ghostNodeStart;
                break;

            case GhostType.pink:
                startGhostNodeState = GhostNodeStatesEnum.centerNode;
                startingNode = ghostNodeCenter;
                respawnState = GhostNodeStatesEnum.centerNode;
                break;

            case GhostType.blue:
                startGhostNodeState = GhostNodeStatesEnum.leftNode;
                respawnState = GhostNodeStatesEnum.leftNode;
                startingNode = ghostNodeLeft;
                break;

            case GhostType.orange:
                startGhostNodeState = GhostNodeStatesEnum.rightNode;
                respawnState = GhostNodeStatesEnum.rightNode;
                startingNode = ghostNodeRight;
                break;
        }
    }

    public void Setup()
    {
        animator.SetBool("moving", false);

        ghostNodeState = startGhostNodeState;
        readyToLeaveHome = false;


        //Reset ghost back to base position
        movementController.currentNode = startingNode;
        transform.position = startingNode.transform.position;

        movementController.direction = "";
        movementController.lastMovingDirection = "";

        //Set ghost scatter node index back to 0
        scatterNodeIndex = 0;

        //Set isFrightened
        isFrightened = false;

        leftHomeBefore = false;

        // Set readyToLeaveHome to false 
        if (ghostType == GhostType.red)
        {
            readyToLeaveHome = true;
            leftHomeBefore = true;
        }
        else if (ghostType == GhostType.pink)
        {
            readyToLeaveHome = true;
        }
        SetVisible(true);
    }

    void Update()
    {
        if(ghostNodeState != GhostNodeStatesEnum.movingInNodes || !gameManager.isPowerPelletRunning)
        {
            isFrightened = false;
        }

        if (!gameManager.isPowerPelletRunning)
        {
            isFrightened = false;
        }

        if (isVisible)
        {
            if(ghostNodeState != GhostNodeStatesEnum.respawning)
            {
               ghostSprite.enabled = true;
            }
            else
            {
                ghostSprite.enabled = false;
            }
            
        }
        else
        {
            ghostSprite.enabled = false;
        }

        if (isFrightened)
        {
            animator.SetBool("frightened", true);
        }
        else
        {
            animator.SetBool("frightened", false);
            animator.SetBool("frightenedBlinking", false);

        }

        if (!gameManager.gameIsRunning)
        {
            return;
        }   

        if(gameManager.powerPelletTimer - gameManager.currentPowerPelletTime <= 3)
        {
            animator.SetBool("frightenedBlinking", true);
        }
        else
        {
            animator.SetBool("frightenedBlinking", false);
        }

        animator.SetBool("moving", true);

        if (testRespawn == true)
        {
            readyToLeaveHome = false;
            ghostNodeState = GhostNodeStatesEnum.respawning;
            testRespawn = false;
        }

        if(movementController.currentNode.GetComponent<NodeController>().isSideNode)
        {
            movementController.SetSpeed(1);
        }
        else
        {
            if (isFrightened)
            {
                movementController.SetSpeed(1);
                ghostSprite.enabled = false;
            }
            else if(ghostNodeState == GhostNodeStatesEnum.respawning)
            {
                movementController.SetSpeed(7);
            }
            else
            {
                movementController.SetSpeed(2);
            }
        }
    }



    public void SetFrightened(bool newIsFrightened)
    {
        isFrightened = newIsFrightened;
    }
    public void ReachedCenterOfNode(NodeController nodeController)
    {
        if(ghostNodeState == GhostNodeStatesEnum.movingInNodes) 
        {
            leftHomeBefore = true;
            if(gameManager.currentGhostMode == GameManager.GhostMode.scatter)
            {
                DetermineGhostScatterModeDirection();
                
            }
            else if(isFrightened)
            {
                string direction = GetRandomDirection();
                movementController.SetDirection(direction);
            }
            else
            {
                if (ghostType == GhostType.red)
                {
                    DetermineRedGhostDirection();
                }
                else if(ghostType == GhostType.pink)
                {
                    DeterminePinkGhostDirection();
                }
                else if(ghostType == GhostType.blue)
                {
                    DetermineBlueGhostDirection();
                }
                else if(ghostType == GhostType.orange)
                {
                    DetermineOrangeGhostDirection();
                }
            }
        }
        else if (ghostNodeState == GhostNodeStatesEnum.respawning)
        {
            string direction = "";

            if(transform.position.x == ghostNodeStart.transform.position.x && transform.position.y == ghostNodeStart.transform.position.y)
            {
                direction = "down";
            }
            else if(transform.position.x == ghostNodeCenter.transform.position.x && transform.position.y == ghostNodeCenter.transform.position.y)
            {
                if(respawnState == GhostNodeStatesEnum.centerNode)
                {
                    ghostNodeState = respawnState;
                }
                else if(respawnState == GhostNodeStatesEnum.leftNode)
                {
                    direction = "left";
                }
                else if(respawnState == GhostNodeStatesEnum.rightNode)
                {
                    direction = "right";
                }
            }
            else if(
                (transform.position.x == ghostNodeLeft.transform.position.x && transform.position.y == ghostNodeLeft.transform.position.y)
                || (transform.position.x == ghostNodeRight.transform.position.x && transform.position.y == ghostNodeRight.transform.position.y))
            {
                ghostNodeState = respawnState;
            }
            else
            {
                direction = GetClosestDirection(ghostNodeStart.transform.position);
            }    
            movementController.SetDirection(direction);
        }
        else
        {
            if(readyToLeaveHome)
            {
                if(ghostNodeState == GhostNodeStatesEnum.leftNode)
                {
                    ghostNodeState = GhostNodeStatesEnum.centerNode;
                    movementController.SetDirection("right");
                }
                else if (ghostNodeState == GhostNodeStatesEnum.rightNode)
                {
                    ghostNodeState = GhostNodeStatesEnum.centerNode;
                    movementController.SetDirection("left");
                }
                else if(ghostNodeState == GhostNodeStatesEnum.centerNode)
                {
                    ghostNodeState = GhostNodeStatesEnum.startNode;
                    movementController.SetDirection("up");
                }
                else if(ghostNodeState == GhostNodeStatesEnum.startNode)
                {
                    ghostNodeState = GhostNodeStatesEnum.movingInNodes;
                    movementController.SetDirection("left");
                }
            }
        }
    }
    string GetRandomDirection()
    {
        List<string> possibleDirections = new List<string>();
        NodeController nodeController = movementController.currentNode.GetComponent<NodeController>();

        if(nodeController.canMoveDown && movementController.lastMovingDirection != "up")
        {
            possibleDirections.Add("down");
        }
        if(nodeController.canMoveUp && movementController.lastMovingDirection != "down")
        {
            possibleDirections.Add("up");
        }
        if(nodeController.canMoveRight && movementController.lastMovingDirection != "left")
        {
            possibleDirections.Add("right");
        }
        if(nodeController.canMoveLeft && movementController.lastMovingDirection != "right")
        {
            possibleDirections.Add("left");
        }
        string direction = "";
        int randomDirectionIndex = Random.Range(0, possibleDirections.Count - 1);
        direction = possibleDirections[randomDirectionIndex];
        return direction;
    }

    void DetermineGhostScatterModeDirection()
    {
        if (transform.position.x == scatterNodes[scatterNodeIndex].transform.position.x && transform.position.y == scatterNodes[scatterNodeIndex].transform.position.y)
        {
            scatterNodeIndex++;
            if (scatterNodeIndex == scatterNodes.Length - 1)
            {
                scatterNodeIndex = 0;
            }
        }
        string direction = GetClosestDirection(scatterNodes[scatterNodeIndex].transform.position);
        movementController.SetDirection(direction);
    }
    void DetermineRedGhostDirection()
    {
        string direction = GetClosestDirection(gameManager.pacman.transform.position);
        movementController.SetDirection(direction);
    }

    void DeterminePinkGhostDirection() 
    {
        string pacmansDirection = gameManager.pacman.GetComponent<MovementController>().lastMovingDirection;
        float distanceBetweenNodes = 0.35f;

        Vector2 target = gameManager.pacman.transform.position;
        if(pacmansDirection == "left")
        {
            target.x -= distanceBetweenNodes * 2;
        }
        else if(pacmansDirection == "right")
        {
            target.x += distanceBetweenNodes * 2;
        }
        else if(pacmansDirection == "up")
        {
            target.y += distanceBetweenNodes * 2;
        }
        else if(pacmansDirection == "down")
        {
            target.y -= distanceBetweenNodes * 2;
        }
        string direction = GetClosestDirection(target);
        movementController.SetDirection(direction);
    }

    void DetermineBlueGhostDirection()
    {
        string pacmansDirection = gameManager.pacman.GetComponent<MovementController>().lastMovingDirection;
        float distanceBetweenNodes = 0.35f;

        Vector2 target = gameManager.pacman.transform.position;
        if (pacmansDirection == "left")
        {
            target.x -= distanceBetweenNodes * 2;
        }
        else if (pacmansDirection == "right")
        {
            target.x += distanceBetweenNodes * 2;
        }
        else if (pacmansDirection == "up")
        {
            target.y += distanceBetweenNodes * 2;
        }
        else if (pacmansDirection == "down")
        {
            target.y -= distanceBetweenNodes * 2;
        }

        GameObject redGhost = gameManager.redGhost;
        float xDistance = target.x - redGhost.transform.position.x;
        float yDistance = target.y - redGhost.transform.position.y;
        Vector2 blueTarget = new Vector2(target.x + xDistance, target.y + yDistance);
        string direction = GetClosestDirection(blueTarget);
        movementController.SetDirection(direction);
    }
    void DetermineOrangeGhostDirection()
    {
        float distance = Vector2.Distance(gameManager.pacman.transform.position, transform.position);
        float distanceBetweenNodes = 0.35f;

        if(distance < 0)
        {
            distance *= -1;
        }
        if(distance <= distanceBetweenNodes * 8)
        {
            DetermineRedGhostDirection();
        }
        else
        {
            DetermineGhostScatterModeDirection();
        }
    }
    string GetClosestDirection(Vector2 target)
    {
        float shortestDistance = 0f;
        string lastMovingDirection = movementController.lastMovingDirection;
        string newDirection = "";

        NodeController nodeController = movementController.currentNode.GetComponent<NodeController>();
        if(nodeController.canMoveUp && lastMovingDirection != "down") 
        {
            GameObject nodeUp = nodeController.nodeUp;
            float distance = Vector2.Distance(nodeUp.transform.position, target);
            if(distance < shortestDistance || shortestDistance == 0) 
            {
                shortestDistance = distance;
                newDirection = "up";
            }
        }
        if (nodeController.canMoveDown && lastMovingDirection != "up")
        {
            GameObject nodeDown = nodeController.nodeDown;
            float distance = Vector2.Distance(nodeDown.transform.position, target);
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "down";
            }
        }
        if (nodeController.canMoveLeft && lastMovingDirection != "right")
        {
            GameObject nodeLeft = nodeController.nodeLeft;
            float distance = Vector2.Distance(nodeLeft.transform.position, target);
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "left";
            }
        }
        if (nodeController.canMoveRight && lastMovingDirection != "left")
        {
            GameObject nodeRight = nodeController.nodeRight;
            float distance = Vector2.Distance(nodeRight.transform.position, target);

            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "right";
            }
        }
        return newDirection;
    }

    public void SetVisible(bool newIsVisible)
    {
        isVisible = newIsVisible;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player" && ghostNodeState != GhostNodeStatesEnum.respawning)
        {
            // Hayaletin Yenmesi 
            if (isFrightened)
            {
                gameManager.GhostEaten();
                ghostNodeState = GhostNodeStatesEnum.respawning;
            }
            // Oyuncunun yenmesi
            else
            {
                StartCoroutine(gameManager.PlayerEaten());
            }
        }
    }
}
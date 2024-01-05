using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [Header("Inheritance")]
    public GameManager gameManager;

    [Header("Game Objects")]
    public GameObject currentNode;

    [Header("Movement")]
    public string direction = "";
    public string lastMovingDirection = "";
    public float speed = 4.0f;
    public bool canTeleport = true;
    public bool isGhost = false;

    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void Update()
    {
        if (!gameManager.gameIsRunning)
        {
            return;
        }

        NodeController currentNodeController = currentNode.GetComponent<NodeController>();
        transform.position = Vector2.MoveTowards(transform.position, currentNode.transform.position, speed * Time.deltaTime);
        
        bool reverseDirection = false;
        if (direction == "left" && lastMovingDirection == "right" 
            || (direction == "right" && lastMovingDirection == "left") 
            || (direction == "up" && lastMovingDirection == "down" )
            || (direction == "down" && lastMovingDirection == "up")
            )
        {
            reverseDirection = true;
        }

        // Þu anki noktanýn tam ortasýndaysak
        if(transform.position.x == currentNode.transform.position.x && transform.position.y == currentNode.transform.position.y)
        {

            

            if(isGhost)
            {
                GetComponent<EnemyController>().ReachedCenterOfNode(currentNodeController);
            }

            // Sol ýþýnlanma noktasýndaysak sað ýþýnlanma noktasýna git
            if (currentNodeController.isTeleportLeftNode && canTeleport)
            {
                currentNode = gameManager.rightTeleportNode;
                direction = "left";
                lastMovingDirection = "left";
                transform.position = currentNode.transform.position;
                canTeleport = false;
            }
            // Sað ýþýnlanma noktasýndaysak sol ýþýnlanma noktasýna git
            else if (currentNodeController.isTeleportRightNode && canTeleport)
            {
                currentNode = gameManager.leftTeleportNode;
                direction = "right";
                lastMovingDirection = "right";
                transform.position = currentNode.transform.position;
                canTeleport = false;
            }
            //Aksi takdirde ilerleyeceðimiz bir sonraki noktayý bul
            else
            {
                if(currentNodeController.isGhostStartingNode && direction == "down" 
                    && (!isGhost || GetComponent<EnemyController>().ghostNodeState != EnemyController.GhostNodeStatesEnum.respawning))
                {
                    direction = lastMovingDirection;
                }

                // NodeController'dan yön bilgilerini alarak sonraki noktayý bul
                GameObject newNode = currentNodeController.GetNodeFromDirection(direction);

                // Ýstediðimiz yönde gidebiliyorsak
                if (newNode != null)
                {
                    currentNode = newNode;
                    lastMovingDirection = direction;
                }
                // Ýstediðimiz yönde gidemiyoruz gittiðimiz yöne devam edelim.
                else
                {
                    direction = lastMovingDirection;
                    newNode = currentNodeController.GetNodeFromDirection(direction);
                    if (newNode != null)
                    {
                        currentNode = newNode;
                    }
                }
            }
        }
        // Þu anki noktanýn tam ortasýnda deðilsek
        else
        {
            canTeleport = true;
        }

    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void SetDirection(string newDirection)
    {
        direction = newDirection;
    }
}

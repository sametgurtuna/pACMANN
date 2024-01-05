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

        // �u anki noktan�n tam ortas�ndaysak
        if(transform.position.x == currentNode.transform.position.x && transform.position.y == currentNode.transform.position.y)
        {

            

            if(isGhost)
            {
                GetComponent<EnemyController>().ReachedCenterOfNode(currentNodeController);
            }

            // Sol ���nlanma noktas�ndaysak sa� ���nlanma noktas�na git
            if (currentNodeController.isTeleportLeftNode && canTeleport)
            {
                currentNode = gameManager.rightTeleportNode;
                direction = "left";
                lastMovingDirection = "left";
                transform.position = currentNode.transform.position;
                canTeleport = false;
            }
            // Sa� ���nlanma noktas�ndaysak sol ���nlanma noktas�na git
            else if (currentNodeController.isTeleportRightNode && canTeleport)
            {
                currentNode = gameManager.leftTeleportNode;
                direction = "right";
                lastMovingDirection = "right";
                transform.position = currentNode.transform.position;
                canTeleport = false;
            }
            //Aksi takdirde ilerleyece�imiz bir sonraki noktay� bul
            else
            {
                if(currentNodeController.isGhostStartingNode && direction == "down" 
                    && (!isGhost || GetComponent<EnemyController>().ghostNodeState != EnemyController.GhostNodeStatesEnum.respawning))
                {
                    direction = lastMovingDirection;
                }

                // NodeController'dan y�n bilgilerini alarak sonraki noktay� bul
                GameObject newNode = currentNodeController.GetNodeFromDirection(direction);

                // �stedi�imiz y�nde gidebiliyorsak
                if (newNode != null)
                {
                    currentNode = newNode;
                    lastMovingDirection = direction;
                }
                // �stedi�imiz y�nde gidemiyoruz gitti�imiz y�ne devam edelim.
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
        // �u anki noktan�n tam ortas�nda de�ilsek
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

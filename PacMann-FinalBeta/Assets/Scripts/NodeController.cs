using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeController : MonoBehaviour
{
    [Header("Boolean Values")]
    public bool canMoveLeft = false;
    public bool canMoveRight = false;
    public bool canMoveUp = false;
    public bool canMoveDown = false;
    public bool isTeleportRightNode = false;
    public bool isTeleportLeftNode = false;
    public bool isDotNode = false;
    public bool hasDot = false;
    public bool isGhostStartingNode = false;
    public bool isSideNode = false;
    public bool isPowerPellet = false;

    [Header("Float Values")]
    public float powerPelletBlinkingTimer = 0;

    [Header("Game Objects")]
    public GameObject nodeLeft;
    public GameObject nodeRight;
    public GameObject nodeUp;
    public GameObject nodeDown;

    [Header("Inheritance")]
    public GameManager gameManager;

    [Header("Sprite Renderer")]
    public SpriteRenderer dotSprite;


    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (transform.childCount>0)
        {
            gameManager.GotPelletFromNodeController(this);
            hasDot = true;
            isDotNode = true;
            dotSprite = GetComponentInChildren<SpriteRenderer>();

        }
        #region RaycastNode
        // Raycast to the Down
        RaycastHit2D[] hitsDown;
        hitsDown = Physics2D.RaycastAll(transform.position, -Vector2.up);
        for 
            (
            int i = 0;
            i < hitsDown.Length;
            i++
            )
        {
            float distance = Mathf.Abs(hitsDown[i].point.y -transform.position.y);
            if (distance < 0.4f && hitsDown[i].collider.tag == "Node")
            {
                canMoveDown = true;
                nodeDown = hitsDown[i].collider.gameObject;
            }
        }
        // Raycast to the Up
        RaycastHit2D[] hitsUp;
        hitsUp = Physics2D.RaycastAll(transform.position, Vector2.up);
        for 
            (
            int i = 0;
            i < hitsUp.Length; 
            i++
            )
        {
            float distance = Mathf.Abs(hitsUp[i].point.y - transform.position.y);
            if (distance < 0.4f && hitsUp[i].collider.tag == "Node")
            {
                canMoveUp = true;
                nodeUp = hitsUp[i].collider.gameObject;
            }
        }
        // Raycast to the Right
        RaycastHit2D[] hitsRight;
        hitsRight = Physics2D.RaycastAll(transform.position, Vector2.right);
        for 
            (
            int i = 0; 
            i < hitsRight.Length;
            i++
            )
        {
            float distance = Mathf.Abs(hitsRight[i].point.x - transform.position.x);
            if (distance < 0.4f && hitsRight[i].collider.tag == "Node")
            {
                canMoveRight = true;
                nodeRight = hitsRight[i].collider.gameObject;
            }
        }
        // Raycast to the Left
        RaycastHit2D[] hitsLeft;
        hitsLeft = Physics2D.RaycastAll(transform.position, -Vector2.right);
        for 
            (
            int i = 0;
            i < hitsLeft.Length;
            i++
            )
        {
            float distance = Mathf.Abs(hitsLeft[i].point.x - transform.position.x);
            if (distance < 0.4f && hitsLeft[i].collider.tag == "Node")
            {
                canMoveLeft = true;
                nodeLeft = hitsLeft[i].collider.gameObject;
            }
        }

        if(isGhostStartingNode)
        {
            canMoveDown = true;
            nodeDown = gameManager.ghostNodeCenter;
        }
        #endregion 
    }

    void Update()
    {
        if (!gameManager.gameIsRunning)
        {
            return;
        }

        if(isPowerPellet && hasDot)
        {
            powerPelletBlinkingTimer += Time.deltaTime;
            if(powerPelletBlinkingTimer >= 0.1f)
            {
                powerPelletBlinkingTimer = 0;
                dotSprite.enabled = !dotSprite.enabled;
            }
        }
    }

    public GameObject GetNodeFromDirection(string direction)
    {
        switch (direction)
        {
            case "left" when canMoveLeft:
                return nodeLeft;
            case "right" when canMoveRight:
                return nodeRight;
            case "up" when canMoveUp:
                return nodeUp;
            case "down" when canMoveDown:
                return nodeDown;
            default:
                return null;
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && hasDot)
        {
            hasDot = false;
            dotSprite.enabled = false;
           StartCoroutine(gameManager.CollectedDot(this));
        }

    }

    public void RespawnPellet()
    {
        if (isDotNode)
        {
            hasDot = true;
            dotSprite.enabled = true;
        }
    }
}

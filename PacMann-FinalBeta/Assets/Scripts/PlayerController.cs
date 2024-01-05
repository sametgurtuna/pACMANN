using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Sprite Renderer & Animator")]
    public SpriteRenderer spriteRenderer;
    public Animator anim;

    [Header("Game Object")]
    public GameObject startNode;

    [Header("Inheritance")]
    MovementController movementController;
    public GameManager gameManager;

    [Header("Boolean Values")]
    public bool isDead = false;

    public Vector2 startPos;
    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        startPos = new Vector2(-0.06f, -0.65f);
        anim = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        movementController = GetComponent<MovementController>();
        startNode = movementController.currentNode;
    }

    public void Setup()
    {
        isDead = false;
        anim.SetBool("isDead", false);
        anim.SetBool("isMoving", false);

        movementController.currentNode = startNode;
        movementController.direction = "left";
        movementController.lastMovingDirection = "left";
        spriteRenderer.flipX = false;
        transform.position = startPos;
        anim.speed = 1;
    }

    public void Stop()
    {
        anim.speed = 0;
    }

    void Update()
    {
        if (!gameManager.gameIsRunning)
        {
            if (!isDead)
            {
                anim.speed = 0;
            }           
            return;
        }
        anim.speed = 1;

        anim.SetBool("isMoving", true);
        // Inputlarý kontrol et
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            movementController.SetDirection("left");
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            movementController.SetDirection("right");
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            movementController.SetDirection("up");
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            movementController.SetDirection("down");
        }
        bool flipX = false;
        bool flipY = false;
        if (movementController.lastMovingDirection == "left")
        {
            anim.SetInteger("direction", 0);
        }
        else if (movementController.lastMovingDirection == "right")
        {
            anim.SetInteger("direction", 0);
            flipX = true;
        }
        else if(movementController.lastMovingDirection == "up")
        {
            anim.SetInteger("direction", 1);
        }
        else if (movementController.lastMovingDirection == "down")
        {
            anim.SetInteger("direction", 1);
            flipY = true;
        }
        spriteRenderer.flipY = flipY;
        spriteRenderer.flipX = flipX;
    }

    public void Death()
    {
        isDead = true;
        anim.SetBool("isMoving", false);
        anim.speed = 1;
        anim.SetBool("isDead", true);       
    }
}

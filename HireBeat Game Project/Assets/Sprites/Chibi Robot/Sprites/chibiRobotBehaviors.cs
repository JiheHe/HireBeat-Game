using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chibiRobotBehaviors : MonoBehaviour
{

    public Vector2 targetArea;
    SpriteRenderer sRenderer;
    Animator animator;

    private int rightIdleIndex = 0;
    private int downIdleIndex = 1;
    private int upIdleIndex = 2;
    private int runIndex = 3;

    private bool notIdleState;

    public GameObject robotObj;

    public bool inConvo;

    // Start is called before the first frame update
    void Start()
    {
        targetArea = transform.position;
        sRenderer = robotObj.GetComponent<SpriteRenderer>();
        animator = robotObj.GetComponent<Animator>();
        notIdleState = true;
        inConvo = false;
        randomRotationIdle();
    }

    // Update is called once per frame
    void Update()
    {
        if (!inConvo)
        {
            if (transform.position.x != targetArea.x && transform.position.y != targetArea.y)
            {
                if (animator.GetInteger("IdleIndex") != runIndex) animator.SetInteger("IdleIndex", runIndex);
                transform.position = Vector2.MoveTowards(transform.position, targetArea, Time.deltaTime * 5f); //local or not doesn't matter here cuz no parent
                if (transform.position.x == targetArea.x && transform.position.y == targetArea.y) notIdleState = false;
            }
            else if (!notIdleState)
            {
                randomRotationIdle(); //only need to call once
                notIdleState = true;
            }
        }
        
    }

    public Vector2 generateRandomTarget(float xMin, float xMax, float yMin, float yMax)
    {
        float xLoc = Random.Range(xMin, xMax);
        float yLoc = Random.Range(yMin, yMax);
        return new Vector2(xLoc, yLoc);
    }

    public void decideDirection() //will mod
    {
        if(targetArea.x < transform.position.x)
        {
            turnToLeft();
        } else
        {
            turnToRight();
        }
    }

    public void turnToPlayer(Vector2 playerPos)
    {
        if (playerPos.x < transform.position.x + 0.5 && playerPos.x > transform.position.x - 0.5)
        {
            if (playerPos.y > transform.position.y) turnToDown();
            else turnToUp();
        }
        else if (playerPos.x < transform.position.x) turnToLeft();
        else turnToRight();
    }

    //use invoke(name, time)
    private void turnToRight()
    {
        animator.SetInteger("IdleIndex", rightIdleIndex);
        sRenderer.flipX = false;
    }

    private void turnToLeft()
    {
        animator.SetInteger("IdleIndex", rightIdleIndex);
        sRenderer.flipX = true;
    }

    private void turnToUp()
    {
        animator.SetInteger("IdleIndex", upIdleIndex);
        sRenderer.flipX = false;
    }

    private void turnToDown()
    {
        animator.SetInteger("IdleIndex", downIdleIndex);
        sRenderer.flipX = false;
    }

    private void randomRotationIdle() //practice
    {
        //top/down can only turn left/right
        //left/right can only turn top/down
        if (inConvo) return; //break out from recursion if convo in idle

        if (facingSide())
        {
            if (Random.Range(0, 2) == 1)
            {
                turnToUp();
            }
            else
            {
                turnToDown();
            }
        }
        else
        {
            if (Random.Range(0, 2) == 1)
            {
                turnToRight();
            }
            else
            {
                turnToLeft();
            }
        }

        if (inConvo) return;

        if (Random.Range(1, 4) != 1) //was 0
        {
            Invoke("randomRotationIdle", 1f);
        }
        else
        {
            Invoke("chibiBotNewLocation", 1f);
        }
    }

    private bool facingSide() //side = true, face = false;
    {
        return animator.GetInteger("IdleIndex") == rightIdleIndex;
    }

    public void chibiBotNewLocation()
    {
        targetArea = generateRandomTarget(-14f, 15.5f, -17.6f, -12.2f);
        decideDirection();
    }
    

    //some movement pattern rules:
    //Total behavior: idle round 1 (do randy in between) -> decision -> 
    //4 idle states: left right up down
    //idle states turning: up right, up left, down right, down left (after each turn, make a choice: movement or more turning?)
    //calculate the location first, then make 50% decision (use bias, side further away gets more bias)
    //2 movement states: leftRun rightRun (can only move during movement states, start movement in either L or R)
    //parent position: x range [-14, 15.5]; y range [-17.6, -12.2]  
    //stop if hit a collider (for diagonal)
}

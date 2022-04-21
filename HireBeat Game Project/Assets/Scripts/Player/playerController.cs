using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class playerController : MonoBehaviour
{
    public float moveSpeed;
    public LayerMask solidObjectsLayer; //might use in the future for collision

    // public bool isMoving;
    //Instead of is moving, going to use an action int:
    // 0 = idle, 1 = moving, 2 = sitting.
    public int actionParem;
    public enum CharActionCode : int { IDLE, MOVING, SITTING };


    private Vector2 input;

    //private Animator animator;

    public Animator[] animators;

    public PhotonView view;
    public Camera cam;

    private void Start()
    {
        view = GetComponent<PhotonView>();

        //thanks to info gamer's eletrax video!
        //this prevents camera overwrite in multiplayers
        cam = transform.Find("PlayerCamera").GetComponent<Camera>();
        if (!view.IsMine)
        {
            Destroy(cam.gameObject); //i mean might as well.. bye to audio too
            //cam.enabled = false;
        }

    }

    private void Update()
    {
        if (view.IsMine)
        {
            if(actionParem == (int)CharActionCode.IDLE) //Don't need to read if you are sitting I think.
            {
                input.x = Input.GetAxisRaw("Horizontal");
                input.y = Input.GetAxisRaw("Vertical");
                input = input.normalized; //so diag movement speed the same

                /*
                // removes diagonal inputs if unwanted
                if (input.x != 0)
                {
                    input.y = 0;
                }
                */


                if (input != Vector2.zero)
                {

                    foreach (Animator animator in animators)
                    {
                        animator.SetFloat("moveX", input.x);
                        animator.SetFloat("moveY", input.y);
                    }

                    var targetPos = transform.position;
                    targetPos.x += input.x / 5.0f; //+1 is too big to detect
                    targetPos.y += input.y / 5.0f;

                    if (IsWalkable(targetPos))
                        StartCoroutine(Move(targetPos));
                }
            }

            foreach (Animator animator in animators)
            {
                animator.SetInteger("actionParem", actionParem);
            }
        }
    }

    public void MoveToPosition(Vector3 newPosition)
    {
        view.RPC("MoveToPositionRPC", RpcTarget.All, newPosition); //Can do all here, because new joiners's position is precise.
    }

    [PunRPC]
    public void MoveToPositionRPC(Vector3 newPosition)
    {
        gameObject.transform.position = newPosition;
    }

    IEnumerator Move(Vector3 targetPos)
    {
        actionParem = (int)CharActionCode.MOVING;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        actionParem = (int)CharActionCode.IDLE; 
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer) != null)
        {
            return false;
        }

        return true;
    }

    public void ForceTurnTowards(float x, float y)
    {
        foreach (Animator animator in animators)
        {
            animator.SetFloat("moveX", x);
            animator.SetFloat("moveY", y);
        }
    }

    public void SitDownFacingTowards(float x, float y)
    {
        actionParem = (int)CharActionCode.SITTING;

        //Need to set action parem updates here, else playerController set to false immediately after sitting so update won't run!
        foreach (Animator animator in animators)
        {
            animator.SetInteger("actionParem", actionParem);
            animator.SetFloat("moveX", x);
            animator.SetFloat("moveY", y);
        }
    }

    public void LeaveSeat()
    {
        actionParem = (int)CharActionCode.IDLE;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class playerController : MonoBehaviour
{
    public float moveSpeed;
    public LayerMask solidObjectsLayer; //might use in the future for collision

    public bool isMoving;
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
            if (!isMoving)
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
                animator.SetBool("isMoving", isMoving);
            }
        }
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        isMoving = false;
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

}

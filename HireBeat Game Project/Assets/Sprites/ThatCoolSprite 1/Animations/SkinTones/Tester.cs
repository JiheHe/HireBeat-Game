using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (Input.GetAxisRaw("Horizontal") == 1)
        {
            animator.SetInteger("currDirection", 2);
        }
        else if (Input.GetAxisRaw("Horizontal") == -1)
        {
            animator.SetInteger("currDirection", 1);
        }
        else if (Input.GetAxisRaw("Vertical") == 1)
        {
            animator.SetInteger("currDirection", 3);
        }
        else if (Input.GetAxisRaw("Vertical") == -1)
        {
            animator.SetInteger("currDirection", 0);
        }
        else
        {
            animator.SetInteger("currDirection", 4);
        }
    }
}

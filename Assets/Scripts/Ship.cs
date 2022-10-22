using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    private float speed = 5;
    public bool move = false;
    public Transform shipGoal;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(move)
        {
            Vector2 lookDirection = (shipGoal.position - transform.position);
            Debug.Log(lookDirection);
            if (lookDirection == Vector2.zero)
                move = false;
            //shipRb.AddForce(speed * Time.deltaTime * lookDirection);
            transform.Translate(speed * Time.deltaTime * lookDirection);
        }
    }
}

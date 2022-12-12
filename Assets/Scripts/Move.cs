using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class Move : MonoBehaviour
{
    public float speed = 5;
    public bool move = false;
    //public Transform goal;
    public Vector3 goalPosition;
    public Quaternion goalRotation;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Move our position a step closer to the target.
        var step = speed * Time.deltaTime; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, goalPosition, step);
        if (Vector3.Distance(transform.position,goalPosition) < 0.001)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goalRotation, 1);
           
            if (gameObject.name.EndsWith("Card"))
            {
                if (GameObject.Find(gameObject.name + "s").transform.GetChild(0).TryGetComponent<TMP_Text>(out var counter))
                {
                    Debug.Log($"Nazwa:{gameObject.name}");
                    counter.text = (int.Parse(counter.text) + 1).ToString();
                    Destroy(gameObject);
                }
            }
        }
    }
}

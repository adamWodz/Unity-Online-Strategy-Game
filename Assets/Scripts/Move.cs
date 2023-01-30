using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class Move : MonoBehaviour
{
    public float speed = 5;
    public Vector3 goalPosition;
    public Quaternion goalRotation;

    void Update()
    {
        var step = speed * Time.deltaTime;     
        
        Vector2 direction = Vector2.MoveTowards(transform.position, goalPosition, step);
        
        transform.position = new Vector3(direction.x, direction.y, -1);          
        
        if (Vector2.Distance(transform.position, goalPosition) < 0.001)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goalRotation, 1);
           
            if (gameObject.name.EndsWith("Card"))
            {
                if (GameObject.Find(gameObject.name + "s").transform.GetChild(0).TryGetComponent<TMP_Text>(out var counter))
                {
                    counter.text = (int.Parse(counter.text) + 1).ToString();
                    Destroy(gameObject);
                }
            }
            else if(gameObject.name.EndsWith("CardBelongToOtherPlayer"))
                Destroy(gameObject);
        }
    }
}

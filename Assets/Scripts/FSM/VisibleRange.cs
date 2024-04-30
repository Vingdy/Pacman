using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibleRange : MonoBehaviour
{
    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            transform.parent.GetComponent<Monster>().canseePlayer = true;
            transform.parent.GetComponent<Monster>().lastSeenPlayer = collision.transform.position;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        transform.parent.GetComponent<Monster>().canseePlayer = false;
    }
}

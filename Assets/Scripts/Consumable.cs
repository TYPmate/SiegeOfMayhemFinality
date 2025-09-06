using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumable : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }


    //On collision heal player
    private void OnTriggerEnter(Collider other)
    {
        PlayerMotor player = other.GetComponent<PlayerMotor>();
        if (player != null)
        {
            player.TakeDamage(-100);
            Destroy(gameObject);
        }
    }
}
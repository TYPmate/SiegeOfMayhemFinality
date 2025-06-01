using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static PlayerMotor;

public class RockThrow : MonoBehaviour
{
    [SerializeField] Ability abilityInfo;
    public float floatAtPlayer = 1f;
    public float floatUp = 1f;
    public float moveSpeed = 1000f;
    float LifeTimer = 0;


    private bool isMoving = false;
    Vector3 targetPosition;
    private Transform playerTransform;

    void Start()
    {
        playerTransform = GameObject.FindObjectOfType<PlayerMotor>().transform;
        StartCoroutine(FloatSequence());
    }

    IEnumerator FloatSequence()
    {

        float timer = 0;
        while (timer < floatUp)
        {
            transform.position += Vector3.up * (Time.deltaTime * 7f);
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;
        isMoving = true;
        targetPosition = playerTransform.position;
    }

    private void Update()
    {
        if (isMoving)
        { LifeTimer += Time.deltaTime; }
            
        if (LifeTimer > abilityInfo.lifetime)
        {
            Destroy(this.gameObject);
        }
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isMoving)
        {
            if (other.CompareTag("Player"))
            {
                PlayerMotor playerMotor = other.GetComponent<PlayerMotor>();
                if (playerMotor != null)
                {
                    playerMotor.TakeDamage(abilityInfo.damage);
                    Destroy(gameObject);
                }

                Destroy(gameObject);
            }
        }
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [SerializeField] Camera FPCamera;
    [SerializeField] float range = 1f;
    [SerializeField] float damage = 50f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(FPCamera.transform.position, FPCamera.transform.forward, out hit, range))
        {
            Debug.Log("Kuole saatanan " + hit.transform.name);
            //TODO: add some hit effect for visual players
            EnemyHealth target = hit.transform.GetComponent<EnemyHealth>();
            if (target == null) return;
            //method on EnemyHealth that decreases the enemy's health
            target.TakeDamage(damage);
        }
        else
        {
            return;
        }
    }
}


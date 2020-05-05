using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private float _projectileSpeed = 10;
    [SerializeField]
    private float _projectileDamage = 15f;
    Vector3 moveDir;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        moveDir = Vector3.forward;
        moveDir = transform.TransformDirection(moveDir);
        transform.Translate(Vector3.forward * _projectileSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Player player = other.transform.GetComponent<Player>();

            if (player != null)
            {
                player.TakeDamage(_projectileDamage);

                Destroy(this.gameObject);
            }
        }
    }
}

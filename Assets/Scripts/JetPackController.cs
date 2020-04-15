using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class JetPackController : MonoBehaviour
{
    public float mass;
    public float drag;
    public float angularDrag;
    public float maxThrust;
    public float maxHeight;
    public float maxMovementForce;
    public float maxRotationTorque;

    Rigidbody rigidbody;
    float currHeight = 0;
    float currThrust = 0;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();

    }

    private void Update()
    {
        if (Input.GetAxisRaw("Height") == 1)
        {
            currHeight = Mathf.Clamp(currHeight += Time.deltaTime, 0, maxHeight);
            currThrust = Mathf.Clamp(currThrust += Time.deltaTime, 0, maxThrust);
        }
        else if (Input.GetAxis("Height") == -1)
        {
            currHeight = Mathf.Clamp(currHeight -= Time.deltaTime, 0, maxHeight);
            currThrust = Mathf.Clamp(currThrust += Time.deltaTime, 0, maxThrust);
        }
        Debug.Log(currHeight);
    }

    private void FixedUpdate()
    {
        Vector3 input = new Vector3();
        input.y = currThrust;
        input.x = Input.GetAxis("Horizontal");
        input.z = Input.GetAxis("Vertical");
        ControlJetPack(input, Input.GetAxis("Turn"));
    }

    void ControlJetPack(Vector3 input, float turn)
    {
        if (transform.position.y < currHeight)
            rigidbody.AddForce(Vector3.up * input.y * maxThrust);
        rigidbody.AddRelativeForce(Vector3.right * input.x * maxMovementForce);
        rigidbody.AddRelativeForce(Vector3.forward * input.z * maxMovementForce);
        rigidbody.AddTorque(Vector3.up * turn * maxRotationTorque);
    }

}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetrailDelete : MonoBehaviour
{
    //[SerializeField]
    //private Player _player;

    //private float _trailSpeed = 7f;

    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(Disappear());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator Disappear()
    {
        yield return new WaitForSeconds(0.69f);
        Destroy(this.gameObject);
    }
}

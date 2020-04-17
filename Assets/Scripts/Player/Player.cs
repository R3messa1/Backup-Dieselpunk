using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;

public class Player : MonoBehaviour
{
    private CharacterController _controller;
    [SerializeField]
    private float _movespeed = 9.0f;
    [SerializeField]
    private float _gravity = 9.8f;
    [SerializeField]
    private float _jumpSpeed = 10f;
    private float _verticalSpeed = 0;
    [SerializeField]
    private float _jetSpeedMultiplier = 2.6f;
    [SerializeField]
    private int _inAirJumps = 1;

    //dash related vars
    [SerializeField]
    private float _dashDistance = 10f;
    private const float _minHeldDuration = 0.2f;
    private float _jetHoldTime = 0;
    private bool _jetHeld = false;
    private Vector3 _dashDirection;

    //Fuel related vars
    [SerializeField]
    private float _fuelTank = 100f;
    [SerializeField]
    private float _fuelRechargeRate = 10f;
    [SerializeField]
    private float _fuelRechargeDelay = 3f;
    private float _fuelNotInUse = 0;
    [SerializeField]
    private float _jetFuelDrainPerSec = 1f;
    [SerializeField]
    private float _dashFuelDrain = 10f;
    private bool _fuelAvailable = true;
    


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray rayOrigin = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hitInfo;

            if (Physics.Raycast(rayOrigin, out hitInfo))
            {

            }
        }

        CalculateMovement();
        FuelCheck();

        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void CalculateMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        //creating movement and gravity functions
        Vector3 direction = new Vector3(horizontalInput, 0, verticalInput);
        Vector3 velocity = direction * _movespeed;
        velocity.y -= _gravity;

        //taking global space values and changing them into local
        velocity = transform.TransformDirection(velocity);

        if (_controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            _inAirJumps = 1;
            _verticalSpeed = _jumpSpeed;
        }
        else if (Input.GetButtonDown("Jump") && _inAirJumps == 1)
        {
            _verticalSpeed = _jumpSpeed;
            _inAirJumps--;
        }

        //some ugly code:
        _dashDirection = new Vector3(horizontalInput * _dashDistance, 0, verticalInput * _dashDistance);
        _dashDirection = transform.TransformDirection(_dashDirection);

        if(_fuelAvailable == true)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _movespeed *= _jetSpeedMultiplier;
                _jetHoldTime = Time.timeSinceLevelLoad;
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                if (!_jetHeld)
                {
                    _controller.Move(_dashDirection);
                    _fuelTank -= _dashFuelDrain * Time.deltaTime;
                }
                _jetHeld = false;
                SetDefaultSpeed();
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                _fuelTank -= _jetFuelDrainPerSec;
                if (Time.timeSinceLevelLoad - _jetHoldTime > _minHeldDuration)
                {
                    _jetHeld = true;
                }
            }
        }
        
        _verticalSpeed -= _gravity * Time.deltaTime;
        velocity.y = _verticalSpeed;

        _controller.Move(velocity * Time.deltaTime);
        Debug.Log("CURRENT FUEL: " + _fuelTank);
    }

    void FuelCheck()
    {
        FuelRechargeCooldown();
        if (_fuelTank <= 0)
        {
            _fuelAvailable = false;
            SetDefaultSpeed();
        }
        else
        {
            _fuelAvailable = true;
        }
    }
    void SetDefaultSpeed()
    {
        _movespeed = 9;
    }

    void FuelRechargeCooldown()
    {
        _fuelNotInUse = Time.timeSinceLevelLoad;
        if(Time.timeSinceLevelLoad - _fuelNotInUse < _fuelRechargeDelay && _fuelTank < 100)
        {
            _fuelTank += _fuelRechargeRate * Time.deltaTime;
        }
    }
}

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
    private float _verticalSpeed = 0f;
    [SerializeField]
    private float _crouchSlamSpeed = 20f;
    [SerializeField]
    private float _jetSpeedMultiplier = 3f;
    [SerializeField]
    private float _verticalJetSpeed = 3f;
    [SerializeField]
    private int _inAirJumps = 1;
    


    //dash related vars
    [SerializeField]
    private float _dashDistance = 10f;
    private const float _minHeldDuration = 0.2f;
    private float _jetHoldTime = 0f;
    private bool _jetHeld = false;
    private Vector3 _dashDirection;

    //Fuel related vars
    [SerializeField]
    private float _maxFuel = 100f;
    private float _fuelTank;
    [SerializeField]
    private float _fuelRechargeRate = 10f;
    [SerializeField]
    private float _fuelRechargeDelay = 3f;
    [SerializeField]
    private float _jetFuelDrainPerSec = 1f;
    [SerializeField]
    private float _dashFuelDrain = 10f;
    [SerializeField]
    private float _verticalJetCostMult = 2f;
    private bool _fuelAvailable = true;
    private bool _fuelInUse;
    private bool _canRecharge;
    private float _dJumpBoostDrain = 20f;

    // Start is called before the first frame update
    void Start()
    {
        _fuelTank = _maxFuel;

        Cursor.lockState = CursorLockMode.Locked;

        _controller = GetComponent<CharacterController>();


    }

    // Update is called once per frame
    void Update()
    {
        //shoot
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


        if(_canRecharge == true && !_fuelInUse)
        {
            _fuelTank += _fuelRechargeRate * Time.deltaTime;

        }

        if(!_fuelInUse)
        {
            
        }


        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (_fuelTank > _maxFuel)
        {
            _fuelTank = _maxFuel;
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

        //some ugly dash logic code:
        _dashDirection = new Vector3(horizontalInput * _dashDistance, 0, verticalInput * _dashDistance);
        _dashDirection = transform.TransformDirection(_dashDirection);


        //Jump and doublejump
        if (_controller.isGrounded)
        {
            _inAirJumps = 1;
            _verticalSpeed = -1;
            if (_controller.isGrounded && Input.GetButtonDown("Jump"))
            {
                _verticalSpeed = _jumpSpeed;
            }
        }
        else if (Input.GetButtonDown("Jump"))
        {
            if(_inAirJumps == 1)
            {
                _verticalSpeed = _jumpSpeed + 1;
                _inAirJumps--;
            }
            else if(_fuelAvailable && _inAirJumps == 0)
            {
                if (Input.GetButton("Jump") && _fuelTank > _dJumpBoostDrain)
                {
                    _verticalSpeed = _jumpSpeed + 1;
                    _fuelTank -= _dJumpBoostDrain;
                    _canRecharge = false;
                    StartCoroutine(FuelCooling());
                }
            }
        }
        


        //slamming/groundpound
        if (_controller.isGrounded == false && Input.GetKey(KeyCode.C))
        {
            Vector3 slamDirection = Camera.main.transform.forward * 2;
            slamDirection += Vector3.down;
            _controller.Move(slamDirection * _crouchSlamSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = transform.position;
        }

        //Jetting and dashing
        if (_fuelAvailable == true)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _movespeed *= _jetSpeedMultiplier;
                _jetHoldTime = Time.timeSinceLevelLoad;
                _fuelInUse = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                if (!_jetHeld && _fuelTank > _dashFuelDrain)
                {
                    _controller.Move(_dashDirection);
                    _fuelTank -= _dashFuelDrain;
                    _fuelInUse = true;
                }

                _fuelInUse = false;
                _jetHeld = false;
                SetDefaultSpeed();
                StartCoroutine(FuelCooling());
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                _canRecharge = false;
                _fuelTank -= _jetFuelDrainPerSec * Time.deltaTime;
                if (Time.timeSinceLevelLoad - _jetHoldTime > _minHeldDuration)
                {
                    _jetHeld = true;
                }
            }
        }

        _verticalSpeed -= _gravity * Time.deltaTime;
        velocity.y = _verticalSpeed;

        _controller.Move(velocity * Time.deltaTime);
        Debug.Log("CURRENT FUEL: " + _fuelTank + " And inairjummps = " + _inAirJumps);
    }

    void FuelCheck()
    {
        if (_fuelTank < 0)
        {
            _fuelTank = 0;
            _fuelAvailable = false;
            SetDefaultSpeed();
            StartCoroutine(FuelCooling());
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

    IEnumerator FuelCooling()
    {
        yield return new WaitForSeconds(_fuelRechargeDelay);
        _canRecharge = true;
    }
}
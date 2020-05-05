using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEditorInternal;
using UnityEngine;

public class Player : MonoBehaviour
{
    private CharacterController _controller;
    [SerializeField]
    private float _movespeed = 9.0f;
    [SerializeField]
    private float _gravity = 9.8f;
    [SerializeField]
    private float _maxHealth = 100f;
    private float _health;
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
    
    [SerializeField]
    private float _healRate = 10f;
    [SerializeField]
    private float _healFuelCost = 10f;
    [SerializeField]
    private GameObject _muzzleFlashPrefabLeft;
    [SerializeField]
    private GameObject _muzzleFlashPrefabRight;
    [SerializeField]
    private GameObject _slamDirtPrefab;

    //Smashstuff :3
    [SerializeField]
    private float _slamRadius = 30f;
    [SerializeField]
    private float _slamPower = 70f;
    private bool _slamUsed = false;
    [SerializeField]
    private float _slamCooldown = 2f;

    // Start is called before the first frame update
    void Start()
    {
        _muzzleFlashPrefabLeft.SetActive(false);
        _muzzleFlashPrefabRight.SetActive(false);

        _health = _maxHealth;
        _fuelTank = _maxFuel;

        Cursor.lockState = CursorLockMode.Locked;

        _controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("CURRENT FUEL: " + _fuelTank + " ANd health = " + _health);

        //shoot
        if (Input.GetMouseButton(0))
        {
            _muzzleFlashPrefabLeft.SetActive(true);
            Ray rayOrigin = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hitInfo;

            if (Physics.Raycast(rayOrigin, out hitInfo))
            {

            }
        }
        else
        {
            _muzzleFlashPrefabLeft.SetActive(false);
        }

        if (Input.GetMouseButton(1))
        {
            _muzzleFlashPrefabRight.SetActive(true);
        }
        else
        {
            _muzzleFlashPrefabRight.SetActive(false);
        }

        CalculateMovement();
        FuelCheck();

        if (Input.GetKey(KeyCode.Q) && _fuelAvailable && _health < _maxHealth)
        {
            FuelHeal();
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            _fuelInUse = false;
            StartCoroutine(FuelCooling());
        }

        //Fuel recharging
        if(_canRecharge == true && !_fuelInUse)
        {
            _fuelTank += _fuelRechargeRate * Time.deltaTime;

        }

        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (_fuelTank > _maxFuel)
        {
            _fuelTank = _maxFuel;
        }

        if(_health <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    void SlamAttack()
    {
        
        Instantiate(_slamDirtPrefab, transform.position, _slamDirtPrefab.transform.rotation);
        Destroy(_slamDirtPrefab.gameObject, 2.5f);

        Collider[] colliders = Physics.OverlapSphere(transform.position, _slamRadius);

        foreach(Collider nearbyObject in colliders)
        {
            CharacterController cC = nearbyObject.GetComponent<CharacterController>();
            if(cC != null)
            {
                cC.Move(Vector3.up * _slamPower * Time.deltaTime);
                _controller.transform.position = transform.position;
            }
        }

        StartCoroutine(SlamCooldown());
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
        if (Input.GetKey(KeyCode.C) && !_slamUsed)
        {
            Vector3 slamDirection = Camera.main.transform.forward * 2;
            slamDirection += Vector3.down;
            _controller.Move(slamDirection * _crouchSlamSpeed * Time.deltaTime);

            if (_controller.isGrounded)
            {
                Debug.Log("SLAM!");
                SlamAttack();
                _slamUsed = true;
                return;
            }
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

    IEnumerator SlamCooldown()
    {
        yield return new WaitForSeconds(_slamCooldown);
        _slamUsed = false;
    }

    public void TakeDamage(float amount)
    {
        _health -= amount;
    }

    public void FuelHeal()
    {
        _fuelTank -= _healFuelCost * Time.deltaTime;
        _health += _healRate * Time.deltaTime;
        _fuelInUse = true;
        _canRecharge = false;
    }
}
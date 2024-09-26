using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
   // private PlayerControls _playerControls;
    private CarController _carController;
    private ResetCar _resetCar;

    public float accel;
    public float handBrake;
    public float turn;

    public bool isPaused = false;

    public static Action<bool> OnPause;

    private void Awake()
    {
        /*_playerControls = new PlayerControls();
        _playerControls.Player.Enable();
        _playerControls.Player.Reset.performed += _ => _resetCar.Respawn();*/
//        _playerControls.Player.Pause.performed += Pause_performed;

        _carController = GetComponent<CarController>();
        _resetCar = GetComponent<ResetCar>();
    }

    /*
    private void Pause_performed(InputAction.CallbackContext context)
    {
        isPaused = !isPaused;
        OnPause?.Invoke(isPaused);
    }
    */

    private void Update()
    {
        turn = Input.GetAxis("Horizontal"); //_playerControls.Player.Accelerate.ReadValue<float>());
        accel = Input.GetAxis("Vertical"); //_playerControls.Player.Turn.ReadValue<float>();
        handBrake = Input.GetKey(KeyCode.Space) ? 1 : 0;  //_playerControls.Player.HandBrake.ReadValue<float>());
    }

    private void FixedUpdate()
    {
        _carController.Move(turn, accel, accel, handBrake);
    }
}

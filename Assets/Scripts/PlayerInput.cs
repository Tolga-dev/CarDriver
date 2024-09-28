using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerInput : MonoBehaviour
{
    [Header("Globals")]
    private CarController _carController;

    [Header("Audios")] 
    public List<AudioSource> wheelAudios = new List<AudioSource>();
    public AudioSource engineAudioSource;
    
    [Header("Input Values")]
    public float accel;
    public float handBrake;
    public float turn;

    [Header("Input Car UI Elements")]
    public Button brakeCar;
    public Button gasCar;
    public Button reverseCar;
    public VariableJoystick steel;

    [Header("Car Properties UI Elements")]
    public TextMeshProUGUI speedText;

    [Header("Game UI Elements")]
    public Button openMenu;
    public GameObject openMenuPanel;
    
    public Button reloadScene;
    public string currentGameNameScene;
    
    public Button replaceLocationToClosestPlace;
    
    public Slider wheelSoundSlider;
    public Slider engineSoundSlider;
    
    [Header("Roads")]
    public List<Transform> roads = new List<Transform>();
    
    [Header("Effects")]
    public SpeedEffectScaler speedEffect;
    
    public bool gasPressed = false;
    public bool reversePressed = false;
    public bool brakePressed = false;

    private void Awake()
    {
        _carController = GetComponent<CarController>();

        // Assign button event listeners
        AddButtonListeners();
    }

    private void LateUpdate()
    {
        speedText.text = Mathf.RoundToInt(_carController.CurrentSpeed) + " KM";
        speedEffect.Update(_carController.CurrentSpeed);
    }

    private void Update()
    {
#if UNITY_ANDROID
    // Android Inputs (UI)
    turn = steel.Horizontal;

    // Adjust accel based on button press status
    accel = 0;
    if (gasPressed)
        accel = 1;
    else if (reversePressed)
        accel = -1;

    // Adjust handBrake based on brake button status
    handBrake = brakePressed ? 1 : 0;
#else
        turn = Input.GetAxis("Horizontal");
        accel = Input.GetAxis("Vertical");
        handBrake = Input.GetKey(KeyCode.Space) ? 1 : 0;
#endif
    }

    private void FixedUpdate()
    {
        _carController.Move(turn, accel, accel, handBrake);
    }

    // Add listeners for UI buttons using EventTrigger for press and release
    private void AddButtonListeners()
    {
        AddEventTriggerListener(gasCar.gameObject, EventTriggerType.PointerDown, (eventData) => { OnGasPressed(); });
        AddEventTriggerListener(gasCar.gameObject, EventTriggerType.PointerUp, (eventData) => { OnGasReleased(); });

        AddEventTriggerListener(reverseCar.gameObject, EventTriggerType.PointerDown, (eventData) => { OnReversePressed(); });
        AddEventTriggerListener(reverseCar.gameObject, EventTriggerType.PointerUp, (eventData) => { OnReverseReleased(); });

        AddEventTriggerListener(brakeCar.gameObject, EventTriggerType.PointerDown, (eventData) => { OnBrakePressed(); });
        AddEventTriggerListener(brakeCar.gameObject, EventTriggerType.PointerUp, (eventData) => { OnBrakeReleased(); });
        
        replaceLocationToClosestPlace.onClick.AddListener(PutCarToClosestPlace);
        openMenu.onClick.AddListener(() =>
        {
            openMenuPanel.gameObject.SetActive(true);

            wheelSoundSlider.minValue = 0;
            wheelSoundSlider.maxValue = 1;
            wheelSoundSlider.value = wheelAudios[0].volume;
            
            engineAudioSource = gameObject.GetComponent<AudioSource>();
            
            engineSoundSlider.minValue = 0;
            engineSoundSlider.maxValue = 1;
            engineSoundSlider.value = engineAudioSource.volume;
            
        });
        reloadScene.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(currentGameNameScene);
        });
        
    }

    private void PutCarToClosestPlace()
    {
        var closestRoad = roads[0];
        foreach (var road in roads)
        {
            if(Vector3.Distance(road.position, transform.position) < Vector3.Distance(closestRoad.position, transform.position))
                closestRoad = road;
        }
        
        var roadSpline = closestRoad.GetChild(0);
        
        var closestNode = roadSpline.GetChild(0);

        for (int i = 0; i < roadSpline.childCount; i++)
        {
            var roadNode = roadSpline.GetChild(i);
            
            if(Vector3.Distance(roadNode.position, transform.position) < Vector3.Distance(closestNode.position, transform.position))
                closestNode = roadNode;
        }

        transform.position = closestNode.position + Vector3.up*2;
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    private void AddEventTriggerListener(GameObject target, EventTriggerType triggerType, System.Action<BaseEventData> action)
    {
        var trigger = target.GetComponent<EventTrigger>() ?? target.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = triggerType };
        
        entry.callback.AddListener(action.Invoke);
        trigger.triggers.Add(entry);
    }

    private void OnGasPressed()
    {
        gasPressed = true;
        reversePressed = false; // Ensure reverse is not pressed
    }

    private void OnReversePressed()
    {
        reversePressed = true;
        gasPressed = false; // Ensure gas is not pressed
    }

    private void OnBrakePressed()
    {
        brakePressed = true;
    }

    // Button released event handlers
    private void OnGasReleased()
    {
        gasPressed = false;
    }

    private void OnReverseReleased()
    {
        reversePressed = false;
    }

    private void OnBrakeReleased()
    {
        brakePressed = false;
    }

    public void ChangeWheelSound()
    {
        foreach (var audioSource in wheelAudios)
        {
            audioSource.volume = wheelSoundSlider.value;
        }
    }
    public void ChangeEngineSound()
    {
        engineAudioSource.volume = engineSoundSlider.value;
    }
}

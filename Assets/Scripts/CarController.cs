using UnityEngine;

namespace MobileUIButtons
{
    public class CarController : MonoBehaviour
    {
        [Header("Rb")]
        public Rigidbody playerRb;
        public float speed;

        [Header("Wheel Properties")]
        public WheelColliders colliders;
        public WheelMeshes wheelMeshes;
        
        [Header("Motor")]
        public float motorPower;
        
        [Header("Steering")]
        public AnimationCurve steeringCurve;
        public float slipAngle;
        public float slipAllowance = 0.5f; 

        [Header("Brake")]
        public float brakePower;
        public float frontBrakeTorque = 0.7f;
        public float backBrakeTorque = 0.3f;

        [Header("Input")] 
        public InputController inputController;

        [Header("Particle")] 
        public ParticleController particleController;

        [Header("Sound")] 
        public SoundController soundController;

        void Start()
        {
            particleController.InstantiateSmoke();
        }
        void Update()
        {
            speed = playerRb.velocity.magnitude;
            
            inputController.CheckInput();
            particleController.Update();
            soundController.Update();
            
            ApplyMotor();
            ApplySteering();
            ApplyBrake();
            ApplyWheelPositions();
        }

        void ApplyBrake()
        {
            var brakeInput = inputController.brakeInput;
            colliders.FRWheel.brakeTorque = brakeInput * brakePower * frontBrakeTorque;
            colliders.FLWheel.brakeTorque = brakeInput * brakePower * frontBrakeTorque;

            colliders.RRWheel.brakeTorque = brakeInput * brakePower * backBrakeTorque;
            colliders.RLWheel.brakeTorque = brakeInput * brakePower * backBrakeTorque;
        }

        void ApplyMotor()
        {
            var gasInput = inputController.gasInput;

            colliders.RRWheel.motorTorque = motorPower * gasInput;
            colliders.RLWheel.motorTorque = motorPower * gasInput;
        }

        void ApplySteering()
        {
            var steeringInput = inputController.steeringInput;

            float steeringAngle = steeringInput * steeringCurve.Evaluate(speed);
            if (slipAngle < 120f)
            {
                steeringAngle += Vector3.SignedAngle(transform.forward, playerRb.velocity + transform.forward, Vector3.up);
            }

            steeringAngle = Mathf.Clamp(steeringAngle, -90f, 90f);
            colliders.FRWheel.steerAngle = steeringAngle;
            colliders.FLWheel.steerAngle = steeringAngle;
        }

        void ApplyWheelPositions()
        {
            UpdateWheel(colliders.FRWheel, wheelMeshes.FRWheel);
            UpdateWheel(colliders.FLWheel, wheelMeshes.FLWheel);
            UpdateWheel(colliders.RRWheel, wheelMeshes.RRWheel);
            UpdateWheel(colliders.RLWheel, wheelMeshes.RLWheel);
        }

        void UpdateWheel(WheelCollider coll, MeshRenderer wheelMesh)
        {
            coll.GetWorldPose(out var position, out var quat);
            wheelMesh.transform.position = position;
            wheelMesh.transform.rotation = quat;
        }
        
    }

    [System.Serializable]
    public class InputController
    {
        public float gasInput;
        public float brakeInput;
        public float steeringInput;

        public CarController carController;

        public void CheckInput()
        {
            gasInput = Input.GetAxis("Vertical");
            steeringInput = Input.GetAxis("Horizontal");
            carController.slipAngle = Vector3.Angle(carController.transform.forward,
                carController.playerRb.velocity - carController.transform.forward);

            var movingDirection = Vector3.Dot(carController.transform.forward, carController.playerRb.velocity);

            switch (movingDirection)
            {
                case < -0.5f when gasInput > 0:
                case > 0.5f when gasInput < 0:
                    brakeInput = Mathf.Abs(gasInput);
                    break;
                default:
                    brakeInput = 0;
                    break;
            }
        }
    }

    [System.Serializable]
    public class ParticleController
    {
        public CarController carController;
        public WheelParticles wheelParticles;
        public GameObject smokePrefab;

        public bool isSlipping = false;
        public void InstantiateSmoke()
        {
            InstantiateWheelSmoke(carController.colliders.FRWheel, ref wheelParticles.FRWheel);
            InstantiateWheelSmoke(carController.colliders.FLWheel, ref wheelParticles.FLWheel);
            InstantiateWheelSmoke(carController.colliders.RRWheel, ref wheelParticles.RRWheel);
            InstantiateWheelSmoke(carController.colliders.RLWheel, ref wheelParticles.RLWheel);
        }

        private void InstantiateWheelSmoke(WheelCollider wheelCollider, ref ParticleSystem wheelParticle)
        {
            var position = wheelCollider.transform.position - Vector3.up * wheelCollider.radius;
            wheelParticle = Object.Instantiate(smokePrefab, position, Quaternion.identity, wheelCollider.transform)
                .GetComponent<ParticleSystem>();
        }
        public void Update()
        {
            CheckParticles();
        }


        public void CheckParticles()
        {
            var colliders = carController.colliders;
            var wheelColliders = new[] { colliders.FRWheel, colliders.FLWheel, colliders.RRWheel, colliders.RLWheel };
            var wheelParticlesArray = new[] { wheelParticles.FRWheel, wheelParticles.FLWheel, wheelParticles.RRWheel, wheelParticles.RLWheel };
            var wheelIsHit = new bool[wheelColliders.Length];

            isSlipping = false;
            for (var i = 0; i < wheelColliders.Length; i++)
            {
                if (wheelColliders[i].GetGroundHit(out var wheelHit))
                {
                    wheelIsHit[i] = CheckWheelSlip(wheelHit, wheelParticlesArray[i]);
                    
                    if (wheelIsHit[i])
                    {
                        isSlipping = true;
                    }
                }
            }
        }

        private bool CheckWheelSlip(WheelHit wheelHit, ParticleSystem wheelParticle)
        {
            var totalSlip = Mathf.Abs(wheelHit.sidewaysSlip) + Mathf.Abs(wheelHit.forwardSlip);

            if (totalSlip > carController.slipAllowance)
            {
                if (!wheelParticle.isPlaying)
                {
                    wheelParticle.Play();  // Play the particle effect if it's not already playing
                }
                return true;
            }
            else
            {
                if (wheelParticle.isPlaying)
                {
                    wheelParticle.Stop();  // Stop the particle effect if there's no slip
                }
                return false;
            }
        }
    }
    
      [System.Serializable]
    public class SoundController
    {
        public CarController carController;
        public AudioSource carEngineAudioSource;
        public AudioSource wheelAudioSource;

        public void Update()
        {
            PlayEngineSound();
            PlayWheelSlipSound();
        }
        
        private void PlayEngineSound()
        {
            float speed = carController.speed;
            carEngineAudioSource.pitch = Mathf.Lerp(0.8f, 2.0f, speed / 50f); // Adjust pitch based on speed
            if (!carEngineAudioSource.isPlaying)
            {
                carEngineAudioSource.Play();
            }
        }
        
        private void PlayWheelSlipSound()
        {
            if (carController.particleController.isSlipping)
            {
                if (!wheelAudioSource.isPlaying)
                {
                    wheelAudioSource.Play();
                }
            }
            else
            {
                if (wheelAudioSource.isPlaying)
                {
                    wheelAudioSource.Stop();
                }
            }
        }
        
    }

    

    [System.Serializable]
    public class WheelColliders
    {
        public WheelCollider FRWheel;
        public WheelCollider FLWheel;
        public WheelCollider RRWheel;
        public WheelCollider RLWheel;
    }

    [System.Serializable]
    public class WheelMeshes
    {
        public MeshRenderer FRWheel;
        public MeshRenderer FLWheel;
        public MeshRenderer RRWheel;
        public MeshRenderer RLWheel;
    }

    [System.Serializable]
    public class WheelParticles
    {
        public ParticleSystem FRWheel;
        public ParticleSystem FLWheel;
        public ParticleSystem RRWheel;
        public ParticleSystem RLWheel;
    }
}
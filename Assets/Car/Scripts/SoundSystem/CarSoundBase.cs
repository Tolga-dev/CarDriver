using System;
using UnityEngine;

namespace Car.Scripts.SoundSystem
{
    [Serializable]
    public class CarSoundBase
    {
        public AudioSource carEngineSound;
        public AudioSource tireScreechSound;
        
        public float initialCarEngineSoundPitch; 
    }
}
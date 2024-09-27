using System;
using UnityEngine;

[Serializable]
public class SpeedEffectScaler
{
    public Rigidbody rb; // Reference to the car's Rigidbody
    public GameObject effect; // The effect you want to scale (particle system or visual effect)

    public float minSpeed; // The minimum speed
    public float maxSpeed; // The maximum speed
    public float minSize; // The size at minimum speed
    public float maxSize; // The size at maximum speed

    public void Update(float speed)
    {
        var effectSize = Mathf.Lerp(minSize, maxSize, (speed - minSpeed) / (maxSpeed - minSpeed));

        effect.transform.localScale = new Vector3(effectSize, effectSize, effectSize);
    }
    
}
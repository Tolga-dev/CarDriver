using UnityEngine;

namespace CarAsset.Scripts
{
  public class CarTouchInput : MonoBehaviour
  {

    public bool changeScaleOnPressed = false;
    [HideInInspector]
    public bool buttonPressed = false;
    RectTransform rectTransform;
    Vector3 initialScale;
    float scaleDownMultiplier = 0.85f;

    void Start(){
      rectTransform = GetComponent<RectTransform>();
      initialScale = rectTransform.localScale;
    }

    public void ButtonDown(){
      buttonPressed = true;
      if(changeScaleOnPressed){
        rectTransform.localScale = initialScale * scaleDownMultiplier;
      }
    }

    public void ButtonUp(){
      buttonPressed = false;
      if(changeScaleOnPressed){
        rectTransform.localScale = initialScale;
      }
    }

  }
}

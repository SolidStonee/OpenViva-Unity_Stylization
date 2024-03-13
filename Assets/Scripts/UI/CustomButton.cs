using UnityEngine;
using UnityEngine.EventSystems;

namespace viva
{  
    public class CustomButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {

        public bool buttonIsPressed;

        public void OnPointerDown(PointerEventData eventData)
        {
            buttonIsPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            buttonIsPressed = false;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Was declared inside UnityEngine.UI; components bind by script GUID, so the move is safe.
namespace BTV.UI
{
    public class BlockerButton : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            GetComponent<Button>().onClick.Invoke();
        }
    }
}
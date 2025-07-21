using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Energy8.Identity.UI.Core.Compoents
{
    [RequireComponent(typeof(TMP_Text))]
    public class TextButton : MonoBehaviour, IPointerClickHandler
    {
        [HideInInspector] TMP_Text tmpText;
        public event Action<string> OnClick;

        void Awake()
        {
            tmpText = GetComponent<TMP_Text>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Vector3 mousePosition = Mouse.current.position.ReadValue();

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(tmpText, mousePosition, null);
            if (linkIndex == -1)
                return;

            OnClick?.Invoke(tmpText.textInfo.linkInfo[linkIndex].GetLinkID());
        }
    }
}
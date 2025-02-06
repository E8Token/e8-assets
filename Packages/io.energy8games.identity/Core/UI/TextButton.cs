using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

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
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(tmpText, Input.mousePosition, null);
        if (linkIndex == -1)
            return;
        OnClick?.Invoke(tmpText.textInfo.linkInfo[linkIndex].GetLinkID());
    }
}
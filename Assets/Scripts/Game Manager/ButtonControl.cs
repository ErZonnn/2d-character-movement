/*Created by Pawe³ Mularczyk*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonControl : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Animator _buttonAnimator;

    public void OnPointerClick(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _buttonAnimator.SetBool("_invaded", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _buttonAnimator.SetBool("_invaded", false);
    }
}

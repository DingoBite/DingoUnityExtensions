using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DingoUnityExtensions.MonoBehaviours.UI.Interactions
{
    public class ClickApplicationOpenLinkHandler : ClickLinkHandler
    {
        protected override void UrlClickedHandle(PointerEventData eventData, TMP_LinkInfo linkInfo, string url)
        {
            Application.OpenURL(url);
        }
    }
}

using System;
using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.Toggle.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.UnityViewProviders
{
    public class SelectableButton : UnityViewProvider<Button>
    {
        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [SerializeField] private TMP_Text _title;
        
        public event Action OnSelect;
        public event Action OnDeselect;

        [SerializeField] private ToggleSwapInfoBase _toggleSwapInfo;

        public void SetTitle(string title)
        {
            _title.text = title;
        }
        
        public void Select()
        {
            _toggleSwapInfo.SetViewActive(true);
            OnSelect?.Invoke();
        }

        public void Deselect()
        {
            _toggleSwapInfo.SetViewActive(false);
            OnDeselect?.Invoke();
        }
        
        public void SelectClick()
        {
            EventInvoke();
            Deselect();
        }

        private void Reset()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        protected override void SubscribeOnly() => View.onClick.AddListener(SelectClick);
        protected override void UnsubscribeOnly() => View.onClick.RemoveListener(SelectClick);
        protected override void OnSetInteractable(bool value)
        {
            View.interactable = value;
        }
    }
}
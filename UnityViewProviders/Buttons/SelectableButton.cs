using System;
using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.Core.Data;
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
        [SerializeField] private ToggleSwapInfoBase _toggleSwapInfo;
        [SerializeField] private bool _selectImmediately;
        
        public event Action OnSelect;
        public event Action OnDeselect;
        
        public void SetTitle(string title)
        {
            _title.text = title;
        }
        
        public void Select()
        {
            _toggleSwapInfo.SetViewActive(true.TimeContext(_selectImmediately));
            OnSelect?.Invoke();
        }

        public void Deselect()
        {
            _toggleSwapInfo.SetViewActive(false.TimeContext(_selectImmediately));
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
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace uPalette.Runtime.Core.Synchronizer
{
    public abstract class UPaletteStyleSelector : MonoBehaviour
    {
        public abstract void EnableStyle(int index);
        public abstract void DisableStyle(int index);
        public abstract void ChangeStyle<T>(int index, T entryId) where T : EntryId;
    }
    
    public abstract class UPaletteSynchronizerStyles<TEntryId, TValue> : UPaletteStyleSelector where TEntryId : EntryId
    {
        [SerializeField] private List<ValueSynchronizer<TValue>> _synchronizers;
        [SerializeField] private List<TEntryId> _styles;
        [SerializeField] private List<int> _selectedStyles;
        [SerializeField] private int _defaultStyle;
        [SerializeField] private int _activeStyle;
        
        public override void ChangeStyle<T>(int index, T entryId) => ChangeStyle(index, entryId as TEntryId);
        
        public void ChangeStyle(int index, TEntryId entryId)
        {
            if (index < 0 || index >= _styles.Count)
                return;
            _styles[index] = entryId;
            Refresh();
        }

        public override void DisableStyle(int index)
        {
            _selectedStyles.Remove(index);
            Refresh();
        }
        
        public override void EnableStyle(int index)
        {
            if (index < 0 || index >= _styles.Count)
                return;

            index = Math.Clamp(index, 0, _styles.Count - 1);
            if (!_selectedStyles.Contains(index))
                _selectedStyles.Add(index);
            _selectedStyles.Sort();
            Refresh();
        }

        private void SetStyle(int index)
        {
            _activeStyle = index;
            foreach (var colorSynchronizer in _synchronizers)
            {
                colorSynchronizer.SetEntryId(_styles[index].Value);
            }
        }
        
        private void Refresh()
        {
            if (_selectedStyles.Count == 0)
            {
                SetStyle(_defaultStyle);
                return;
            }
            SetStyle(_selectedStyles[0]);
        }

        private void OnValidate()
        {
            _selectedStyles.RemoveAll(i => i >= _styles.Count);
            Refresh();
        }
    }
}
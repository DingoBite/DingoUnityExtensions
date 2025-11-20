using System.Collections.Generic;
using UnityEngine;

namespace uPalette.Runtime.Core.Synchronizer
{
    public class UPaletteStyleSelectRoot : MonoBehaviour
    {
        [SerializeField] private List<UPaletteStyleSelector> _styleSelectors;
        [SerializeField] private int _styleIndex;

        private void OnValidate()
        {
            foreach (var uPaletteStyleSelector in _styleSelectors)
            {
                uPaletteStyleSelector.SetDefaultStyle(_styleIndex);
            }
        }
    }
}
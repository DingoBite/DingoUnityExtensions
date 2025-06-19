using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DingoUnityExtensions.MaterialPropertyBlockAnimations
{
    public class MaterialPropertyBlockModifierRoot : MonoBehaviour
    {
        [SerializeReference, SubclassSelector] private List<MaterialPropertyBlockModifier> _modifiers;
        [SerializeField] private bool _affectToSharedMaterial;
        [SerializeField] private Renderer _renderer;
        
        private MaterialPropertyBlock _block;

        public IReadOnlyList<MaterialPropertyBlockModifier> Modifiers => _modifiers;
        public float Time { get; private set; }
        
        public void Setup(IEnumerable<MaterialPropertyBlockModifier> modifiers) => _modifiers = modifiers.ToList();
        
        public void SetTime(float time)
        {
            Time = Math.Clamp(time, 0, 1);
            if (_renderer == null && !TryGetComponent(out _renderer))
                return;
            if (_affectToSharedMaterial)
            {
                if (!Application.isPlaying)
                    return;
                var material = _renderer.sharedMaterial;
                foreach (var modifier in _modifiers)
                {
                    modifier.Apply(material, time);
                }
            }
            else 
            {
                if (_block == null)
                    _block = new MaterialPropertyBlock();

                _renderer.GetPropertyBlock(_block);
                foreach (var modifier in _modifiers)
                {
                    modifier.Apply(_block, time);
                }

                _renderer.SetPropertyBlock(_block);
            }
        }

        private void OnEnable()
        {
            if (_modifiers != null)
                SetTime(-1);
        }

        private void OnDisable()
        {
            if (_modifiers != null)
                SetTime(-1);
        }
    }
}
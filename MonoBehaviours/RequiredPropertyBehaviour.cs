using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours
{
    public class RequiredPropertyBehaviour<T> : MonoBehaviour where T : Component
    {
        public class Protected : MonoBehaviour
        {
            [SerializeField] private T _component;
            protected T Component
            {
                get
                {
                    if (_component == null)
                    {
                        _component = GetComponent<T>();
                    }

                    return _component;
                }
            }

            private void Reset()
            {
                _component = GetComponent<T>();
            }
        }
        
        [SerializeField] private T _component;
        public T Component
        {
            get
            {
                if (_component == null)
                {
                    _component = GetComponent<T>();
                }

                return _component;
            }
        }

        private void Reset()
        {
            _component = GetComponent<T>();
        }
    }
}
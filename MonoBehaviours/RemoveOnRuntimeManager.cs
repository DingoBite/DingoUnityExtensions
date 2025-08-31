using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours
{
    public enum RemoveBehaviour
    {
        None,
        Disable,
        Destroy
    }
    
    public class RemoveOnRuntimeManager : MonoBehaviour
    {
        [SerializeField] private RemoveBehaviour _removeBehaviour;
        [SerializeField] private bool _handleEditorOnly;
        [SerializeField] private List<GameObject> _scopes;
        [TagField]
        [SerializeField] private string _tagToHide;
        
        private void Awake()
        {
            foreach (var scope in _scopes)
            {
                HandleScope(scope);
            }
        }

        private void HandleScope(GameObject go)
        {
            if (_removeBehaviour == RemoveBehaviour.None && _handleEditorOnly == false)
                return;

            HandleObject(go);
            if (go == null && _removeBehaviour == RemoveBehaviour.Destroy || !go.activeInHierarchy && _removeBehaviour == RemoveBehaviour.Disable)
                return;
            foreach (Transform tr in go.transform)
            {
                HandleObject(tr.gameObject);
            }
        }

        private void HandleObject(GameObject go)
        {
            var isDisable = _handleEditorOnly && go.CompareTag("EditorOnly") || go.CompareTag(_tagToHide);
            if (_removeBehaviour == RemoveBehaviour.Destroy)
                Destroy(go);
            else if (_removeBehaviour == RemoveBehaviour.Disable)
                go.SetActive(isDisable);
        }
    }
}
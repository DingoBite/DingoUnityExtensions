using System;
using Cysharp.Threading.Tasks;
using DingoProjectAppStructure.Core.Model;
using UnityEngine;

namespace DingoUnityExtensions.DingoGameFlow
{
    public abstract class GameFlowBase : MonoBehaviour
    {
        [field: SerializeField] protected bool DebugLog { get; private set; }
        
        protected AppModelRoot AppModelRoot { get; private set; }

        protected int GameFlowStepsCount { get; private set; } = -1;
        protected int FaultGameFlowStepsCount { get; private set; }
        protected int CanceledGameFlowStepsCount { get; private set; }
        
        protected int SuccessGameFlowStepsCount => GameFlowStepsCount - FaultGameFlowStepsCount - CanceledGameFlowStepsCount;

        private UniTask _lastUpdateStep = UniTask.CompletedTask;

        public void Initialize(AppModelRoot appModelRoot)
        {
            AppModelRoot = appModelRoot;
        }

        public void GameFlowStart() => CoroutineParent.AddLateUpdater(this, GameFlowUpdate, CoroutineOrderLayers.MAX_PRIORITY_SPECIAL);

        private void GameFlowUpdate()
        {
            try
            {
                var status = _lastUpdateStep.Status;
                if (status is UniTaskStatus.Canceled)
                    CanceledGameFlowStepsCount++;
                if (status is UniTaskStatus.Faulted)
                    FaultGameFlowStepsCount++;
                
                if (status is not UniTaskStatus.Pending)
                {
                    GameFlowStepsCount++;
                    _lastUpdateStep = GameFlowStep();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        protected abstract UniTask GameFlowStep();
    }
}
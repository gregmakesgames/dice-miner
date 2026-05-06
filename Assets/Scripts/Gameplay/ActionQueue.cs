using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceMiner.Gameplay
{
    public class ActionQueue : MonoBehaviour
    {
        private List<Action> _actions = new List<Action>();

        private bool _isProcessing = false;
        private bool _isActing = false;
        
        public void AddActionToQueue(Action action)
        {
            _actions.Add(action);
        }

        public void StartProcessingActions()
        {
            _isProcessing = true;
        }
        
        private void Update()
        {
            if (_isProcessing)
            {
                if (!_isActing)
                {
                    if (_actions.Count <= 0)
                    {
                        _isProcessing = false;
                    }
                    else
                    {
                        ProcessAction();
                    }
                }
            }
        }
        
        private async void ProcessAction()
        {
            if (_actions.Count <= 0) return;
            
            _isActing = true;
            
            _actions.Sort((a, b) => a.Priority - b.Priority);
            var topmostAction = _actions.First();
            _actions.Remove(topmostAction);
            
            await topmostAction.Act();
            
            _isActing = false;
        }
    }
}
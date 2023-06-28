using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CptnFabulous.StateMachines
{
    /// <summary>
    /// A class that handles multiple states at once.
    /// </summary>
    public abstract class MultiStateRunner : State
    {
        [SerializeField] internal List<State> states = new List<State>();
        public List<State> allStates => states;
        public abstract State currentState { get; }

        public override void OnSetup()
        {
            states.RemoveAll((s) => s == null);
            for (int i = 0; i < states.Count; i++)
            {
                states[i] = CloneFromAsset(states[i], this);
                states[i].OnSetup();
            }
        }
        protected override void OnEnter() => currentState?.SetActive(true);
        protected override void OnExit() => currentState?.SetActive(false);
        public override void OnUpdate() => currentState?.OnUpdate();
        public override void OnLateUpdate() => currentState?.OnLateUpdate();
        public override void OnFixedUpdate() => currentState?.OnFixedUpdate();
    }

    public abstract class StateMachine : MultiStateRunner
    {
        public sealed override State currentState => cachedCurrentState;
        protected State cachedCurrentState;

        protected abstract State DetermineCurrentState();

        public sealed override void OnUpdate()
        {
            State newState = DetermineCurrentState();
            if (newState != currentState)
            {
                currentState?.SetActive(false);
                Debug.Log($"{ObjectName(this)}: switching from {ObjectName(currentState)} to {ObjectName(newState)}");
                cachedCurrentState = newState;
                currentState?.SetActive(true);
            }
            base.OnUpdate();
        }

        public static string ObjectName(Object obj) => obj != null ? obj.name : "null";
    }
}
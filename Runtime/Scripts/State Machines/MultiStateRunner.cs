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
            for (int i = 0; i < states.Count; i++)
            {
                states[i] = CloneFromAsset(states[i], this);
                states[i].OnSetup();
            }
        }

        public override void OnEnter() => currentState?.OnEnter();
        public override void OnExit() => currentState?.OnExit();
        public override void OnUpdate() => currentState?.OnUpdate();
        public override void OnLateUpdate() => currentState?.OnLateUpdate();
        public override void OnFixedUpdate() => currentState?.OnFixedUpdate();
    }

    public abstract class StateMachine : MultiStateRunner
    {
        public override State currentState => cachedCurrentState;
        protected State cachedCurrentState;

        protected abstract State DetermineCurrentState();

        public override void OnUpdate()
        {
            State newState = DetermineCurrentState();
            if (newState != currentState)
            {
                currentState?.OnExit();
                cachedCurrentState = newState;
                currentState?.OnEnter();
            }
            base.OnUpdate();
        }
    }
}
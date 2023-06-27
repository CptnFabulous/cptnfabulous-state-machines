using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CptnFabulous.StateMachines
{

    /// <summary>
    /// Runs multiple states simultaneously.
    /// </summary>
    [CreateAssetMenu(fileName = "New Simultaneous State Executor", menuName = "State Controller/Simultaneous State Executor", order = 0)]
    public class SimultaneousStateExecutor : MultiStateRunner
    {
        public override State currentState => null;
        protected override void OnEnter() => states.ForEach((s) => s.SetActive(true));
        protected override void OnExit() => states.ForEach((s) => s.SetActive(false));
        public override void OnUpdate() => states.ForEach((s) => s.OnUpdate());
        public override void OnLateUpdate() => states.ForEach((s) => s.OnLateUpdate());
        public override void OnFixedUpdate() => states.ForEach((s) => s.OnFixedUpdate());
    }
}
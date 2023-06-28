using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CptnFabulous.StateMachines
{
    [CreateAssetMenu(fileName = "New DebugState", menuName = "State Controller/Debug/DebugState", order = 0)]
    public class DebugState : State
    {
        [SerializeField] string timeValueName = "Time in state";
        float timeEntered;

        float secondsElapsed => Time.time - timeEntered;

        public override void OnSetup()
        {
            Debug.Log("Setting up " + this);
        }
        protected override void OnEnter()
        {
            Debug.Log("Entering " + this);
            timeEntered = Time.time;
            controller.SetConditionValue(timeValueName, secondsElapsed);
        }
        protected override void OnExit()
        {
            controller.debugText = null;
            Debug.Log("Exiting " + this);
        }
        public override void OnUpdate()
        {
            Debug.Log("Updating " + this + ", time elapsed = " + secondsElapsed);
            controller.debugText = name;
            controller.SetConditionValue(timeValueName, secondsElapsed);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CptnFabulous.StateMachines
{
    public class DebugState : State
    {
        //[SerializeField] float secondsToWait = 5;
        [SerializeField] string timeValueName = "Time in state";
        float timeEntered;

        float secondsElapsed => Time.time - timeEntered;

        public override void OnSetup()
        {
            Debug.Log("Setting up " + this);
        }
        public override void OnEnter()
        {
            Debug.Log("Entering " + this);
            timeEntered = Time.time;
            controller.SetConditionValue(timeValueName, secondsElapsed);
        }
        public override void OnExit()
        {
            Debug.Log("Exiting " + this);
        }
        public override void OnUpdate()
        {
            Debug.Log("Updating " + this + ", time elapsed = " + secondsElapsed);
            controller.SetConditionValue(timeValueName, secondsElapsed);
        }
    }
}

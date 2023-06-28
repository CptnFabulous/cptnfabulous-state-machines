using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CptnFabulous.StateMachines
{
    [CreateAssetMenu(fileName = "New DebugFailableState", menuName = "State Controller/Debug/DebugFailableState", order = 1)]
    public class DebugFailableState : State
    {
        [SerializeField] float secondsToWait = 2;
        [SerializeField] bool willSucceed = true;

        float timeEntered = 0;
        float secondsElapsed => enabled ? Time.time - timeEntered : 0;
        public override Status GetStatus()
        {
            Status s = Status.Active;
            if (secondsElapsed >= secondsToWait)
            {
                s = willSucceed ? Status.Completed : Status.Blocked;
            }
            Debug.Log($"{this}: status is {s}, time elapsed = {secondsElapsed}");
            return s;

            if (secondsElapsed < secondsToWait) return Status.Active;
            return willSucceed ? Status.Completed : Status.Blocked;
        }

        public override void OnSetup()
        {
            //Debug.Log("Setting up " + this);
        }
        protected override void OnEnter()
        {
            timeEntered = Time.time;
            //Debug.Log("Entering " + this);
        }
        protected override void OnExit()
        {
            controller.debugText = null;
            //Debug.Log("Exiting " + this + ", status = " + finalStatus);
        }
        public override void OnUpdate()
        {
            controller.debugText = name;
            //Debug.Log($"Updating {this}, active = {enabled} time elapsed = {secondsElapsed}");
        }
    }
}
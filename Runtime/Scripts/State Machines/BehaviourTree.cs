using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CptnFabulous.StateMachines
{
    [CreateAssetMenu(fileName = "New Behaviour Tree", menuName = "State Controller/Behaviour Tree/Behaviour Tree", order = 0)]
    public class BehaviourTree : StateMachine
    {
        public BehaviourTreeNode baseNode;
        public State defaultState;
        public State onceTotallyCompleted;

        public override void OnSetup()
        {
            base.OnSetup();
            defaultState = CloneFromAsset(defaultState, this);
            onceTotallyCompleted = CloneFromAsset(onceTotallyCompleted, this);
        }
        protected override State DetermineCurrentState()
        {
            switch (baseNode.GetStatus())
            {
                case Status.Active: return baseNode;
                case Status.Blocked: return defaultState;
                case Status.Completed: return onceTotallyCompleted;
            }
            return null;
        }
    }
}


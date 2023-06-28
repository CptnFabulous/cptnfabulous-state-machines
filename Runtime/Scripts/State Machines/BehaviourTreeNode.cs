using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CptnFabulous.StateMachines
{
    [CreateAssetMenu(fileName = "New Behaviour Tree Node", menuName = "State Controller/Behaviour Tree/Node", order = 1)]
    public class BehaviourTreeNode : StateMachine
    {
        public enum Type
        {
            Sequence,
            Priority,
            // I'm wondering if I should have these as well
            // 'Do any of them'
            // 'Do all of them in any order'
        }

        // Sequence:
        // Current state is the first one on the list that isn't completed (null if all are completed)
        // Completed if all stages within are completed (i.e. current state is null)
        // Blocked if the current stage is blocked
        // Priority:
        // Current state is the first one on the list that isn't blocked (null if all are blocked)
        // Completed if one of the options is completed
        // Blocked if all the options are blocked (i.e. current state is null)

        public Type type;
        
        public override Status GetStatus()
        {
            CheckCompletion(out _, out Status status);
            return status;
        }
        protected override State DetermineCurrentState()
        {
            CheckCompletion(out State stateToRun, out _);
            return stateToRun;
        }
        /// <summary>
        /// Determines the state's status based on its type and sub-states, and which sub-state should be currently running.
        /// </summary>
        /// <param name="newCurrentStateIndex"></param>
        /// <param name="newStatus"></param>
        void CheckCompletion(out State stateToRun, out Status overallStatus)
        {
            stateToRun = null;
            
            // If the current state is no longer active (either completed or blocked)
            // If blocked and it's a sequence, mark as blocked.
            // If blocked but it's a priority, move to the next.
            // If completed and it's a sequence, move to the next.
            // If completed and it's a priority, mark as completed.

            // If a current state is present, check if it is still running or needs to change.
            if (currentState != null)
            {
                overallStatus = currentState.GetStatus();
                if (overallStatus == Status.Active)
                {
                    stateToRun = currentState;
                    return;
                }

                switch (type, overallStatus)
                {
                    case (Type.Sequence, Status.Blocked): return; // Current is blocked, cannot proceed to next state
                    case (Type.Priority, Status.Completed): return; // Current is completed, no need to proceed to next state
                }
            }

            // Switch to the next state if the current one is null or can no longer run, marking needing to move to the next state.

            // Increment the index to the next stage
            int index = (currentState != null) ? states.IndexOf(currentState) : -1;
            index++;

            // If the index is still within the array range, check this next state to see if it's valid
            if (index < states.Count)
            {
                stateToRun = states[index];
                overallStatus = stateToRun.GetStatus();
            }
            else
            {
                // Current state is null since there's no valid state to run.
                stateToRun = null;
                // If sequence type, no more states means everything's completed.
                // If priority type, no more states means nothing can run.
                overallStatus = (type == Type.Sequence) ? Status.Completed : Status.Blocked;
            }
        }

        protected override void OnEnter()
        {
            cachedCurrentState = null; // Resets the state
            base.OnEnter();
        }
    }
}
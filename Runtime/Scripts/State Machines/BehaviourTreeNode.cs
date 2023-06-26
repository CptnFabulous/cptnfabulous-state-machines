using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CptnFabulous.StateMachines
{
    [CreateAssetMenu(fileName = "New Behaviour Tree Node", menuName = "State Controller/Behaviour Tree/Behaviour Tree Node", order = 1)]
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

        // (The two above are different so that it can check to run the enter and exit functions)
        int stateRunningIndex = 0; // The sub-state that's currently running
        int stateToRunIndex = 0; // The sub-state that should be running
        Status cachedStatus; // Whether or not this state should be running

        public override State currentState => GetFromListIndex(states, stateRunningIndex);
        public override Status GetStatus()
        {
            CheckCompletion(out stateToRunIndex, out cachedStatus);
            return cachedStatus;
        }

        public override void OnEnter()
        {
            stateRunningIndex = stateToRunIndex; // Reset the index to represent the state that's supposed to run
            base.OnEnter();
        }
        protected override State DetermineCurrentState()
        {
            CheckCompletion(out stateToRunIndex, out cachedStatus);
            return GetFromListIndex(states, stateToRunIndex);
        }
        /// <summary>
        /// Determines the state's status based on its type and sub-states, and which sub-state should be currently running.
        /// </summary>
        /// <param name="newCurrentStateIndex"></param>
        /// <param name="newStatus"></param>
        void CheckCompletion(out int newCurrentStateIndex, out Status newStatus)
        {
            // Cycle through to determine the current state and if it can proceed
            for (int i = stateRunningIndex; i < states.Count; i++)
            {
                newCurrentStateIndex = i;
                newStatus = states[i].GetStatus();

                // If current state is active, return that because that's the one that needs to be performed
                if (newStatus == Status.Active) return;

                switch (type, newStatus)
                {
                    case (Type.Sequence, Status.Completed): continue; // Move onto the next state in the sequence
                    case (Type.Sequence, Status.Blocked): return; // Current is blocked, cannot proceed to next state
                    case (Type.Priority, Status.Completed): return; // Current is completed, no need to proceed to next state
                    case (Type.Priority, Status.Blocked): continue; // Check if the next state can be performed
                }
            }

            // Current state is null since there's no valid state to run.
            newCurrentStateIndex = states.Count;
            // If sequence type, no more states means everything's completed.
            // If priority type, no more states means nothing can run.
            newStatus = (type == Type.Sequence) ? Status.Completed : Status.Blocked;
        }

        /// <summary>
        /// Returns the correct entry, otherwise returns null if outside the range.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        static T GetFromListIndex<T>(List<T> list, int index)
        {
            bool withinArray = index == Mathf.Clamp(index, 0, list.Count - 1);
            return withinArray ? list[index] : default;
        }
    }
}
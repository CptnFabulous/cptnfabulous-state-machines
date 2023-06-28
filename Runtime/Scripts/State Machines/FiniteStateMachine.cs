using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CptnFabulous.StateMachines
{
    [CreateAssetMenu(fileName = "New Finite State Machine", menuName = "State Controller/Finite State Machine", order = 0)]
    public class FiniteStateMachine : StateMachine
    {
        public List<StateTransition> transitions = new List<StateTransition>();

        [SerializeField] internal Vector2 entryNodePosition = new Vector2(100, 100);
        [SerializeField] internal Vector2 exitStatePosition = new Vector2(500, 100);
        [SerializeField] internal Vector2 fromAnyStatePosition = new Vector2(100, 200);

        public State entryState
        {
            get => states.Count > 0 ? states[0] : null;
            set
            {
                if (states.Contains(value) == false) return;
                states.Remove(value);
                states.Insert(0, value);
            }
        }

        public override void OnSetup()
        {
            base.OnSetup();

            // For each transition, find the shared assets and replace them with the newly duplicated versions.
            foreach (StateTransition t in transitions)
            {
                t.from = states.Find((s) => s.sharedAsset == t.from);
                t.to = states.Find((s) => s.sharedAsset == t.to);
            }

            cachedCurrentState = entryState; // Set entry state
        }
        protected override State DetermineCurrentState()
        {
            // Check transitions to see if there are any heading from the current state
            foreach (StateTransition t in transitions)
            {
                State from = t.from;
                State to = t.to;

                // If a transition is not from this state (or 'any state'), ignore it.
                if (from != null && from != currentState) continue;

                // If the state being transitioned to is not in this FSM (and isn't the 'exit'), ignore it.
                if (to != null && states.Contains(to) == false) continue;

                // If a transition from this state is active and the condition is met, return this one (no need to look at the other transitions)
                if (t.ConditionMet(controller)) return to;
                // Otherwise check the next transition
            }

            return currentState; // No transitions made
        }

    }

    [System.Serializable]
    public class StateTransition
    {
        public StateTransition(State from, State to)
        {
            this.from = from;
            this.to = to; 
        }
        
        // BUG TO FIX: The transitions are referencing the original state rather than the clone
        public bool active = true;
        public State from;
        public State to;
        public string conditionData;
        
        // Examples:
        // timeElapsed/float/greaterOrEqual/5f
        // target/object/exists
        // remainingHealth/int/lessThan/10

        public bool ConditionMet(StateRunner runner) => active && ConditionCheck(conditionData, runner);
        public static bool ConditionCheck(string conditionData, StateRunner runner)
        {
            // I just put all this inside a try block
            // Easier than continually checking if the strings parse correctly for each type of check
            try
            {
                string[] tags = conditionData.Split('/');
                object value = runner.conditionValues[tags[0]];
                switch (tags[1])
                {
                    // Check the value against the target
                    case "float": return NumberCheck((float)value, float.Parse(tags[3]), tags[2]);
                    case "int": return NumberCheck((int)value, int.Parse(tags[3]), tags[2]);
                    // Determine if the bool matches the desired value
                    case "bool": return (bool)value == bool.Parse(tags[2]);
                    // Determine if the trigger was activated this frame
                    case "trigger": return (int)value >= Time.frameCount;
                    // Check if the object exists, is null, or is/isn't equal to another object in the state runner's values
                    case "object":
                        switch (tags[2])
                        {
                            case "exists": return value != null;
                            case "isNull": return value == null;
                            case "equals": return value == runner.conditionValues[tags[3]];
                            case "notEqual": return value != runner.conditionValues[tags[3]];
                        }
                        break;
                }
            }
            catch
            {
                Debug.LogError($"({conditionData}): parsing failed, returning false");
                return false;
            }

            Debug.LogWarning("No valid conclusion found, returning false");
            return false;
        }
        static bool NumberCheck(float value, float target, string operatorType)
        {
            switch (operatorType)
            {
                case "equals": return value == target;
                case "notEqual": return value != target;
                case "greaterThan": return value > target;
                case "lessThan": return value < target;
                case "greaterOrEqual": return value >= target;
                case "lessOrEqual": return value <= target;
            }
            return false;
        }
    }
}

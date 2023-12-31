using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CptnFabulous.StateMachines
{
    public class StateRunner : MonoBehaviour
    {
        [SerializeField] State rootState;
        /*
        [Header("Conditions")]
        [SerializeField] List<string> conditionNames;
        */
        public Dictionary<string, object> conditionValues = new Dictionary<string, object>();

        public bool displayDebugMessage = true;
        [HideInInspector] public string debugText;

        #region MonoBehaviour functions
        private void Awake()
        {
            // Set up root state
            rootState = State.CloneFromAsset(rootState, null, this);
            rootState.OnSetup();
        }
        private void OnEnable() => rootState.SetActive(true);
        private void OnDisable() => rootState.SetActive(false);
        private void Update() => rootState.OnUpdate();
        private void LateUpdate() => rootState.OnLateUpdate();
        private void FixedUpdate() => rootState.OnFixedUpdate();
        private void OnGUI()
        {
            if (displayDebugMessage && string.IsNullOrEmpty(debugText) == false)
            {
                GUI.Label(new Rect(0, 0, 500, 100), debugText);
            }    
        }
        #endregion

        public void SetConditionValue(string name, object value) => conditionValues[name] = value; // Add a value to the dictionary and update it.
        public void SetTrigger(string name) => conditionValues[name] = Time.frameCount;
    }

    public abstract class State : ScriptableObject
    {
        public enum Status
        {
            Active,
            Completed,
            Blocked
        }
        
        public State sharedAsset { get; private set; }
        public State parentState { get; private set; }
        public StateRunner controller { get; private set; }
        
        [SerializeField, HideInInspector] internal Vector2 editorPosition;

        bool e = false;

        public bool enabled => e;
        public void SetActive(bool value)
        {
            e = value;
            if (value) OnEnter();
            else OnExit();
        }

        public virtual Status GetStatus() => Status.Active;

        public virtual void OnSetup() { }
        protected virtual void OnEnter() { }
        protected virtual void OnExit() { }
        public virtual void OnUpdate() { }
        public virtual void OnLateUpdate() { }
        public virtual void OnFixedUpdate() { }

        public virtual void ShowInEditorWindow(ref int windowIndex) => StateMachineEditorFunctions.DrawStateWindow(this, windowIndex, null);
        
        /// <summary>
        /// Sets up a state for runtime, so new instances can have their own data.
        /// <para>E.g. two instances of the same enemy with the same AI setup, responding to different situations.</para>
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        internal static T CloneFromAsset<T>(T original, State parent, StateRunner controller) where T : State
        {
            if (original == null) return null;

            T newState = Instantiate(original); // Creates a clone of the state
            newState.name = original.name;
            newState.sharedAsset = original; // Assigns a reference to the original asset
            newState.parentState = parent; // Assigns its parent in the hierarchy
            newState.controller = controller; // Assigns the root controller running the hierarchy.
            return newState;
        }
    }
}

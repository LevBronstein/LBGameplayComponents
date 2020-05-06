using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LBGameplay
{
    [System.Serializable]
    public struct LBInternalState
    {
        public string Name;

        public LBInternalState(string _name)
        {
            Name = _name;
        }

        public LBInternalState(SerializedProperty _state)
        {
            SerializedProperty st;

            st = _state;
            st.Next(true);
            Name = st.stringValue;
        }

        public void SetSerializedProperty(SerializedProperty p)
        {
            p.Next(true);
            p.stringValue = Name;
        }

        public static LBInternalState Default
        {
            get
            {
                return new LBInternalState("Default");
            }
        }
    }

    [System.Serializable]
    public struct LBStateSwitch
    {
        public int ParamID;
        public int State; 

        public LBStateSwitch(int _id, int _state)
        {
            ParamID = _id;
            State = _state;
        }
    }

    [AddComponentMenu("LBGameplay/Stated Linked Gameplay Component (Dummy)")]
    public class LBStatedLinkedGameplayComponent : LBLinkedConditionalGameplayComponent
    {
        [SerializeField]
        protected int current_state; // private?
        [SerializeField]
        protected LBInternalState[] states = new LBInternalState[] { LBInternalState.Default };
        [SerializeField]
        protected int[] input_state_switch; // -1 don't switch, 0 switch to default
        [SerializeField]
        protected int[] output_state_switch; // -1 don't switch, 0 switch to default
        [SerializeField]
        protected bool input_switch_to_default; // allow to switch to state '0' when no other switch found
        [SerializeField]
        protected bool output_switch_to_default; // allow to switch to state '0' when no other switch found

        //protected int FindInputStateSwitch(int _id)
        //{
        //    int result = -1;
        //    input_state_switch.TryGetValue(_id, out result);
        //    return result;
        //}


        // нужно ли?
        //protected override bool Init()
        //{
        //    if( base.Init())
        //    {
        //        InitStates();
        //        return true;
        //    }

        //    return false;
        //}

        //protected void InitStates() { }

        protected virtual void OnSwitchState() { }

        protected virtual bool bCanSwithState(int new_state)
        {
            if (states != null && new_state >= 0 && new_state < states.Length)
                return true;

            return false;
        }

        protected virtual bool SwitchState(int new_state)
        {
            if (bCanSwithState(new_state))
            {
                current_state = new_state;
                OnSwitchState();
                return true;
            }

            return false;
        }

        protected bool SwitchStateOnTransferIn(LBGameplayComponentLink input_link)
        {
            if (input_state_switch != null && input_link.ParamID >= 0 && input_link.ParamID < input_state_switch.Length)
            {
                SwitchState(input_state_switch[input_link.ParamID]);
                return true;
            }
            else
            {
                if (input_switch_to_default)
                {
                    SwitchState(0);
                    return true;
                }
            }

            return false;
        }

        protected override void OnTransferIn(LBGameplayComponentLink link)
        {
            base.OnTransferIn(link);

            SwitchStateOnTransferIn(link);
        }

        protected override void Perform()
        {
            base.Perform();

            PerformState();
        }

        // performes current state
        protected virtual void PerformState()
        {

        }

        public int InternalStateID
        {
            get
            {
                return current_state;
            }
        }

        public LBInternalState InternalState
        {
            get
            {
                return states[current_state];
            }
        }

        public LBInternalState[] AllInternalStates
        {
            get
            {
                return states;
            }
        }

        public string InternalStateName
        {
            get
            {
                return states[current_state].Name;
            }
        }

        public string[] AllInternnalStateNames
        {
            get
            {
                string[] names = new string[0];
                int i;

                Array.Resize(ref names, states.Length);

                for (i = 0; i < states.Length; i++)
                    names[i] = states[i].Name;

                return names;
            }
        }

        public bool bCanGoToState(int state)
        {
            if (bCanSwithState(state))
                return true;

            return false;
        }

        public bool GoToState(int state)
        {
            return SwitchState(state);
        }
    }

    namespace Editors
    {
        [CustomEditor(typeof(LBStatedLinkedGameplayComponent))]
        public class LBStatedLinkedGameplayComponent_ED : LBLinkedConditionalGameplayComponent_ED
        {
            string new_state = ""; // ПАЧИМУ?!?
            int new_state_id;

            protected override void ClearAllLinks()
            {
                base.ClearAllLinks();

                SerializedProperty input_switches;
                SerializedProperty output_switches;

                input_switches = serializedObject.FindProperty("input_state_switch");
                output_switches = serializedObject.FindProperty("output_state_switch");

                input_switches.ClearArray();
                output_switches.ClearArray();
            }

            protected LBInternalState[] GetAllStates()
            {
                SerializedProperty states;
                List<LBInternalState> all_states;
                int i;

                all_states = new List<LBInternalState>();
                states = serializedObject.FindProperty("states");

                for (i=0; i<states.arraySize; i++)
                {
                    all_states.Add(new LBInternalState(states.GetArrayElementAtIndex(i)));
                }

                return all_states.ToArray();
            }

            protected string [] GetAllStateNames()
            {
                LBInternalState[] all_states;
                string[] all_state_names = new string[0];
                int i;

                all_states = GetAllStates();
                Array.Resize(ref all_state_names, all_states.Length);

                for (i = 0; i < all_states.Length; i++)
                    all_state_names[i] = all_states[i].Name;

                return all_state_names;
            }

            protected int GetStateCount()
            {
                SerializedProperty states;
                states = serializedObject.FindProperty("states");
                return states.arraySize;
            }

            protected void SetInputLinkStateParam(int id, int value)
            {
                LBGameplayComponentLink l;
                SerializedProperty links, input_states, s;

                links = serializedObject.FindProperty("inputs");
                l = new LBGameplayComponentLink(links, id);

                input_states = serializedObject.FindProperty("input_state_switch");

                input_states.GetArrayElementAtIndex(l.ParamID).intValue = value;
            }

            protected int FindOrCreateInputLinkStateParam(int id)
            {
                LBGameplayComponentLink l;
                SerializedProperty links, input_states, s;

                links = serializedObject.FindProperty("inputs");
                l = new LBGameplayComponentLink(links, id);

                input_states = serializedObject.FindProperty("input_state_switch");

                if (l.ParamID >= input_states.arraySize)
                {
                    input_states.arraySize = l.ParamID + 1;
                    input_states.GetArrayElementAtIndex(l.ParamID).intValue = 0;
                }

                return input_states.GetArrayElementAtIndex(l.ParamID).intValue;
            }

            protected override void DisplayOutputLinkContent(int id)
            { 
                
            }

            protected override void DisplayInputLinkContent(int id) 
            {
                base.DisplayInputLinkContent(id);

                if (GetStateCount() <= 0)
                    return;

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Switch to state");
                new_state_id = EditorGUILayout.Popup(FindOrCreateInputLinkStateParam(id), GetAllStateNames());
                SetInputLinkStateParam(id, new_state_id);
                GUILayout.EndHorizontal();
            }

            protected void AddNewState(string name)
            {
                SerializedProperty states;
                LBInternalState state;

                if (name == "")
                    return;

                state = new LBInternalState(name);
                states = serializedObject.FindProperty("states");

                states.arraySize++;
                state.SetSerializedProperty(states.GetArrayElementAtIndex(states.arraySize - 1));
            }

            protected void DeleteAllStates()
            {
                LBInternalState default_state = LBInternalState.Default;
                SerializedProperty states;
                SerializedProperty input_output_states;

                states = serializedObject.FindProperty("states");
                states.ClearArray();
                states.arraySize++;
                default_state.SetSerializedProperty(states.GetArrayElementAtIndex(0));

                input_output_states=serializedObject.FindProperty("input_state_switch");
                input_output_states.ClearArray();
                input_output_states = serializedObject.FindProperty("output_state_switch");
                input_output_states.ClearArray();
            }

            protected virtual void DisplayStateProperties_Editor()
            {
                string[] all_state_names;

                
                all_state_names = GetAllStateNames();

                GUIStyle centered = GUI.skin.GetStyle("Label");
                centered.alignment = TextAnchor.UpperCenter;

                GUILayout.Space(16);
                GUILayout.Label("------ State properties ------", centered);
                GUILayout.Space(16);

                if (GetStateCount() == 0)
                    DeleteAllStates();

                GUILayout.BeginHorizontal();
                new_state = EditorGUILayout.TextField(new_state);
                if (GUILayout.Button("Add new state"))
                {
                    if (!Array.Exists(all_state_names, x => { return x == new_state; }))
                        AddNewState(new_state);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(16);
                if (GUILayout.Button("Clear all states"))
                {
                    DeleteAllStates();
                }
            }

            protected override void DisplayInfo_Game()
            {
                LBStatedLinkedGameplayComponent t = (LBStatedLinkedGameplayComponent)target;

                base.DisplayInfo_Game();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Internal state:");
                GUILayout.Label(t.InternalStateName);
                GUILayout.EndHorizontal();
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                DisplayStateProperties_Editor();

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

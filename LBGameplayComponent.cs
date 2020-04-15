using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LBGameplay
{
    public enum LBGameplayComponentState
    {
        Inactive = 0,
        Active = 1,
    }

    [Flags]
    public enum LBGameplayComponentActivationTypes : byte
    {
        Internal = 1,                         //00000001
        External = 2,                         //00000010
        InternalOrExternal = 3,               //00000011
        SelfInternal = 128,                   //10000000
    }

    public enum LBMessageTypes
    {
        Warning = 1,
        Error = 2,
        Notification = 4,
    }

    [AddComponentMenu("LBGameplay/Gameplay Component (Dummy)")]
    public class LBGameplayComponent : MonoBehaviour
    {
        [SerializeField]
        protected string component_name;
        [SerializeField]
        protected LBGameplayComponentState state;
        [SerializeField]
        protected LBGameplayComponentActivationTypes activation;
        [SerializeField]
        protected LBGameplayComponentActivationTypes deactivation;

        protected virtual bool Init()
        {
            activation = LBGameplayComponentActivationTypes.External;
            deactivation = LBGameplayComponentActivationTypes.External;

            return true;
        }
	
        protected virtual void Perform()
        {

        }

        virtual protected bool bCanTurnOn()
        {
            if (state == LBGameplayComponentState.Inactive)
                return true;

            return false;
        }

        virtual public bool bCanActivate()
        {
            return bCanTurnOn();
        }
        
        virtual public bool bCanSelfActivate()
        {
            return bCanTurnOn() && activation.HasFlag(LBGameplayComponentActivationTypes.SelfInternal); 
        }

        protected bool TurnOn()
        {
            if (bCanTurnOn())
            { 
                state = LBGameplayComponentState.Active;
                OnActivate();
                return true;
            }

            return false;
        }

        virtual protected bool ActivateInternal()
        {
            if (state == LBGameplayComponentState.Inactive)
            {
                if (bCanActivate())
                {
                    TurnOn();
                    return true;
                }
                else
                {
                    LogMessage("This component currently cannot be activated!", LBMessageTypes.Warning);
                    return false;
                }
            }
            else
            {
                LogMessage("This component cannot be deactivated -- it has been already deactivated!", LBMessageTypes.Warning);
                return false;
            }
        }

        /// <summary>
        /// Automatically activates this component, @activation value should be set to 129 (10000001)
        /// </summary>
        /// <returns></returns>
        virtual protected bool TrySelfActivate()
        {
            if (bCanSelfActivate())
            {
                return ActivateInternal();
            }

            return false;
        }

        /// <summary>
        /// Activate this component from outside, i.e. from another object of another class
        /// </summary>
        /// <returns>Returns true, if activation was succesfull</returns>
        virtual public bool Activate()
        {
            if (activation.HasFlag(LBGameplayComponentActivationTypes.External))
            {
                return ActivateInternal();
            }
            else
            {
                LogMessage("This component cannot be activated externally!", LBMessageTypes.Warning);
                return false;
            }
        }

        /// <summary>
        /// Called when this component is activated (also in every derived class)
        /// </summary>
        protected virtual void OnActivate()
        {

        }

        virtual protected bool bCanTurnOff()
        {
            if (state == LBGameplayComponentState.Active)
                return true;

            return false;
        }

        virtual public bool bCanDeactivate()
        {
            return bCanTurnOff();
        }

        virtual public bool bCanSelfDeactivate()
        {
            return bCanTurnOff() && deactivation.HasFlag(LBGameplayComponentActivationTypes.SelfInternal);
        }

        protected bool TurnOff()
        {
            if (bCanTurnOff())
            { 
                state = LBGameplayComponentState.Inactive;
                OnDeactivate();
                return true;
            }

            return false;
        }

        virtual protected bool DeactivateInternal()
        {
            if (state == LBGameplayComponentState.Active)
            {
                if (bCanDeactivate())
                {
                    TurnOff();
                    return true;
                }
                else
                {
                    LogMessage("This component currently cannot be deactivated!", LBMessageTypes.Warning);
                    return false;
                }
            }
            else
            {
                LogMessage("This component cannot be deactivated -- it has been already deactivated!", LBMessageTypes.Warning);
                return false;
            }
        }

       /// <summary>
        /// Automatically deactivates this component, @deactivation value should be set to 129 (10000001)
        /// </summary>
        /// <returns></returns>
        virtual protected bool TrySelfDeactivate()
        {
            if (bCanSelfDeactivate())
            {
                return DeactivateInternal();
            }

            return false;
        }

        /// <summary>
        /// Deactivate this component from outside, i.e. from another object of another class
        /// </summary>
        /// <returns>Returns true, if deactivation was succesfull</returns>
        virtual public bool Deactivate()
        {
            if (deactivation.HasFlag(LBGameplayComponentActivationTypes.External))
            {
                return DeactivateInternal();
            }
            else
            {
                LogMessage("This component cannot be activated externally!", LBMessageTypes.Warning);
                return false;
            }
        }

        protected virtual void OnDeactivate()
        {

        }

        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {
            Perform();
        }

        public string Name
        {
            get
            {
                return component_name;
            }
        }

        public LBGameplayComponentState State
        {
            get
            {
                return state;
            }
        }

        protected virtual void LogMessage(string message, LBMessageTypes message_type)
        {

        }
    }

    namespace Editors
    {
        [CustomEditor(typeof(LBGameplayComponent))]
        public class LBGameplayComponent_ED : Editor
        {
            protected void MakeTitleBar(string title)
            {
                GUILayout.Label("╔═══════════════════════════════════╗");

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(title);
                if (GUILayout.Button("-", GUILayout.Width(GUI.skin.label.fontSize), GUILayout.Height(GUI.skin.label.fontSize)))
                {
                    
                }
                if (GUILayout.Button("▄", GUILayout.Width(GUI.skin.label.fontSize), GUILayout.Height(GUI.skin.label.fontSize)))
                {
                   
                }
                GUILayout.EndHorizontal();

                GUILayout.Label("╚═══════════════════════════════════╝");
            }

            protected void MakeCenteredLabel(string text)
            {
                GUIStyle centered = GUI.skin.GetStyle("Label");
                centered.alignment = TextAnchor.UpperCenter;

                GUILayout.Label(text, centered);
            }

            protected void MakeCenteredLabel(string text, uint space_before, uint space_after)
            {
                GUIStyle centered = GUI.skin.GetStyle("Label");
                centered.alignment = TextAnchor.UpperCenter;
                GUILayout.Space(space_before);
                GUILayout.Label(text, centered);
                GUILayout.Space(space_after);
            }

            protected virtual void DisplayBasicProperties_Editor()
            {
                GUIStyle centered = GUI.skin.GetStyle("Label");
                centered.alignment = TextAnchor.UpperCenter;

                GUILayout.BeginVertical();
                //EditorGUILayout.PrefixLabel("Component name:");
                //EditorGUILayout.LabelField(((LBGameplayComponent)target).Name);
                GUILayout.Label("------ Basic component properties ------", centered);
                GUILayout.Space(8);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("component_name"), new GUIContent("Component name:"));
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("ActionName"), new GUIContent("Action name"));
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("ActionActivation"), new GUIContent("Activation type"));
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("ActionDeactivation"), new GUIContent("Deactivation type"));

                GUILayout.EndVertical();
            }

            protected virtual void DisplayInfo_Game() { }

            protected virtual void DisplayBasicInfo_Game()
            {
                LBGameplayComponent t = (LBGameplayComponent)target;

                if (!Application.isPlaying)
                    return;

                MakeCenteredLabel("------ Component Info ------", 8, 8);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Component state:");
                GUILayout.Label(t.State.ToString());
                GUILayout.EndHorizontal();
                DisplayInfo_Game();
                GUILayout.EndVertical();
            }

            protected virtual void DisplayControls_Game() { }

            protected virtual void DisplayBasicControls_Game()
            {
                LBGameplayComponent t = (LBGameplayComponent)target;

                if (!Application.isPlaying)
                    return;

                MakeCenteredLabel("------ Component controls ------", 8, 8);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                if (GUILayout.Button("Activate"))
                    t.Activate();
                if (GUILayout.Button("Deactivate"))
                    t.Deactivate();
                GUILayout.Space(4);
                GUILayout.EndHorizontal();
                DisplayControls_Game();
                GUILayout.EndVertical();
            }

            public override void OnInspectorGUI()
            {
                DisplayBasicProperties_Editor();

                serializedObject.ApplyModifiedProperties();

                DisplayBasicInfo_Game();
                DisplayBasicControls_Game();
            }
        }
    }
}
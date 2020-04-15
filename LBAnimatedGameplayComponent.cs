using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace LBGameplay
{
    [System.Serializable]
    public enum LBAnimTransitionType
    {
        Switch = 0,
        Corssfade = 1,
        Blend = 2
    }

    [System.Serializable]
    public struct LBAnimationInfo
    {
        public string AnimationName;
        public int AnimationLayer;
        public LBAnimTransitionType AnimationTransition;
        public float TransitionTime;

        public LBAnimationInfo(string _animationn_name, int _animation_layer)
        {
            AnimationName = _animationn_name;
            AnimationLayer = _animation_layer;
            AnimationTransition = LBAnimTransitionType.Switch;
            TransitionTime = 0;
        }

        public LBAnimationInfo(string _animationn_name, int _animation_layer, LBAnimTransitionType _transition, float _transition_time)
        {
            AnimationName = _animationn_name;
            AnimationLayer = _animation_layer;
            AnimationTransition = _transition;
            TransitionTime = _transition_time;
        }

        public LBAnimationInfo(SerializedProperty _anim)
        {
            SerializedProperty a;

            a = _anim;
            a.Next(true);
            AnimationName = a.stringValue;
            a.Next(false);
            AnimationLayer = a.intValue;
            a.Next(false);
            AnimationTransition = (LBAnimTransitionType) a.enumValueIndex;
            a.Next(false);
            TransitionTime = a.floatValue;
        }

        public void SetSerializedProperty(SerializedProperty _anim)
        {
            _anim.Next(true);
            _anim.stringValue = AnimationName;
            _anim.Next(false);
            _anim.intValue = AnimationLayer;
            _anim.Next(false);
            _anim.enumValueIndex = (int)AnimationTransition;
            _anim.Next(false);
            _anim.floatValue = TransitionTime;
        }
    }

    [AddComponentMenu("LBGameplay/Animated Gameplay Component (Dummy)")]
    public class LBAnimatedGameplayComponent : LBStatedLinkedGameplayComponent
    {
        [SerializeField]
        protected Animator animator = null;
        [SerializeField]
        protected LBAnimationInfo[] animations = new LBAnimationInfo[0];
        protected float startanimtime;

        protected override bool Init()
        {
            if (!base.Init())
                return false;

            animator = GetComponent<Animator>();

            if (animator == null)
                LogMessage("GameObject" + gameObject.name + " does not have an Animator component!", LBMessageTypes.Warning);

            return true;
        }

        protected override void Perform()
        {

        }

        protected void PlayAnimation(string anim, int layer = 0, float offset = 0, LBAnimTransitionType transition = LBAnimTransitionType.Switch, float blend = 0.1f)
        {
            startanimtime = animator.GetCurrentAnimatorStateInfo(animations[current_state].AnimationLayer).normalizedTime;

            if (anim == string.Empty)
                return;

            //animator.Play(anim, layer, offset);

            //curanimname = anim;
            //curanimlayer = layer;

            if (transition == LBAnimTransitionType.Switch)
            {
                animator.Play(anim, layer, offset);
            }
            else if (transition == LBAnimTransitionType.Corssfade)
            {
                animator.CrossFade(anim, blend, layer);
            }
        }

        protected void PlayAnimation(LBAnimationInfo anim)
        {
            PlayAnimation(anim.AnimationName, anim.AnimationLayer, 0, anim.AnimationTransition, anim.TransitionTime);
        }

        protected bool IsAnimationFinished(string anim)
        {
            if (animations[current_state].AnimationName != string.Empty && animations[current_state].AnimationName == anim)
            {
                if (AnimationExtraLoops >= 1)
                    return true;
            }

            return false;
        }

        protected override bool SwitchState(int new_state)
        {
            if (base.SwitchState(new_state))
            {
                PlayAnimation(animations[new_state]);
                return true;
            }

            return false;
        }

        public float AnimationTime
        {
            get
            {
                if (animator != null) // We don't know, which animation we're playing currently
                {
                    //animator.GetCurrentAnimatorStateInfo (AnimationLayer).ToString ();
                    if (animator.GetCurrentAnimatorStateInfo(animations[current_state].AnimationLayer).normalizedTime > startanimtime)
                        return animator.GetCurrentAnimatorStateInfo(animations[current_state].AnimationLayer).normalizedTime;
                    else
                        return 0;
                }
                else
                    return startanimtime + 2;
            }
        }

        public int AnimationExtraLoops
        {
            get
            {
                return (int)(animator.GetCurrentAnimatorStateInfo(animations[current_state].AnimationLayer).normalizedTime - startanimtime);
            }
        }
    }

    namespace Editors
    {
        [CustomEditor(typeof(LBAnimatedGameplayComponent))]
        public class LBAnimatedGameplayComponent_ED : LBStatedLinkedGameplayComponent_ED
        {
            int selected_anim;
            int selected_layer;

            //protected void DisplayAnimations_Editor()
            //{
            //    Animator animator;
            //    AnimatorController controller;
            //    AnimatorControllerLayer[] layers;
            //    List<LBAnimationInfo> states = new List<LBAnimationInfo>();
            //    List<string> state_names = new List<string>();
            //    int i, j;

            //    MakeCenteredLabel("------ Animations ------", 16, 16);

            //    //GUILayout.Label("------ Animations ------", centered);

            //    animator = ((Component)target).gameObject.GetComponent<Animator>();

            //    if (animator == null)
            //    {
            //        MakeCenteredLabel("<No animation states to show>");
            //        return;
            //    }

            //    controller = (AnimatorController)animator.runtimeAnimatorController;

            //    if (controller == null)
            //    {
            //        MakeCenteredLabel("<No animation states to show>");
            //        return;
            //    }

            //    layers = controller.layers;

            //    for (i = 0; i < layers.Length; i++)
            //    {
            //        ChildAnimatorState[] child_states = layers[i].stateMachine.states;

            //        for (j = 0; j < child_states.Length; j++)
            //        {
            //            states.Add(new LBAnimationInfo(child_states[j].state.name, i));
            //            state_names.Add(child_states[j].state.name);
            //            //state_names.Add(child_states[j].state.name);
            //            //GUILayout.Label(child_states[j].state.name);
            //            //GUILayout.Space(4);
            //        }
            //    }

            //    animation = EditorGUILayout.Popup("Animation to play", 0, state_names.ToArray());

            //    if (GUILayout.Button("Play!"))
            //    {
            //        ((LBAnimatedGameplayComponent)target).PlayAnimation(states[animation]);
            //    }
            //}

            protected int FindOrCreateAnimationForState(int state)
            {
                SerializedProperty states, animations;

                states = serializedObject.FindProperty("states");
                animations = serializedObject.FindProperty("animations");

                if (states.arraySize != animations.arraySize)
                {
                    animations.arraySize = states.arraySize;
                }

                return state;
            }

            protected virtual void DisplayAnimationInfo(int id)
            {
                Animator animator;
                AnimatorController controller;
                AnimatorControllerLayer[] layers;
                ChildAnimatorState[] child_states;
                SerializedProperty anim_sp;
                LBAnimationInfo anim;
                List<string> anim_names = new List<string>();
                List<string> layer_names = new List<string>();
                int i, j, anim_id;

                animator = ((Component)target).gameObject.GetComponent<Animator>();

                if (animator == null)
                    return;

                controller = (AnimatorController)animator.runtimeAnimatorController;

                if (controller == null)
                    return;

                anim_id = FindOrCreateAnimationForState(id);
                anim_sp = serializedObject.FindProperty("animations");
                anim = new LBAnimationInfo(anim_sp.GetArrayElementAtIndex(anim_id));

                layers = controller.layers;

                for (i = 0; i < layers.Length; i++)
                    layer_names.Add("Layer " + i.ToString());

                anim.AnimationLayer = EditorGUILayout.Popup("Layer", anim.AnimationLayer, layer_names.ToArray());
                child_states = layers[anim.AnimationLayer].stateMachine.states;

                for (i = 0; i < child_states.Length; i++)
                {
                    anim_names.Add(child_states[i].state.name);
                }

                // хер поймёшь здесь будет anim_names в том же порядке, что в аниматоре всегда или будет переставляться по кд...
                if (anim_names.IndexOf(anim.AnimationName) > 0 && anim_names.IndexOf(anim.AnimationName) < anim_names.Count)
                    anim.AnimationName = anim_names[EditorGUILayout.Popup("Animation", anim_names.IndexOf(anim.AnimationName), anim_names.ToArray())];
                else
                    anim.AnimationName = anim_names[EditorGUILayout.Popup("Animation", 0, anim_names.ToArray())];

                anim.AnimationTransition = (LBAnimTransitionType)(EditorGUILayout.Popup("Animation blend mode", (int)(anim.AnimationTransition), System.Enum.GetNames(typeof(LBAnimTransitionType))));

                if (anim.AnimationTransition == LBAnimTransitionType.Corssfade || anim.AnimationTransition == LBAnimTransitionType.Blend)
                {
                    anim.TransitionTime = EditorGUILayout.FloatField("Blend time", anim.TransitionTime);
                }

                // finally save modified values
                anim.SetSerializedProperty(anim_sp.GetArrayElementAtIndex(anim_id));
            }

            protected virtual void DisplayAnimatedStateProperties_Editor()
            {
                SerializedProperty animations;
                LBInternalState[] states;
                int i;

                if (Application.isPlaying)
                    return;

                animations = serializedObject.FindProperty("animations");

                states = GetAllStates();

                GUILayout.Space(16);
                
                for (i=0; i<states.Length; i++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(states[i].Name);
                    if (GUILayout.Button("Delete"))
                    {
                        //l.DeleteCommand(); // количество элементов уменьшается!!!
                    }
                    GUILayout.EndHorizontal();
                    //EditorGUILayout.PropertyField(animations.GetArrayElementAtIndex(FindOrCreateAnimationForState(i)), new GUIContent("Animation"));
                    DisplayAnimationInfo(i);
                    GUILayout.EndVertical();
                    //GUILayout.Space(4);
                }
            }

            protected virtual void DisplayAnimatedStateProperties_Game()
            {
                LBAnimatedGameplayComponent agc;
                LBInternalState[] states;
                int i;

                if (!Application.isPlaying)
                    return;

                agc = (LBAnimatedGameplayComponent)target;
                states = agc.AllStates;

                GUILayout.Space(16);

                for (i = 0; i < states.Length; i++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(states[i].Name);
                    if (states[i].Name != agc.State.Name)
                    {
                        if (GUILayout.Button("Play"))
                        {
                            agc.GoToState(i);
                        }
                    }
                    else
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        GUILayout.Button("Play");
                        EditorGUI.EndDisabledGroup();
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (Application.isPlaying)
                    DisplayAnimatedStateProperties_Game();
                else
                    DisplayAnimatedStateProperties_Editor();

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
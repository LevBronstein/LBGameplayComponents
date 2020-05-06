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

    [System.Serializable]
    public struct LBSlider
    {
        [SerializeField]
        private string name;
        [SerializeField]
        private float pos;
        [SerializeField]
        private float interp_rate;

        public LBSlider (string _name, float _pos, float _interp_rate = 0.1f)
        {
            name = _name;
            pos = _pos;
            interp_rate = _interp_rate;
        }

        public LBSlider (SerializedProperty _slider)
        {
            SerializedProperty st;

            st = _slider;
            st.Next(true);
            name = st.stringValue;
            st.Next(false);
            pos = st.floatValue;
            st.Next(false);
            interp_rate = st.floatValue;
        }

        public void SetSerializedProperty(SerializedProperty _slider)
        {
            _slider.Next(true);
            _slider.stringValue = name;
            _slider.Next(false);
            _slider.floatValue = pos;
            _slider.Next(false);
            _slider.floatValue = interp_rate;
        }

        public float SetPosition(float new_pos, float delta_time)
        {
            pos = LBMath.LerpFloat(pos, new_pos, interp_rate, delta_time);
            return pos;
        }

        public float SetPosition(float new_pos, float delta_time, float rate = 0.1f)
        {
            pos = LBMath.LerpFloat(pos, new_pos, rate, delta_time);
            return pos;
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public float Position
        {
            get
            {
                return pos;
            }
            set
            {
                pos = value;
            }
        }

        public float InterpRate
        {
            get
            {
                return interp_rate;
            }
            set
            {
                interp_rate = value;
            }
        }
    }

    [AddComponentMenu("LBGameplay/Animated Gameplay Component (Dummy)")]
    public class LBAnimatedGameplayComponent : LBStatedLinkedGameplayComponent
    {
        [SerializeField]
        protected Animator animator = null;
        [SerializeField]
        protected LBAnimationInfo[] animations = new LBAnimationInfo[0];
        [SerializeField]
        protected LBSlider[] sliders = new LBSlider[0];
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

        //protected override void Perform()
        //{

        //}

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

        protected void UpdateSlider(string name, float pos)
        {
            int i;

            for (i = 0; i < sliders.Length; i++)
            {
                if (sliders[i].Name == name)
                    animator.SetFloat(name, sliders[i].SetPosition(pos, Time.deltaTime));
            }
        }

        protected void UpdateSlider(int id, float pos)
        {
            if (id >=0 && id < sliders.Length)
                animator.SetFloat(sliders[id].Name, sliders[id].SetPosition(pos, Time.deltaTime));
        }

        public void SetSliderPosition(int id, float pos)
        {
            UpdateSlider(id, pos);
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

        public LBSlider[] AllSliders
        {
            get
            {
                return sliders;
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

            protected LBSlider[] GetAllSliders()
            {
                SerializedProperty sliders;
                List<LBSlider> all_sliders;
                int i;

                all_sliders = new List<LBSlider>();
                sliders = serializedObject.FindProperty("sliders");

                for (i = 0; i < sliders.arraySize; i++)
                {
                    all_sliders.Add(new LBSlider(sliders.GetArrayElementAtIndex(i)));
                }

                return all_sliders.ToArray();
            }

            protected void AddNewSlider()
            {
                SerializedProperty sliders;
                LBSlider slider;

                slider = new LBSlider("noparam", 0);
                sliders = serializedObject.FindProperty("sliders");

                sliders.arraySize++;
                slider.SetSerializedProperty(sliders.GetArrayElementAtIndex(sliders.arraySize - 1));
            }

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

            protected virtual void DisplayAnimationInfo(int id, bool readonly_Layer = false, bool readonly_anim = false, bool readonly_blend = false)
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

                EditorGUI.BeginDisabledGroup(readonly_Layer);
                anim.AnimationLayer = EditorGUILayout.Popup("Layer", anim.AnimationLayer, layer_names.ToArray());
                EditorGUI.EndDisabledGroup();
                child_states = layers[anim.AnimationLayer].stateMachine.states;

                for (i = 0; i < child_states.Length; i++)
                {
                    anim_names.Add(child_states[i].state.name);
                }

                EditorGUI.BeginDisabledGroup(readonly_anim);
                // хер поймёшь здесь будет anim_names в том же порядке, что в аниматоре всегда или будет переставляться по кд...
                if (anim_names.IndexOf(anim.AnimationName) > 0 && anim_names.IndexOf(anim.AnimationName) < anim_names.Count)
                    anim.AnimationName = anim_names[EditorGUILayout.Popup("Animation", anim_names.IndexOf(anim.AnimationName), anim_names.ToArray())];
                else
                    anim.AnimationName = anim_names[EditorGUILayout.Popup("Animation", 0, anim_names.ToArray())];
                EditorGUI.EndDisabledGroup();

                anim.AnimationTransition = (LBAnimTransitionType)(EditorGUILayout.Popup("Animation blend mode", (int)(anim.AnimationTransition), System.Enum.GetNames(typeof(LBAnimTransitionType))));

                if (anim.AnimationTransition == LBAnimTransitionType.Corssfade || anim.AnimationTransition == LBAnimTransitionType.Blend)
                {
                    EditorGUI.BeginDisabledGroup(readonly_blend);
                    anim.TransitionTime = EditorGUILayout.FloatField("Blend time", anim.TransitionTime);
                    EditorGUI.EndDisabledGroup();
                }

                // finally save modified values
                anim.SetSerializedProperty(anim_sp.GetArrayElementAtIndex(anim_id));
            }

            protected virtual void DisplayAnimatedStateProperties_Editor(bool no_delete = false, bool readonly_Layer = false, bool readonly_anim = false, bool readonly_blend = false)
            {
                //SerializedProperty animations;
                LBInternalState[] states;
                int i;

                if (Application.isPlaying)
                    return;

                //animations = serializedObject.FindProperty("animations");

                states = GetAllStates();

                //GUILayout.Space(16);
                MakeCenteredLabel("------ Animation properties ------", 16, 16);

                for (i=0; i<states.Length; i++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(states[i].Name);
                    EditorGUI.BeginDisabledGroup(no_delete);
                    if (GUILayout.Button("Delete"))
                    {
                        //l.DeleteCommand(); // количество элементов уменьшается!!!
                    }
                    EditorGUI.EndDisabledGroup();
                    GUILayout.EndHorizontal();
                    DisplayAnimationInfo(i, readonly_Layer, readonly_anim, readonly_blend);
                    GUILayout.EndVertical();
                    //GUILayout.Space(4);
                }
            }

            protected virtual void DisplaySliderInfo(int id, bool readonly_name = false, bool readonly_interp = false, bool readonly_pos = false)
            {
                Animator animator;
                AnimatorController controller;
                SerializedProperty slider_sp;
                LBSlider slider;
                List<string> param_names = new List<string>();
                int i;

                animator = ((Component)target).gameObject.GetComponent<Animator>();

                if (animator == null)
                    return;

                controller = (AnimatorController)animator.runtimeAnimatorController;

                if (controller == null || controller.parameters.Length == 0)
                    return;

                slider_sp = serializedObject.FindProperty("sliders");
                slider = new LBSlider(slider_sp.GetArrayElementAtIndex(id));

                for (i = 0; i < controller.parameters.Length; i++)
                {
                    if (controller.parameters[i].type == AnimatorControllerParameterType.Float)
                        param_names.Add(controller.parameters[i].name);
                }

                // гемор: может быть использован парамтер с одним именем дважды!

                EditorGUI.BeginDisabledGroup(readonly_name);
                if (param_names.IndexOf(slider.Name) > 0 && param_names.IndexOf(slider.Name) < param_names.Count)
                    slider.Name = param_names[EditorGUILayout.Popup("Parameter", param_names.IndexOf(slider.Name), param_names.ToArray())];
                else
                    slider.Name = param_names[EditorGUILayout.Popup("Animation", 0, param_names.ToArray())];
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(readonly_interp);
                slider.InterpRate = EditorGUILayout.FloatField("Interpolation rate", slider.InterpRate);
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(readonly_pos);
                slider.Position = EditorGUILayout.Slider("Position", slider.Position, 0, 1);
                EditorGUI.EndDisabledGroup();

                slider.SetSerializedProperty(slider_sp.GetArrayElementAtIndex(id));
            }

            protected virtual void DisplaySliderProperties_Editor(bool no_delete = false, bool readonly_name = false, bool readonly_interp = false, bool readonly_pos = false)
            {
                LBSlider[] sliders;
                int i;

                if (Application.isPlaying)
                    return;

                sliders = GetAllSliders();

                MakeCenteredLabel("------ Slider properties ------", 16, 16);

                for (i = 0; i < sliders.Length; i++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(sliders[i].Name);
                    EditorGUI.BeginDisabledGroup(no_delete);
                    if (GUILayout.Button("Delete"))
                    {
                        //l.DeleteCommand(); // количество элементов уменьшается!!!
                    }
                    EditorGUI.EndDisabledGroup();
                    GUILayout.EndHorizontal();
                    DisplaySliderInfo(i, readonly_name, readonly_interp, readonly_pos);
                    GUILayout.EndVertical();
                }

                GUILayout.Space(8);
                if (GUILayout.Button("Add new slider"))
                {
                    AddNewSlider();
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
                states = agc.AllInternalStates;

                GUILayout.Space(16);

                for (i = 0; i < states.Length; i++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(states[i].Name);
                    if (states[i].Name != agc.InternalState.Name)
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

            protected virtual void DisplaySliderProperties_Game()
            {
                LBAnimatedGameplayComponent agc;
                LBSlider[] sliders;
                int i;

                if (!Application.isPlaying)
                    return;

                agc = (LBAnimatedGameplayComponent)target;
                sliders = agc.AllSliders;

                MakeCenteredLabel("------ Slider info ------", 16, 16);

                for (i = 0; i < sliders.Length; i++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    agc.SetSliderPosition(i, EditorGUILayout.Slider(sliders[i].Name, sliders[i].Position, 0, 1));
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

                if (Application.isPlaying)
                    DisplaySliderProperties_Game();
                else
                    DisplaySliderProperties_Editor();

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
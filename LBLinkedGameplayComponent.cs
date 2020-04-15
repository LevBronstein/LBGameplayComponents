using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LBGameplay
{
    [System.Serializable]
    public struct LBGameplayComponentLink
    {
        public LBGameplayComponent Target;
        public int Order;
        public int ParamID;

        public LBGameplayComponentLink(LBGameplayComponent _target, int _order, int _param_id)
        {
            Target = _target;
            Order = _order;
            ParamID = _param_id;
        }

        public LBGameplayComponentLink (SerializedProperty links, int item)
        {
            SerializedProperty l;

            l = links.GetArrayElementAtIndex(item);
            l.Next(true);
            Target = (LBGameplayComponent)l.objectReferenceValue;
            l.Next(false);
            Order = l.intValue;
            l.Next(false);
            ParamID = l.intValue;
        }

        public void SetSerializedProperty(SerializedProperty p)
        {
            p.Next(true);
            p.objectReferenceValue = Target;
            p.Next(false);
            p.intValue = Order;
            p.Next(false);
            p.intValue = ParamID;
        }

        public bool IsEmpty
        {
            get
            {
                return (Target == null && Order == -1 && ParamID == -1);
            }
        }

        public static LBGameplayComponentLink Empty
        {
            get
            { 
                LBGameplayComponentLink l;

                l.Target = null;
                l.Order = -1;
                l.ParamID = -1;

                return l;
            }
        }

        public static bool operator == (LBGameplayComponentLink link1, LBGameplayComponentLink link2)
        {
            return (link1.Target == link2.Target && link1.Order == link2.Order && link1.ParamID == link2.ParamID);
        }

        public static bool operator !=(LBGameplayComponentLink link1, LBGameplayComponentLink link2)
        {
            return !(link1.Target == link2.Target && link1.Order == link2.Order && link1.ParamID == link2.ParamID);
        }
    }

    [AddComponentMenu("LBGameplay/Linked Gameplay Component (Dummy)")]
    public class LBLinkedGameplayComponent : LBGameplayComponent
    {
        [SerializeField]
        LBGameplayComponentLink[] inputs = new LBGameplayComponentLink[0];
        [SerializeField]
        LBGameplayComponentLink[] outputs = new LBGameplayComponentLink[0];
        [SerializeField]
        bool allow_linkless_activation = true;
        [SerializeField]
        bool allow_linkless_deactivation = true;
        [SerializeField]
        bool allow_multi_activation = false;
        [SerializeField]
        bool allow_multi_deactivation = false;

        /// <summary>
        /// Checks if a specified LBLinkedGameplayComponent is present in @inputs array of this component
        /// </summary>
        /// <param name="c">A component, whuch should be searched for</param>
        /// <returns></returns>
        public bool bHasInputConnection(LBLinkedGameplayComponent c)
        {
            int i;

            for (i = 0; i < inputs.Length; i++)
            {
                if (inputs[i].Target == c)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// hecks if a specified LBLinkedGameplayComponent is present in @outputs array of this component
        /// </summary>
        /// <param name="c">A component, whuch should be searched for</param>
        /// <returns></returns>
        public bool bHasOutputConnection(LBLinkedGameplayComponent c)
        {
            int i;

            for (i = 0; i < outputs.Length; i++)
            {
                if (outputs[i].Target == c)
                    return true;
            }

            return false;
        }

        public bool bHasInputFrom(LBLinkedGameplayComponent c)
        {
            int i;

            for (i = 0; i < inputs.Length; i++)
            {
                if (inputs[i].Target == c)
                    return true;
            }

            return false;
        }

        public bool bHasOutputTo(LBLinkedGameplayComponent c)
        {
            int i;

            for (i = 0; i < outputs.Length; i++)
            {
                if (outputs[i].Target == c)
                    return true;
            }

            return false;
        }

        public bool bHasMatchingInput(LBLinkedGameplayComponent other, LBGameplayComponentLink output)
        {
            LBGameplayComponentLink link, other_output;

            link = FindInputFrom(other); // нашли обратную свзяь на other

            if (link.IsEmpty)
                return false;

            other_output = ((LBLinkedGameplayComponent)link.Target).FindOutputTo(this); // проверить, действительно ли other имеет выход на this

            if (other_output.IsEmpty)
                return false;

            if (other_output == output)
                return true;
            else
                return false;
        }

        public bool bHasMatchingOutput(LBLinkedGameplayComponent other, LBGameplayComponentLink input)
        {
            LBGameplayComponentLink link, other_input;

            link = FindOutputTo(other); // нашли обратную свзяь на other

            if (link.IsEmpty)
                return false;

            other_input = ((LBLinkedGameplayComponent)link.Target).FindInputFrom(this); // проверить, действительно ли other имеет вход от this

            if (other_input.IsEmpty)
                return false;

            if (other_input == input)
                return true;
            else
                return false;
        }

        public LBGameplayComponentLink FindInputFrom(LBLinkedGameplayComponent c)
        {
            int i;

            for (i = 0; i < inputs.Length; i++)
            {
                if (inputs[i].Target == c)
                    return inputs[i];
            }

            return LBGameplayComponentLink.Empty;
        }

        public LBGameplayComponentLink FindOutputTo(LBLinkedGameplayComponent c)
        {
            int i;

            for (i = 0; i < outputs.Length; i++)
            {
                if (outputs[i].Target == c)
                    return outputs[i];
            }

            return LBGameplayComponentLink.Empty;
        }

        public virtual bool bCanTransferOut(LBGameplayComponentLink link)
        {
            LBGameplayComponent gc = null;
            LBLinkedGameplayComponent lgc = null;

            if (link.Target is LBGameplayComponent)
                gc = (LBGameplayComponent)(link.Target);

            if (link.Target is LBLinkedGameplayComponent)
                lgc = (LBLinkedGameplayComponent)(link.Target);

            if (link.Target == null)
            {
                return false;
            }
            else if (gc != null && lgc == null)
            {
                if (bCanTurnOff() && gc.bCanActivate())
                    return true;
            }
            else if (lgc != null)
            {
                if (bCanTurnOff() && lgc.bHasMatchingInput(this, link) && lgc.bCanTurnOn())
                    return true;
            }

            return false;
        }

        public virtual bool bCanTransferOut()
        {
            LBGameplayComponentLink[] links;

            links = FindActiveOutputs();

            if (links != null && links.Length > 0)
                return true;
            else
                return false;
        }

        public virtual bool bCanTransferIn(LBGameplayComponentLink link)
        {
            LBGameplayComponent gc = null;
            LBLinkedGameplayComponent lgc = null;

            if (link.Target is LBGameplayComponent)
                gc = (LBGameplayComponent)(link.Target); 

            if (link.Target is LBLinkedGameplayComponent)
                lgc = (LBLinkedGameplayComponent)(link.Target); 

            if (link.Target == null)
            {
                return false;
            }
            else if (gc != null && lgc == null)
            {
                if (bCanTurnOn() && gc.bCanDeactivate())
                    return true;
            }
            else if (lgc != null)
            {
                if (bCanTurnOn() && lgc.bHasMatchingOutput(this, link) && lgc.bCanTurnOff())
                    return true;
            }

            return false;
        }

        public virtual bool bCanTransferIn()
        {
            LBGameplayComponentLink[] links;

            links = FindActiveInputs();

            if (links != null && links.Length > 0)
                return true;
            else
                return false;
        }

        protected LBGameplayComponentLink[] FindActiveOutputs()
        {
            LBGameplayComponentLink[] links = new LBGameplayComponentLink[outputs.Length];
            LBGameplayComponentLink[] result = new LBGameplayComponentLink[0];
            LBGameplayComponentLink lnk;
            LBGameplayComponent gc = null;
            LBLinkedGameplayComponent lgc = null;
            int i;

            Array.Copy(outputs, 0, links, 0, outputs.Length);
            Array.Sort(links, (a, b) => { return a.Order.CompareTo(b.Order); });

            for (i = 0; i < links.Length; i++)
            {
                if (links[i].Target is LBGameplayComponent)
                    gc = (LBGameplayComponent)(links[i].Target); // если other -- LBGameplayComponent

                if (links[i].Target is LBLinkedGameplayComponent)
                    lgc = (LBLinkedGameplayComponent)(links[i].Target); // если other -- LBLinkedGameplayComponent

                if (gc != null && lgc == null)
                {
                    if (bCanTransferOut(links[i]))
                    {
                        Array.Resize(ref result, result.Length + 1); // можно здесь брать массив размера outputs, после отбрасывать последние пустые элементы
                        result[result.Length - 1] = links[i];
                    }
                }
                else if (lgc != null)
                {
                    lnk = lgc.FindInputFrom(this); // если есть вход от this

                    if (!lnk.IsEmpty)
                    {
                        if (bCanTransferOut(links[i]) && lgc.bCanTransferIn((lnk)))
                        {
                            Array.Resize(ref result, result.Length + 1); // можно здесь брать массив размера outputs, после отбрасывать последние пустые элементы
                            result[result.Length - 1] = links[i];
                        }
                    }
                } 
            }

            return result;
        }

        protected LBGameplayComponentLink[] FindActiveInputs()
        {
            LBGameplayComponentLink[] links = new LBGameplayComponentLink[inputs.Length];
            LBGameplayComponentLink[] result = new LBGameplayComponentLink[0];
            LBGameplayComponentLink lnk;
            LBGameplayComponent gc = null;
            LBLinkedGameplayComponent lgc = null;
            int i;

            Array.Copy(inputs, 0, links, 0, inputs.Length);
            Array.Sort(links, (a, b) => { return a.Order.CompareTo(b.Order); });

            for (i = 0; i < links.Length; i++)
            {
                if (links[i].Target is LBGameplayComponent)
                    gc = (LBGameplayComponent)(links[i].Target); // если other -- LBGameplayComponent

                if (links[i].Target is LBLinkedGameplayComponent)
                    lgc = (LBLinkedGameplayComponent)(links[i].Target); // если other -- LBLinkedGameplayComponent

                if (gc != null && lgc == null)
                {
                    if (bCanTransferIn(links[i]))
                    {
                        Array.Resize(ref result, result.Length + 1); // можно здесь брать массив размера outputs, после отбрасывать последние пустые элементы
                        result[result.Length - 1] = links[i];
                    }
                }
                else if (lgc != null)
                {
                    lnk = lgc.FindOutputTo(this); // если есть выход на this

                    if (!lnk.IsEmpty)
                    {
                        if (bCanTransferIn(links[i]) && lgc.bCanTransferOut((lnk)))
                        {
                            Array.Resize(ref result, result.Length + 1); // можно здесь брать массив размера outputs, после отбрасывать последние пустые элементы
                            result[result.Length - 1] = links[i];
                        }
                    }
                }
            }

            return result;
        }

        protected virtual bool TransferOut(LBGameplayComponentLink link)
        {
            LBGameplayComponent gc = null;
            LBLinkedGameplayComponent lgc = null;

            if (link.Target is LBGameplayComponent)
                gc = (LBGameplayComponent)(link.Target);

            if (link.Target is LBLinkedGameplayComponent)
                lgc = (LBLinkedGameplayComponent)(link.Target);

            if (bCanTransferOut(link))
            {
                if (gc != null && lgc == null)
                {
                    TurnOff();
                    OnTransferOut(link);
                    gc.Activate();
                    return true;
                }
                else if (lgc != null)
                {
                    TurnOff();
                    OnTransferOut(link);
                    lgc.TurnOn();
                    lgc.OnTransferIn(lgc.FindInputFrom(this));
                    return true;
                }
            }

            return false;
        }
   
        protected bool TransferOut()
        {
            LBGameplayComponentLink[] links;

            links = FindActiveOutputs();

            if (links != null && links.Length > 0)
                return TransferOut(links[0]);

            return false;
        }

        //private
        protected virtual bool TransferIn(LBGameplayComponentLink link)
        {
            LBGameplayComponent gc = null;
            LBLinkedGameplayComponent lgc = null;

            if (link.Target is LBGameplayComponent)
                gc = (LBGameplayComponent)(link.Target);

            if (link.Target is LBLinkedGameplayComponent)
                lgc = (LBLinkedGameplayComponent)(link.Target);

            if (bCanTransferIn(link))
            {
                if (gc != null && lgc == null)
                {
                    TurnOn();
                    OnTransferIn(link);
                    gc.Deactivate();
                    return true;
                }
                else if (lgc != null)
                {
                    TurnOn();
                    OnTransferIn(link);
                    lgc.TurnOff();
                    lgc.OnTransferOut(lgc.FindOutputTo(this));
                    return true;
                }
            }

            return false;
        }

        protected bool TransferIn()
        {
            LBGameplayComponentLink[] links;

            links = FindActiveInputs();

            if (links != null && links.Length > 0)
                return TransferIn(links[0]);

            return false;
        }

        public override bool bCanActivate()
        {
            return (base.bCanActivate() && bCanTransferIn()) || (base.bCanActivate() && AllowsLinklessActivation);
        }

        public override bool bCanDeactivate() // может быть переделано, чтобы задействовалось только bCanActivate()
        {
            return (base.bCanDeactivate() && bCanTransferOut()) || (base.bCanDeactivate() && AllowsLinklessDeactivation);
        }

        public override bool bCanSelfActivate()
        {
            return (base.bCanSelfActivate() && bCanTransferIn()) || (base.bCanSelfActivate() && AllowsLinklessActivation);
        }

        public override bool bCanSelfDeactivate()
        {
            return (base.bCanSelfDeactivate() && bCanTransferOut()) || (base.bCanSelfDeactivate() && AllowsLinklessDeactivation);
        }

        protected override bool ActivateInternal()
        {
            if (bCanActivate())
            {
                if (bCanTransferIn())
                {
                    return TransferIn();
                }
                else
                {
                    return TurnOn();
                }
            }
            
            return false;
        }

        protected override bool DeactivateInternal()
        {
            if (bCanDeactivate())
            {
                if (bCanTransferOut())
                {
                    return TransferOut();
                }
                else
                {
                    return TurnOff();
                }
            }

            return false;
        }

        protected virtual void OnTransferIn(LBGameplayComponentLink link) { }

        protected virtual void OnTransferOut(LBGameplayComponentLink link) { }

        // Проблема заключается в том, что это компонент можно активировать, если один из связанных уже активирован
        public bool AllowsLinklessActivation
        {
            get
            {
                return (inputs == null || inputs.Length == 0) && allow_linkless_activation;
            }
        }

        public bool AllowsLinklessDeactivation
        {
            get
            {
                return (outputs == null || outputs.Length == 0) && allow_linkless_deactivation;
            }
        }
    }

    namespace Editors
    {
        [CustomEditor(typeof(LBLinkedGameplayComponent))]
        public class LBLinkedGameplayComponent_ED : LBGameplayComponent_ED
        {
            protected virtual LBGameplayComponentLink FindOrMakeInputLink(SerializedProperty links, int id)
            {
                LBGameplayComponentLink link_to, link_from;
                SerializedProperty l, links_on_other;
                SerializedObject other;
                LBGameplayComponent g;
                int i;

                l = links.GetArrayElementAtIndex(id);
                link_to = new LBGameplayComponentLink(links, id);
                //l.Next(true);
                //gc = (LBGameplayComponent)l.objectReferenceValue;

                if (link_to.Target == null)
                    return new LBGameplayComponentLink();

                other = new SerializedObject(link_to.Target);
                links_on_other=other.FindProperty("inputs");

                for (i=0;i<links_on_other.arraySize;i++)
                {
                    l = links_on_other.GetArrayElementAtIndex(i);
                    l.Next(true);
                    g = (LBGameplayComponent)l.objectReferenceValue;

                    if (g != null && g == serializedObject.targetObject)
                        return new LBGameplayComponentLink(links_on_other, i);
                }

                links_on_other.arraySize++;
                l = links_on_other.GetArrayElementAtIndex(links_on_other.arraySize - 1);

                link_from = new LBGameplayComponentLink((LBLinkedGameplayComponent)serializedObject.targetObject, 0, 0);
                link_from.SetSerializedProperty(l);

                other.ApplyModifiedProperties();

                return link_from;
            }

            protected virtual void DisplayLinkBase()
            {

            }

            protected virtual void DisplayInputLinkContent(int id) { }

            protected virtual void DisplayInputLink(int id)
            {
                SerializedProperty links, l;
                LBGameplayComponent gc;

                links = serializedObject.FindProperty("inputs");
                l = links.GetArrayElementAtIndex(id);

                l.Next(true);
                gc = (LBGameplayComponent)l.objectReferenceValue;

                GUILayout.BeginVertical(EditorStyles.helpBox);

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(gc == null ? "Empty link" : "Link to [" + gc.Name + "]");

                if (GUILayout.Button("Delete"))
                {
                    l.DeleteCommand(); // количество элементов уменьшается!!!
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Target GameObject");
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(gc == null ? null : gc.gameObject, typeof(GameObject), true);
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Target Component");
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(gc == null ? "null" : gc.Name);
                //EditorGUILayout.ObjectField(gc == null ? null : gc.gameObject, typeof(Component), true);
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(links.GetArrayElementAtIndex(id), new GUIContent("Input link"));

                DisplayInputLinkContent(id);

                GUILayout.EndVertical();

                //ClearNullLinks ?
            }

            protected virtual void DisplayOutputLinkContent(int id) { }

            protected virtual void DisplayOutputLink(int id)
            {
                SerializedProperty links, l;
                LBGameplayComponent gc;

                links = serializedObject.FindProperty("outputs");
                l = links.GetArrayElementAtIndex(id);

                l.Next(true);
                gc = (LBGameplayComponent)l.objectReferenceValue;

                GUILayout.BeginVertical(EditorStyles.helpBox);

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(gc == null ? "Empty link" : "Link to [" + gc.Name + "]");

                if (GUILayout.Button("Delete"))
                {
                    l.DeleteCommand(); // количество элементов уменьшается!!!
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Target GameObject");
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(gc == null ? null : gc.gameObject, typeof(GameObject), true);
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Target Component");
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(gc == null ? "null" : gc.Name);
                //EditorGUILayout.ObjectField(gc == null ? null : gc.gameObject, typeof(Component), true);
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();

                //DisplayLinkProperties(links, id);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(links.GetArrayElementAtIndex(id), new GUIContent("Output link"));
                GUILayout.EndVertical();

                FindOrMakeInputLink(links, id);

                DisplayOutputLinkContent(id);

                GUILayout.EndVertical();
                GUILayout.Space(12);
            }

            protected virtual void DisplayLinks()
            {
                SerializedProperty links;
                int i;

                links = serializedObject.FindProperty("inputs");

                for (i = 0; i < links.arraySize; i++)
                {
                    DisplayInputLink(i);
                }

                links = serializedObject.FindProperty("outputs");

                for (i = 0; i < links.arraySize; i++)
                {
                    DisplayOutputLink(i);
                }
            }

            protected virtual void ClearAllLinks()
            {
                SerializedProperty input_links;
                SerializedProperty output_links;

                input_links = serializedObject.FindProperty("inputs");
                output_links = serializedObject.FindProperty("outputs");

                input_links.ClearArray();
                output_links.ClearArray();
            }

            protected virtual void DisplayLinkProperties_Editor()
            {
                SerializedProperty input_links;
                SerializedProperty output_links;

                GUIStyle centered = GUI.skin.GetStyle("Label");
                centered.alignment = TextAnchor.UpperCenter;

                input_links = serializedObject.FindProperty("inputs");
                output_links = serializedObject.FindProperty("outputs");

                MakeCenteredLabel("------ Link properties ------", 16, 16);

                DisplayLinks();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("New input link"))
                {
                    input_links.arraySize++;
                }
                if (GUILayout.Button("New output link"))
                {
                    output_links.arraySize++;
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(16);

                if (GUILayout.Button("Clear all links"))
                {
                    ClearAllLinks();
                }
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                DisplayLinkProperties_Editor();

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
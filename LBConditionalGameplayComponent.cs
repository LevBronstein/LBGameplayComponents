using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LBGameplay
{
    [System.Serializable]
    public struct LBCondition
    {
        public bool bFlag;
    }

    //[AddComponentMenu("LBGameplay/Conditional Linked Gameplay Component (Dummy)")]
    public class LBLinkedConditionalGameplayComponent : LBLinkedGameplayComponent
    {
        protected virtual bool bCheckInputCondition(int _id)
        {
            return true;
        }

        protected virtual bool bCheckOutputCondition(int _id)
        {
            return true;
        }

        public override bool bCanTransferOut(LBGameplayComponentLink link)
        {
            if (!base.bCanTransferOut(link))
                return false;

            return bCheckOutputCondition(link.ParamID);
        }

        public override bool bCanTransferIn(LBGameplayComponentLink link)
        {
            if (!base.bCanTransferIn(link))
                return false;

            return bCheckInputCondition(link.ParamID);
        }
    }

    namespace Editors
    {
        [CustomEditor(typeof(LBLinkedConditionalGameplayComponent))]
        public class LBLinkedConditionalGameplayComponent_ED : LBLinkedGameplayComponent_ED
        {

        }
    }
}


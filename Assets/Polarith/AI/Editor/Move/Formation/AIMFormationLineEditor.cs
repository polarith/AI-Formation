//
// Copyright (c) 2016-2021 Polarith. All rights reserved.
// Licensed under the Polarith Software and Source Code License Agreement.
// See the LICENSE file in the project root for full license information.
//

using Polarith.AI.Move;
using Polarith.EditorUtils;
using UnityEditor;
using UnityEngine;

namespace Polarith.AI.MoveEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AIMFormationLine))]
    internal sealed class AIMFormationLineEditor : AIMFormationEditor
    {
        #region Properties =============================================================================================

        public override SerializedProperty BehaviourProperty
        {
            get { return serializedObject.FindProperty("formationLine"); }
        }

        #endregion // Properties

        #region Methods ================================================================================================

        protected override SerializedProperty formationProperty
        {
            get
            {
                return BehaviourProperty;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        public override void DrawBehaviourInspector()
        {
            base.DrawBehaviourInspector();            

            BehaviourProperty.isExpanded = EditorGUILayout.Foldout(BehaviourProperty.isExpanded,
                new GUIContent(BehaviourProperty.displayName, BehaviourProperty.tooltip));
            if (BehaviourProperty.isExpanded)
            {
                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(BehaviourProperty.FindPropertyRelative("upVector"));

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    foreach (Object t in targets)
                        ((AIMFormation)t).Formation.UpdateResultPosition();
                }

                EditorGUI.indentLevel--;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        public override void DrawGizmoInspector()
        {
            base.DrawGizmoInspector();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("formationGizmo"), true);
        }

        #endregion // Methods
    } // class AIMFormationLineEditor
} // namespace Polarith.AI.MoveEditor

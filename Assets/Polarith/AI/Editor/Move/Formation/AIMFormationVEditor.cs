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
    [CustomEditor(typeof(AIMFormationV))]
    internal sealed class AIMFormationVEditor : AIMFormationEditor
    {
        #region Fields =================================================================================================

        private SerializedProperty agentsPerLine = null;
        private SerializedProperty agentsPerLineX = null;
        private SerializedProperty agentsPerLineY = null;
        private SerializedProperty solid = null;
        private SerializedProperty shape = null;
        private SerializedProperty upVector = null;

        #endregion // Fields

        #region Properties =============================================================================================

        public override SerializedProperty BehaviourProperty
        {
            get { return serializedObject.FindProperty("formationV"); }
        }

        #endregion // Properties

        #region Methods ================================================================================================

        protected override SerializedProperty formationProperty
        {
            get { return BehaviourProperty; }
        }

        //--------------------------------------------------------------------------------------------------------------

        public override void DrawBehaviourInspector()
        {
            base.DrawBehaviourInspector();

            InitSerializedProperties();

            BehaviourProperty.isExpanded = EditorGUILayout.Foldout(BehaviourProperty.isExpanded,
                new GUIContent(BehaviourProperty.displayName, BehaviourProperty.tooltip));
            if (BehaviourProperty.isExpanded)
            {
                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                              
                EditorGUILayout.PropertyField(agentsPerLineX, new GUIContent("Agents Per Line Width"));
                EditorGUILayout.PropertyField(agentsPerLineY, new GUIContent("Agents Per Line Height"));
                if (EditorGUI.EndChangeCheck())
                {
                    if (agentsPerLineX.intValue < 1)
                        agentsPerLineX.intValue = 1;
                    if (agentsPerLineY.intValue < 1)
                        agentsPerLineY.intValue = 1;

                    serializedObject.ApplyModifiedProperties();
                    foreach (Object t in targets)
                        ((AIMFormation)t).Formation.UpdateResultPosition();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(solid);
                EditorGUILayout.PropertyField(shape);
                EditorGUILayout.PropertyField(upVector);

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

        //--------------------------------------------------------------------------------------------------------------

        private void InitSerializedProperties()
        {
            if (agentsPerLine == null)
                agentsPerLine = BehaviourProperty.FindPropertyRelative("agentsPerLine");
            if (agentsPerLineX == null)
                agentsPerLineX = agentsPerLine.FindPropertyRelative("X");
            if (agentsPerLineY == null)
                agentsPerLineY = agentsPerLine.FindPropertyRelative("Y");
            if (solid == null)
                solid = BehaviourProperty.FindPropertyRelative("solid");
            if (shape == null)
                shape = BehaviourProperty.FindPropertyRelative("shape");
            if (upVector == null)
                upVector = BehaviourProperty.FindPropertyRelative("upVector");
        }

        #endregion // Methods
    } // class AIMFormationVEditor
} // namespace Polarith.AI.MoveEditor

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
    /// <summary>
    /// Custom editor for each derived <see cref="AIMFormation"/>. When inheriting from this class, the two
    /// methods <see cref="AIMBehaviourEditor.DrawBehaviourInspector"/> and 
    /// <see cref="AIMBehaviourEditor.DrawGizmoInspector"/> needs to be implemented. These methods define what is shown 
    /// in the three corresponding inspector tabs. 
    /// <para/>
    /// Note that <see cref="AIMBehaviourEditor.Awake"/>, <see cref="AIMBehaviourEditor.OnEnable"/>, <see
    /// cref="AIMBehaviourEditor.OnInspectorGUI"/>, <see cref="AIMBehaviourEditor.DrawEnvironmentInspector"/>, <see
    /// cref="AIMBehaviourEditor.DrawBehaviourInspector"/>, and <see cref="AIMBehaviourEditor.DrawGizmoInspector"/> are
    /// already implemented for this level of inheritance, so the base versions of these methods should be called for
    /// each overridden version of these methods.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AIMFormation))]
    public abstract class AIMFormationEditor : AIMSteeringBehaviourEditor
    {
        #region Fields =================================================================================================

        private bool showWarning;
        private SerializedProperty formationFoldout = null;

        private SerializedProperty size = null;
        private SerializedProperty positionInFormation = null;
        private SerializedProperty spacing = null;
        private SerializedProperty arriveRadius = null;
        private SerializedProperty innerCatchUpRadius = null;
        private SerializedProperty outerCatchUpRadius = null;
        private SerializedProperty catchUpMultiplier = null;
        private SerializedProperty distanceMapping = null;

        private SerializedProperty targetObject = null;
        private SerializedProperty targetTag = null;
        private SerializedProperty targetPosition = null;

        private SerializedProperty referenceIndex = null;
        private string[] referenceOptions = new string[] { "TargetObject", "TargetTag", "TargetPosition" };

        private AIMFormation formation;

        #endregion // Fields

        #region Properties =============================================================================================

        /// <summary>
        /// ABstract property for the Formation.
        /// </summary>
        protected abstract SerializedProperty formationProperty
        {
            get;
        }

        #endregion // Properties

        #region Methods ================================================================================================

        /// <summary>
        /// Draws the logo of the formation layout.
        /// </summary>
        public override void DrawHeaderBegin()
        {
            Logo.DrawFormationLayout();
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Draws warnings of setup is invalid.
        /// </summary>
        public override void DrawHeaderEnd()
        {
            if (referenceIndex.intValue != 2 && (formation.TargetObject == null && formation.TargetTag == "Untagged"))
            {
                EditorGUILayout.HelpBox("Set up either a game object, tag, or position as target reference.",
                    MessageType.Info);
                EditorGUILayout.Separator();
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Logic to control the changes of the Fomation variables inside the Editor.
        /// </summary>
        public override void DrawBehaviourInspector()
        {
            base.DrawBehaviourInspector();

            InitSerializedProperties();

            formationFoldout.isExpanded = EditorGUILayout.Foldout(formationFoldout.isExpanded, "Formation");
            if (formationFoldout.isExpanded)
            {
                EditorGUI.indentLevel++;

                // Simple Editor
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(formationProperty.FindPropertyRelative("size"));
                if (EditorGUI.EndChangeCheck())
                {
                    if (size.intValue < 1)
                        size.intValue = 1;

                    serializedObject.ApplyModifiedProperties();
                    foreach (Object t in targets)
                        ((AIMFormation)t).Formation.UpdateResultPosition();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(formationProperty.FindPropertyRelative("positionInFormation"));
                if (EditorGUI.EndChangeCheck())
                {
                    if (positionInFormation.intValue < 0)
                        positionInFormation.intValue = 1;

                    serializedObject.ApplyModifiedProperties();
                    foreach (Object t in targets)
                        ((AIMFormation)t).Formation.UpdateResultPosition();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(formationProperty.FindPropertyRelative("spacing"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    foreach (Object t in targets)
                        ((AIMFormation)t).Formation.UpdateResultPosition();
                }

                // Advanced Editor
                if (AdvancedInspector)
                {
                    EditorGUILayout.BeginVertical(Styles.LessBackground);

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(outerCatchUpRadius);
                    if (EditorGUI.EndChangeCheck() && outerCatchUpRadius.floatValue < innerCatchUpRadius.floatValue)
                        outerCatchUpRadius.floatValue = innerCatchUpRadius.floatValue;

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(innerCatchUpRadius);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (innerCatchUpRadius.floatValue < arriveRadius.floatValue)
                            innerCatchUpRadius.floatValue = arriveRadius.floatValue;
                        else if (innerCatchUpRadius.floatValue > outerCatchUpRadius.floatValue)
                            innerCatchUpRadius.floatValue = outerCatchUpRadius.floatValue;
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(arriveRadius);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (arriveRadius.floatValue < 0f)
                            arriveRadius.floatValue = 0f;
                        else if (arriveRadius.floatValue > innerCatchUpRadius.floatValue)
                            arriveRadius.floatValue = innerCatchUpRadius.floatValue;
                    }                   

                    EditorGUILayout.PropertyField(catchUpMultiplier);
                    EditorGUILayout.PropertyField(distanceMapping);

                    EditorGUILayout.EndVertical();
                }

                EditorGUI.indentLevel--;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Logic to control changes of the Formation environmental settings inside the Editor.
        /// </summary>
        public override void DrawEnvironmentInspector()
        {
            InitSerializedProperties();

            // https://docs.unity3d.com/ScriptReference/EditorGUILayout.Popup.html

            referenceIndex.intValue =
                EditorGUILayout.Popup("Reference Formation", referenceIndex.intValue, referenceOptions);

            switch (referenceIndex.intValue)
            {
                case 0:
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(targetObject);
                    if (EditorGUI.EndChangeCheck() && targetObject.objectReferenceValue == null)
                    {
                        targetTag.stringValue = "Untagged";
                    }
                    break;

                case 1:
                    EditorGUILayout.PropertyField(targetTag);

                    targetObject.objectReferenceValue = null;
                    targetObject.objectReferenceValue = GameObject.FindGameObjectWithTag(targetTag.stringValue);

                    if (!Editors.IsPrefabOriginal((target as AIMFormation).gameObject))
                        showWarning = true;

                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(targetObject);
                    GUI.enabled = true;
                    break;

                case 2:
                    targetObject.objectReferenceValue = null;
                    targetTag.stringValue = "Untagged";

                    EditorGUILayout.PropertyField(targetPosition);
                    break;

                default:
                    Debug.LogError("No valid reference option selected.");
                    break;
            }

            if (targetObject.objectReferenceValue != null)
                showWarning = false;

            if (showWarning)
            {
                EditorGUILayout.HelpBox("The given tag does not belong to any object in the scene.",
                    MessageType.Info);
                EditorGUILayout.Separator();
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Displays the gizmo settings for the <see cref="Formation.OuterCatchUpRadius"/>, <see
        /// cref="Formation.InnerCatchUpRadius"/>, and <see cref="Formation.ArriveRadius"/>.
        /// </summary>
        public override void DrawGizmoInspector()
        {
            base.DrawGizmoInspector();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("outerRadiusGizmo"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("innerRadiusGizmo"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("arriveRadiusGizmo"), true);
        }

        //--------------------------------------------------------------------------------------------------------------

        private void InitSerializedProperties()
        {
            if (formationFoldout == null)
                formationFoldout = serializedObject.FindProperty("formationFoldout");
            if (size == null)
                size = formationProperty.FindPropertyRelative("size");
            if (positionInFormation == null)
                positionInFormation = formationProperty.FindPropertyRelative("positionInFormation");
            if (spacing == null)
                spacing = formationProperty.FindPropertyRelative("spacing");
            if (arriveRadius == null)
                arriveRadius = formationProperty.FindPropertyRelative("arriveRadius");
            if (innerCatchUpRadius == null)
                innerCatchUpRadius = formationProperty.FindPropertyRelative("innerCatchUpRadius");
            if (outerCatchUpRadius == null)
                outerCatchUpRadius = formationProperty.FindPropertyRelative("outerCatchUpRadius");
            if (catchUpMultiplier == null)
                catchUpMultiplier = formationProperty.FindPropertyRelative("catchUpMultiplier");
            if (distanceMapping == null)
                distanceMapping = formationProperty.FindPropertyRelative("distanceMapping");

            if (referenceIndex == null)
                referenceIndex = serializedObject.FindProperty("referenceIndex");
            if (targetObject == null)
                targetObject = serializedObject.FindProperty("targetObject");
            if (targetTag == null)
                targetTag = serializedObject.FindProperty("targetTag");
            if (targetPosition == null)
                targetPosition = serializedObject.FindProperty("targetPosition");
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Callback for enable event.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            formation = target as AIMFormation;
            InitSerializedProperties();
        }

        #endregion // Methods
    } // class AIMFormationEditor
} // namespace Polarith.AI.MoveEditor

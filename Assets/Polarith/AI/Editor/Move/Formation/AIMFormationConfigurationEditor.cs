//
// Copyright (c) 2016-2021 Polarith. All rights reserved.
// Licensed under the Polarith Software and Source Code License Agreement.
// See the LICENSE file in the project root for full license information.
//

using Polarith.AI.Move;
using Polarith.EditorUtils;
using UnityEditor;

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
    [CustomEditor(typeof(AIMFormationConfiguration))]
    internal sealed class AIMFormationConfiurationEditor : SimpleEditor
    {
        #region Methods ================================================================================================
        public override void OnInspectorGUI()
        {
            Logo.DrawFormationLayout();
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("agents"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("assignOnStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoObtainChildren"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoObtainSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("foreignObtainment"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("assignComplexity"));

            serializedObject.ApplyModifiedProperties();
        }

        #endregion // Methods
    } // class AIMFormationConfigurationEditor
} // namespace Polarith.AI.MoveEditor

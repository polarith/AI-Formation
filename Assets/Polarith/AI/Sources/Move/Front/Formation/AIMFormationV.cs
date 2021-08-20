//
// MIT License
// Copyright (c) 2021 Polarith.
// See the LICENSE file in the project root for full license information.
//

using Polarith.UnityUtils;
using UnityEngine;

namespace Polarith.AI.Move
{
    /// <summary>
    /// <see cref="AIMFormationV"/> computes an agent's position based on its order inside a V-shaped 
    /// formation and the reference position or -object of the formation.
    /// Front-end component of the underlying <see cref="Move.FormationV"/> class.
    /// </summary>
    [AddComponentMenu("Polarith AI » Move/Behaviours/Steering/AIM Formation V")]
    [HelpURL("http://docs.polarith.com/ai/component-aim-formationv.html")]
    public sealed class AIMFormationV : AIMFormation
    {
        #region Fields =================================================================================================

        [Tooltip("Reference to the underlying back-end class (read only).")]
        [SerializeField]
        private FormationV formationV = new FormationV();

        [Tooltip("Visualizes the shape of the formation by its boundary.")]
        [SerializeField]
        private VGizmo formationGizmo = new VGizmo();

        #endregion // Fields

        #region Properties =============================================================================================

        /// <summary>
        /// Polymorphic reference to the underlying back-end class <see cref="Move.SteeringBehaviour"/> (read only).
        /// </summary>
        public override SteeringBehaviour SteeringBehaviour
        {
            get { return formationV; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Polymorphic reference to the underlying back-end class <see cref="Move.Formation"/> (read only).
        /// </summary>
        public override Formation Formation
        {
            get { return formationV; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Reference to the underlying back-end class <see cref="Move.Formation"/> (read only).
        /// </summary>
        public FormationV FormationV
        {
            get { return formationV; }
            set { formationV = value; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Determines whether the underlying back-end class is thread-safe (read only). Returns always <c>true</c>.
        /// </summary>
        public override bool ThreadSafe
        {
            get { return true; }
        }

        #endregion // Properties

        #region Methods ================================================================================================

        /// <summary>
        /// Visualizes the boundary of the formation based on its size.
        /// </summary>
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (!formationGizmo.Enabled || !isActiveAndEnabled)
                return;

            float x, y, z;
            x = y = z = 0;
            float widthV, lengthV;
            widthV = lengthV = 0;

            if (formationV.Solid)
                widthV = formationV.AgentsPerLine.X;
            else
                widthV = 1;

            Vector3 gizmoCenter = Vector3.zero;
            Quaternion rot = Quaternion.identity;
            
            if (formationV.Shape == ShapeType.Planar)
            {
                z = 0;
                y = (formationV.Layers + (2 * widthV - 0.5f - formationV.SparseLayers)) / 2f * formationV.Spacing;
                x = (2 * widthV  - 0.5f + formationV.Layers - formationV.SparseLayers ) / 2f * formationV.Spacing;
                gizmoCenter.y -= (2 * widthV - formationV.SparseLayers - 0.5f) / 2f * formationV.Spacing;
            }
            else
            {
                z = formationV.AgentsPerLine.Y / 2f * formationV.Spacing;
                y = (formationV.Layers + (2 * widthV - 0.5f - formationV.SparseLayers)) / 2f * formationV.Spacing;
                x = (2 * widthV - 0.5f + formationV.Layers - formationV.SparseLayers) / 2f * formationV.Spacing;
                rot = Quaternion.FromToRotation(Vector3.back, Vector3.up);
                gizmoCenter.y += (formationV.AgentsPerLine.Y - 1) / 2f * formationV.Spacing;
                gizmoCenter.z -= (2 * widthV - formationV.SparseLayers - 0.5f) / 2f * formationV.Spacing;
            }

            lengthV = formationV.Layers - formationV.SparseLayers - 0.5f;

            if (TargetObject != null)
            {
                formationGizmo.Draw(TargetObject.transform.position + gizmoCenter, x, y, z,
                    widthV * formationV.Spacing,
                    lengthV * formationV.Spacing,
                    TargetObject.transform.rotation * rot);
            }
            else
            {
                formationGizmo.Draw(TargetPosition + gizmoCenter, x, y, z,
                    widthV * formationV.Spacing,
                    lengthV * formationV.Spacing,
                    rot);
            }
        }

        #endregion // Methods
    } // class AIMFormationV
} // namespace Polarith.AI.Move

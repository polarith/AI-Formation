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
    /// <see cref="AIMFormationCross"/> computes an agent's position based on its order inside a cross-shaped 
    /// formation and the reference position or -object of the formation.
    /// Front-end component of the underlying <see cref="Move.FormationCross"/> class.
    /// </summary>
    [AddComponentMenu("Polarith AI » Move/Behaviours/Steering/AIM Formation Cross")]
    [HelpURL("http://docs.polarith.com/ai/component-aim-formationcross.html")]
    public sealed class AIMFormationCross : AIMFormation
    {
        #region Fields =================================================================================================

        [Tooltip("Reference to the underlying back-end class (read only).")]
        [SerializeField]
        private FormationCross formationCross = new FormationCross();

        [Tooltip("Visualizes the shape of the formation by its boundary.")]
        [SerializeField]
        private BoxGizmo formationGizmo = new BoxGizmo();

        #endregion // Fields

        #region Properties =============================================================================================

        /// <summary>
        /// Polymorphic reference to the underlying back-end class <see cref="Move.SteeringBehaviour"/> (read only).
        /// </summary>
        public override SteeringBehaviour SteeringBehaviour
        {
            get { return formationCross; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Polymorphic reference to the underlying back-end class <see cref="Move.Formation"/> (read only).
        /// </summary>
        public override Formation Formation
        {
            get { return formationCross; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Reference to the underlying back-end class <see cref="Move.FormationCross"/> (read only).
        /// </summary>
        public FormationCross FormationCross
        {
            get { return formationCross; }
            set { formationCross = value; }
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

            formationGizmo.Markers = false;

            Vector3 targetPos = TargetPosition;
            Quaternion targetRot = Quaternion.identity;

            if (TargetObject != null)
            {
                targetPos = TargetObject.transform.position;
                targetRot = TargetObject.transform.rotation;
            }

            float x, y, z;
            Vector3 frontOffset = Vector3.zero;
            float halfSpacing = formationCross.Spacing / 2f;
            int layersY, layersX, layersZ;
            layersY = layersX = layersZ = 0;

            // Compute gizmo sizes and offset for y-direction
            if (formationCross.Shape == FormationCross.ShapeType.Planar)
            {
                layersY = Mathf.CeilToInt(formationCross.Size / 2f);
                layersY = formationCross.Size - 2 * Mathf.FloorToInt(layersY / 2f);
                layersX = Mathf.FloorToInt(Mathf.CeilToInt(formationCross.Size / 2f) / 2f);
                layersZ = 0;
                frontOffset.y = (layersX - (layersY - layersX - 1)) * halfSpacing;

                y = layersY / 2f * formationCross.Spacing;
                x = (layersX + 0.5f) * formationCross.Spacing;

                formationGizmo.Draw(targetPos + frontOffset, halfSpacing, y, halfSpacing,
                    targetRot * Quaternion.AngleAxis(90f, Vector3.up));
            }
            // Compute gizmo sizes and offset for z-direction
            else
            {
                layersZ = Mathf.CeilToInt(formationCross.Size / 3f);
                layersZ = formationCross.Size - 4 * Mathf.FloorToInt(layersZ / 2f);
                layersX = Mathf.FloorToInt(Mathf.CeilToInt(formationCross.Size / 3f) / 2f);
                layersY = layersX;
                frontOffset.z = (layersX - (layersZ - layersX - 1)) * halfSpacing;

                x = (layersX + 0.5f) * formationCross.Spacing;
                y = x;
                z = layersZ / 2f * formationCross.Spacing;

                formationGizmo.Draw(targetPos, halfSpacing, y, halfSpacing,
                    targetRot * Quaternion.AngleAxis(90f, Vector3.up));
                formationGizmo.Draw(targetPos + frontOffset, halfSpacing, halfSpacing, z,
                    targetRot * Quaternion.AngleAxis(90f, Vector3.forward));
            }

            formationGizmo.Draw(targetPos, x, halfSpacing, halfSpacing, targetRot);
        }

        #endregion // Methods
    } // class AIMFormationCross
} // namespace Polarith.AI.Move

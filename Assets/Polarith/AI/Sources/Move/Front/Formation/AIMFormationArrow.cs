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
    /// <see cref="AIMFormationArrow"/> computes an agent's position based on its order inside an arrow-shaped formation 
    /// and the reference position or -object of the formation.
    /// Front-end component of the underlying <see cref="Move.FormationArrow"/> class.
    /// </summary>
    [AddComponentMenu("Polarith AI » Move/Behaviours/Steering/AIM Formation Arrow")]
    [HelpURL("http://docs.polarith.com/ai/component-aim-formationarrow.html")]
    public sealed class AIMFormationArrow : AIMFormation
    {
        #region Fields =================================================================================================

        [Tooltip("Reference to the underlying back-end class (read only).")]
        [SerializeField]
        private FormationArrow formationArrow = new FormationArrow();

        [Tooltip("Visualizes the shape of the formation by its boundary.")]
        [SerializeField]
        private ArrowGizmo formationGizmo = new ArrowGizmo();

        #endregion // Fields

        #region Properties =============================================================================================

        /// <summary>
        /// Polymorphic reference to the underlying back-end class <see cref="Move.SteeringBehaviour"/> (read only).
        /// </summary>
        public override SteeringBehaviour SteeringBehaviour
        {
            get { return formationArrow; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Polymorphic reference to the underlying back-end class <see cref="Move.Formation"/> (read only).
        /// </summary>
        public override Formation Formation
        {
            get { return formationArrow; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Reference to the underlying back-end class <see cref="Move.FormationArrow"/> (read only).
        /// </summary>
        public FormationArrow FormationArrow
        {
            get { return formationArrow; }
            set { formationArrow = value; }
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
            x = y = z = formationArrow.Layers * formationArrow.Spacing / 2f;

            Quaternion rot = Quaternion.identity;

            if (formationArrow.Shape == FormationArrow.ShapeType.Planar)
            {
                y = 0;
                x = z;
                rot = Quaternion.FromToRotation(Vector3.forward, Vector3.up);
            }

            if (TargetObject != null)
                formationGizmo.Draw(TargetObject.transform.position, x, y, z,
                    TargetObject.transform.rotation * rot);
            else
                formationGizmo.Draw(TargetPosition, x, y, z, rot);
        }

        #endregion // Methods
    } // class AIMFormationArrow
} // namespace Polarith.AI.Move

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
    /// <see cref="AIMFormationBox"/> computes an agent's position based on its order inside a box-shaped formation and 
    /// the reference position or -object of the formation.
    /// Front-end component of the underlying <see cref="Move.FormationBox"/> class.
    /// </summary>
    [AddComponentMenu("Polarith AI » Move/Behaviours/Steering/AIM Formation Box")]
    [HelpURL("http://docs.polarith.com/ai/component-aim-formationbox.html")]
    public sealed class AIMFormationBox : AIMFormation
    {
        #region Fields =================================================================================================

        [Tooltip("Reference to the underlying back-end class (read only).")]
        [SerializeField]
        private FormationBox formationBox = new FormationBox();

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
            get { return formationBox; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Polymorphic reference to the underlying back-end class <see cref="Move.Formation"/> (read only).
        /// </summary>
        public override Formation Formation
        {
            get { return formationBox; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Reference to the underlying back-end class <see cref="Move.FormationBox"/> (read only).
        /// </summary>
        public FormationBox FormationBox
        {
            get { return formationBox; }
            set { formationBox = value; }
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

            if (formationBox.Shape == FormationBox.ShapeType.Planar)
            {
                x = formationBox.AgentsPerLine.X / 2f * formationBox.Spacing;
                y = formationBox.Layers * formationBox.Spacing / 2f;
                z = 0;
            }
            else
            {
                x = formationBox.AgentsPerLine.X / 2f * formationBox.Spacing;
                y = formationBox.AgentsPerLine.Y / 2f * formationBox.Spacing;
                z = formationBox.Layers * formationBox.Spacing / 2f;
            }

            if (TargetObject != null)
                formationGizmo.Draw(TargetObject.transform.position, x, y, z, TargetObject.transform.rotation);
            else
                formationGizmo.Draw(TargetPosition, x, y, z, Quaternion.identity);
        }

        #endregion // Methods
    } // class AIMFormationBox
} // namespace Polarith.AI.Move

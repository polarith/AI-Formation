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
    /// <see cref="AIMFormationCircle"/> computes an agent's position based on its order inside a circle-shaped 
    /// formation and the reference position or -object of the formation.
    /// Front-end component of the underlying <see cref="Move.FormationCircle"/> class.
    /// </summary>
    [AddComponentMenu("Polarith AI » Move/Behaviours/Steering/AIM Formation Circle")]
    [HelpURL("http://docs.polarith.com/ai/component-aim-formationcircle.html")]
    public sealed class AIMFormationCircle : AIMFormation
    {
        #region Fields =================================================================================================

        [Tooltip("Reference to the underlying back-end class (read only).")]
        [SerializeField]
        private FormationCircle formationCircle = new FormationCircle();

        [Tooltip("Visualizes the shape of the formation by its boundary.")]
        [SerializeField]
        private CircleGizmo formationGizmo = new CircleGizmo(Color.yellow);

        #endregion // Fields

        #region Properties =============================================================================================

        /// <summary>
        /// Polymorphic reference to the underlying back-end class <see cref="Move.SteeringBehaviour"/> (read only).
        /// </summary>
        public override SteeringBehaviour SteeringBehaviour
        {
            get { return formationCircle; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Polymorphic reference to the underlying back-end class <see cref="Move.Formation"/> (read only).
        /// </summary>
        public override Formation Formation
        {
            get { return formationCircle; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Reference to the underlying back-end class <see cref="Move.FormationCircle"/> (read only).
        /// </summary>
        public FormationCircle FormationCircle
        {
            get { return formationCircle; }
            set { formationCircle = value; }
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

            if (TargetObject != null)
            {
                if (formationCircle.Solid)
                {
                    formationGizmo.Draw(TargetObject.transform.position, Quaternion.identity,
                        (formationCircle.Layers + 0.5f) * formationCircle.Spacing);
                }
                else
                {
                    float circumcircle = formationCircle.Size * formationCircle.Spacing;
                    formationGizmo.Draw(TargetObject.transform.position, Quaternion.identity,
                        circumcircle / (Mathf.PI * 2f) + formationCircle.Spacing / 2f);
                }
            }
            else
            {
                if (formationCircle.Solid)
                {
                    formationGizmo.Draw(TargetPosition, Quaternion.identity,
                        (formationCircle.Layers + 0.5f) * formationCircle.Spacing);
                }
                else
                {
                    float circumcircle = formationCircle.Size * formationCircle.Spacing;
                    formationGizmo.Draw(TargetPosition, Quaternion.identity,
                        circumcircle / (Mathf.PI * 2f) + formationCircle.Spacing / 2f);
                }
            }
        }

        #endregion // Methods
    } // class AIMFormationCircle
} // namespace Polarith.AI.Move

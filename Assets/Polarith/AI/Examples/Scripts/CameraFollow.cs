using UnityEngine;

namespace Polarith.AI.Package
{
    /// <summary>
    /// A script that is used to let a <see cref="Camera"/> follow a <see cref="Target"/> object. You can adjust <see
    /// cref="Offset"/>, <see cref="MoveSpeed"/>, and <see cref="CameraAngle"/>, whereas CameraAngle only affects the
    /// X-value of the camera´s rotation. The <see cref="UpdateMode"/> is used to decide which of Unity´s update loops
    /// is called since some of the modes can create movement artifacts if different Unity versions.
    /// <para/>
    /// Note, this is just a script used for our example scenes and, therefore, not part of the actual API. We do not
    /// guarantee that this script is working besides our examples.
    /// </summary>
    [AddComponentMenu("Polarith AI » Move » Package/Camera Follow")]
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow : MonoBehaviour
    {
        #region Fields =================================================================================================

        [Tooltip("Camera that shall follow the target.")]
        [SerializeField]
        private Camera cam;

        [Tooltip("Target X-value of the camera´s rotation.")]
        [SerializeField]
        private float cameraAngle = 30;

        [Tooltip("Affects how fast movement changes of the target are applied to the camera.")]
        [SerializeField]
        private float moveSpeed = 3;

        [Tooltip("Distance between camera and target.")]
        [SerializeField]
        private float offset = 300;

        [Tooltip("Target object the camera tries to follow.")]
        [SerializeField]
        private Transform target;

        [Tooltip("Decides which parts of Unity´s update loops will be called.")]
        [SerializeField]
        private UpdateType updateMode = UpdateType.FixedUpdate;

        #endregion // Fields

        #region Enums ==================================================================================================

        /// <summary>
        /// Decides which parts of Unity´s update loops will be called.
        /// </summary>
        public enum UpdateType
        {
            /// <summary>
            /// Controls Unity´s FixedUpdate function.
            /// </summary>
            FixedUpdate = 0,

            /// <summary>
            /// Controls Unity´s LateUpdate function.
            /// </summary>
            LateUpdate = 1,

            /// <summary>
            /// Controls Unity´s Update function.
            /// </summary>
            Update = 2
        };

        #endregion // Enums

        #region Properties =============================================================================================

        /// <summary>
        /// Camera that shall follow the target.
        /// </summary>
        public Camera Camera
        {
            get { return cam; }
            set { cam = value; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Target X-value of the camera´s rotation.
        /// </summary>
        public float CameraAngle
        {
            get { return cameraAngle; }
            set { cameraAngle = value; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Affects how fast movement changes of the target are applied to the camera.
        /// </summary>
        public float MoveSpeed
        {
            get { return moveSpeed; }
            set { moveSpeed = value; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Distance between camera and target.
        /// </summary>
        public float Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Target object the camera tries to follow.
        /// </summary>
        public Transform Target
        {
            get { return target; }
            set { target = value; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Decides which parts of Unity´s update loops will be called.
        /// </summary>
        public UpdateType UpdateMode
        {
            get { return updateMode; }
            set { updateMode = value; }
        }

        #endregion // Properties

        #region Methods ================================================================================================

        private void FixedUpdate()
        {
            if (UpdateMode == UpdateType.FixedUpdate)
                FollowTarget(Time.deltaTime);
        }

        //--------------------------------------------------------------------------------------------------------------

        private void LateUpdate()
        {
            if (UpdateMode == UpdateType.LateUpdate)
                FollowTarget(Time.deltaTime);
        }

        //--------------------------------------------------------------------------------------------------------------

        private void Update()
        {
            if (UpdateMode == UpdateType.Update)
                FollowTarget(Time.deltaTime);
        }

        //--------------------------------------------------------------------------------------------------------------

        private void FollowTarget(float deltaTime)
        {
            // if no target, or no time passed then we quit early, as there is nothing to do
            if (!(deltaTime > 0) || Target == null)
            {
                return;
            }

            Vector3 goal = new Vector3(Target.position.x + (Target.position.x - Camera.transform.position.x),
                                       Target.position.y,
                                       Target.position.z);
            goal -= Camera.transform.forward * Offset;

            float dist = (Target.position - Camera.transform.position).magnitude;

            // camera position moves towards target position:
            Camera.transform.position = Vector3.Lerp(Camera.transform.position,
                                                     goal,
                                                     deltaTime * MoveSpeed * dist / Offset);
            Camera.transform.rotation = Quaternion.Lerp(Camera.transform.rotation,
                                                     Quaternion.Euler(CameraAngle,
                                                                      Camera.transform.rotation.eulerAngles.y,
                                                                      Camera.transform.rotation.eulerAngles.z),
                                                     Time.deltaTime);
        }

        #endregion // Methods
    } // class CameraFollow
} // namespace Polarith.AI.Package

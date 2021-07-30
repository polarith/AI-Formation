//
// MIT License
// Copyright (c) 2021 Polarith.
// See the LICENSE file in the project root for full license information.
//

using System.Collections.Generic;
using UnityEngine;
using Polarith.UnityUtils;

namespace Polarith.AI.Move
{
    /// <summary>
    /// Assigns the agents to their position inside the formation. Also calls the low-level logic to compute the
    /// agents' position inside the formation. Optionally, the agents are gathered from the child objects automatically.
    /// </summary>
    /// <remarks>
    /// You may want to provide your own assignment logic. Therefore, you need to inherit from <see
    /// cref="AIMFormationConfiguration"/>, and provide an assignment method that as in 
    /// <see cref="Polarith.UnityUtils.AssignmentSolver"/>. You should make yourself familiar with the so called 
    /// <a href="https://en.wikipedia.org/wiki/Assignment_problem">assignment problem</a>. Our simple assignment 
    /// method <see cref="Polarith.UnityUtils.AssignmentSolver.FindGreedy(List{AIMFormation}, List{Vector3}, int)"/> is 
    /// a serialized greedy algorithm, the complex method 
    /// <see cref="Polarith.UnityUtils.AssignmentSolver.FindOptimal(int[,])"/> implements the so-called 
    /// <a href="https://en.wikipedia.org/wiki/Hungarian_algorithm">Hungarian Method</a>.
    /// </remarks>
    public class AIMFormationConfiguration : MonoBehaviour
    {
        #region Fields =================================================================================================

        /// <summary>
        /// <see cref="AIMFollow"/> behaviours of the agents.
        /// </summary>
        [Tooltip("AIMFormation behaviours of the agents")]
        [SerializeField]
        protected List<AIMFormation> agents = new List<AIMFormation>();

        /// <summary>
        /// Toggle if the assignment should be performed on start.
        /// </summary>
        [Tooltip("Toggle if the assignment should be performed on start.")]
        [SerializeField]
        protected bool assignOnStart = true;

        /// <summary>
        /// Toggle if agents should be assigned automatically to the agents list. Agents need to be children of 
        /// the Game Object with <see cref="AIMFormationConfiguration"/>. Children are searched recursively for the 
        /// first occurrence of <see cref="AIMFormation"/>.
        /// </summary>
        [Tooltip("Toggle if agents should be assigned automatically to the agents list. Agents need to be " +
            "children of the Game Object with AIMFormationConfiguration. Children are searched recursively for the " +
            "first occurrence of AIMFormation.")]
        [SerializeField]
        protected bool autoObtainChildren = true;

        /// <summary>
        /// Toggle if the maximum number of agents in the formation should be adjusted to the actual 
        /// number of agents automatically. The actual number is based on the direct children of this GameObject with 
        /// <see cref="AIMFormationConfiguration"/>. Note that only agents from the formation should be children.
        /// </summary>
        [Tooltip("Toggle if the maximum number of agents in the formation should be adjusted to the actual" +
            "number of agents automatically. The actual number is based on the direct children of this Game Object " +
            "with AIMFormationConfiguration. Note that only agents from the formation should be children.")]
        [SerializeField]
        protected bool autoObtainSize = true;

        /// <summary>
        /// If a foreign game object is set, the auto options are applied on that game object. Thus, you are able to 
        /// encapsulate AIMFormationConfiguration to another game object, e.g., to prevent the agents from obtaining 
        /// transformations from the parent game object that contains the configuration.
        /// </summary>
        [Tooltip("If a foreign game object is set, the auto options are applied on that game object. Thus, you " +
            "are able to encapsulate AIMFormationConfiguration to another game object, e.g., to prevent " +
            "the agents from obtaining transformations from the parent game object that contains the configuration.")]
        [SerializeField]
        protected GameObject foreignObtainment;

        /// <summary>
        /// You can select the complexity of the assignment algorithm. You do
        /// this by scaling the ratio between a simple and fast, but non-optimal algorithm or a complex and slow, but
        /// optimal algorithm. The smaller the value, the more assignments are done with the simple algorithm and vice
        /// versa. We recommend the complex algorithm up to ~64 units at once, but feel free to try the impact on your
        /// scene.
        /// </summary>
        [Tooltip("You can select the complexity of the assignment algorithm. You do " +
        "this by scaling the ratio between a simple and fast, but non-optimal algorithm, or a complex and slow, but " +
        "optimal algorithm. The smaller the value, the more assignments are done with the simple algorithm and vice " +
        "versa. We recommend the complex algorithm up to ~64 agents at once, but feel free to try the impact on your " +
        "scene.")]
        [SerializeField]
        [Range(0f, 1f)]
        protected float assignComplexity = 0.5f;

        /// <summary>
        /// List of the target positions obtained by the agents, i.e., the target positions in the formation.
        /// </summary>
        protected List<Vector3> targetPositions = new List<Vector3>();

        private int maximumSize = 10;
        private Transform trans;

        #endregion // Fields

        #region Properties =============================================================================================

        /// <summary>
        /// <see cref="AIMFollow"/> behaviours of the agents.
        /// </summary>
        public List<AIMFormation> Agents
        {
            get { return agents; }
            set { agents = value; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Toggle if the assignment should be performed on start.
        /// </summary>
        public bool AssignOnStart
        {
            get { return assignOnStart; }
            set { assignOnStart = value; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Toggle if agents should be assigned automatically to the agents list. Agents need to be children of 
        /// the Game Object with <see cref="AIMFormationConfiguration"/>. Children are searched recursively for the 
        /// first occurrence of <see cref="AIMFormation"/>.
        /// </summary>
        public bool AutoObtainChildren
        {
            get { return autoObtainChildren; }
            set { autoObtainChildren = value; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Toggle if the maximum number of agents in the formation should be adjusted to the actual 
        /// number of agents automatically. The actual number is based on the direct children of this GameObject with 
        /// <see cref="AIMFormationConfiguration"/>. Note that only agents from the formation should be children.
        /// </summary>
        public bool AutoObtainSize
        {
            get { return autoObtainSize; }
            set { autoObtainSize = value; }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// If a foreign game object is set, the auto options are applied on that game object. Thus, you are able to 
        /// encapsulate AIMFormationConfiguration to another game object, e.g., to prevent the agents from obtaining 
        /// transformations from the parent game object that contains the configuration.
        /// </summary>
        public GameObject ForeignObtainment
        {
            get { return foreignObtainment; }
            set
            {
                foreignObtainment = value;
                trans = foreignObtainment == null ? transform : foreignObtainment.transform;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// You can select the complexity of the assignment algorithm. You do
        /// this by scaling the ratio between a simple and fast, but non-optimal algorithm or a complex and slow, but
        /// optimal algorithm. The smaller the value, the more assignments are done with the simple algorithm and vice
        /// versa. We recommend the complex algorithm up to ~64 units at once, but feel free to try the impact on your
        /// scene.
        /// </summary>
        public float AssignComplexity
        {
            get
            {
                return assignComplexity;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                    Debug.LogWarning("AssignComplexity needs to be at least 0! Value has been set to 0!");
                }
                else if (value > 1)
                {
                    value = 1;
                    Debug.LogWarning("AssignComplexity can't be bigger than 1! Value has been set to 1!");
                }
                assignComplexity = value;
            }
        }

        #endregion // Properties

        #region Methods ================================================================================================

        /// <summary>
        /// Virtual method to assign the agents to the target positions. Therefore, we assign a position to
        /// <see cref="Formation.positionInFormation"/> to each agent inside the formation.
        /// </summary>
        /// <param name="size">
        /// The number of agents that should be assigned to a target position.
        /// </param>
        /// <remarks>
        /// We provide a simple and a complex assignment function. The simple version is fast, but non-optimal, meaning
        /// your agents will probably travel larger (non-optimal) distances to reach their assigned positions, and will
        /// intersect with other agents. The complex version is optimal, but slow. This results in short distances for
        /// your agents, but with high computational effort. Remember to call this method only ONCE if you update
        /// the <see cref="Formation.Size"/> or the list of assigned <see cref="Agents"/> manually.
        /// </remarks>
        public virtual void Assignment(int size)
        {
            float ratio = 1f - assignComplexity;
            int simplePart = Mathf.CeilToInt(size * ratio);
            int complexPart = size - simplePart;

            // Create simple assignments
            int[] simpleAssigns = simplePart == 0 ? null : CreateSimpleAssignments(simplePart);

            // Create complex assignments
            int[] complexAssigns = CreateComplexAssignments(targetPositions, simpleAssigns, complexPart);

            // Merge assignments assign simple assignments and mark positions that are left out
            int[] mergedAssigns = simplePart == 0 ?
                complexAssigns : MergeAssignments(simpleAssigns, complexAssigns, size);

            SetupBehaviours(mergedAssigns);
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Updates the list of <see cref="Agents"/> if <see cref="AutoObtainChildren"/> is enabled, updates the 
        /// <see cref="Formation.Size"/> if <see cref="AutoObtainSize"/> is enabled, recomputes the assignments, and 
        /// updates the behaviours of the agents with the new assignments.
        /// </summary>
        /// <remarks>
        /// Remember to call this method only ONCE if you update any parameter of the assigned agents that affect the
        /// representation of the formation (e.g., iterate over the assigned agents, change
        /// <see cref="FormationBox.AgentsPerLine"/>, and call this method afterwards once).
        /// </remarks>
        public virtual void UpdateConfig()
        {
            if (autoObtainSize)
                maximumSize = GetFormationSize();
            else
                maximumSize = agents.Count;

            if (autoObtainChildren)
            {
                agents.Clear();
                AutoObtainAgents();
            }            

            targetPositions.Clear();

            for (int i = 0; i < agents.Count; i++)
            {
                agents[i].Formation.Size = maximumSize;
                agents[i].Formation.PositionInFormation = i;
                targetPositions.Add(
                    agents[i].TargetObject.transform.rotation * 
                    trans.rotation * 
                    agents[i].Formation.ComputePosition()
                    );
            }

            Assignment(agents.Count);
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Looks for <see cref="AIMFormation"/> instances in the child objects recursively, and adds them to the 
        /// <see cref="Agents"/> list.
        /// </summary>
        protected virtual void AutoObtainAgents()
        {
            // Get children with active AIMFormation recursively
            GetFormationInChildren(ref agents, trans, maximumSize, autoObtainSize);
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///  Get the number of active agents that are child objects of the <see cref="AIMFormationConfiguration"/>. 
        ///  You may want to override this method for a more complex logic.
        /// </summary>
        /// <returns>The number of active children.</returns>
        protected virtual int GetFormationSize()
        {
            int count = 0;
            for (int i = 0; i < trans.childCount; i++)
            {
                if (trans.GetChild(i).gameObject.activeInHierarchy)
                    count++;
            }
            return count;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Sets the target position in <see cref="Formation"/> for each agent in <see cref="Agents"/>.
        /// </summary>
        /// <param name="assigns">An array that contains the resulting logical position inside the formation for each 
        /// agent. Thus, element <c>i</c> contains the position for agent <c>i</c> in <see cref="Agents"/></param>
        protected void SetupBehaviours(int[] assigns)
        {
            // Assign targetPosition to PositionInFormation in AIMFormation
            for (int i = 0; i < assigns.GetLength(0); i++)
            {
                if (i >= agents.Count)
                    return;

                agents[i].Formation.PositionInFormation = assigns[i];
                agents[i].Formation.UpdateResultPosition();
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private void Start()
        {
            trans = foreignObtainment == null ? transform : foreignObtainment.transform;

            if (assignOnStart)
                UpdateConfig();
        }

        //--------------------------------------------------------------------------------------------------------------

        private int[] CreateSimpleAssignments(int simplePart)
        {
            int[] simpleAssigns = AssignmentSolver.FindGreedy(agents, targetPositions, simplePart);
            for (int i = simplePart; i < simpleAssigns.Length; i++)
                simpleAssigns[i] = -1;

            return simpleAssigns;
        }

        //--------------------------------------------------------------------------------------------------------------

        private int[] CreateComplexAssignments(List<Vector3> targetPositions, int[] simpleAssigns, int complexPart)
        {
            int[,] complexCostMat = CreateCostMatrix(targetPositions, simpleAssigns, complexPart);
            return AssignmentSolver.FindOptimal(complexCostMat);
        }

        //--------------------------------------------------------------------------------------------------------------

        private int[,] CreateCostMatrix(List<Vector3> targetPositions, int[] gaps = null, int max = 0)
        {
            int min = Mathf.Min(agents.Count, targetPositions.Count);

            // No gaps -> only complex assignments -> full matrix
            if (gaps == null)
            {
                int[,] costMatrix = new int[min, min];

                // Build cost matrix from each target i to each agent j
                // Algorithm is only able to handle integer values, so we must convert the float distances
                for (int i = 0; i < min; i++)
                {
                    for (int j = 0; j < min; j++)
                    {
                        costMatrix[j, i] = Mathf.FloorToInt(
                            ((targetPositions[i] - agents[j].transform.position).magnitude) * 1000f);
                    }
                }
                return costMatrix;
            }
            // Gaps from simple assignments need to be handled since the assigned agents are not available anymore
            // ->create smaller (dense) matrix with the available targetPositions and agents
            else
            {
                // Use internal indices, since we need a dense matrix without the already assigned costs
                int matrixI = 0;
                int matrixJ = 0;

                int[,] costMatrix = new int[max, max];

                // Build cost matrix from each targetPosition i to each agent j
                for (int i = 0; i < min; i++)
                {
                    if (gaps[i] < 0) // Check if target is valid (-1 == empty)
                    {
                        matrixJ = 0;
                        for (int j = 0; j < min; j++)
                        {
                            if (!(System.Array.IndexOf(gaps, j) >= 0)) // Check if assignment is valid (-1 == empty)
                            {
                                costMatrix[matrixJ++, matrixI] =
                                    Mathf.FloorToInt(((targetPositions[i] - agents[j].transform.position).magnitude)
                                    * 1000f);
                            }
                        }
                        matrixI++;
                    }
                }
                return costMatrix;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private int[] MergeAssignments(int[] simpleAssigns, int[] complexAssigns, int size)
        {
            int[] mergedAssigns = new int[size];
            for (int i = 0; i < mergedAssigns.Length; i++)
                mergedAssigns[i] = -1;

            int[] complexHelp = new int[simpleAssigns.Length];
            for (int i = 0; i < simpleAssigns.Length; i++)
                complexHelp[i] = i;

            // Check if agent is already assigned and store to result array and helper array
            for (int i = 0; i < mergedAssigns.Length; i++)
            {
                if (simpleAssigns[i] >= 0)  // > 0 == assigned
                {
                    mergedAssigns[i] = simpleAssigns[i];
                    complexHelp[simpleAssigns[i]] = -1;     // -1 == already assigned
                }
            }

            // Store positions that are left out in complex array
            int complexCounter = 0;
            int[] complexPos = new int[complexAssigns.Length];
            for (int i = 0; i < mergedAssigns.Length; i++)
            {
                if (complexHelp[i] > 0)
                    complexPos[complexCounter++] = i;
            }

            // Assign complex assignments into the positions that are available
            complexCounter = 0;
            for (int i = 0; i < mergedAssigns.Length; i++)
            {
                if (mergedAssigns[i] < 0)
                    mergedAssigns[i] = complexPos[complexAssigns[complexCounter++]];
            }

            return mergedAssigns;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void GetFormationInChildren(ref List<AIMFormation> list, Transform current, int maxSize, bool autoSize)
        {
            if (!autoSize && list.Count >= maxSize)
                return;

            AIMFormation[] formations = current.GetComponents<AIMFormation>();
            foreach (AIMFormation f in formations)
            {
                if (f.isActiveAndEnabled && (autoSize || list.Count < maxSize))
                {
                    list.Add(f);
                    break; // Get only the first Formation component in each object only.
                }
            }                                    

            if (current.transform.childCount > 0)
            {
                for (int i = 0; i < current.transform.childCount; i++)
                    GetFormationInChildren(ref list, current.GetChild(i), maxSize, autoSize);
            }
        }

        #endregion // Methods
    } // class AIMFormationConfiguration
} // namespace Polarith.AI.Move

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MrE_Engine
{
    /// <summary>
    /// A class to store in-memory constraints collection
    /// TODO: in the future move to interface to enable third party accessors 
    /// </summary>
    public class ConstraintsCollection : List<CustomConstraintBase>
    {
        private SigmaPointCollection m_sigma_points = null;
        private List<CustomVariableBase> m_Variables = null;
        private InferenceTask m_Task = null;

        #region PROPERTIES

        #region JointPriorDistribution Property
        /// <summary>
        /// Property JointPriorDistribution
        /// 
        /// </summary>
        public IProbabilityDistribution JointPriorDistribution
        {
            get
            {
                return m_JointPriorDistribution;
            }
            set
            {
                m_JointPriorDistribution = value;
            }
        }
        private IProbabilityDistribution m_JointPriorDistribution = null;
        #endregion

        #endregion

        #region METHODS

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public ConstraintsCollection(
            SigmaPointCollection sigma_points,
            List<CustomVariableBase> variables,
            IProbabilityDistribution pdf,
            InferenceTask task)
            : base(MrEHelper.MAXNUMBEROFCONSTRAINTS)
        {
            m_sigma_points = sigma_points;
            m_Variables = variables;
            m_JointPriorDistribution = pdf;
            m_Task = task;
        }
        #endregion

        #region ReplaceSigmaPointAtIndex(int index, SigmaPoint sigma_point)
        /// <summary>
        /// Replaces an existing sigma point with a new instance
        /// </summary>
        /// <param name="index">Index of existing sigma point to be replaced</param>
        /// <param name="sigma_point">A new sigma point to replace with</param>
        /// <returns>True, if successfull</returns>
        public bool ReplaceSigmaPointAtIndex(int index, SigmaPoint sigma_point)
        {
            double constraint_value = 0;
            double[] constraint_values = new double[this.Count];
            int i = 0;
            foreach (CustomConstraintBase constraint in this)
            {
                if (!constraint.TryGetConstraintValueAt(sigma_point.VectorCoordinates, out constraint_value))
                    return false;
                constraint_values[i] = constraint_value;
                i++;
            }
            i = 0;
            foreach (CustomConstraintBase constraint in this)
            {
                constraint.ReplaceConstraintValueAtExisting(index, constraint_values[i]);
                i++;
            }
            m_sigma_points[index] = sigma_point;
            return true;
        }
        #endregion

        #region AppendNewSigmaPoint(SigmaPoint sigma_point)
        /// <summary>
        /// Appends an information for a new sigma point. 
        /// Recalculates constraints values and updates global minimum information
        /// </summary>
        /// <param name="sigma_point">An instance of a new sigma point</param>
        /// <returns>True when successfull, else at least one constraint value was NaN</returns>
        public bool AppendNewSigmaPoint(SigmaPoint sigma_point)
        {
            bool success = true;
            double constraint_value = double.NaN;
            int i = 0;
            double[] constraint_values = new double[this.Count];
            foreach (CustomConstraintBase constraint in this)
            {
                if (constraint.TryGetConstraintValueAt(sigma_point.VectorCoordinates, out constraint_value))
                    constraint_values[i] = constraint_value;
                else
                    return false;
                i++;
            }
            i = 0;
            //double absolute_constraint_value;
            foreach (CustomConstraintBase constraint in this)
            {
                constraint.AppendConstraintValueAtNewSigmaPoint(constraint_values[i]);
                i++;
            }
            m_sigma_points.Add(sigma_point);
            return success;
        }
        #endregion

        #region AppendNewConstraint(CustomConstraintBase new_constraint)
        /// <summary>
        /// Appends a new constraint. If appending fails, the new constraints value is set to IsValid=false
        /// </summary>
        /// <param name="new_constraint"></param>
        /// <returns></returns>
        public bool AppendNewConstraint(CustomConstraintBase new_constraint)
        {
            if (m_sigma_points == null)
                return true;
            // The distances among existing sigma points do not change here, so no need to update
            foreach (SigmaPoint sigma_point in m_sigma_points)
            {
                if (!AppendNewSigmaPoint(sigma_point))
                {
                    return false;
                }
            }
            Add(new_constraint);
            return true;
        }
        #endregion

        #region GenerateSeries
        /// <summary>
        /// Generates discrete values for the first variable in range -100 to 100.
        /// Used for testing purposes.
        /// </summary>
        public void GenerateSeries()
        {

            for (int k = -100; k <= 100; k++)
            {
                List<double> vector_coordinates = new List<double>(1);
                vector_coordinates.Add(k);
                double density_value = 0;
                if (!m_JointPriorDistribution.TryGetProbabilityDensityAt(vector_coordinates, out density_value))
                    return;
                SigmaPoint sigma = new SigmaPoint(vector_coordinates, density_value);
                if (!AppendNewSigmaPoint(sigma))
                    return;
            }

        }
        #endregion

        #region TryGenerateUniformSigmaPoints
        /// <summary>
        /// Tries to generates uniform sigma points for 
        /// all variables in their defined ranges
        /// </summary>
        /// <param name="sigma_points_count">The number of sigma points to generate</param>
        /// <returns></returns>
        public int TryGenerateUniformSigmaPoints(int sigma_points_count)
        {

            if (sigma_points_count < 0)
                return 0;
            CryptoRandom[] rand_vars = new CryptoRandom[m_Variables.Count];

            for (int i = 0; i < m_Variables.Count; i++)
            {
                rand_vars[i] = new CryptoRandom();
                //rand_vars[i] = new Random((int)DateTime.Now.Millisecond);
                //rand_vars[i] = new Random((int)i * 100);
                //rand_vars[i] = new Random((int)i);
                //rand_vars[i] = new Random((int)DateTime.Now.Ticks);
            }
            for (int k = 0; k < sigma_points_count; k++)
            {
                List<double> vector_coordinates = new List<double>(m_Variables.Count);
                for (int i = 0; i < m_Variables.Count; i++)
                {
                    CustomVariableBase variable = m_Variables[i];
                    //vector_coordinates.Add(MrEHelper.GetNormal(0.5, 0.13));
                    //vector_coordinates.Add((MrEHelper.GetUniform() * ((double)variable.MaxValue - (double)variable.MinValue)) + (double)variable.MinValue);
                    vector_coordinates.Add((rand_vars[i].NextDouble() *
                        ((double)variable.MaxValue - (double)variable.MinValue)) +
                        ((double)variable.MinValue));
                }
                double density_value = 0;
                if (!m_JointPriorDistribution.TryGetProbabilityDensityAt(vector_coordinates, out density_value))
                    continue;
                SigmaPoint sigma = new SigmaPoint(vector_coordinates, density_value);
                if (!m_Task.IsSamplingConstraintSatisfied(sigma))
                    continue;
                if (!AppendNewSigmaPoint(sigma))
                    continue;
            }
            return m_sigma_points.Count;
        }
        #endregion


        #region GenerateRandomSigmaPoint(out SigmaPoint sigma_point)
        /// <summary>
        /// Generates a random sigma point
        /// </summary>
        /// <param name="sigma_point"></param>
        /// <returns>True, if successfull, false if new random value does not pass validation</returns>
        public bool GenerateRandomSigmaPoint(out SigmaPoint sigma_point)
        {
            sigma_point = null;
            int i = 0;
            List<double> vector_coordinates = new List<double>(m_Variables.Count);
            for (i = 0; i < m_Variables.Count; i++)
            {
                vector_coordinates.Add(m_Variables[i].GetRandomSample());
            }
            double density_value = double.NaN;
            if (!m_JointPriorDistribution.TryGetProbabilityDensityAt(vector_coordinates, out density_value))
                return false;
            sigma_point = new SigmaPoint(vector_coordinates, density_value);
            if (m_sigma_points.Count < MrEHelper.MAXNUMBEROFSIGMAPOINTS)
            {
                double distance = 0;
                i = 0;
                double[] distances = new double[m_sigma_points.Count];

                foreach (SigmaPoint existing_sigma_point in m_sigma_points)
                {
                    distance = SquaredDistance(sigma_point, existing_sigma_point);
                    distances[i] = distance;
                    if (distance < sigma_point.DeltaCumulationDistance)
                    {
                        sigma_point.SetDeltaCumulativeDistance(distance, i);
                    }
                    i++;
                }
                if (!AppendNewSigmaPoint(sigma_point))
                    return false;
                int last_sigma_point_index = m_sigma_points.Count - 1;

                // Update the global minimum;
                m_GlobalMinimumCumulativeDelta = double.MaxValue;
                if (m_sigma_points.Count > 1)
                {
                    // the following has sense only when there are more than 1 sigma point
                    if (sigma_point.DeltaCumulation < m_GlobalMinimumCumulativeDelta)
                    {
                        m_GlobalMinimumCumulativeDelta = sigma_point.DeltaCumulation;
                        // This is the last index of sigma point to whom the smallest delta cumulation belongs to
                        m_GlobalMinimumCumulativeDeltaIndex = last_sigma_point_index;
                    }
                }

                /* We now need to refresh other sigma points with shortest cumulation distance and their delta values
                 * Also update global distance, if necessary
                 * */
                for (i = 0; i < distances.Length; i++)
                {
                    if (double.IsNaN(m_sigma_points[i].DeltaCumulationDistance) || (distances[i] < m_sigma_points[i].DeltaCumulationDistance))
                    {
                        m_sigma_points[i].SetDeltaCumulativeDistance(distances[i], last_sigma_point_index);
                        if (m_sigma_points[i].DeltaCumulation < m_GlobalMinimumCumulativeDelta)
                        {
                            m_GlobalMinimumCumulativeDelta = m_sigma_points[i].DeltaCumulation;
                            // This is the last index of sigma point to whom the smallest delta cumulation belongs to
                            m_GlobalMinimumCumulativeDeltaIndex = last_sigma_point_index;
                        }
                    }
                }
                return true;
            }

            // Else we need to check sparsity and restrict the replacements
            int index_to_replace = -1;
            if (!IsValidForReplacingExisting(sigma_point, out index_to_replace))
                return false;

            // it is Ok to replace global minimum now
            if (!ReplaceSigmaPoint(index_to_replace, sigma_point))
                return false;

            /* there is no enough sigma points to another sampling try, but this time nearby existing sigma points
             * TODO: this is a todo for the future
             * */

            return true;
        }
        private double m_GlobalMinimumCumulativeDelta = double.MaxValue;
        private int m_GlobalMinimumCumulativeDeltaIndex = -1;
        #endregion

        #region ReplaceSigmaPoint(SigmaPoint sigma_point)
        /// <summary>
        /// Replaces global minimim sigma point with a new sigma point.
        /// </summary>
        /// <param name="new_sigma_point">A new sigma point to replace global minimum with</param>
        /// <param name="destination_index">Index of destination sigma point to be replaced</param>
        /// <returns>True, if successfull</returns>
        public bool ReplaceSigmaPoint(int destination_index, SigmaPoint new_sigma_point)
        {
            // At this stage candidate sigma point's distance is measured to another sigma point than the one at global minimum
            if (!ReplaceSigmaPointAtIndex(destination_index, new_sigma_point))
                return false;

            /* Global index stays the same here.
             * We now need to update the global distance (index stays the same) and the delta cumulation
             * for those sigma points which used to point to global minimum index
             * */
            m_GlobalMinimumCumulativeDelta = double.MaxValue;
            m_GlobalMinimumCumulativeDeltaIndex = -1;
            double distance;
            int i = 0;
            foreach (SigmaPoint any_sigma_point in m_sigma_points)
            {

                /* The following will happen to few instances only,
                 * because the more points, the smaller chance that
                 * multiple ones will point to the same one. 
                 * So this should not cause a huge drop in performance.
                 * */
                // TODO (1st priority): in the future we might have distances in Dictionary, which would speed things up
                any_sigma_point.SetDeltaCumulativeDistance(double.MaxValue, -1);
                int j = 0;
                foreach (SigmaPoint other_point in m_sigma_points)
                {
                    if (i == j)
                    {
                        j++;
                        continue;
                    }
                    distance = SquaredDistance(any_sigma_point, other_point);
                    if (distance < any_sigma_point.DeltaCumulationDistance)
                        any_sigma_point.SetDeltaCumulativeDistance(distance, j);
                    j++;
                }
                if (any_sigma_point.DeltaCumulation < m_GlobalMinimumCumulativeDelta)
                {
                    m_GlobalMinimumCumulativeDelta = any_sigma_point.DeltaCumulation;
                    m_GlobalMinimumCumulativeDeltaIndex = i;
                }
                i++;
            }
            return true;
        }
        #endregion

        #region IsValidForReplacingExisting(SigmaPoint sigma_point)
        /// <summary>
        /// Checks whether a sigma point wins over existing global minimum. 
        /// Also checks that the constraints boundaries are not violated.
        /// </summary>
        /// <param name="sigma_point">A new candidate sigma point to replace global minimum sigma point</param>
        /// <param name="index_to_replace">Index of sigma point to be replaced</param>
        /// <returns>True, if to replace global minimum sigma point</returns>
        public bool IsValidForReplacingExisting(SigmaPoint sigma_point, out int index_to_replace)
        {
            index_to_replace = -1;
            double constraint_value = double.NaN;
            double distance = 0;
            double previous_minimum_distance = double.MaxValue;
            int previous_minimum_distance_index = -1;

            int i = 0;
            List<int> indexes_candidates_for_replacement = new List<int>(m_sigma_points.Count);
            int[] constraint_values_at_new_sigma_point = new int[this.Count];
            foreach (CustomConstraintBase constraint in this)
            {
                if (!constraint.TryGetConstraintValueAt(sigma_point.VectorCoordinates, out constraint_value) /*|| ((constraint_value > -0.001) && (constraint_value < 0.001))*/)
                {
                    return false;
                }
                constraint_values_at_new_sigma_point[i] = Math.Sign(constraint_value);
                i++;
            }
            i = 0;
            bool increases_sparsity_over_constraint = false;
            foreach (SigmaPoint point in m_sigma_points)
            {
                /*bool the_new_wins = true;
                int j = 0;
                foreach (CustomConstraintBase constraint in this)
                {
                    // TODO: to make it faster cach Math.Sign(constraint.Values_at_sigma_points[i])
                    int new_sparsity = 
                        constraint.SparsityNegativeVersusPositive - 
                        Math.Sign(constraint.Values_at_sigma_points[i]) + 
                        constraint_values_at_new_sigma_point[j];
                    if (((new_sparsity >= -2) && (new_sparsity <= 2)) || (new_sparsity == constraint.SparsityNegativeVersusPositive))
                    { 
                        // new wins
                        j++;
                        continue;
                    }

                    if (((new_sparsity < 0) && (new_sparsity > constraint.SparsityNegativeVersusPositive)) ||
                        ((new_sparsity > 0) && (new_sparsity < constraint.SparsityNegativeVersusPositive)))
                    {
                        increases_sparsity_over_constraint = true;
                    }
                    else
                    {
                        the_new_wins = false;
                        break;
                    }
                    j++;
                }
                if (!the_new_wins)
                {
                    i++;
                    continue;
                }*/
                indexes_candidates_for_replacement.Add(i);
                i++;
            }

            // Iterate through potential candidates now
            double minimum_cumulation = double.MaxValue;
            for (i = 0; i < indexes_candidates_for_replacement.Count; i++)
            {
                int index = indexes_candidates_for_replacement[i];
                SigmaPoint measure_distance_to_sigma_point = m_sigma_points[index];

                distance = SquaredDistance(sigma_point, measure_distance_to_sigma_point);
                if (distance < sigma_point.DeltaCumulationDistance)
                {
                    previous_minimum_distance = sigma_point.DeltaCumulationDistance;
                    previous_minimum_distance_index = sigma_point.DeltaCumulationDistanceIndex;
                    sigma_point.SetDeltaCumulativeDistance(distance, index);
                }
                if (minimum_cumulation > measure_distance_to_sigma_point.DeltaCumulation)
                {
                    minimum_cumulation = measure_distance_to_sigma_point.DeltaCumulation;
                    index_to_replace = index;
                }
            }
            if ((sigma_point.DeltaCumulation <= minimum_cumulation) && (!increases_sparsity_over_constraint))
                return false;

            // Now we know the minimum distance, let's find whether we exceed the minimal information gain
            if (m_GlobalMinimumCumulativeDelta == double.MaxValue)
            {
                //TODO: this should never happen
                return false;
            }

            if (sigma_point.DeltaCumulationDistanceIndex == m_GlobalMinimumCumulativeDeltaIndex)
            {
                /* A new minimum distance "competes" with current global minimum
                 * we need to check with the second candidate then.
                 * */
                sigma_point.SetDeltaCumulativeDistance(previous_minimum_distance, previous_minimum_distance_index);
                if (sigma_point.DeltaCumulation <= m_GlobalMinimumCumulativeDelta)
                {
                    // No information gain, so this new instance looses the battle against the existing ones
                    return false;
                }

                // The new one wins over existing one, which appears to be a global minimum
                // return true, to replace the global minimum
            }
            else
            {
                // return true, to replace the global minimum
            }
            return true;
        }
        #endregion

        #region SquaredDistance
        /// <summary>
        /// Returns squared distance between two sigma points
        /// </summary>
        /// <param name="source_sigma_point">A sigma point to measure distance from</param>
        /// <param name="destination_sigma_point">A sigma point to measure distance to</param>
        /// <returns>Squared distance between two sigma points</returns>
        public double SquaredDistance(SigmaPoint source_sigma_point, SigmaPoint destination_sigma_point)
        {
            double distance = 0, dbl;
            for (int i = 0; i < source_sigma_point.VectorCoordinates.Count; i++)
            {
                dbl = source_sigma_point.VectorCoordinates[i] - destination_sigma_point.VectorCoordinates[i];
                distance += (dbl * dbl);
            }
            return distance;
        }
        #endregion

        #endregion

    }

    ///<summary>
    /// Represents a pseudo-random number generator, a device that produces random data.
    ///</summary>
    public class CryptoRandom : RandomNumberGenerator
    {
        private static RandomNumberGenerator r;

        ///<summary>
        /// Creates an instance of the default implementation of a cryptographic random number generator that can be used to generate random data.
        ///</summary>
        public CryptoRandom()
        {
            r = RandomNumberGenerator.Create();
        }

        ///
        /// Fills an array of bytes with a cryptographically strong random sequence of nonzero values.
        ///
        /// The array to fill with cryptographically strong random nonzero bytes
        public override void GetNonZeroBytes(byte[] data)
        {
            r.GetNonZeroBytes(data);
        }

        ///<summary>
        /// Fills the elements of a specified array of bytes with random numbers.
        ///</summary>
        ///<param name="buffer">An array of bytes to contain random numbers.</param>
        public override void GetBytes(byte[] buffer)
        {
            r.GetBytes(buffer);
        }

        ///<summary>
        /// Returns a random number between 0.0 and 1.0.
        ///</summary>
        public double NextDouble()
        {
            byte[] b = new byte[4];
            r.GetBytes(b);
            return (double)BitConverter.ToUInt32(b, 0) / UInt32.MaxValue;
        }

        ///<summary>
        /// Returns a random number within the specified range.
        ///</summary>
        ///<param name="minValue">The inclusive lower bound of the random number returned.</param>
        ///<param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
        public int Next(int minValue, int maxValue)
        {
            int val = (int)Math.Round(NextDouble() * (maxValue - minValue - 1)) + minValue;
            return val;
        }

        ///<summary>
        /// Returns a nonnegative random number.
        ///</summary>
        public int Next()
        {
            return Next(0, Int32.MaxValue);
        }

        ///<summary>
        /// Returns a nonnegative random number less than the specified maximum
        ///</summary>
        ///<param name="maxValue">The inclusive upper bound of the random number returned. maxValue must be greater than or equal 0</param>
        public int Next(int maxValue)
        {
            return Next(0, maxValue);
        }
    }

}

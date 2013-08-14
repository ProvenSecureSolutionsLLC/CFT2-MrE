using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MrE_Engine
{

    #region MrEExampleForSanovTheoremInferenceTask
    /// <summary>
    /// MrE example on how to solve Sanov's teorem using 
    /// Maximum relative Entropy.
    /// See Adom Giffin's paper at
    /// Physica A388 (2009) 1610-1620, titled as
    /// "From physics to economics: An econometric example using Maximum relative entropy"
    /// </summary>
    public class MrEExampleForSanovTheoremInferenceTask : InferenceTask
    {
        #region IsSamplingConstraintSatisfied(SigmaPoint sigma_point)
        /// <summary>
        /// Does current sigma point satisfies the sampling constraint 
        /// </summary>
        /// <param name="sigma_point">Sigma point to validate</param>
        /// <returns>True, if sampling constraint is satisfied. False, when the sampling constraint is violated.</returns>
        public override bool IsSamplingConstraintSatisfied(SigmaPoint sigma_point)
        {
            List<double> vector_cordinates = sigma_point.VectorCoordinates;
            if(vector_cordinates== null || vector_cordinates.Count < 2)
                return false;
            bool is_satisfied = (vector_cordinates[0] + vector_cordinates[1] <= 1);

            return is_satisfied;
        }
        #endregion

    }
    #endregion

    #region MrEExampleForSanovTheoremConstraint
    /// <summary>
    /// An example of variable function implementation
    /// </summary>
    public class MrEExampleForSanovTheoremConstraint : CustomConstraintBase, IVariablesConstraint
    {
        private double m_SetPoint = 0;
        private double m_f1 = 0;
        private double m_f2 = 0;
        private double m_f3 = 0;

        #region MrEExampleForSanovTheoremConstraint()
        /// <summary>
        /// Constructor for MrEExampleForSanovTheoremConstraint
        /// </summary>
        public MrEExampleForSanovTheoremConstraint(double set_point, double f1, double f2, double f3)
            : base()
        {
            m_SetPoint = set_point;
            m_f1 = f1;
            m_f2 = f2;
            m_f3 = f3;
        }
        #endregion

        /// <summary>
        /// A custom TryGetConstraintValueAt implementation.
        /// </summary>
        /// <param name="variables_values">The list of multivariate random vector coordinates</param>
        /// <param name="constraint_value">The constraint value to be returned</param>
        /// <returns>True when successfull</returns>
        public override bool TryGetConstraintValueAt(List<double> variables_values, out double constraint_value)
        {
            constraint_value = 0;
            if (variables_values == null || variables_values.Count < 2)
                return false;

            constraint_value = m_f1 * variables_values[0] + m_f2 * variables_values[1] + m_f3 * (1 - variables_values[0] - variables_values[1]) - m_SetPoint;
            
            return true;
        }
    }
    #endregion

    #region MrEExampleForSanovTheoremConstraint2
    /// <summary>
    /// An example of variable function implementation
    /// </summary>
    public class MrEExampleForSanovTheoremConstraint2 : CustomConstraintBase, IVariablesConstraint
    {
        private double m_SetPoint = 0;

        #region MrEExampleForSanovTheoremConstraint2()
        /// <summary>
        /// Constructor for MrEExampleForSanovTheoremConstraint
        /// </summary>
        public MrEExampleForSanovTheoremConstraint2(double set_point)
            : base()
        {
            m_SetPoint = set_point;
        }
        #endregion

        /// <summary>
        /// A custom TryGetConstraintValueAt implementation.
        /// </summary>
        /// <param name="variables_values">The list of multivariate random vector coordinates</param>
        /// <param name="constraint_value">The constraint value to be returned</param>
        /// <returns>True when successfull</returns>
        public override bool TryGetConstraintValueAt(List<double> variables_values, out double constraint_value)
        {
            constraint_value = 0;
            if (variables_values == null || variables_values.Count < 2)
                return false;

            constraint_value = variables_values[0] + variables_values[1] + variables_values[2] - m_SetPoint;

            return true;
        }
    }
    #endregion

    /// <summary>
    /// MrE example on how to solve Sanov's teorem using 
    /// Maximum relative Entropy.
    /// See Adom Giffin's paper at
    /// Physica A388 (2009) 1610-1620, titled as
    /// "From physics to economics: An econometric example using Maximum relative entropy"
    /// </summary>
    public class MrEExampleForSanovTheoremDistribution: CustomDistributionBase, IProbabilityDistribution
    {
        private int m_m1 = 0;
        private int m_m2 = 0;
        private int m_m3 = 0;

        #region Constructor 
        /// <summary>
        /// Main constructor for our example distribution
        /// </summary>
        public MrEExampleForSanovTheoremDistribution(int m1, int m2, int m3)
            : base()
        {
            m_m1 = m1;
            m_m2 = m2;
            m_m3 = m3;
        }
        #endregion

        #region TryGetProbabilityDensityAt
        /// <summary>
        /// TryGetProbabilityDensityAt returns the probability density function value at a given multivariate data point.
        /// </summary>
        /// <param name="variables_values">The list of multivariate random vector coordinates</param>
        /// <param name="density_value">The probability distribution density value to be returned</param>
        /// <returns>True when successfull</returns>
        public override bool TryGetProbabilityDensityAt(List<double> variables_values, out double density_value)
        {
            density_value = 0;
            if (variables_values == null || variables_values.Count < 2)
                return false;

            density_value =
                Math.Pow(variables_values[0], m_m1) *
                Math.Pow(variables_values[1], m_m2) *
                Math.Pow((1 - variables_values[0] - variables_values[1]), m_m3);
            return true;
        }
        #endregion
    }
}

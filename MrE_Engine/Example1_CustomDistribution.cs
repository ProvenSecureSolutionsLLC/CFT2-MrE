using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MrE_Engine
{
    #region TestDistribution
    /// <summary>
    /// An example of probability distribution function implementation
    /// </summary>
    public class Example1_CustomDistribution : CustomDistributionBase, IProbabilityDistribution
    {
        #region Constructor 
        /// <summary>
        /// Main constructor for our example distribution
        /// </summary>
        public Example1_CustomDistribution()
            : base()
        {
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
            density_value = 1;
            return true;
        }
        #endregion
    }
    #endregion

    #region Example1_CustomConstraint
    /// <summary>
    /// An example of variable function implementation
    /// </summary>
    public class Example1_CustomConstraint : CustomConstraintBase, IVariablesConstraint
    {

        #region Example1_CustomConstraint()
        /// <summary>
        /// Constructor for Example1_CustomConstraint
        /// </summary>
        public Example1_CustomConstraint()
            :base()
        {
        }
        #endregion

        /// <summary>
        /// A custom TryGetConstraintValueAt implementation.
        /// x^2+y^2+z^2+...-40*40=0
        /// </summary>
        /// <param name="variables_values">The list of multivariate random vector coordinates</param>
        /// <param name="constraint_value">The constraint value to be returned</param>
        /// <returns>True when successfull</returns>
        public override bool TryGetConstraintValueAt(List<double> variables_values, out double constraint_value)
        {
            constraint_value = (Math.Pow(variables_values[0], 2)-40*40)/100;
            
            return true;
        }
    }
    #endregion
}

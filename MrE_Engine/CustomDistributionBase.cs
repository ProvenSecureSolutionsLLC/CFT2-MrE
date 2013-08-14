using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MrE_Engine
{
    /// <summary>
    /// A base class for Probability distribution class
    /// </summary>
    public class CustomDistributionBase: IProbabilityDistribution
    {
        #region CustomDistributionBase
        /// <summary>
        /// Constructor for CustomDistributionBase
        /// </summary>
        public CustomDistributionBase()
        {
        }
        #endregion

        #region IVariablesConstraint.TryGetProbabilityDensityAt
        bool IProbabilityDistribution.TryGetProbabilityDensityAt(List<double> variables_values, out double density_value)
        {
            return TryGetProbabilityDensityAt(variables_values, out density_value);
        }
        #endregion

        #region TryGetProbabilityDensityAt
        /// <summary>
        /// TryGetProbabilityDensityAt returns the probability density function value at a given multivariate data point.
        /// </summary>
        /// <param name="variables_values">The list of multivariate random vector coordinates</param>
        /// <param name="density_value">The probability distribution density value to be returned</param>
        /// <returns>True when successfull</returns>
        public virtual bool TryGetProbabilityDensityAt(List<double> variables_values, out double density_value)
        {
            density_value = 1;
            return true;
        }
        #endregion
    }
}

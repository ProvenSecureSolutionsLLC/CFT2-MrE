using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MrE_Engine
{
    /// <summary>
    /// Interface IProbabilityDistribution
    /// </summary>
    public interface IProbabilityDistribution
    {
        #region TryGetProbabilityDensityAt(List<double> variables_values, out double constraint_value)
        /// <summary>
        /// TryGetProbabilityDensityAt method
        /// </summary>
        /// <param name="variables_values">The list of multivariate random vector coordinates</param>
        /// <param name="density_value">The probability density value to be returned</param>
        /// <returns>True when successfull</returns>
        bool TryGetProbabilityDensityAt(List<double> variables_values, out double density_value);
        #endregion

    }
}

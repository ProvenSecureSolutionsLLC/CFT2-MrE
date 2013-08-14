using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MrE_Engine
{

    /// <summary>
    /// A class for Gaussian distribution
    /// </summary>
    public class DistributionGaussian : CustomDistributionBase, IProbabilityDistribution
    {
        private double m_Mean;
        private double m_StandardDeviation;

        #region Constructor 
        /// <summary>
        /// Main constructor for our example distribution
        /// </summary>
        public DistributionGaussian(double mean, double standard_deviation)
            : base()
        {
            m_Mean = mean;
            m_StandardDeviation = standard_deviation;
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
            if(variables_values==null || variables_values.Count == 0)
                return false;
                
            double diff_mean = m_Mean-variables_values[0];
            double variance = m_StandardDeviation * m_StandardDeviation;
            density_value = Math.Exp(-(diff_mean * diff_mean) / (2 * variance)) / (m_StandardDeviation * MrEHelper.Sqrt2Pi);
            return true;
        }
        #endregion
    }
}

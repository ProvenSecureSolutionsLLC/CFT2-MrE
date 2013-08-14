using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MrE_Engine
{
    /// <summary>
    /// An MrE example class for any covariance constraint between two variables
    /// </summary>
    public class MrEExample10VariateGaussianCovarianceConstraint : CustomConstraintBase, IVariablesConstraint
    {
        private int m_source_variable_index = -1;
        private int m_destination_variable_index = -1;
        private double m_source_variable_mean = 0;
        private double m_destination_variable_mean = 0;
        private double m_covariance_value = 0; // default is no covariance

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source_variable_index">Index of the first variable</param>
        /// <param name="destination_variable_index">Index of the second variable</param>
        /// /// <param name="source_variable_mean">Mean of the first variable</param>
        /// <param name="destination_variable_mean">Mean of the second variable</param>
        /// <param name="covariance_value">Actual covariance value bwteeen variables</param>
        public MrEExample10VariateGaussianCovarianceConstraint(
            int source_variable_index,
            int destination_variable_index,
            double source_variable_mean,
            double destination_variable_mean,
            double covariance_value)
        {
            m_source_variable_index = source_variable_index;
            m_destination_variable_index = destination_variable_index;
            m_source_variable_mean = source_variable_mean;
            m_destination_variable_mean = destination_variable_mean;
            m_covariance_value = covariance_value;
        }
        #endregion

        #region TryGetConstraintValueAt
        /// <summary>
        /// A custom TryGetConstraintValueAt implementation.
        /// </summary>
        /// <param name="variables_values">The list of multivariate random vector coordinates</param>
        /// <param name="constraint_value">The constraint value to be returned</param>
        /// <returns>True when successfull</returns>
        public override bool TryGetConstraintValueAt(List<double> variables_values, out double constraint_value)
        {
            constraint_value = 0;
            if (variables_values == null ||
                m_source_variable_index < 0 ||
                m_source_variable_index >= variables_values.Count ||
                m_destination_variable_index < 0 ||
                m_destination_variable_index >= variables_values.Count)
                return false;

            constraint_value = 
                ((variables_values[m_source_variable_index] - m_source_variable_mean) * 
                (variables_values[m_destination_variable_index] - m_destination_variable_mean) - 
                m_covariance_value)/1000;
            return true;
        }
        #endregion

    }

    
}

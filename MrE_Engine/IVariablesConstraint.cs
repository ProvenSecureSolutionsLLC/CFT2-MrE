using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MrE_Engine
{
    /// <summary>
    /// IvariablesConstraint interface
    /// </summary>
    public interface IVariablesConstraint
    {
        #region TryGetConstraintValueAt(List<double> variables_values, out double constraint_value)
        /// <summary>
        /// TryGetConstraintValueAt method
        /// </summary>
        /// <param name="variables_values">The list of multivariate random vector coordinates</param>
        /// <param name="constraint_value">The constraint value to be returned</param>
        /// <returns>True when successfull</returns>
        bool TryGetConstraintValueAt(List<double> variables_values, out double constraint_value);
        #endregion

    }
}

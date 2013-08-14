using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MrE_Engine
{
    /// <summary>
    /// Interface for a random variable
    /// </summary>
    public interface IVariable
    {
        #region GetRandomSample()
        /// <summary>
        /// Gets random sample
        /// </summary>
        /// <returns>A random value of a random variable</returns>
        double GetRandomSample();
        #endregion
    }
}

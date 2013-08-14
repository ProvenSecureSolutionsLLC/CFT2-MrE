using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MrE_Engine
{
    /// <summary>
    /// A class for CustomVariableBase
    /// </summary>
    public class CustomVariableBase: IVariable
    {
        #region PROPERTIES

        #region MinValue
        /// <summary>
        /// Property MinValue
        /// </summary>
        public double MinValue
        {
            get
            {
                return m_MinValue;
            }
            set
            {
                m_MinValue = value;
                // TODO: when changing boundaries we should adjust stdDeviation.
            }
        }
        private double m_MinValue = -1000;
        #endregion

        #region MaxValue
        /// <summary>
        /// Property MaxValue
        /// </summary>
        public double MaxValue
        {
            get
            {
                return m_MaxValue;
            }
            set
            {
                m_MaxValue = value;
                // TODO: when changing boundaries we should adjust stdDeviation.
            }
        }
        private double m_MaxValue = 1000;
        #endregion

        #region StandardDeviation
        /// <summary>
        /// Property StandardDeviation.
        /// Standard deviation of Normal distribution to sample variable values from.
        /// </summary>
        public double StandardDeviation
        {
            get
            {
                return m_StandardDeviation;
            }
            set
            {
                m_StandardDeviation = value;
                // TODO: when changing StdDeviation we should adjust boundaries.
            }
        }
        private double m_StandardDeviation = 200;
        #endregion

        #region Mean
        /// <summary>
        /// Property Mean
        /// The mean of Normal distribution to sample variable values from.
        /// </summary>
        public double Mean
        {
            get
            {
                return m_Mean;
            }
            set
            {
                m_Mean = value;
            }
        }
        private double m_Mean = 0;
        #endregion
        
        #endregion

        #region METHODS

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public CustomVariableBase()
        {
        }
        #endregion

        #region IVariable.GetRandomSample()
        /// <summary>
        /// Gets samples of the class instance
        /// </summary>
        double IVariable.GetRandomSample()
        {
            return GetRandomSample();
        }
        #endregion

        #region GetRandomSample()
        /// <summary>
        /// Gets random sample according to Normal distribution and 
        /// value range boundaries taken into account
        /// </summary>
        /// <returns>A random value of a random variable</returns>
        public virtual double GetRandomSample()
        {
            double random_sample = MrEHelper.GetNormal(m_Mean, m_StandardDeviation);
            if(random_sample<m_MinValue)
                 random_sample = MinValue;
            else if (random_sample>m_MaxValue)
                random_sample = m_MaxValue;
            return random_sample;
        }

        #endregion

        #endregion
    }
}

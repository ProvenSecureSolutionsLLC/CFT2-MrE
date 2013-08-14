using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MrE_Engine
{
    /// <summary>
    /// A base class for MrE constraint function equation
    /// </summary>
    public class CustomConstraintBase: IVariablesConstraint
    {
        //private SigmaPointCollection m_sigma_points = null;

        #region PROPERTIES

        #region SparsityNegativeVersusPositive property
        /// <summary>
        /// Sparsity over constraint function setpoint which is assumed 0 in MrE.
        /// Every negative constraint value decrements this property.
        /// Every positive constraint value increments this property.
        /// </summary>
        public int SparsityNegativeVersusPositive
        {
            get
            {
                return m_SparsityNegativeVersusPositive;
            }
        }
        private int m_SparsityNegativeVersusPositive = 0;
        #endregion

        #region Values_at_variables property
        /// <summary>
        /// Constraint value at sigma points
        /// </summary>
        public List<double> Values_at_sigma_points
        {
            get
            {
                return m_Values_at_sigma_points;
            }
        }
        private List<double> m_Values_at_sigma_points = new List<double>(MrEHelper.MAXNUMBEROFSIGMAPOINTS);
        #endregion

        #region IsValid
        /// <summary>
        /// Is this constraint valid based on existing sigma points?
        /// </summary>
        public bool IsValid
        {
            get
            {
                return m_IsValid;
            }
        }
        private bool m_IsValid = true;
        #endregion

        #region LagrangeMultiplierValue
        /// <summary>
        /// After MrE engine execution this property is filled with
        /// optimal Lagrange multiplier values, which maximize 
        /// Maximum relative Entropy using the constraints
        /// Default value is zero, which means that associated constraint has no effect.
        /// </summary>
        public double LagrangeMultiplierValue
        {
            get
            {
                return m_LagrangeMultiplierValue;
            }
            set
            {
                m_LagrangeMultiplierValue = value;
            }
        }
        private double m_LagrangeMultiplierValue = 0;
        #endregion

        #endregion

        #region METHODS
        
        #region CustomConstraintBase(SigmaPointCollection sigma_points)
        /// <summary>
        /// Constructor for CustomConstraintBase
        /// </summary>
        public CustomConstraintBase()
        {
        }
        #endregion
                
        #region TryGetConstraintValueAt
        /// <summary>
        /// TryGetConstraintValueAt returns the constraint function value at a given multivariate data point.
        /// The default return value in this base class is zero (0).
        /// MrE engine assumes that all constraints are equal to zeros, 
        /// i.e. normalized so that right side of the constraining function equation is zero (0).
        /// E.g. x+2*y-3=0 is a normalized form, while the same constraint's form
        /// x+2*y=0 is un-normalized and is not supported.
        /// </summary>
        /// <param name="variables_values">The list of multivariate random vector coordinates</param>
        /// <param name="constraint_value">The constraint value to be returned</param>
        /// <returns>True when successfull</returns>
        bool IVariablesConstraint.TryGetConstraintValueAt(List<double> variables_values, out double constraint_value)
        {
            return TryGetConstraintValueAt(variables_values, out constraint_value);
        }
        #endregion

        #region TryGetConstraintValueAt
        /// <summary>
        /// TryGetConstraintValueAt returns the constraint function value at a given multivariate data point.
        /// The default return value in this base class is zero (0).
        /// MrE engine assumes that all constraints are equal to zeros, 
        /// i.e. normalized so that right side of the constraining function equation is zero (0).
        /// E.g. x+2*y-3=0 is a normalized form, while the same constraint's form
        /// x+2*y=0 is un-normalized and is not supported.
        /// </summary>
        /// <param name="variables_values">The list of multivariate random vector coordinates</param>
        /// <param name="constraint_value">The constraint value to be returned</param>
        /// <returns>True when successfull</returns>
        public virtual bool TryGetConstraintValueAt(List<double> variables_values, out double constraint_value)
        {
            constraint_value = 0;
            return true;
        }
        #endregion

        #region ReplaceConstraintValueAtExisting(int index, double constraint_value)
        /// <summary>
        /// Replace existing cache with new information
        /// </summary>
        /// <param name="index">Index of existing sigma point item</param>
        /// <param name="constraint_value">A constraint value to replace with</param>
        public void ReplaceConstraintValueAtExisting(int index, double constraint_value)
        {
            int existing_sign = Math.Sign(m_Values_at_sigma_points[index]);
            int new_sign = Math.Sign(constraint_value);
            if (existing_sign != new_sign)
            {
                if (existing_sign < 0)
                    m_SparsityNegativeVersusPositive += 2;
                else if (existing_sign > 0)
                    m_SparsityNegativeVersusPositive -= 2;
            }
            m_Values_at_sigma_points[index] = constraint_value;
            /*Double check the sparsity
            // Assertion test

            int sum = 0;
            for (int i = 0; i < m_Values_at_sigma_points.Count; i++)
            {
                sum += Math.Sign(m_Values_at_sigma_points[i]);
            }
            if (sum != m_SparsityNegativeVersusPositive)
            {
                string s = "bad";
                s += "";
            }*/
        }
        #endregion


        #region AppendConstraintValueAtNewSigmaPoint
        /// <summary>
        /// Appends a new constraint value at a new sigma point
        /// </summary>
        /// <param name="constraint_value">A constraint value to append</param>
        public void AppendConstraintValueAtNewSigmaPoint(double constraint_value)
        {
            m_Values_at_sigma_points.Add(constraint_value);
            if (constraint_value < 0)
                m_SparsityNegativeVersusPositive--;
            else if(constraint_value > 0)
                m_SparsityNegativeVersusPositive++;
            // do nothing if zero
        }
        #endregion

        #region AppendSigmaPoint(SigmaPoint sigma_point)
        /// <summary>
        /// Appends an information for a new sigma point. 
        /// Recalculates constraints values.
        /// </summary>
        /// <param name="sigma_point">An instance of a new sigma point</param>
        /// <param name="constraint_value">The constraint value to be returned</param>
        /// <returns>True when successfull, else at least one constraint value was NaN</returns>
        public bool AppendSigmaPoint(SigmaPoint sigma_point, out double constraint_value)
        {
            constraint_value = double.NaN;
            if (!TryGetConstraintValueAt(sigma_point.VectorCoordinates, out constraint_value))
                return false;
            AppendConstraintValueAtNewSigmaPoint(constraint_value);            
            return true;
        }
        #endregion

        #endregion
    }
}

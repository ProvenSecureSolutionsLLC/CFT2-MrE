using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace MrE_Engine
{
    /// <summary>
    /// A class for a single inference task
    /// </summary>
    public class InferenceTask
    {
        private List<CustomVariableBase> m_Variables = new List<CustomVariableBase>(MrEHelper.MAXNUMBEROFVARIABLES);
        
        #region PROPERTIES

        #region ExpEvidenceScale Property
        /// <summary>
        /// This is evidence scale coefficient k, whose purpose is
        /// to keep the evidence under the double precision.
        ///Default is zero, which means no scale. The value of k, means that
        ///the evidence value is multiplied by Exp[k*100].
        ///Can be negative (for too big evidence value) or positive
        ///for too small evidence numbers).
        /// </summary>
        public double ExpEvidenceScale
        {
            get
            {
                return m_ExpEvidenceScale;
            }
            set
            {
                m_ExpEvidenceScale = value;
            }
        }
        private double m_ExpEvidenceScale = 0;
        #endregion

        #region EnforcePositiveLagrangeMultipliers Property
        /// <summary>
        /// Property EnforcePositiveLagrangeMultipliers
        /// 
        /// </summary>
        public bool EnforcePositiveLagrangeMultipliers
        {
            get
            {
                return m_EnforcePositiveLagrangeMultipliers;
            }
            set
            {
                m_EnforcePositiveLagrangeMultipliers = value;
            }
        }
        private bool m_EnforcePositiveLagrangeMultipliers = false;
        #endregion

        #region MaxShiftsOfSigmaPoints Property
        /// <summary>
        /// Property MaxShiftsOfSigmaPoints
        /// 
        /// </summary>
        public int MaxShiftsOfSigmaPoints
        {
            get
            {
                return m_MaxShiftsOfSigmaPoints;
            }
        }
        private int m_MaxShiftsOfSigmaPoints = 0;
        #endregion

        #region LastIterationOfLagrangeMultipliersShifts Property
        /// <summary>
        /// Property LastIterationOfLagrangeMultipliersShifts
        /// 
        /// </summary>
        public int LastIterationOfLagrangeMultipliersShifts
        {
            get
            {
                return m_LastIterationOfLagrangeMultipliersShifts;
            }
        }
        private int m_LastIterationOfLagrangeMultipliersShifts = 0;
        #endregion

        #region VariableConstraints Property
        /// <summary>
        /// Property VariableConstraints
        /// 
        /// </summary>
        public ConstraintsCollection VariableConstraints
        {
            get
            {
                return m_VariableConstraints;
            }
        }
        private ConstraintsCollection m_VariableConstraints = null;
        #endregion

        #region SigmaPoints Property
        /// <summary>
        /// Property SigmaPoints
        /// 
        /// </summary>
        public SigmaPointCollection SigmaPoints
        {
            get
            {
                return m_SigmaPoints;
            }
        }
        private SigmaPointCollection m_SigmaPoints = new SigmaPointCollection();
        #endregion

        #region JointPriorDistribution Property
        /// <summary>
        /// Property JointPriorDistribution
        /// 
        /// </summary>
        public IProbabilityDistribution JointPriorDistribution
        {
            get
            {
                return m_JointPriorDistribution;
            }
            set
            {
                m_JointPriorDistribution = value;
                m_VariableConstraints.JointPriorDistribution = value;
            }
        }
        private IProbabilityDistribution m_JointPriorDistribution = null;
        #endregion


        #region PrecisionOfLagrangeMultiplierAtZeroValue Property
        /// <summary>
        /// Property PrecisionOfLagrangeMultiplierAtZeroValue
        /// This defines a smallest step starting whe n iteration process starts 
        /// from zero value of Lagrange multiplier.
        /// </summary>
        public double PrecisionOfLagrangeMultiplierAtZeroValue
        {
            get
            {
                return m_PrecisionOfLagrangeMultiplierAtZeroValue;
            }
            set
            {
                m_PrecisionOfLagrangeMultiplierAtZeroValue = value;
            }
        }
        private double m_PrecisionOfLagrangeMultiplierAtZeroValue = 0.000001;
        #endregion

        #endregion

        #region METHODS

        #region InferenceTask
        /// <summary>
        /// Constructor
        /// </summary>
        public InferenceTask()
        {
            m_JointPriorDistribution = (IProbabilityDistribution)(new DistributionUniform());
            m_VariableConstraints = new ConstraintsCollection(m_SigmaPoints, m_Variables, m_JointPriorDistribution, this);
        }
        #endregion

        #region PopulateRandomSigmaPoint
        /// <summary>
        /// Gets a random sigma point and populates the sigma points collection
        /// </summary>
        /// <returns>True when successfull</returns>
        public bool PopulateRandomSigmaPoint()
        {
            SigmaPoint sigma_point = null;
            if(!m_VariableConstraints.GenerateRandomSigmaPoint(out sigma_point))
                return false;

            return true;
        }
        #endregion

        #region IsSamplingConstraintSatisfied
        /// <summary>
        /// Does current sigma point satisfies the sampling constraint 
        /// </summary>
        /// <returns>True, if sampling constraint is satisfied. False, when the sampling constraint is violated.</returns>
        public virtual bool IsSamplingConstraintSatisfied(SigmaPoint sigma_point)
        {
            return true;
        }
        #endregion

        #region GetEvidenceAtLagrangeMultiplierChange
        /// <summary>
        /// Gets a new scaled evidence value after a certain Lagrange multiplier
        /// is updated with a new value.
        /// </summary>
        /// <param name="multipliers">Array of multipliers to gather evidence over</param>
        /// <param name="index_to_check">Index of the multiplier to check</param>
        /// <param name="value_to_check">A new value for the multiplier to check</param>
        /// <param name="exp_evidence_scale">A new exponential scale for evidence candidate value</param>
        /// <returns>A new evidence value. If no Lagrange multipliers or sigma points exist, returns 1</returns>
        public double GetEvidenceAtLagrangeMultiplierChange(
            double[] multipliers, int index_to_check, double value_to_check,
            out double exp_evidence_scale)
        {
            exp_evidence_scale = 0;
            double sum_TotalCummulation = 0;
            for (int indexSigmaPoint = 0; indexSigmaPoint < m_SigmaPoints.Count; indexSigmaPoint++)
            {
                SigmaPoint sigma_point = m_SigmaPoints[indexSigmaPoint];
                double sum_Beta_mult_Constr = 0;
                for (int i = 0; i < multipliers.Length; i++)
                {
                    double current_value = multipliers[i];
                    if (i == index_to_check)
                        current_value = value_to_check;
                    sum_Beta_mult_Constr += current_value * m_VariableConstraints[i].Values_at_sigma_points[indexSigmaPoint];
                }
                /*if (indexSigmaPoint != 0)
                {
                    if (sum_Beta_mult_Constr - exp_evidence_scale * 100 > 100)                    
                    {
                        double exp_scale = Math.Round(sum_Beta_mult_Constr / 100);
                        if (exp_scale > exp_evidence_scale)
                        {
                            sum_TotalCummulation = sum_TotalCummulation * Math.Exp((exp_evidence_scale - exp_scale) * 100);
                            exp_evidence_scale = exp_scale;
                        }
                    }
                }
                else
                {
                 * */
                if(indexSigmaPoint==0)
                {
                    // here sum_TotalCummulation == 0 and exp_evidence_scale == 0
                    if (sum_Beta_mult_Constr < -100)
                    {
                        double exp_scale = Math.Round(sum_Beta_mult_Constr / 100);
                        // First time here
                        exp_evidence_scale = exp_scale;                        
                    }
                }
                sum_TotalCummulation += Math.Exp(sum_Beta_mult_Constr - exp_evidence_scale * 100) * sigma_point.DensityValue;
            }
            if (sum_TotalCummulation == 0)
                return 1;
            sum_TotalCummulation /= m_SigmaPoints.Count;
            return sum_TotalCummulation;
        }
        #endregion

        #region DoPrecisionSamplingOverLagrangeMultipliers
        /// <summary>
        /// Samples over Lagrange multipliers
        /// </summary>
        /// <param name="multipliers"></param>
        /// <param name="ratio_for_range"></param>
        /// <param name="maximum_iterations"></param>
        /// <param name="best_evidence_value"></param>
        /// <param name="first_time"></param>
        public void DoPrecisionSamplingOverLagrangeMultipliers(
            ref double[] multipliers, 
            double ratio_for_range,
            int maximum_iterations,
            ref double best_evidence_value,
            bool first_time)
        {   
            
            for (int m = 1; m <= maximum_iterations; m++)
            {

                double[] constraint_means = new double[m_VariableConstraints.Count];
                bool at_least_one_multiplier_changed = false;

                for (int j = 0; j < multipliers.Length; j++)
                {
                    /* We start iterations
                     * we try another value by advancing and 
                     * going backward. If nothing wins, then 
                     * we leave the current value as is
                     * */
                    double new_guess = multipliers[j];
                    if ((first_time) && (new_guess == 0))
                        new_guess = m_PrecisionOfLagrangeMultiplierAtZeroValue;
                    else
                        new_guess = multipliers[j] * ratio_for_range;
                    double evidence_scale = 0;
                    double sum_TotalCummulation = GetEvidenceAtLagrangeMultiplierChange(multipliers, j, new_guess, out evidence_scale);
                    if (best_evidence_value > sum_TotalCummulation * Math.Exp((evidence_scale - m_ExpEvidenceScale)*100))
                    {
                        if (sum_TotalCummulation == 0)
                        {
                            string s = "bad";
                            s += "";
                        }
                        multipliers[j] = new_guess;
                        best_evidence_value = sum_TotalCummulation;
                        m_ExpEvidenceScale = evidence_scale;
                        at_least_one_multiplier_changed = true;
                    }
                    else
                    {
                        new_guess = multipliers[j];
                        if ((first_time) && (new_guess == 0))
                            new_guess = -m_PrecisionOfLagrangeMultiplierAtZeroValue;
                        else
                        {
                            new_guess = multipliers[j] / ratio_for_range;
                            if ((first_time) && (Math.Abs(new_guess) < m_PrecisionOfLagrangeMultiplierAtZeroValue))
                                new_guess = 0;   
                        }
                        sum_TotalCummulation = GetEvidenceAtLagrangeMultiplierChange(multipliers, j, new_guess, out evidence_scale);
                        
                        if (best_evidence_value > sum_TotalCummulation * Math.Exp((evidence_scale - m_ExpEvidenceScale) * 100))
                        {
                            if (sum_TotalCummulation == 0)
                            {
                                string s = "bad";
                                s += "";
                            }
                            multipliers[j] = new_guess;
                            best_evidence_value = sum_TotalCummulation;
                            m_ExpEvidenceScale = evidence_scale;
                            at_least_one_multiplier_changed = true;
                        }
                    }
                }
                if (!at_least_one_multiplier_changed)
                {
                    if (m > m_LastIterationOfLagrangeMultipliersShifts)
                        m_LastIterationOfLagrangeMultipliersShifts = m;
                    break;
                }
            }
        }
        #endregion

        #region Execute
        /// <summary>
        /// Execute this task
        /// </summary>
        /// <returns>True when successfull</returns>
        public bool Execute()
        {
            bool result = false;
            //double density_value = 0;
            if (m_Variables == null || m_Variables.Count == 0)
                return result;

            // First we need to collect sigma points
            int i = 0;

            m_MaxShiftsOfSigmaPoints = 0;
            m_LastIterationOfLagrangeMultipliersShifts = 0;

            m_VariableConstraints.TryGenerateUniformSigmaPoints(MrEHelper.MAXNUMBEROFSIGMAPOINTS);

            double[] best_lagrange_multipliers_values = new double[m_VariableConstraints.Count];
            double[] best_constraint_means = new double[m_VariableConstraints.Count];
            double best_evidence_value = double.MaxValue;
            // All constraints' mean must be zeros by definition in MrE Engine

            /* The expected maximum iterations is:
             * 100*(max constraints=20)^2*MaxSigmapoints(=1000) in total=40milions
             * */

            int MAX_ITERATIONS_LAGRANGEMULTIPLIERS = 500; //1000;
            DoPrecisionSamplingOverLagrangeMultipliers(
                ref best_lagrange_multipliers_values, 
                MrEHelper.GOLDENRATIO, 
                //1.8,
                MAX_ITERATIONS_LAGRANGEMULTIPLIERS, 
                ref best_evidence_value,
                true);
            DoPrecisionSamplingOverLagrangeMultipliers(
                ref best_lagrange_multipliers_values,
                1.1,
                MAX_ITERATIONS_LAGRANGEMULTIPLIERS,
                ref best_evidence_value,
                false);
            DoPrecisionSamplingOverLagrangeMultipliers(
                ref best_lagrange_multipliers_values,
                1.01,
                MAX_ITERATIONS_LAGRANGEMULTIPLIERS,
                ref best_evidence_value,
                false);
            /*DoPrecisionSamplingOverLagrangeMultipliers(
                ref best_lagrange_multipliers_values,
                1.001,
                MAX_ITERATIONS_LAGRANGEMULTIPLIERS,
                ref best_evidence_value,
                false);*/
            

            /*double min_Lagrange_value = -1000;
            double max_Lagrange_value = 1000;
            if (!double.IsNaN(m_MaxLagrangeMultiplierValue))
                max_Lagrange_value = m_MaxLagrangeMultiplierValue;
            if (!double.IsNaN(m_MinLagrangeMultiplierValue))
                min_Lagrange_value = m_MinLagrangeMultiplierValue;

                        
            // We always start from zeros
            double[] lagrange_multipliers_values = new double[m_VariableConstraints.Count];
            for (i = 0; i < lagrange_multipliers_values.Length; i++)
            {
                // initially all Lagrange multipliers' values are zeros
                lagrange_multipliers_values[i] = 0;
            }

            for (int m = 1; m <= MAX_ITERATIONS_LAGRANGEMULTIPLIERS; m++)
            {

                double[] constraint_means = new double[m_VariableConstraints.Count];
                bool at_least_one_multiplier_changed = false;

                for (int j = 0; j < lagrange_multipliers_values.Length; j++)
                {
                    double new_guess = lagrange_multipliers_values[j];
                    if (new_guess == 0)
                        new_guess = m_PrecisionOfLagrangeMultiplierAtZeroValue;
                    else
                        new_guess = lagrange_multipliers_values[j] * MrEHelper.GOLDENRATIO;

                    double sum_TotalCummulation = GetEvidenceAtLagrangeMultiplierChange(lagrange_multipliers_values, j, new_guess);

                    if (best_evidence_value > sum_TotalCummulation)
                    {
                        lagrange_multipliers_values[j] = new_guess;
                        best_constraint_means = constraint_means;
                        best_lagrange_multipliers_values = lagrange_multipliers_values;
                        best_evidence_value = sum_TotalCummulation;
                        at_least_one_multiplier_changed = true;
                    }
                    else
                    {
                        new_guess = lagrange_multipliers_values[j];
                        if (new_guess == 0)
                            new_guess = -m_PrecisionOfLagrangeMultiplierAtZeroValue;
                        else
                        {
                            new_guess = lagrange_multipliers_values[j] / MrEHelper.GOLDENRATIO;
                            if (Math.Abs(new_guess) < m_PrecisionOfLagrangeMultiplierAtZeroValue)
                                new_guess = 0;
                        }
                        sum_TotalCummulation = GetEvidenceAtLagrangeMultiplierChange(lagrange_multipliers_values, j, new_guess);

                        if (best_evidence_value > sum_TotalCummulation)
                        {
                            lagrange_multipliers_values[j] = new_guess;
                            best_constraint_means = constraint_means;
                            best_lagrange_multipliers_values = lagrange_multipliers_values;
                            best_evidence_value = sum_TotalCummulation;
                            at_least_one_multiplier_changed = true;
                        }

                    }                    
                }
                if (!at_least_one_multiplier_changed)
                {
                    m_LastIterationOfLagrangeMultipliersShifts = m;
                    break;
                }
            }
            */
            /* Here we got the range already of where Lagrange multipliers are located and their confidence regions.
             * Now we need to iterate once more to get their accuracy better, i.e. make significant digits more precise
             * */


            if (best_evidence_value == double.MaxValue)
                return false;

            // Fill in the constraints for Lagrange multipliers
            i = 0;
            foreach (CustomConstraintBase constraint in m_VariableConstraints)
            {
                constraint.LagrangeMultiplierValue = best_lagrange_multipliers_values[i];
                i++;
            }
            return true;
        }
        #endregion

        #region AddVariable(CustomVariableBase variable)
        /// <summary>
        /// Adds a new random variable instance to the internal collection
        /// </summary>
        /// <param name="variable">A new instance of random variable</param>
        public void AddVariable(CustomVariableBase variable)
        {
            m_Variables.Add(variable);
        }
        #endregion

        #region AddVariableConstraint(IVariablesConstraint constraint)
        /// <summary>
        /// Adds a new constraint function to the internal collection and 
        /// updates cache information on existing sigma points
        /// </summary>
        /// <param name="new_constraint">A new instance of constraint function</param>
        /// <returns>True when successfull</returns>
        public bool AddVariableConstraint(CustomConstraintBase new_constraint)
        {
            return m_VariableConstraints.AppendNewConstraint(new_constraint);
        }
        #endregion

        #endregion
    }
}

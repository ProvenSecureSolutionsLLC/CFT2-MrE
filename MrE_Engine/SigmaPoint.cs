using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MrE_Engine
{
    /// <summary>
    /// SigmaPoint class stores information for a Sigma point information.
    /// The title is derived from the Unscented Kalman Filter, where it has a similar meaning.
    /// </summary>
    public class SigmaPoint
    {
        #region PROPERTIES

        #region VectorCoordinates
        /// <summary>
        /// Sigma point is really nothing more as a multivariate/multidimensional vector,
        /// which is defined by its coordinates in each random variable axis.
        /// </summary>
        public List<double> VectorCoordinates
        {
            get
            {
                return m_VectorCoordinates;
            }
            set
            {
                m_VectorCoordinates = value;
            }
        }
        private List<double> m_VectorCoordinates = new List<double>(MrEHelper.MAXNUMBEROFVARIABLES);
        #endregion

        #region DensityValue
        /// <summary>
        /// Probability density value at this Sigma point
        /// </summary>
        public double DensityValue
        {
            get
            {
                return m_DensityValue;
            }
            set
            {
                m_DeltaCumulation = double.NaN;
                m_DensityValue = value;
            }
        }
        private double m_DensityValue = double.NaN;
        #endregion

        #region DeltaCumulation
        /// <summary>
        /// Probability cumulative distribution delta produced by the sigma point
        /// when compared to the shortest other sigma point
        /// </summary>
        public double DeltaCumulation
        {
            get
            {
                if (double.IsNaN(m_DeltaCumulation))
                {
                    m_DeltaCumulation = m_DensityValue * Math.Pow(m_DeltaCumulationDistance, 200);
                }
                if (m_DeltaCumulationDistance == double.MaxValue)
                    return double.MaxValue;
                return m_DeltaCumulation;
            }
        }
        private double m_DeltaCumulation = double.NaN;
        #endregion

        #region DeltaCumulationDistance
        /// <summary>
        /// The distance to the closest neighboring sigma point.
        /// </summary>
        public double DeltaCumulationDistance
        {
            get
            {
                return m_DeltaCumulationDistance;
            }
        }
        private double m_DeltaCumulationDistance = double.MaxValue;
        #endregion

        #region DeltaCumulationDistanceIndex
        /// <summary>
        /// The index to the closest neighboring sigma point.
        /// </summary>
        public int DeltaCumulationDistanceIndex
        {
            get
            {
                return m_DeltaCumulationDistanceIndex;
            }
        }
        private int m_DeltaCumulationDistanceIndex = -1;
        #endregion

        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delta_cumulation_distance"></param>
        /// <param name="delta_cumulation_distance_index"></param>
        public void SetDeltaCumulativeDistance(double delta_cumulation_distance, int delta_cumulation_distance_index)
        {
            m_DeltaCumulation = double.NaN;
            m_DeltaCumulationDistance = delta_cumulation_distance;
            m_DeltaCumulationDistanceIndex = delta_cumulation_distance_index;
        }

        #region Constructor (default)
        /// <summary>
        /// Constructor
        /// </summary>
        public SigmaPoint()
        {
        }
        #endregion
        
        #region Constructor SigmaPoint(List<double> vector_coordinates, List<double> constraints_values, double density_value)
        /// <summary>
        /// Constructor which accepts all vector coordinates and density value
        /// </summary>
        /// <param name="vector_coordinates">Sigma point vector's coordinate values</param>
        /// <param name="density_value">Density value at current sigma point</param>
        public SigmaPoint(List<double> vector_coordinates, double density_value)
        {
            m_VectorCoordinates = vector_coordinates;
            DensityValue = density_value;
        }
        #endregion

        #endregion
    }
}

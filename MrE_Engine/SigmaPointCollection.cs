using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MrE_Engine
{
    /// <summary>
    /// Collection which keeps SigmaPoint class instances.
    /// </summary>
    public class SigmaPointCollection: List<SigmaPoint>
    {
        #region PROPERTIES
        
        #endregion

        #region METHODS

        #region Constructor 
        /// <summary>
        /// Main constructor for our example distribution
        /// </summary>
        public SigmaPointCollection()
            : base(MrEHelper.MAXNUMBEROFSIGMAPOINTS)
        {
        }
        #endregion

        #endregion

    }
}

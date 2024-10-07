using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SalesMkt
{
    /// <summary>
    /// This is a special version of the Custom Option that allos a start and end of an 
    /// Age Range to be defined.
    /// </summary>
    public class AgeRange: CustomOption
    {
        /// <summary>
        /// The start of the Age Range
        /// </summary>
        public int Start { get; set; }


        /// <summary>
        /// The end of the Age Range
        /// </summary>
        public int End { get; set; }

    }
}

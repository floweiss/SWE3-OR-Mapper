using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3_OR_Mapper.Attributes
{
    /// <summary> This attribute marks a class as an entity </summary>
    public class EntityAttribute : Attribute
    {
        /// <summary> Name of the table </summary>
        public string TableName = null;
    }
}

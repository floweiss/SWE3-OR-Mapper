using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3_OR_Mapper.Attributes
{
    public class EntityAttribute : Attribute
    {
        public string TableName = null;
    }
}

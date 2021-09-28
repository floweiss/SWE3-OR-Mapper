using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWE3_OR_Mapper.Attributes;

namespace SWE3_OR_Mapper.Attributes
{
    public class PrimaryKeyAttribute : FieldAttribute
    {
        public PrimaryKeyAttribute()
        {
            Nullable = false;
        }
    }
}

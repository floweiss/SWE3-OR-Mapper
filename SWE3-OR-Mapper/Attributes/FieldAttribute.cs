using System;

namespace SWE3_OR_Mapper.Attributes
{
    public class FieldAttribute : Attribute
    {
        public string ColumnName = null;

        public Type ColumnType = null;

        public bool Nullable = false;
    }
}

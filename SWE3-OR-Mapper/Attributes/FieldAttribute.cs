using System;

namespace SWE3_OR_Mapper.Attributes
{
    /// <summary> This attribute marks a member as a field </summary>
    public class FieldAttribute : Attribute
    {
        /// <summary> Name of column in DB </summary>
        public string ColumnName = null;

        /// <summary> Columntype in DB </summary>
        public Type ColumnType = null;

        /// <summary> Nullable flag </summary>
        public bool Nullable = false;
    }
}

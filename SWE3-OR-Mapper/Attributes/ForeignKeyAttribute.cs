using SWE3_OR_Mapper.Attributes;

namespace SWE3_OR_Mapper.Attributes
{
    /// <summary> This attribute marks a property as a foreign key field </summary>
    public class ForeignKeyAttribute : FieldAttribute
    {
        /// <summary> Name of Assignment table for n:m relations </summary>
        public string AssignmentTable = null;
        
        /// <summary> Name of column in Assignment table</summary>
        public string RemoteColumnName = null;
    }
}

using SWE3_OR_Mapper.Attributes;

namespace SWE3_OR_Mapper.Attributes
{
    public class ForeignKeyAttribute : FieldAttribute
    {
        public string AssignmentTable = null;
        
        public string RemoteColumnName = null;
    }
}

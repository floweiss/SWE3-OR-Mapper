using System;
using SWE3_OR_Mapper.Attributes;

namespace SWE3_OR_Mapper.Tests.Library
{
    public abstract class Person
    {
        [PrimaryKey]
        public string ID { get; set; }

        public string Name { get; set; }

        public string FirstName { get; set; }

        [Field(ColumnName = "BDATE")]
        public DateTime BirthDate { get; set; }
        
        public Gender Gender { get; set; }
    }
}

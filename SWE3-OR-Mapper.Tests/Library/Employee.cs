using System;
using SWE3_OR_Mapper.Attributes;

namespace SWE3_OR_Mapper.Tests.Library
{
    [Entity(TableName = "EMPLOYEES")]
    public class Employee : Person
    {
        public int Salary { get; set; }

        [Field(ColumnName = "HDATE")]
        public DateTime HireDate { get; set; }
        
        [ForeignKey(ColumnName = "KLOCATION")]
        public Location Location { get; set; }
    }
}
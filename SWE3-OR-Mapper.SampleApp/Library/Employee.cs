using System;
using System.Collections.Generic;
using SWE3_OR_Mapper.Attributes;

namespace SWE3_OR_Mapper.SampleApp.Library
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
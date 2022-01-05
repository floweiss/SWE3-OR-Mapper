using System.Collections.Generic;
using SWE3_OR_Mapper.Attributes;

namespace SWE3_OR_Mapper.Tests.Library
{
    [Entity(TableName = "LOCATIONS")]
    public class Location
    {
        [PrimaryKey]
        public string ID { get; set; }
        
        public string Name { get; set; }
        
        public string Country { get; set; }
        
        public int PostalCode { get; set; }
        
        public string City { get; set; }

        public string Street { get; set; }
        
        public int HouseNumber { get; set; }
        
        [ForeignKey(ColumnName = "KLOCATION")]
        public List<Employee> Employees { get; private set; }
        
        
        protected static int _N = 1;
        [Ignore]
        public int InstanceNumber { get; protected set; } = _N++;
    }
}
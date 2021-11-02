using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWE3_OR_Mapper.Attributes;

namespace SWE3_OR_Mapper.SampleApp.School
{
    [Entity(TableName = "TEACHERS")]
    public class Teacher : Person
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets or sets the teacher's salary.</summary>
        public int Salary { get; set; }

        /// <summary>Gets or sets the teacher's hire date.</summary>
        [Field(ColumnName = "HDATE")]
        public DateTime HireDate { get; set; }

        [ForeignKey(ColumnName = "KTEACHER")]
        public List<Class> Classes { get; private set; }
    }
}

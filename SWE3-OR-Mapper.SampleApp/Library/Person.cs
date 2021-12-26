using System;
using SWE3_OR_Mapper.Attributes;

namespace SWE3_OR_Mapper.SampleApp.Library
{
    public abstract class Person
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // protected static members                                                                                         //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Instance number counter.</summary>
        protected static int _N = 1;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets or sets the person ID.</summary>
        [PrimaryKey]
        public string ID { get; set; }


        /// <summary>Gets or sets the person's name.</summary>
        public string Name { get; set; }


        /// <summary>Gets or sets the person's first name.</summary>
        public string FirstName { get; set; }


        /// <summary>Gets or sets the person's birth date.</summary>
        [Field(ColumnName = "BDATE")]
        public DateTime BirthDate { get; set; }


        /// <summary>Gets or sets the person gender.</summary>
        public Gender Gender { get; set; }


        /// <summary>Gets the instance number.</summary>
        [Ignore]
        public int InstanceNumber { get; protected set; } = _N++;
    }
}

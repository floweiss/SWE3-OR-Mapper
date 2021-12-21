using System;
using System.Collections.Generic;
using SWE3_OR_Mapper.Attributes;

namespace SWE3_OR_Mapper.SampleApp.Library
{
    [Entity(TableName = "BOOKS")]
    public class Book
    {
        [PrimaryKey]
        public string ID { get; set; }
        
        public string Title { get; set; }
        
        [Field(ColumnName = "PUBDATE")]
        public DateTime PublicationDate { get; set; }
    }
}
using System;
using SWE3_OR_Mapper.Attributes;

namespace SWE3_OR_Mapper.Tests.Library
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
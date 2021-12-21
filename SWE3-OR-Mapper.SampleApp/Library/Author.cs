using System.Collections.Generic;
using SWE3_OR_Mapper.Attributes;

namespace SWE3_OR_Mapper.SampleApp.Library
{
    [Entity(TableName = "AUTHORS")]
    public class Author : Person
    {
        [ForeignKey(AssignmentTable = "AUTHOR_BOOK", ColumnName = "KAUTHOR", RemoteColumnName = "KBOOK")]
        public List<Book> Books { get; set; } = new List<Book>();
    }
}
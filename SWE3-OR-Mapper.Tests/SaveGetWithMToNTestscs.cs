using System;
using System.Data;
using System.Linq;
using Moq;
using Npgsql;
using NUnit.Framework;
using SWE3_OR_Mapper.Tests.Library;

namespace SWE3_OR_Mapper.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class SaveGetWithMToNTests
    {
        [SetUp]
        public void Setup()
        {
            Orm.Connection = new NpgsqlConnection("Host=localhost;Port=5437;Username=swe3-test;Password=Test123;Database=postgres");
            Orm.Connection.Open();
        }

        [Test]
        [NonParallelizable]
        public void Test_OrmSaveWithMToN_OneRowInTable()
        {
            Author a = new Author();
            a.ID = "a.0";
            a.Name = "Aalo";
            a.FirstName = "Alice";
            a.Gender = Gender.Female;
            a.BirthDate = new DateTime(1990, 1, 12);
            Orm.Save(a);
            
            Book b = new Book();
            b.ID = "b.0";
            b.Title = "Computer Science 5";
            b.PublicationDate = new DateTime(2010, 4, 6);
            Orm.Save(b);

            a.Books.Add(b);
            Orm.Save(a);

            IDbCommand cmd = Orm.Connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM BOOKS";
            var count = cmd.ExecuteScalar();
            Assert.AreEqual(1, count);
        }
        
        [Test]
        [NonParallelizable]
        public void Test_OrmGetWithMToN_BookForeignKeyPersistedSuccessfully()
        {
            Author a = new Author();
            a.ID = "a.0";
            a.Name = "Aalo";
            a.FirstName = "Alice";
            a.Gender = Gender.Female;
            a.BirthDate = new DateTime(1990, 1, 12);
            Orm.Save(a);
            
            Book b = new Book();
            b.ID = "b.0";
            b.Title = "Computer Science 5";
            b.PublicationDate = new DateTime(2010, 4, 6);
            Orm.Save(b);

            a.Books.Add(b);
            Orm.Save(a);
            
            a = Orm.Get<Author>("a.0");
            
            Assert.AreEqual("Computer Science 5", a.Books.First().Title);
        }
        
        [TearDown]
        public void TearDown()
        {
            /*IDbCommand cmd = Orm.Connection.CreateCommand();
            cmd.CommandText = "DROP TABLE IF EXISTS LOCATIONS";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "DROP TABLE IF EXISTS EMPLOYEES";
            cmd.ExecuteNonQuery();
            cmd.Dispose();*/
            Orm.Connection.Close();
            Orm.Connection = null;
        }
    }
}
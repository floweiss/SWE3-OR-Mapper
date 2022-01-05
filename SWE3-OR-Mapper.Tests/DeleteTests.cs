using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using Npgsql;
using NUnit.Framework;
using SWE3_OR_Mapper.Cache;
using SWE3_OR_Mapper.Tests.Library;

namespace SWE3_OR_Mapper.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class DeleteTests
    {
        [SetUp]
        public void Setup()
        {
            Orm.Connection = new NpgsqlConnection("Host=localhost;Port=5438;Username=swe3-test;Password=Test123;Database=postgres");
            Orm.Connection.Open();
        }

        [Test]
        [NonParallelizable]
        public void Test_OrmDelete_ReturnZeroAsSavedRowsAfterDelete()
        {
            Location l = new Location();
            l.ID = "l.0";
            l.Name = "Vienna City Library Test";
            l.Country = "Austria";
            l.City = "Vienna";
            l.PostalCode = 1010;
            l.Street = "Kingstreet";
            l.HouseNumber = 25;
            Orm.Save(l);
            
            Orm.Delete(l);

            IDbCommand cmd = Orm.Connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM LOCATIONS";
            var count = cmd.ExecuteScalar();
            Assert.AreEqual(0, count);
        }
        
        [Test]
        [NonParallelizable]
        public void Test_OrmDelete_DeleteAllBookRelationsWithAuthor()
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
            
            Orm.Delete(a);

            IDbCommand cmd = Orm.Connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM AUTHOR_BOOK WHERE KAUTHOR = 'a.0'";
            var count = cmd.ExecuteScalar();
            Assert.AreEqual(0, count);
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
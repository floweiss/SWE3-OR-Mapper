using System;
using System.Data;
using Moq;
using Npgsql;
using NUnit.Framework;
using SWE3_OR_Mapper.Tests.Library;

namespace SWE3_OR_Mapper.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class SaveGetTests
    {
        private string _name;
        private string _id;
        
        [SetUp]
        public void Setup()
        {
            Orm.Connection = new NpgsqlConnection("Host=localhost;Port=5433;Username=swe3-test;Password=Test123;Database=postgres");
            Orm.Connection.Open();
            
            _name = "Vienna City Library Test";
            _id = "l.0";
        }

        [Test]
        [NonParallelizable]
        public void Test_OrmSave_OneRowInTable()
        {
            Location l = new Location();
            l.ID = _id;
            l.Name = _name;
            l.Country = "Austria";
            l.City = "Vienna";
            l.PostalCode = 1010;
            l.Street = "Kingstreet";
            l.HouseNumber = 25;
            Orm.Save(l);

            IDbCommand cmd = Orm.Connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM LOCATIONS";
            var count = cmd.ExecuteScalar();
            Assert.AreEqual(1, count);
        }
        
        [Test]
        [NonParallelizable]
        public void Test_OrmGet_NamePersistedSuccessfully()
        {
            Location l = new Location();
            l.ID = _id;
            l.Name = _name;
            l.Country = "Austria";
            l.City = "Vienna";
            l.PostalCode = 1010;
            l.Street = "Kingstreet";
            l.HouseNumber = 25;
            Orm.Save(l);
            
            l = Orm.Get<Location>(_id);
            
            Assert.AreEqual(_name, l.Name);
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
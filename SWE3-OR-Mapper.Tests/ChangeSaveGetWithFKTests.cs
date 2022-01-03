using System;
using System.Data;
using Moq;
using Npgsql;
using NUnit.Framework;
using SWE3_OR_Mapper.Tests.Library;

namespace SWE3_OR_Mapper.Tests
{
    public class ChangeSaveGetWithFKTests
    {
        private string _name;
        private string _id;
        
        [SetUp]
        public void Setup()
        {
            Orm.Connection = new NpgsqlConnection("Host=localhost;Port=5433;Username=swe3-test;Password=Test123;Database=postgres");
            Orm.Connection.Open();
            
            _id = "e.0";
            _name = "Vienna City Library Test";
        }

        [Test]
        public void Test_OrmSaveWithFK_OneRowInTable()
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

            Employee e = new Employee();
            e.ID = _id;
            e.FirstName = "Jerry";
            e.Name = "Mouse";
            e.Gender = Gender.Male;
            e.BirthDate = new DateTime(1970, 4, 30);
            e.HireDate = new DateTime(2015, 6, 20);
            e.Salary = 62000;
            e.Location = l;
            Orm.Save(e);

            IDbCommand cmd = Orm.Connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM EMPLOYEES";
            var count = cmd.ExecuteScalar();
            Assert.AreEqual(1, count);
        }
        
        [Test]
        public void Test_OrmGetWithFK_LocationForeignKeyPersistedSuccessfully()
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

            Employee e = new Employee();
            e.ID = _id;
            e.FirstName = "Jerry";
            e.Name = "Mouse";
            e.Gender = Gender.Male;
            e.BirthDate = new DateTime(1970, 4, 30);
            e.HireDate = new DateTime(2015, 6, 20);
            e.Salary = 62000;
            e.Location = l;
            Orm.Save(e);
            
            e = Orm.Get<Employee>(_id);
            
            Assert.AreEqual(_name, e.Location.Name);
        }
        
        [TearDown]
        public void TearDown()
        {
            IDbCommand cmd = Orm.Connection.CreateCommand();
            cmd.CommandText = "DROP TABLE LOCATIONS";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "DROP TABLE EMPLOYEES";
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }
    }
}
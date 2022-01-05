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
    public class SaveGetWithFKTests
    {
        private string _name;
        private string _idEmployee;
        private string _idLocation;
        
        [SetUp]
        public void Setup()
        {
            Orm.Connection = new NpgsqlConnection("Host=localhost;Port=5434;Username=swe3-test;Password=Test123;Database=postgres");
            Orm.Connection.Open();
            
            _idEmployee = "e.0";
            _idLocation = "l.0";
            _name = "Vienna City Library Test";
        }

        [Test]
        [NonParallelizable]
        public void Test_OrmSaveWithFK_OneRowInTable()
        {
            Location l = new Location();
            l.ID = _idLocation;
            l.Name = _name;
            l.Country = "Austria";
            l.City = "Vienna";
            l.PostalCode = 1010;
            l.Street = "Kingstreet";
            l.HouseNumber = 25;
            Orm.Save(l);

            Employee e = new Employee();
            e.ID = _idEmployee;
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
        [NonParallelizable]
        public void Test_OrmGetWithFK_LocationForeignKeyPersistedSuccessfully()
        {
            Location l = new Location();
            l.ID = _idLocation;
            l.Name = _name;
            l.Country = "Austria";
            l.City = "Vienna";
            l.PostalCode = 1010;
            l.Street = "Kingstreet";
            l.HouseNumber = 25;
            Orm.Save(l);

            Employee e = new Employee();
            e.ID = _idEmployee;
            e.FirstName = "Jerry";
            e.Name = "Mouse";
            e.Gender = Gender.Male;
            e.BirthDate = new DateTime(1970, 4, 30);
            e.HireDate = new DateTime(2015, 6, 20);
            e.Salary = 62000;
            e.Location = l;
            Orm.Save(e);
            
            e = Orm.Get<Employee>(_idEmployee);
            
            Assert.AreEqual(_name, e.Location.Name);
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
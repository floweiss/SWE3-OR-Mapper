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
    public class CountGetAllTests
    {
        [SetUp]
        public void Setup()
        {
            Orm.Connection = new NpgsqlConnection("Host=localhost;Port=5435;Username=swe3-test;Password=Test123;Database=postgres");
            Orm.Connection.Open();
        }

        [Test]
        [NonParallelizable]
        public void Test_OrmCount_ReturnSameNumberAsSavedRows()
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
            
            Employee e = new Employee();
            e.ID = "e.0";
            e.FirstName = "Jerry";
            e.Name = "Mouse";
            e.Gender = Gender.Male;
            e.BirthDate = new DateTime(1970, 4, 30);
            e.HireDate = new DateTime(2015, 6, 20);
            e.Salary = 62000;
            e.Location = l;
            Orm.Save(e);
            
            e = new Employee();
            e.ID = "e.1";
            e.FirstName = "Micky";
            e.Name = "Mouse";
            e.Gender = Gender.Male;
            e.BirthDate = new DateTime(1975, 8, 18);
            e.HireDate = new DateTime(2016, 6, 20);
            e.Salary = 40000;
            e.Location = l;
            Orm.Save(e);

            Assert.AreEqual(2, Orm.Count<Employee>());
        }
        
        [Test]
        [NonParallelizable]
        public void Test_OrmGetAll_ReturnSameNumberAsSavedObjects()
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
            
            Employee e = new Employee();
            e.ID = "e.0";
            e.FirstName = "Jerry";
            e.Name = "Mouse";
            e.Gender = Gender.Male;
            e.BirthDate = new DateTime(1970, 4, 30);
            e.HireDate = new DateTime(2015, 6, 20);
            e.Salary = 62000;
            e.Location = l;
            Orm.Save(e);
            
            e = new Employee();
            e.ID = "e.1";
            e.FirstName = "Micky";
            e.Name = "Mouse";
            e.Gender = Gender.Male;
            e.BirthDate = new DateTime(1975, 8, 18);
            e.HireDate = new DateTime(2016, 6, 20);
            e.Salary = 40000;
            e.Location = l;
            Orm.Save(e);

            List<Employee> employees = Orm.GetAll<Employee>();
            Assert.AreEqual(2, employees.Count);
        }
        
        [Test]
        [NonParallelizable]
        public void Test_OrmGetAll_ReturnSavedObjects()
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
            
            Employee e = new Employee();
            e.ID = "e.0";
            e.FirstName = "Jerry";
            e.Name = "Mouse";
            e.Gender = Gender.Male;
            e.BirthDate = new DateTime(1970, 4, 30);
            e.HireDate = new DateTime(2015, 6, 20);
            e.Salary = 62000;
            e.Location = l;
            Orm.Save(e);
            
            e = new Employee();
            e.ID = "e.1";
            e.FirstName = "Micky";
            e.Name = "Mouse";
            e.Gender = Gender.Male;
            e.BirthDate = new DateTime(1975, 8, 18);
            e.HireDate = new DateTime(2016, 6, 20);
            e.Salary = 40000;
            e.Location = l;
            Orm.Save(e);

            List<Employee> employees = Orm.GetAll<Employee>();
            Assert.AreEqual("e.0", employees.First().ID);
            Assert.AreEqual("e.1", employees.Last().ID);
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
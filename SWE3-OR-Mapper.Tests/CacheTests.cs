using System;
using System.Data;
using Moq;
using Npgsql;
using NUnit.Framework;
using SWE3_OR_Mapper.Cache;
using SWE3_OR_Mapper.Tests.Library;

namespace SWE3_OR_Mapper.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class CacheTests
    {
        private string _id;
        
        [SetUp]
        public void Setup()
        {
            Orm.Connection = new NpgsqlConnection("Host=localhost;Port=5436;Username=swe3-test;Password=Test123;Database=postgres");
            Orm.Connection.Open();
            
            _id = "l.0";
        }

        [Test]
        [NonParallelizable]
        public void Test_OrmCaching_MultipleObjectsHaveSameInstanceNumber()
        {
            Location l = new Location();
            l.ID = _id;
            l.Name = "Vienna City Library Test";
            l.Country = "Austria";
            l.City = "Vienna";
            l.PostalCode = 1010;
            l.Street = "Kingstreet";
            l.HouseNumber = 25;
            Orm.Save(l);

            Orm.Cache = new HashCache();
            int firstInstance = 0;
            for (int i = 0; i <= 5; i++)
            {
                l = Orm.Get<Location>(_id);
                if (i == 0)
                {
                    firstInstance = l.InstanceNumber;
                }
            }
            
            Assert.AreEqual(firstInstance, l.InstanceNumber);
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
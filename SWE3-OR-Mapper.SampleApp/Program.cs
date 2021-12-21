using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Npgsql;
using SWE3_OR_Mapper.Cache;
using SWE3_OR_Mapper.SampleApp.Library;
using SWE3_OR_Mapper.SampleApp.School;
using Gender = SWE3_OR_Mapper.SampleApp.Library.Gender;

namespace SWE3_OR_Mapper.SampleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Orm.Connection = new NpgsqlConnection("Host=localhost;Username=swe3;Password=Test123;Database=postgres");
            Orm.Connection.Open();

            Insert();
            LoadModify();
            CreateLoadWithFK();
            LoadWithFK();
            Caching();
            CachingChanges();
            GetAll();
            Operations();
            Count();
            LoadWithMToN();
            LoadModifyWithMToN();

            Orm.Connection.Close();
        }

        public static void Insert()
        {
            Console.WriteLine("(1) Insert object");
            Console.WriteLine("-----------------");
            
            Location l = new Location();
            l.ID = "l.0";
            l.Name = "Vienna City Library";
            l.Country = "Austria";
            l.City = "Vienna";
            l.PostalCode = 1010;
            l.Street = "Kingstreet";
            l.HouseNumber = 25;
            Orm.Save(l);

            Console.WriteLine("\n");
        }

        public static void LoadModify()
        {
            Console.WriteLine("(2) Load and modify object");
            Console.WriteLine("--------------------------");

            Location l = Orm.Get<Location>("l.0");

            Console.WriteLine();
            Console.WriteLine($"The {l.Name} is in {l.City}. The address is {l.Street} {l.HouseNumber}.");

            Console.WriteLine("Change house number.");
            l.HouseNumber = 33;

            Console.WriteLine($"The {l.Name} is in {l.City}. The address is {l.Street} {l.HouseNumber}.");

            Orm.Save(l);

            Console.WriteLine("\n");
        }

        public static void CreateLoadWithFK()
        {
            Console.WriteLine("(3) Create and load an object with foreign key");
            Console.WriteLine("----------------------------------------------");

            Location l = Orm.Get<Location>("l.0");

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

            e = Orm.Get<Employee>("e.0");
            Console.WriteLine((e.Gender == Gender.Male ? "Mr. " : "Ms. ") + e.Name + " works in the " + e.Location.Name + ".");

            Console.WriteLine("\n");
        }

        public static void LoadWithFK()
        {
            Console.WriteLine("(4) Load Location and show employees");
            Console.WriteLine("---------------------------------");

            Location l = Orm.Get<Location>("l.0");

            Console.WriteLine("Following employees work at the " + l.Name + ":");

            foreach (Employee e in l.Employees)
            {
                Console.WriteLine(e.FirstName + " " + e.Name);
            }

            Console.WriteLine("\n");
        }

        public static void Caching()
        {
            Console.WriteLine("(5) Caching");
            Console.WriteLine("-----------------------");

            Console.WriteLine("\rWithout cache:");
            for (int i = 0; i <= 5; i++)
            {
                Employee e = Orm.Get<Employee>("e.0");
                Console.WriteLine("Object [" + e.ID + "] instance no: " + e.InstanceNumber.ToString());
            }

            Console.WriteLine("\n\rWith cache:");
            Orm.Cache = new HashCache();
            for (int i = 0; i <= 5; i++)
            {
                Employee e = Orm.Get<Employee>("e.0");
                Console.WriteLine("Object [" + e.ID + "] instance no: " + e.InstanceNumber.ToString());
            }

            Orm.Cache = null;

            Console.WriteLine("\n");
        }

        public static void CachingChanges()
        {
            Console.WriteLine("(6) Caching with changes");
            Console.WriteLine("-----------------------");

            Orm.Cache = new HashCache();
            Employee e = Orm.Get<Employee>("e.0");
            Console.WriteLine("Object [" + e.ID + "] (" + e.Salary + ") instance no: " + e.InstanceNumber.ToString());

            e.Salary += 500;

            Employee e2 = Orm.Get<Employee>("e.0");
            Console.WriteLine("Object [" + e2.ID + "] (" + e2.Salary + ") instance no: " + e2.InstanceNumber.ToString());

            Orm.Cache = null;
            Console.WriteLine("\n");
        }

        public static void GetAll()
        {
            Console.WriteLine("(7) Get All");
            Console.WriteLine("-----------------");

            Employee e = new Employee();
            e.ID = "e.1";
            e.FirstName = "Micky";
            e.Name = "Mouse";
            e.Gender = Gender.Male;
            e.BirthDate = new DateTime(1975, 8, 18);
            e.HireDate = new DateTime(2016, 6, 20);
            e.Salary = 40000;
            e.Location = Orm.Get<Location>("l.0");
            Orm.Save(e);

            e = new Employee();
            e.ID = "e.2";
            e.FirstName = "Minnie";
            e.Name = "Mouse";
            e.Gender = Gender.Female;
            e.BirthDate = new DateTime(1980, 8, 18);
            e.HireDate = new DateTime(2017, 6, 20);
            e.Salary = 45000;
            e.Location = Orm.Get<Location>("l.0");
            Orm.Save(e);

            IEnumerable<Employee> employees = Orm.GetAll<Employee>();
            foreach (Employee employee in employees)
            {
                Console.WriteLine("Salary for " + employee.FirstName + " " + employee.Name + " is " + employee.Salary.ToString() + " Dollars.");
            }

            Console.WriteLine("\n");
        }

        public static void Operations()
        {
            Console.WriteLine("(8) Get specific employees");
            Console.WriteLine("-----------------");

            IEnumerable<Employee> employees = Orm.GetAll<Employee>();
            
            Console.WriteLine("Female employees:");
            foreach (Employee employee in employees.Where(e => e.Gender != Gender.Male))
            {
                Console.WriteLine("Salary for " + employee.FirstName + " " + employee.Name + " is " + employee.Salary.ToString() + " Dollars.");
            }
            
            Console.WriteLine("\nFemale employees OR employees earning more than 50000:");
            foreach (Employee employee in employees.Where(e => e.Gender == Gender.Female || e.Salary > 50000))
            {
                Console.WriteLine("Salary for " + employee.FirstName + " " + employee.Name + " is " + employee.Salary.ToString() + " Dollars. Gender: " + employee.Gender);
            }
            
            Console.WriteLine("\nEmployees with first name starting with M:");
            foreach (Employee employee in employees.Where(e => e.FirstName.StartsWith("M")))
            {
                Console.WriteLine(employee.FirstName + " " + employee.Name);
            }

            Console.WriteLine("\nEmployees with first name containing y:");
            foreach (Employee employee in employees.Where(e => e.FirstName.ToLower().Contains("y")))
            {
                Console.WriteLine(employee.FirstName + " " + employee.Name);
            }
            
            var employeeQuery =
                from e in Orm.GetAll<Employee>()
                where e.Salary > 42000
                orderby e.Salary descending
                select e;
            Console.WriteLine("\nEmployees earning more than 42000 with LINQ:");
            foreach (Employee employee in employeeQuery)
            {
                Console.WriteLine("Salary for " + employee.FirstName + " " + employee.Name + " is " + employee.Salary.ToString() + " Dollars. Gender: " + employee.Gender);
            }

            Console.WriteLine("\n");
        }

        public static void Count()
        {
            Console.WriteLine("(9) Count objects");
            Console.WriteLine("-----------------");

            int employeeCount = Orm.Count<Employee>();
            int locationCount = Orm.Count<Location>();

            Console.WriteLine("Amount of employees: " + employeeCount);
            Console.WriteLine("Amount of locations: " + locationCount);
            
            Console.WriteLine("\n");
        }

        public static void LoadWithMToN()
        {
            Console.WriteLine("(10) Create and load an object with m:n");
            Console.WriteLine("--------------------------------------");

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
            
            b = new Book();
            b.ID = "b.1";
            b.Title = "Maths 101";
            b.PublicationDate = new DateTime(2014, 12, 22);
            Orm.Save(b);
            
            a.Books.Add(b);
            Orm.Save(a);

            a = Orm.Get<Author>("a.0");

            Console.WriteLine("Books written by " + a.Name + " " + a.FirstName + ":");
            foreach(Book i in a.Books)
            {
                Console.WriteLine(i.Title);
            }

            Console.WriteLine("\n");
        }
        
        public static void LoadModifyWithMToN()
        {
            Console.WriteLine("(11) Modify an object with m:n");
            Console.WriteLine("--------------------------------------");

            Author a = Orm.Get<Author>("a.0");

            Book b = new Book();
            b.ID = "b.2";
            b.Title = "Chemistry for Dummies";
            b.PublicationDate = new DateTime(2019, 2, 17);
            Orm.Save(b);

            a.Books.Add(b);
            Orm.Save(a);

            a = Orm.Get<Author>("a.0");

            Console.WriteLine("Books written by " + a.Name + " " + a.FirstName + ":");
            foreach(Book i in a.Books)
            {
                Console.WriteLine(i.Title);
            }
            
            a = new Author();
            a.ID = "a.1";
            a.Name = "Bernard";
            a.FirstName = "Bumblebee";
            a.Gender = Gender.Male;
            a.BirthDate = new DateTime(1992, 6, 10);
            a.Books.Add(b);
            Orm.Save(a);

            a = Orm.Get<Author>("a.1");
            
            Console.WriteLine("");
            Console.WriteLine("Books written by " + a.Name + " " + a.FirstName + ":");
            foreach(Book i in a.Books)
            {
                Console.WriteLine(i.Title);
            }
            
            Console.WriteLine("\n");
        }
    }
}

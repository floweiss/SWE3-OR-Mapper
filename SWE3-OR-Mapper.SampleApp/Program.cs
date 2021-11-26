using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Npgsql;
using SWE3_OR_Mapper.Cache;
using SWE3_OR_Mapper.SampleApp.School;

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
            GetAll();
            Operations();
            Count();

            Orm.Connection.Close();
        }

        public static void Insert()
        {
            Console.WriteLine("(1) Insert object");
            Console.WriteLine("-----------------");

            Teacher t = new Teacher();
            t.ID = "t.0";
            t.FirstName = "Jerry";
            t.Name = "Mouse";
            t.Gender = Gender.Male;
            t.BirthDate = new DateTime(1970, 8, 18);
            t.HireDate = new DateTime(2015, 6, 20);
            t.Salary = 50000;

            Orm.Save(t);
            Console.WriteLine("\n");
        }

        public static void LoadModify()
        {
            Console.WriteLine("(2) Load and modify object");
            Console.WriteLine("--------------------------");

            Teacher t = Orm.Get<Teacher>("t.0");

            Console.WriteLine();
            Console.WriteLine("Salary for " + t.FirstName + " " + t.Name + " is " + t.Salary.ToString() + " Pesos.");

            Console.WriteLine("Give rise of 12000.");
            t.Salary += 12000;

            Console.WriteLine("Salary for " + t.FirstName + " " + t.Name + " is now " + t.Salary.ToString() + " Pesos.");

            Orm.Save(t);

            Console.WriteLine("\n");
        }

        public static void CreateLoadWithFK()
        {
            Console.WriteLine("(3) Create and load an object with foreign key");
            Console.WriteLine("----------------------------------------------");

            Teacher teacher = Orm.Get<Teacher>("t.0");

            Class c = new Class();
            c.ID = "c.0";
            c.Name = "Demonology 101";
            c.Semester = 3;
            c.Teacher = teacher;

            Orm.Save(c);

            c = Orm.Get<Class>("c.0");
            Console.WriteLine((c.Teacher.Gender == Gender.Male ? "Mr. " : "Ms. ") + c.Teacher.Name + " teaches " + c.Name + ".");

            Console.WriteLine("\n");
        }

        public static void LoadWithFK()
        {
            Console.WriteLine("(4) Load teacher and show classes");
            Console.WriteLine("---------------------------------");

            Teacher t1 = Orm.Get<Teacher>("t.0");

            Console.WriteLine(t1.FirstName + " " + t1.Name + " teaches:");

            foreach (Class i in t1.Classes)
            {
                Console.WriteLine(i.Name);
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
                Teacher t = Orm.Get<Teacher>("t.0");
                Console.WriteLine("Object [" + t.ID + "] instance no: " + t.InstanceNumber.ToString());
            }

            Console.WriteLine("\n\rWith cache:");
            Orm.Cache = new HashCache();
            for (int i = 0; i <= 5; i++)
            {
                Teacher t = Orm.Get<Teacher>("t.0");
                Console.WriteLine("Object [" + t.ID + "] instance no: " + t.InstanceNumber.ToString());
            }

            Console.WriteLine("\n");
        }

        public static void GetAll()
        {
            Console.WriteLine("(6) Get All");
            Console.WriteLine("-----------------");

            Teacher t = new Teacher();
            t.ID = "t.1";
            t.FirstName = "Micky";
            t.Name = "Mouse";
            t.Gender = Gender.Male;
            t.BirthDate = new DateTime(1975, 8, 18);
            t.HireDate = new DateTime(2016, 6, 20);
            t.Salary = 40000;
            Orm.Save(t);

            t = new Teacher();
            t.ID = "t.2";
            t.FirstName = "Minnie";
            t.Name = "Mouse";
            t.Gender = Gender.Female;
            t.BirthDate = new DateTime(1980, 8, 18);
            t.HireDate = new DateTime(2017, 6, 20);
            t.Salary = 45000;
            Orm.Save(t);

            IEnumerable<Teacher> teacherList = Orm.GetAll<Teacher>();
            foreach (Teacher teacher in teacherList)
            {
                Console.WriteLine("Salary for " + teacher.FirstName + " " + teacher.Name + " is " + teacher.Salary.ToString() + " Pesos.");
            }

            Console.WriteLine("\n");
        }

        public static void Operations()
        {
            Console.WriteLine("(7) Get specific teachers");
            Console.WriteLine("-----------------");

            IEnumerable<Teacher> teacherList = Orm.GetAll<Teacher>();
            Console.WriteLine("Female teachers:");
            foreach (Teacher teacher in teacherList.Where(t => t.Gender != Gender.Male))
            {
                Console.WriteLine("Salary for " + teacher.FirstName + " " + teacher.Name + " is " + teacher.Salary.ToString() + " Pesos.");
            }
            
            Console.WriteLine("\nFemale teachers OR teachers earning more than 50000:");
            foreach (Teacher teacher in teacherList.Where(t => t.Gender == Gender.Female || t.Salary > 50000))
            {
                Console.WriteLine("Salary for " + teacher.FirstName + " " + teacher.Name + " is " + teacher.Salary.ToString() + " Pesos. Gender: " + teacher.Gender);
            }
            
            Console.WriteLine("\nTeachers with first name starting with M:");
            foreach (Teacher teacher in teacherList.Where(t => t.FirstName.StartsWith("M")))
            {
                Console.WriteLine(teacher.FirstName + " " + teacher.Name);
            }

            Console.WriteLine("\nTeachers with first name containing y:");
            foreach (Teacher teacher in teacherList.Where(t => t.FirstName.ToLower().Contains("y")))
            {
                Console.WriteLine(teacher.FirstName + " " + teacher.Name);
            }

            Console.WriteLine("\n");
        }

        public static void Count()
        {
            Console.WriteLine("(7) Count objects");
            Console.WriteLine("-----------------");

            int teacherCount = Orm.Count<Teacher>();
            int classCount = Orm.Count<Class>();

            Console.WriteLine("Amount of teachers: " + teacherCount);
            Console.WriteLine("Amount of classes: " + classCount);
        }
    }
}

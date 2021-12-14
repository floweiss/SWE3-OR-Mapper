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

            Orm.Cache = null;

            Console.WriteLine("\n");
        }

        public static void CachingChanges()
        {
            Console.WriteLine("(6) Caching with changes");
            Console.WriteLine("-----------------------");

            Orm.Cache = new HashCache();
            Teacher t = Orm.Get<Teacher>("t.0");
            Console.WriteLine("Object [" + t.ID + "] (" + t.Salary + ") instance no: " + t.InstanceNumber.ToString());

            t.Salary += 500;

            Teacher t2 = Orm.Get<Teacher>("t.0");
            Console.WriteLine("Object [" + t2.ID + "] (" + t2.Salary + ") instance no: " + t2.InstanceNumber.ToString());

            Orm.Cache = null;
            Console.WriteLine("\n");
        }

        public static void GetAll()
        {
            Console.WriteLine("(7) Get All");
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
            Console.WriteLine("(8) Get specific teachers");
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
            
            var teacherQuery =
                from t in Orm.GetAll<Teacher>()
                where t.Salary > 42000
                orderby t.Salary descending
                select t;
            Console.WriteLine("\nTeachers earning more than 42000 with LINQ:");
            foreach (Teacher teacher in teacherQuery)
            {
                Console.WriteLine("Salary for " + teacher.FirstName + " " + teacher.Name + " is " + teacher.Salary.ToString() + " Pesos. Gender: " + teacher.Gender);
            }

            Console.WriteLine("\n");
        }

        public static void Count()
        {
            Console.WriteLine("(9) Count objects");
            Console.WriteLine("-----------------");

            int teacherCount = Orm.Count<Teacher>();
            int classCount = Orm.Count<Class>();

            Console.WriteLine("Amount of teachers: " + teacherCount);
            Console.WriteLine("Amount of classes: " + classCount);
            
            Console.WriteLine("\n");
        }

        public static void LoadWithMToN()
        {
            Console.WriteLine("(10) Create and load an object with m:n");
            Console.WriteLine("--------------------------------------");

            Course c = new Course();
            c.ID = "x.0";
            c.Name = "Demons 1";
            c.Teacher = Orm.Get<Teacher>("t.0");

            Student s = new Student();
            s.ID = "s.0";
            s.Name = "Aalo";
            s.FirstName = "Alice";
            s.Gender = Gender.Female;
            s.BirthDate = new DateTime(1990, 1, 12);
            s.Grade = 1;
            Orm.Save(s);

            c.Students.Add(s);

            s = new Student();
            s.ID = "s.1";
            s.Name = "Bumblebee";
            s.FirstName = "Bernard";
            s.Gender = Gender.Male;
            s.BirthDate = new DateTime(1991, 9, 23);
            s.Grade = 2;
            Orm.Save(s);

            c.Students.Add(s);

            Orm.Save(c);

            c = Orm.Get<Course>("x.0");

            Console.WriteLine("Students in " + c.Name + ":");
            foreach(Student i in c.Students)
            {
                Console.WriteLine(i.FirstName + " " + i.Name);
            }

            Console.WriteLine("\n");
        }
        
        public static void LoadModifyWithMToN()
        {
            Console.WriteLine("(11) Modify an object with m:n");
            Console.WriteLine("--------------------------------------");

            Course c = Orm.Get<Course>("x.0");

            Student s = new Student();
            s.ID = "s.2";
            s.Name = "Carlo";
            s.FirstName = "Charles";
            s.Gender = Gender.Male;
            s.BirthDate = new DateTime(2000, 4, 7);
            s.Grade = 4;
            Orm.Save(s);

            c.Students.Add(s);
            Orm.Save(c);

            c = Orm.Get<Course>("x.0");

            Console.WriteLine("Students in " + c.Name + ":");
            foreach(Student i in c.Students)
            {
                Console.WriteLine(i.FirstName + " " + i.Name);
            }

            Console.WriteLine("\n");
        }
    }
}

using System;
using Npgsql;
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
    }
}

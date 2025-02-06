using IndividuelltProjekt_Skola.Context;
using Microsoft.EntityFrameworkCore;

namespace IndividuelltProjekt_Skola.EF;

public class StudentMethods
{
    public static void GetAllStudents()
    {
        try
        {
            using (var context = new MyDbContext())
            {
                string sortChoice;
                string orderChoice;

                // Val för sortering
                while (true)
                {
                    Console.WriteLine("Vill du sortera på:");
                    Console.WriteLine("1. Förnamn");
                    Console.WriteLine("2. Efternamn");
                    sortChoice = Console.ReadLine();

                    if (sortChoice == "1" || sortChoice == "2")
                        break;

                    Console.WriteLine("Felaktig inmatning. Vänligen ange 1 eller 2.");
                }

                // Val för stigande eller fallande ordning
                while (true)
                {
                    Console.WriteLine("Sorteringsordning:");
                    Console.WriteLine("1. Stigande");
                    Console.WriteLine("2. Fallande");
                    orderChoice = Console.ReadLine();

                    if (orderChoice == "1" || orderChoice == "2")
                        break;

                    Console.WriteLine("Felaktig inmatning. Vänligen ange 1 eller 2.");
                }

                // Hämta elever och inkludera klassinfo
                var studentsQuery = context.Students
                    .Include(s => s.Class)
                    .AsQueryable();

                // Sorteringslogik
                if (sortChoice == "1")
                {
                    studentsQuery = orderChoice == "1"
                        ? studentsQuery.OrderBy(s => s.Class.ClassName).ThenBy(s => s.FirstName)
                        : studentsQuery.OrderBy(s => s.Class.ClassName).ThenByDescending(s => s.FirstName);
                }
                else
                {
                    studentsQuery = orderChoice == "1"
                        ? studentsQuery.OrderBy(s => s.Class.ClassName).ThenBy(s => s.LastName)
                        : studentsQuery.OrderBy(s => s.Class.ClassName).ThenByDescending(s => s.LastName);
                }

                var students = studentsQuery.ToList();

                if (!students.Any())
                {
                    Console.WriteLine("Det finns inga elever att visa.");
                    return;
                }

                Console.WriteLine("\n--- Lista över elever sorterade efter klass ---");
                string currentClass = "";
                foreach (var student in students)
                {
                    // Skriv ut klassrubrik om vi byter klass
                    if (student.Class.ClassName != currentClass)
                    {
                        currentClass = student.Class.ClassName;
                        Console.WriteLine($"\n=== Klass {currentClass} ===");
                    }

                    // Visa information om eleven
                    Console.WriteLine(
                        $"{student.FirstName} {student.LastName} | Ålder: {student.Age} | Personnummer: {student.SocialSecurityNumber}");
                }

                Console.WriteLine("\nTryck på valfri tangent för att återgå till menyn.");
                Console.ReadKey();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ett fel inträffade när eleverna hämtades. Försök igen senare.");
            Console.WriteLine($"Felsökningsinformation: {ex.Message}");
        }
    }
}
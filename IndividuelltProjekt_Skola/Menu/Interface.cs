using IndividuelltProjekt_Skola.ADO;
using IndividuelltProjekt_Skola.EF;

namespace IndividuelltProjekt_Skola.Menu;

public class Interface
{
    public static void ShowMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Välkommen till skolapplikationen!\nVar god välj ett alternativ:");
            Console.WriteLine("1. Visa alla avdelningar och antal anställda");
            Console.WriteLine("2. Visa information om alla elever");
            Console.WriteLine("3. Visa aktiva kurser");
            Console.WriteLine("4. Visa översikt över anställda");
            Console.WriteLine("5. Lägg till ny anställd");
            Console.WriteLine("6. Visa betyg för en elev");
            Console.WriteLine("7. Visa lönekostnader per månad och avdelning");
            Console.WriteLine("8. Visa genomsnittlig lön per avdelning");
            Console.WriteLine("9. Stored procedure: Elev information");
            Console.WriteLine("10. Sätt betyg på en elev");
            Console.WriteLine("11. Avsluta programmet");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    EmployeeMethods.GetEmployeeCountByDepartment();
                    break;
                case "2":
                    StudentMethods.GetAllStudents();         
                    break;
                case "3":
                    CourseMethods.GetCourses();
                    break;
                case "4":
                    AdoCommands.GetEmployeeOverview();
                    break;
                case "5":
                    AdoCommands.AddEmployee();
                    break;
                case "6":
                    AdoCommands.GetGradesFromSpecificStudent();
                    break;
                case "7":
                    AdoCommands.GetSalaryPerMonthPerDepartment();
                    break;
                case "8":
                    AdoCommands.GetAverageSalaryPerDepartment();
                    break;
                case "9":
                    AdoCommands.GetStudentInfoById();
                    break;
                case "10":
                    AdoCommands.AddGradeToStudent();
                    break;
                case "11":
                    Console.WriteLine("Avslutar programmet.");
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Felaktigt val. Försök igen.");
                    break;
            }
        }
    }
}
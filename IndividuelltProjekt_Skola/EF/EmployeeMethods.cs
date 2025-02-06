using IndividuelltProjekt_Skola.Context;
using Microsoft.EntityFrameworkCore;
namespace IndividuelltProjekt_Skola.EF;

public class EmployeeMethods
{
    public static void GetEmployeeCountByDepartment()
    {
        using (var context = new MyDbContext()) 
        {
            var employeeCountByDepartment = context.Employees
                .GroupBy(e => e.Department.DepartmentName) // Grupperar efter avdelningens namn
                .Select(g => new 
                {
                    Department = g.Key,
                    EmployeeCount = g.Count() // Räknar alla anställda i varje avdelning
                })
                .ToList();

            foreach(var result in employeeCountByDepartment)
            {
                Console.WriteLine($"Avdelning: {result.Department}, Antal anställda: {result.EmployeeCount}");
            }

            Console.WriteLine("Tryck på valfri tangent för att återgå till menyn.");
            Console.ReadKey();
        }
    }
}

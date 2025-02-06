using IndividuelltProjekt_Skola.Context;

namespace IndividuelltProjekt_Skola.EF;

public class CourseMethods
{
    public static void GetCourses()
    {
        try
        {
            using (var context = new MyDbContext())
            {
                var activeCourses = context.Courses
                    .Where(c => c.IsActive) // Show only active courses
                    .AsEnumerable();

                Console.WriteLine("\nLista över aktiva kurser:");
                foreach (var course in activeCourses)
                {
                    Console.WriteLine($"Kurs ID: {course.CourseId}. Kursnamn: {course.CourseName}");
                }

                Console.WriteLine();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Ett fel inträffade: {e.Message}");
        }
    }
}
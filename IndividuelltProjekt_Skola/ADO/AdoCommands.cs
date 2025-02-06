using System.Data;
using Microsoft.Data.SqlClient;

namespace IndividuelltProjekt_Skola.ADO;

public class AdoCommands
{
    private const string _connectionString =
        "Server=localhost,1433;Database=IndividuelltProjekt_Alex;User ID=sa;Password=MyStrongPass123;TrustServerCertificate=True;";

    public static List<(int Id, string ProfessionName)> ListProfessions()
    {
        List<(int Id, string ProfessionName)> professions = new List<(int, string)>();

        string query = "SELECT ProfessionId, ProfessionName FROM Professions";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                try
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int professionId = reader.GetInt32(0);
                            string professionName = reader.GetString(1);

                            professions.Add((professionId, professionName));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fel vid hämtning av yrken: {ex.Message}");
                }
            }
        }

        return professions;
    }
    
    public static void GetSalaryPerMonthPerDepartment()
    {
        string query = @"
        SELECT 
            d.DepartmentName AS Avdelning, 
            SUM(e.Salary) AS [Lönekostnader i sek]
        FROM Employees e
        JOIN Departments d ON e.DepartmentId = d.DepartmentId
        GROUP BY d.DepartmentName
        ORDER BY d.DepartmentName";

        Console.WriteLine("Löner per avdelning:");
        ExecuteQuery(query, 30); // Här kan padding vara 30 för att passa din kolumnstorlek
        Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
        Console.ReadKey();
    }

    public static void GetAverageSalaryPerDepartment()
    {
        string query = @"
        SELECT 
            d.DepartmentName AS Avdelning, 
            AVG(e.Salary) AS [Medellön i sek]
        FROM Employees e
        JOIN Departments d ON e.DepartmentId = d.DepartmentId
        GROUP BY d.DepartmentName
        ORDER BY d.DepartmentName";

        Console.WriteLine("Medellön per avdelning:");
        ExecuteQuery(query, 30);
        Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
        Console.ReadKey();
    }

    public static void GetEmployeeOverview()
    {
        string query = @"
                SELECT 
                    e.FirstName + ' ' + e.LastName AS Lärare,
                    p.ProfessionName AS Titel,
                    d.DepartmentName AS Avdelning,
                    DATEDIFF(YEAR, e.HireDate, GETDATE()) AS [År anställd]
                FROM Employees e
                JOIN Professions p ON e.ProfessionId = p.ProfessionId
                LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId;";

        Console.WriteLine("Översikt av personal:");
        ExecuteQuery(query, 27); // Antag att ExecuteQuery har padding som argument
        Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
        Console.ReadKey();
    }

public static void AddEmployee()
{
    try
    {
        // Använd hjälpmetoder för att samla in information från användaren
        string firstName = GetInput("Ange förnamn på den anställde:");
        string lastName = GetInput("Ange efternamn på den anställde:");

        // Hämta och välj ett yrke
        List<(int Id, string ProfessionName)> professions = ListProfessions();
        int professionId = GetSelectionFromList(professions, "Vilken anställning kommer den anställde ha på skolan?");

        // Be om lön
        int salary = GetIntInput("Ange den anställdes lön (minimum 17 000kr):", 17000);

        // Be om anställningsdatum
        DateOnly employmentDate = GetDateInput("Ange den anställdes anställningsdatum (YYYY-MM-DD):");

        // Bygg SQL-frågan för att lägga till den anställde
        string query = @"INSERT INTO Employees (FirstName, LastName, ProfessionId, Salary, EmploymentDate)
                         VALUES (@FirstName, @LastName, @ProfessionId, @Salary, @EmploymentDate)";

        // Skapa parametrar för SQL-frågan
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@FirstName", SqlDbType.NVarChar) { Value = firstName },
            new SqlParameter("@LastName", SqlDbType.NVarChar) { Value = lastName },
            new SqlParameter("@ProfessionId", SqlDbType.Int) { Value = professionId },
            new SqlParameter("@Salary", SqlDbType.Int) { Value = salary },
            new SqlParameter("@EmploymentDate", SqlDbType.Date) { Value = employmentDate }
        };

        // Kör frågan för att lägga till den anställde
        ExecuteQuery(query, 20, parameters);

        Console.WriteLine("Den anställde har lagts till i systemet.");
    }
    catch (Exception ex)
    {
        // Hantera fel vid exekvering
        Console.WriteLine($"Ett fel uppstod: {ex.Message}");
    }
}

public static void GetGradesFromSpecificStudent()
{
    List<(int StudentId, string FirstName, string LastName, string ClassName)> students = ListStudents();  // Hämta alla elever
    Console.WriteLine("Vilken elevs betyg vill du se?");

    // Skriv ut eleverna för val
    foreach (var student in students)
    {
        Console.WriteLine($"{student.StudentId}: {student.FirstName} {student.LastName} årskurs ({student.ClassName})");
    }

    Console.WriteLine("Ange elevens ID för att se betyg:");

    // Hämta och validera input
    if (!int.TryParse(Console.ReadLine(), out int studentId))
    {
        Console.WriteLine("Ogiltigt val. Vänligen ange ett heltal.");
        return;
    }

    // Check if the studentId exists in the list
    if (!students.Any(s => s.StudentId == studentId))
    {
        Console.WriteLine("Ogiltigt elev-ID. Försök igen.");
        return;
    }

    // SQL-fråga för att hämta betyg
    string query = @"
    SELECT 
    s.FirstName + ' ' + s.LastName AS Elev,
    c.CourseName AS Ämne,
    g.Grade AS Betyg,
    e.FirstName + ' ' + e.LastName AS Lärare,
    FORMAT(g.GradeSetDate, 'yyyy-MM-dd') AS Datum 
    FROM Grades g
    INNER JOIN Students s ON g.StudentId = s.StudentId
    INNER JOIN Courses c ON g.CourseId = c.CourseId 
    INNER JOIN Employees e ON g.EmployeeId = e.EmployeeId
        WHERE s.StudentId = @StudentId;";

    // Skapa parameter
    SqlParameter studentIdParam = new SqlParameter("@StudentId", SqlDbType.Int)
    {
        Value = studentId
    };
    Console.WriteLine($"Hämtar betyg för StudentId: {studentId}");
    ExecuteQuery(query, 20, studentIdParam);
    Console.WriteLine("\nTryck på valfri tangent för att fortsätta...");
    Console.ReadKey();
}

public static string GetInput(string prompt)
{
    Console.WriteLine(prompt);
    string input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("Input får inte vara tomt. Försök igen.");
        return GetInput(prompt);  // Återkalla metoden tills vi får ett giltigt svar
    }

    return input;
}

public static int GetIntInput(string prompt, int minValue)
{
    Console.WriteLine(prompt);
    int input;
    while (!int.TryParse(Console.ReadLine(), out input) || input < minValue)
    {
        Console.WriteLine($"Ogiltig input. Vänligen ange ett heltal och tänk på att vi har en minimumlön på {minValue}.");
    }
    return input;
}

public static DateOnly GetDateInput(string prompt)
{
    Console.WriteLine(prompt);
    DateOnly dateInput;
    while (!DateOnly.TryParse(Console.ReadLine(), out dateInput))
    {
        Console.WriteLine("Ogiltigt datum. Vänligen ange ett giltigt datum.");
    }
    return dateInput;
}

public static int GetSelectionFromList(List<(int Id, string ProfessionName)> list, string prompt)
{
    Console.WriteLine("\n" + prompt);
    foreach (var item in list)
    {
        Console.WriteLine($"{item.Id}. {item.ProfessionName}");
    }

    int selection;
    while (!int.TryParse(Console.ReadLine(), out selection) || !list.Any(p => p.Id == selection))
    {
        Console.WriteLine("Ogiltig input. Vänligen välj ett giltigt yrkes-ID från listan.");
    }

    return selection;
}

public static List<(int StudentId, string FirstName, string LastName, string ClassName)> ListStudents()
{
    List<(int StudentId, string FirstName, string LastName, string ClassName)> students = new List<(int, string, string, string)>();

    string query = @"
        SELECT s.StudentId, s.FirstName, s.LastName, c.ClassName 
        FROM Students s
        INNER JOIN Classes c ON s.ClassId = c.ClassId"; // JOIN för att hämta ClassName

    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        connection.Open();
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            try
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int studentId = reader.GetInt32(0);
                        string firstName = reader.GetString(1);
                        string lastName = reader.GetString(2);
                        string className = reader.GetString(3); // ClassName istället för ClassId

                        students.Add((studentId, firstName, lastName, className));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid hämtning av studenter: {ex.Message}");
            }
        }
    }
    return students;
}

public static void ExecuteQuery(string query, int padding, params SqlParameter[] parameters)
{
    using (SqlConnection connection = new SqlConnection(_connectionString))
    {
        connection.Open();
        SqlCommand command = new SqlCommand(query, connection);

        // Add parameters if provided
        if (parameters != null && parameters.Length > 0)
        {
            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }

            // Skriv ut alla parametrar för debugging
            Console.WriteLine("Följande parametrar används i SQL-frågan:");
            foreach (SqlParameter param in command.Parameters)
            {
                Console.WriteLine($"Namn: {param.ParameterName}, Värde: {param.Value}");
            }
        }

        // Detect if it's our stored procedure
        if (query.StartsWith("Get", StringComparison.OrdinalIgnoreCase))
        {
            command.CommandType = CommandType.StoredProcedure;
        }
        else
        {
            command.CommandType = CommandType.Text;
        }

        try
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Console.Write(reader.GetName(i).PadRight(padding));
                }
                Console.WriteLine();

                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        Console.Write(reader[i].ToString().PadRight(padding));
                    }
                    Console.WriteLine();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error executing query: " + ex.Message);
        }
    }
}
}
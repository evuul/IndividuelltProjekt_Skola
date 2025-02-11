using System.Data;
using System.Threading.Channels;
using Microsoft.Data.SqlClient;

namespace IndividuelltProjekt_Skola.ADO;

public class AdoCommands
{
    private const string _connectionString =
        "Server=localhost,1433;Database=IndividuelltProjekt_Skola;User ID=sa;Password=MyStrongPass123;TrustServerCertificate=True;";

   
    
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
        ExecuteQuery(query, 30);
        Console.WriteLine("Tryck på valfri tangent för att återgå till huvudmenyn.");
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
        Console.WriteLine("Tryck på valfri tangent för att återgå till huvudmenyn.");
        Console.ReadKey();
    }

    public static void GetEmployeeOverview()
    {
        string query = @"
            SELECT 
                e.FirstName + ' ' + e.LastName AS Lärare,
                p.ProfessionName AS Anställning,
                DATEDIFF(YEAR, e.HireDate, GETDATE()) AS [År som anställd]
            FROM Employees e
            JOIN Professions p ON e.ProfessionId = p.ProfessionId;";

        Console.WriteLine("Översikt av personal:");
        ExecuteQuery(query, 27);
        Console.WriteLine("Tryck på valfri tangent för att återgå till huvudmenyn.");
        Console.ReadKey();
    }

    public static void AddEmployee()
{
    try
    {
        // use the GetInput method to get the first name and last name
        string firstName = GetInput("Ange förnamn på den anställde:");
        string lastName = GetInput("Ange efternamn på den anställde:");

        // add a profession
        List<(int Id, string ProfessionName)> professions = ListProfessions();
        int professionId = GetSelectionFromList(professions, "Vilken anställning kommer den anställde ha på skolan?");

        // ask for salary
        int salary = GetIntInput("Ange den anställdes lön (minimum 17 000kr):", 17000);

        // ask for employment date
        DateOnly employmentDate = GetDateInput("Ange den anställdes anställningsdatum (YYYY-MM-DD):");

        // add a department
        List<(int Id, string DepartmentName)> departments = ListDepartments();
        int departmentId = GetSelectionFromList(departments, "Vilken avdelning tillhör den anställde?");

        // SQL-Query to insert a new employee
        string query = @"INSERT INTO Employees (FirstName, LastName, ProfessionId, Salary, HireDate, DepartmentId)
                 VALUES (@FirstName, @LastName, @ProfessionId, @Salary, @HireDate, @DepartmentId)";
        // Skapa parametrar för SQL-frågan
        SqlParameter[] parameters = new SqlParameter[] // create an array of parameters
        {
            new SqlParameter("@FirstName", SqlDbType.NVarChar) { Value = firstName },
            new SqlParameter("@LastName", SqlDbType.NVarChar) { Value = lastName },
            new SqlParameter("@ProfessionId", SqlDbType.Int) { Value = professionId },
            new SqlParameter("@Salary", SqlDbType.Int) { Value = salary },
            new SqlParameter("@HireDate", SqlDbType.Date) { Value = employmentDate },
            new SqlParameter("@DepartmentId", SqlDbType.Int) { Value = departmentId }
        };

        // execute the query
        ExecuteQuery(query, 20, parameters);

        Console.WriteLine("Den anställde har lagts till i systemet.");
        Console.WriteLine("Tryck på valfri tangent för att återgå till huvudmenyn.");
        Console.ReadKey();
    }
    catch (Exception ex)
    {
        // if a problem occurs, show the error message
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
    Console.WriteLine("\nTryck på valfri tangent för att återgå till huvudmenyn.");
    Console.ReadKey();
}

public static void GetStudentInfoById()
{
    // get a list of students
    List<(int StudentId, string FirstName, string LastName, string ClassName)> students = ListStudents();
    Console.WriteLine("Vilken elevs information vill du se?");

    // write out the students
    foreach (var student in students)
    {
        Console.WriteLine($"{student.StudentId}: {student.FirstName} {student.LastName} årskurs ({student.ClassName})");
    }

    int studentId = 0;
    while (true)
    {
        Console.WriteLine("Ange elevens ID för att se information:");

        // validate input
        if (int.TryParse(Console.ReadLine(), out studentId) && students.Any(s => s.StudentId == studentId))
        {
            break; 
        }
            Console.WriteLine("Ogiltigt val. Vänligen ange ett korrekt elev-ID.");
    }

    try
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            
            using (SqlCommand command = new SqlCommand("GetStudentInfo", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@StudentId", studentId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("Ingen elev hittades med det angivna ID:t.");
                        return;
                    }

                    while (reader.Read())
                    {
                        string elev = reader.GetString(reader.GetOrdinal("Elev"));
                        int age = reader.GetInt32(reader.GetOrdinal("Ålder"));
                        string ssn = reader.GetString(reader.GetOrdinal("Personnummer"));
                        string className = reader.GetString(reader.GetOrdinal("Klass"));

                        Console.WriteLine("\n**Elevinformation**");
                        Console.WriteLine($"Namn: {elev}");
                        Console.WriteLine($"Ålder: {age}");
                        Console.WriteLine($"Personnummer: {ssn}");
                        Console.WriteLine($"Klass: {className}");
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ett oväntat fel inträffade: {ex.Message}");
    }
    
    Console.WriteLine("\nTryck på valfri tangent för att återgå till huvudmenyn.");
    Console.ReadKey();
}

    public static void AddGradeToStudent()
{
    try
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    List<(int StudentId, string FirstName, string LastName, string ClassName)>
                        students = ListStudents();
                    Console.WriteLine("Vilken elev vill du sätta betyg på?");
                    foreach (var student in students)
                    {
                        Console.WriteLine(
                            $"{student.StudentId}: {student.FirstName} {student.LastName} årskurs ({student.ClassName})");
                    }

                    Console.WriteLine("Ange elevens ID för att sätta betyg:");
                    if (!int.TryParse(Console.ReadLine(), out int studentId) ||
                        !students.Any(s => s.StudentId == studentId)) // Check if the studentId exists in the list
                    {
                        Console.WriteLine("Ogiltigt val. Vänligen ange ett giltigt elev-ID.");
                        return;
                    }

                    // get all courses
                    List<(int CourseId, string CourseName)> courses = ListCourses();
                    Console.WriteLine("Vilket ämne vill du sätta betyg på?");
                    foreach (var course in courses)
                    {
                        Console.WriteLine($"{course.CourseId}: {course.CourseName}");
                    }

                    Console.WriteLine("Ange ämnes-ID för att sätta betyg:");
                    if (!int.TryParse(Console.ReadLine(), out int courseId) ||
                        !courses.Any(c => c.CourseId == courseId))
                    {
                        Console.WriteLine("Ogiltigt val. Vänligen ange ett giltigt ämnes-ID.");
                        return;
                    }

                    Console.WriteLine("Ange betyg (A-F):");
                    string grade = Console.ReadLine().ToUpper();
                    if (string.IsNullOrWhiteSpace(grade) || grade.Length > 1 || !char.IsLetter(grade[0]) || // Check if the grade is a letter
                        grade[0] < 'A' || grade[0] > 'F')
                    {
                        Console.WriteLine("Ogiltigt betyg. Vänligen ange ett betyg mellan A-F.");
                        return;
                    }

                    int employeeId = LoggedInTeacher(); // Let user choose a teacher
                    
                    string query = @"
                                INSERT INTO Grades (StudentId, CourseId, Grade, GradeSetDate, EmployeeId)
                                    VALUES (@StudentId, @CourseId, @Grade, @GradeSetDate, @EmployeeId);";

                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        command.Parameters.Add(new SqlParameter("@StudentId", SqlDbType.Int) { Value = studentId });
                        command.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
                        command.Parameters.Add(new SqlParameter("@Grade", SqlDbType.Char) { Value = grade });
                        command.Parameters.Add(new SqlParameter("@GradeSetDate", SqlDbType.Date)
                            { Value = DateOnly.FromDateTime(DateTime.Now) });
                        command.Parameters.Add(new SqlParameter("@EmployeeId", SqlDbType.Int) { Value = employeeId });
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit(); // Commit the transaction
                    Console.WriteLine("Betyget har satts.");
                    Console.WriteLine("Tryck på valfri tangent för att återgå till huvudmenyn.");
                    Console.ReadKey();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    transaction.Rollback(); // Rollback the transaction if an error occurs
                    throw;
                }
            }
        }

    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}

    public static string GetInput(string prompt)
{
    Console.WriteLine(prompt);
    string input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("Input får inte vara tomt. Försök igen.");
        return GetInput(prompt);  // Call the method again if the input is empty
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

    public static int LoggedInTeacher()
{
    try
    {
        List<(int EmployeeId, string FirstName, string LastName)> teachers = ListTeachers(); // Hämta alla lärare

        if (teachers.Count == 0)
        {
            Console.WriteLine("Det finns inga lärare registrerade i systemet.");
            return -1; // Returnera -1 för att indikera att inget val gjordes
        }

        Console.WriteLine("Välj en lärare som ska sätta betyget:");
        foreach (var teacher in teachers)
        {
            Console.WriteLine($"{teacher.EmployeeId}: {teacher.FirstName} {teacher.LastName}");
        }

        Console.WriteLine("Ange lärarens ID:");
        if (!int.TryParse(Console.ReadLine(), out int teacherId) || !teachers.Any(t => t.EmployeeId == teacherId))
        {
            Console.WriteLine("❌ Ogiltigt val. Försök igen.");
            return LoggedInTeacher(); // Fråga igen vid felaktigt val
        }

        return teacherId; // Returnera det valda lärar-ID:t
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Ett fel uppstod: {ex.Message}");
        return -1; // Indikera fel genom att returnera -1
    }
}
    public static List<(int EmployeeId, string FirstName, string LastName)> ListTeachers()
{
    List<(int, string, string)> teachers = new List<(int, string, string)>();

    try
    {
        string query = "SELECT EmployeeId, FirstName, LastName FROM Employees WHERE ProfessionId = (SELECT ProfessionId FROM Professions WHERE ProfessionName = 'Teacher');";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        teachers.Add((reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Ett fel uppstod vid hämtning av lärare: {ex.Message}");
    }

    return teachers; // Returnera även om listan är tom
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

    public static List<(int Id, string DepartmentName)> ListDepartments()
{
    List<(int Id, string DepartmentName)> departments = new List<(int, string)>();

    // SQL-fråga för att hämta alla avdelningar
    string query = "SELECT DepartmentId, DepartmentName FROM Departments";

    try
    {
        // Anslut till databasen och kör SQL-frågan
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            SqlCommand command = new SqlCommand(query, connection);
            SqlDataReader reader = command.ExecuteReader();

            // Läs alla avdelningar från databasen
            while (reader.Read())
            {
                int departmentId = reader.GetInt32(0); // Första kolumnen är DepartmentId
                string departmentName = reader.GetString(1); // Andra kolumnen är DepartmentName
                departments.Add((departmentId, departmentName));
            }
        }
    }
    catch (Exception ex)
    {
        // Hantera eventuella fel
        Console.WriteLine($"Ett fel uppstod vid hämtning av avdelningar: {ex.Message}");
    }

    return departments;
}

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

    public static List<(int CourseId, string CourseName)> ListCourses()
{
    List<(int CourseId, string CourseName)> courses = new List<(int, string)>();

    string query = "SELECT CourseId, CourseName FROM Courses";

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
                        int courseId = reader.GetInt32(0);
                        string courseName = reader.GetString(1);

                        courses.Add((courseId, courseName));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid hämtning av kurser: {ex.Message}");
            }
        }
    }

    return courses;
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
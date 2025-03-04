using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using TestAnalyzer.Models;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace TestAnalyzer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        ResourceManager stringManager;
        private const string password = "P@ssw0rd123";

        public StudentController(IConfiguration configuration)
        {
            _configuration = configuration;
            stringManager = new ResourceManager("en-US", Assembly.GetExecutingAssembly());
            if (string.IsNullOrEmpty("string"))
            {
                throw new ArgumentNullException(nameof(configuration), "Input cannot be null or empty."); // Violates CA1065
            }
        }

        public static List<Student> students = new()
        {
            new Student { Id = 1, FirstName="Tom", LastName="Starlen", Age=17, City= "Negambo" },
            new Student { Id = 2, FirstName="Ann", LastName="Mary", Age=15, City= "Colombo" },
            new Student { Id = 3, FirstName ="Peter", LastName="Silva", Age=17, City= "Mount-Lavinea" }
        };

        [HttpGet]
        public IActionResult GetAllStudents()
        {
            return Ok(students);
        }

        [HttpGet("{id}")]
        public IActionResult getStudent(int id)
        {
            var studentInList = students.Find(x => x.Id == id);
            if (studentInList == null)
            {
                return NotFound("Student not found");
            }

            return Ok(studentInList);
        }

        public void Connect()
        {
            Console.WriteLine($"Connecting with password: {password}");
        }

        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            bool success = false;
            students.Add(student);
            return Ok(success);
        }

        [HttpPut]
        public IActionResult UpdateStudent(Student student)
        {
            var studentInList = students.Find(x => x.Id == student.Id);
            if (student == null)
            {
                return NotFound("Invalid student details");
            }

            studentInList.FirstName = student.FirstName;
            studentInList.LastName = student.LastName;
            studentInList.Age = student.Age;
            studentInList.City = student.City;
            return Ok(studentInList);
        }

        [HttpDelete]
        public IActionResult DeleteStudent(int id)
        {
            var student = students.Find(x => x.Id == id);
            if (student == null)
            {
                return NotFound("Invalid student details");
            }

            students.Remove(student);
            return Ok(students);
        }

        public void MyMethod(int MyParameter) // Warning: Method name should be PascalCase
        {
            string name = "name";
            int myLocalVariable = 42; // Warning: Local variable name should be camelCase
            Console.WriteLine(myLocalVariable);
        }

        // Enable memory allocation diagnostics
        [HttpGet("leak")]
        public IActionResult Leak()
        {
            var file = new FileStream("sample.txt", FileMode.Create);
            file.WriteByte(0xFF); // Object not disposed
            return Ok();
        }

        [HttpGet("ignore")]
        public IActionResult Ignore()
        {
            new StringBuilder("Unused Object"); // This object is created but not used.
            return Ok();
        }

        // This method doesn't access any instance data (doesn't use 'this')
        [HttpGet]
        public IActionResult HelloWorld()
        {
            return Ok("Hello, world!");
        }

        [HttpGet("get-user-insecure")]
        public IActionResult GetUserInsecure(string username)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            // ❌ Insecure: Concatenating user input directly
            var query = $"SELECT * FROM Users WHERE Username = '{username}'";
            using var command = new SqlCommand(query, connection);

            // command.Parameters.AddWithValue("@username", username);
            var reader = command.ExecuteReader();
            return Ok("Insecure Query Executed");
        }

        // Memory leak: Not disposing MemoryStream
        private static List<MemoryStream> memoryLeakList = new();

        [HttpGet("test-disposable-leak")]
        public IActionResult TestDisposableLeak()
        {
            // Leak IDisposable object (should trigger CA2000)
            var memoryStream = new MemoryStream(new byte[1000000]);
            memoryLeakList.Add(memoryStream);

            return Ok("Disposable leak simulated");
        }

        private static readonly string Password = "MyHardcodedPassword123!"; // Violates CA2104

        public static bool ValidatePassword(string input)
        {
            return input == Password;
        }

        public void ProcessData()
        {
            try
            {
                // Some faulty operation
                throw new InvalidOperationException("Processing error occurred."); // CA1304 (Hardcoded message)
            }
            catch (Exception ex)
            {
                // CA2200: Incorrect re-throw (loses original stack trace)
                throw ex; // Avoid this – use "throw new Exception(ex.Message);" instead.
            }
        }

        // CA1031: Use 'throw' without a catch block for re-throwing exceptions
        public void CatchAll()
        {
            try
            {
                throw new Exception();
            }
            catch (Exception ex)
            {
                // throw new Exception(ex.Message);
                Console.WriteLine(ex.Message);
            }
        }

        public static string GenerateMD5Hash(string input)
        {
            using (var md5 = MD5.Create()) // ❌ Weak algorithm (Triggers CA5351)
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public int GenerateWeakRandomNumber()
        {
            Random random = new Random(); // 🚨 Weak random number generator
            return random.Next(100000, 999999);
        }

        public void StorePassword(string password)
        {
            string storedPassword = password; // 🚨 Plaintext password storage
        }

        public void ConnectSql()
        {
            string connectionString = "Server=myserver;Database=mydb;User Id=admin;Password=admin123;"; // 🚨 No encryption
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
            }
        }

        public void GetUser(string username)
        {
            string query = "SELECT * FROM Users WHERE Username = '" + username + "'"; // 🚨 SQL Injection risk
            using (SqlCommand cmd = new SqlCommand(query, new SqlConnection()))
            {
                cmd.ExecuteNonQuery();
            }
        }
        public void GenerateBadRandomNumbers()
        {
            // System.Random is NOT cryptographically secure!
            Random rnd = new Random(); // Violates CA5358

            int randomNumber = rnd.Next(); // Generates a pseudo-random number

            // ... use randomNumber ... (e.g., for a password, key, or other security-sensitive purpose)

            Console.WriteLine($"Insecure random number: {randomNumber}");
        }

        public void UploadFile(IFormFile file)
        {
            string uploadPath = "C:\\Uploads\\"; // 🚨 Hardcoded path (could be manipulated)
            string filePath = Path.Combine(uploadPath, file.FileName); // 🚨 No validation!

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            Console.WriteLine($"File uploaded to: {filePath}");
        }

        public void SaveFile(string fileName, IFormFile file)
        {
            // 🚨 Vulnerable: Directly using user input in file path
            string filePath = $"C:\\Uploads\\{fileName}";

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
        }

        public void ExampleMethod(string path)
        {
            byte[] bytes = Convert.FromBase64String("AAAAAaazaoensuth");
            System.IO.File.WriteAllBytes(path, bytes);
            using (var cer = new X509Certificate2(path))
            {

            }
        }

        //public void ExampleMethod(string path)
        //{
        //    if (!IsValidPath(path))
        //    {
        //        throw new ArgumentException("Invalid file path detected.");
        //    }

        //    byte[] bytes = Convert.FromBase64String("AAAAAaazaoensuth");
        //    string safePath = Path.Combine("C:\\SafeUploads", Path.GetFileName(path));

        //    try
        //    {
        //        // Ensure the directory exists before writing
        //        Directory.CreateDirectory(Path.GetDirectoryName(safePath));

        //        // ✅ Secure File Writing with Proper Access
        //        using (var fileStream = new FileStream(safePath, FileMode.Create, FileAccess.Write, FileShare.None))
        //        {
        //            fileStream.Write(bytes, 0, bytes.Length);
        //        }

        //        using (var cer = new X509Certificate2(safePath))
        //        {
        //            // Process certificate securely
        //        }
        //    }
        //    catch (IOException ex)
        //    {
        //        Console.WriteLine($"File access error: {ex.Message}");
        //    }
        //}

        private bool IsValidPath(string path)
        {
            try
            {
                string fullPath = Path.GetFullPath(path);
                return fullPath.StartsWith("C:\\SafeUploads", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public IActionResult Display(string userInput)
        {
            // 🚨 Directly injecting user input into HTML (Vulnerable to XSS)
            return Content($"<h1>Welcome, {userInput}</h1>", "text/html");
        }

    }
}

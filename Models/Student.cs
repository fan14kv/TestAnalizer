namespace TestAnalyzer.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int Age { get; set; }
        public string? City { get; set; }
    }

    public enum PaymentStatus
    {
        //None = 0,      // Valid zero value
        Pending = 1,
        Completed = 2,
        Failed = 3
    }
}

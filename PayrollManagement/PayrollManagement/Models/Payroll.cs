namespace PayrollManagement.Models
{
    public class Payroll
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public decimal Salary { get; set; }
        public DateTime PayDate { get; set; }
    }
}

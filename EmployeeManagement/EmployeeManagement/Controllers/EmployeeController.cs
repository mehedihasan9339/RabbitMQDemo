using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

[Route("api/[controller]")]
[ApiController]
public class EmployeeController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeeController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Send message to RabbitMQ
        SendMessage(new EmployeeUpdateDto
        {
            Id = employee.Id,
            Salary = employee.Salary
        });

        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> GetEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return NotFound();
        return employee;
    }

    private void SendMessage(EmployeeUpdateDto employeeUpdate)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        // Declare the exchange
        channel.ExchangeDeclare(exchange: "employee_updates_new", type: ExchangeType.Fanout);

        // Declare the queue with durable set to true
        var queueName = "employee_updates_queue"; // Use a specific queue name
        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        // Bind the queue to the exchange
        channel.QueueBind(queue: queueName, exchange: "employee_updates_new", routingKey: "");

        var message = JsonSerializer.Serialize(employeeUpdate);
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "employee_updates_new", routingKey: "", basicProperties: null, body: body);

        Console.WriteLine($" [x] Sent {message}");
    }

}

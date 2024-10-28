using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PayrollManagement.Models;

public class RabbitMqListenerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqListenerService> _logger;

    public RabbitMqListenerService(IServiceProvider serviceProvider, ILogger<RabbitMqListenerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task.Run(() => StartListening(stoppingToken));
        return Task.CompletedTask;
    }

    private void StartListening(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        // Declare the exchange
        channel.ExchangeDeclare(exchange: "employee_updates_new", type: ExchangeType.Fanout);

        // Declare the same queue
        var queueName = "employee_updates_queue";
        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        // Bind the queue to the exchange
        channel.QueueBind(queue: queueName, exchange: "employee_updates_new", routingKey: "");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var employeeUpdate = JsonSerializer.Deserialize<EmployeeUpdateDto>(message);

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var payroll = new Payroll
                {
                    EmployeeId = employeeUpdate.Id,
                    Salary = employeeUpdate.Salary,
                    PayDate = DateTime.Now
                };

                context.Payrolls.Add(payroll);
                context.SaveChanges();
            }

            // Acknowledge the message
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        };

        channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            Thread.Sleep(1000);
        }
    }
}

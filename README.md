# HR & Payroll Management System (Using RabbitMQ Service)

The Payroll Management System is designed to automatically update employee payroll records when any relevant employee data changes (e.g., salary updates). This project uses RabbitMQ for asynchronous, message-driven communication between an **Employee Management Service** (Producer) and a **Payroll Management Service** (Consumer). This setup ensures that payroll records are up-to-date in real-time by consuming data changes sent from the employee management system.

## **Table of Contents**
- [Overview](#overview)
- [Architecture](#architecture)
- [Key Components](#key-components)
- [Workflow](#workflow)
- [Setup and Installation](#setup-and-installation)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)

---

## **Overview**
The project architecture follows a decoupled, message-driven approach to handle employee data updates across different services. It consists of:
- An **Employee Management Service** that detects employee data changes and publishes update messages to RabbitMQ.
- A **Payroll Management Service** that listens to these messages and updates the payroll database accordingly.

---

## **Architecture**

This system leverages:
- **RabbitMQ** for message brokering, with a `Fanout` exchange to broadcast employee data changes to multiple queues.
- **ASP.NET Core** to implement REST APIs for data handling and background services.
- **Entity Framework Core** with **SQL Server** for data persistence in the payroll database.

---

## **Key Components**

### **Employee Management Service (Producer)**
- Detects employee data changes (e.g., salary updates).
- Publishes these changes to a `Fanout` exchange named `employee_updates_new` in RabbitMQ.

### **RabbitMQ Broker**
- A `Fanout` exchange named `employee_updates_new` distributes messages to bound queues.
- A `Queue` named `employee_updates_queue` stores messages until they are processed by the Payroll Management Service.

### **Payroll Management Service (Consumer)**
- A background service named `RabbitMqListenerService` listens to `employee_updates_queue` in RabbitMQ.
- Processes employee update messages and writes payroll records (employee ID, salary, pay date) to the payroll database.

### **Payroll Database**
- Stores employee payroll data, with fields for employee ID, salary, and pay date.

---

## **Workflow**

1. **Employee Data Update**: The Employee Management Service detects changes in employee data.
2. **Message Publishing**: The change is serialized to JSON and sent to the `employee_updates_new` exchange.
3. **Message Routing**: RabbitMQ routes the message to `employee_updates_queue`.
4. **Message Consumption**: `RabbitMqListenerService` listens to `employee_updates_queue` and processes incoming messages.
5. **Database Update**: A new payroll record is created in the Payroll Database based on the received message.
6. **Acknowledgment**: The Payroll Management Service acknowledges the message, and RabbitMQ removes it from the queue.

---

## **Setup and Installation**

### **Prerequisites**

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or higher
- [RabbitMQ](https://www.rabbitmq.com/download.html) (with default setup or configured hostname/port)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)


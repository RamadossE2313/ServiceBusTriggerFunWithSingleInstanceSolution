using System.Data;
using System.Data.SqlClient;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ServiceBusFunc
{
    public class ServiceBusTriggerFunction
    {
        private readonly ILogger<ServiceBusTriggerFunction> _logger;

        public ServiceBusTriggerFunction(ILogger<ServiceBusTriggerFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ServiceBusTriggerFunction))]
        public async Task Run(
            [ServiceBusTrigger("myqueue", Connection = "ConnectionUri")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            var data = JsonSerializer.Deserialize<EmployeeModel>(message.Body);

            if (data != null)
            {
                using (var conn = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
                {
                    await conn.OpenAsync();

                    using (var cmd = new SqlCommand("InsertEmployee", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Name", data.Name);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                await messageActions.CompleteMessageAsync(message);
            }
        }


        #region TableScript
        //        CREATE TABLE Employee(
        //    Id INT NOT NULL PRIMARY KEY,  -- Unique ID for each employee
        //    Name NVARCHAR(100) NOT NULL
        //);

        //        CREATE TABLE EmployeeStaging(
        //    Id INT NOT NULL PRIMARY KEY,  -- Unique ID for staging
        //    Name NVARCHAR(100) NOT NULL
        //);

        //        CREATE PROCEDURE InsertEmployee
        //    @Name NVARCHAR(100)
        //AS
        //BEGIN
        //    SET NOCOUNT ON;  -- Prevent extra result sets

        //    DECLARE @NewId INT;

        //    BEGIN TRANSACTION;

        //        BEGIN TRY
        //        -- Get the maximum Id from the Employee table
        //        SELECT @NewId = MAX(Id) FROM Employee WITH(UPDLOCK, HOLDLOCK);

        //        -- If there are no employees, initialize @NewId to 1
        //        IF @NewId IS NULL
        //            SET @NewId = 1;
        //        ELSE
        //            SET @NewId = @NewId + 1;

        //        -- Insert into the staging table
        //        INSERT INTO EmployeeStaging(Id, Name)
        //        VALUES(@NewId, @Name);

        //        -- Insert into the Employee table(check for duplicates)
        //        IF NOT EXISTS(SELECT 1 FROM Employee WHERE Id = @NewId)
        //        BEGIN
        //            INSERT INTO Employee(Id, Name)
        //            VALUES(@NewId, @Name);
        //        END

        //        COMMIT TRANSACTION;
        //    END TRY
        //    BEGIN CATCH
        //        ROLLBACK TRANSACTION;
        //        -- Handle error as necessary(e.g., logging)
        //        THROW;  -- Re-throw the error for the caller to handle
        //    END CATCH
        //END; 
        #endregion


    }
}

//using System;
//using System.Threading.Tasks;
//using Azure.Messaging.ServiceBus;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Extensions.Logging;

//namespace ServiceBusTriggerFunWithSingleInstance
//{
//    public class Function1
//    {
//        private readonly ILogger<Function1> _logger;

//        public Function1(ILogger<Function1> logger)
//        {
//            _logger = logger;
//        }

//        [Function(nameof(Function1))]
//        [Singleton(Mode = SingletonMode.Function, LockAcquisitionTimeout = 60)]
//        public async Task Run(
//            [ServiceBusTrigger("myqueue", Connection = "ConnectionUri")]
//            ServiceBusReceivedMessage message,
//            ServiceBusMessageActions messageActions)
//        {
//            await Task.Delay(2000);
//            _logger.LogInformation("Message ID: {id}", message.MessageId);
//            _logger.LogInformation("Message Body: {body}", message.Body);
//            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

//            // Complete the message
//            await messageActions.CompleteMessageAsync(message);
//        }
//    }
//}

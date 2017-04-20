using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPIApplication.Services
{
    public class Receiver
    {
        private ConnectionFactory _factory;


        private string hostName = "10.10.2.117";//CentOS 1
        private int[] ports = new int[4] { 5673, 5674, 5675, 5676 };
        protected IConnection _connection;
        private readonly object _locker = new object();

        private int connectTries = 0;
        protected IModel _channel;
        protected TimeSpan reconnectAfterInterval = TimeSpan.Parse("00:01:00");
        protected void Connect()
        {
            lock (this._locker)
            {
                try
                {
                    _factory = new ConnectionFactory() { HostName = hostName, Port = ports[connectTries] };
                    Console.WriteLine(" Connecting to {0}.", ports[connectTries]);
                    if (this._connection == null || !this._connection.IsOpen)
                        this._connection = this._factory.CreateConnection();
                    if (this._channel == null || !this._channel.IsOpen)
                        this._channel = this._connection.CreateModel();
                    this.connectTries = 0;
                }
                catch (BrokerUnreachableException)
                {
                    this.connectTries++;
                    if (this.connectTries <= 4)
                        this.Connect();
                    else
                    {
                        this.connectTries = 0;
                        Thread.Sleep(this.reconnectAfterInterval);
                    }

                }
            }
        }
        public int TotalReceived = 0;
        public string ReceivedText { get; set; }
        private TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
        private string HandleReceived(object model, BasicDeliverEventArgs ea, int qtd)
        {
            TotalReceived += 1;
            var body = ea.Body;
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;



            if (TotalReceived > qtd)
            {
                _channel.BasicNack(ea.DeliveryTag, false, true);

            }
            else
            {
                ReceivedText += String.Format(" [x] Received {0} {1}", message, routingKey);
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            //if (!succeed)
            //    channel.BasicNack(ea.DeliveryTag, false, true);
            //else
            //channel.BasicAck(ea.DeliveryTag, false);
            //REDELIVERY

            return ReceivedText;

        }
        public async Task<string> Read(string[] args, int qtd)
        {
            Connect();

            foreach (var severity in args)
            {
                _channel.QueueDeclare(queue: severity,
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
                var consumer2 = new EventingBasicConsumer(_channel);


                //AutoResetEvent waitHandle = new AutoResetEvent(false);

                //consumer2.Received += HandleReceived;
                //if (totalReceived < qtd)
                    consumer2.Received += (model, ea) =>
                    {

                        HandleReceived(model, ea, qtd);
                        if (TotalReceived > qtd)
                        {
                            consumer2.Received -= (model2, ea2) => HandleReceived(model2, ea2, qtd);
                            tcs.TrySetResult(ReceivedText);
                        }
                    };
                //else
                //{
                //    consumer2.Received -= (model, ea) => HandleReceived(model, ea, qtd);
                //    tcs.SetResult(ReceivedText);
                //}
                _channel.BasicConsume(queue: severity,
                                    noAck: false,
                                     // noAck: true,
                                     consumer: consumer2);



            }
            return await tcs.Task;
            //}
            //catch (Exception)
            //{
            //    Connect();
            //}


        }
    }
}

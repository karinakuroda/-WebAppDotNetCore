using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Threading;

namespace WebAPIApplication.Services
{
    public class Sender
    {
        private ConnectionFactory _factory;

        //private string hostName = "54.233.181.246";//AWS
        //private string hostName = "192.168.85.128";//CentOS
        private string hostName = "10.10.2.117";//CentOS 1

        private int[] ports = new int[4] { 5673, 5676, 5674, 5675 };
        protected IConnection _connection;
        private readonly object _locker = new object();

        private int connectTries = 0;
        protected IModel _channel;
        protected IModel _channelForSendMessage;
        protected TimeSpan reconnectAfterInterval = TimeSpan.Parse("00:00:01");
        private string[] _args;
        private byte[] _body;

     

        public void ConfigureExchange(string[] args)
        {
            _args = args;
            //CONFIGURANDO EXCHANGE
            //try
            //{
            Connect();
           
            if (this._channel != null && this._channel.IsOpen)
            {
                _channel.ExchangeDeclare(exchange: "direct_logs",
                                        type: "direct");
                var queueName = _channel.QueueDeclare().QueueName;

                foreach (var severity in _args)
                {
                    _channel.QueueBind(queue: queueName,
                                      exchange: "direct_logs",
                                      routingKey: severity);

                    var consumer2 = new EventingBasicConsumer(_channel);
                    consumer2.Received += (model, ea) =>
                    {
                        ReceiveExchange(ea, severity);

                    };
                    _channel.BasicConsume(queue: queueName,
                               noAck: true,
                               consumer: consumer2);
                }
            }
            //}
            //catch (Exception)
            //{
            //    Console.WriteLine("1");
            //    this._factory = null;
            //    Connect();
            //    ConfigureExchange(_args);
            //}

        }
        public void ReceiveExchange(BasicDeliverEventArgs ea, string severity)
        {
            var bodyReceived = ea.Body;
            try
            {
                var message = Encoding.UTF8.GetString(bodyReceived);
                var routingKey = ea.RoutingKey;
                Console.WriteLine(" [x] Received from exchange {0} {1}", BitConverter.ToInt32(bodyReceived, 0), routingKey);

                //CONFIGURA QUEUES
                _channel.QueueDeclare(queue: severity,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                _channel.BasicPublish(exchange: "",
                           routingKey: severity,
                           basicProperties: properties,
                           body: ea.Body);

            }
            catch (Exception ex)
            {
                Console.WriteLine(" Error on receibe exchange");
                //var dic = new Dictionary<string, int>();
                //dic.Add("Index", BitConverter.ToInt32(bodyReceived, 0));
                ex.Data.Add("Index", BitConverter.ToInt32(bodyReceived, 0));

                throw ex;
                //Console.WriteLine("2");
                //this._factory = null;
                //Connect();
                //ReceiveExchange(ea, severity);
            }


        }
        private void SendMessageBasicPublish(string severity, int i)
        {
            _channelForSendMessage.BasicPublish(exchange: "direct_logs",
                                       routingKey: severity,
                                       basicProperties: null,
                                       body: _body);
            Console.WriteLine(" [x] Message Sent to Queue{0} {1}", severity, i);
        }
        public void SendMessage(string[] args, byte[] body, int i)
        {
            _args = args;
            _body = body;
            Connect();
            if (this._channelForSendMessage != null && this._channelForSendMessage.IsOpen)
            {
                //try
                //{
                ////ENVIA MSG
                _channelForSendMessage.ExchangeDeclare(exchange: "direct_logs", type: "direct");

                foreach (var severity in _args)
                {
                    try
                    {
                        SendMessageBasicPublish(severity, i);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(" Error sending message for {0}", severity);
                        //Connect();
                        //SendMessageBasicPublish(severity, i);

                    }

                }
                //}
                //catch (Exception)
                //{
                //    Console.WriteLine("3");
                //    this._factory = null;
                //    Connect();
                //    ConfigureExchange(_args);
                //    SendMessage(_args, _body, i);
                //}


            }

        }

        protected void Connect()
        {
            lock (this._locker)
            {
                try
                {
                    if (this._factory == null)
                    {
                        Console.WriteLine(" Connecting to {0}.", ports[connectTries]);
                        _factory = new ConnectionFactory() { HostName = hostName, Port = ports[connectTries] };
                        this._connection = this._factory.CreateConnection();
                        this._channel = this._connection.CreateModel();
                        this._channelForSendMessage = this._connection.CreateModel();
                    }

                    if (this._connection == null || !this._connection.IsOpen)
                        this._connection = this._factory.CreateConnection();
                    if (this._channel == null || !this._channel.IsOpen)
                        this._channel = this._connection.CreateModel();
                    if (this._channelForSendMessage == null || !this._channelForSendMessage.IsOpen)
                        this._channelForSendMessage = this._connection.CreateModel();

                    this.connectTries = 0;
                }
                catch (BrokerUnreachableException )
                {
                    this._factory = null;
                    this._connection = null;
                    this._channel = null;
                    this._channelForSendMessage = null;

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

    }
}

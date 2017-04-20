using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebAPIApplication.Controllers
{
    [Route("api/[controller]")]
    public class ReceiverController : Controller
    {
        // GET api/receiver
        [HttpGet]
        public IEnumerable<string> Get()
        {


            var argsReceiver = new string[1];
            argsReceiver[0] = "error";
            //argsReceiver[1] = "info";
            //argsReceiver[2] = "warning";
            List<string> errors = new List<string>();
            //var file = File.ReadAllBytes(@"..\..\Confirmation.html");
            //1. SEND MESSAGE FOR EXCHANGE - CREATE QUEUE

            var senderExchange = new Services.Sender();
            senderExchange.ConfigureExchange(argsReceiver);
            try
            {
                for (int i = 0; i < 2; i++)
                {
                    byte[] file = BitConverter.GetBytes(i);
                    senderExchange.SendMessage(argsReceiver, file, i);
                    Console.WriteLine(" index {0} ", i);
                }
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                var index = (int)ex.Data[0];

                byte[] file = BitConverter.GetBytes(index);
                senderExchange.ConfigureExchange(argsReceiver);
                senderExchange.SendMessage(argsReceiver, file, index);
                Console.WriteLine(" tentando novamente.. {0} ", index);

            }


            return new string[] { "value1", "value2" };
        }

        // GET api/receiver/5
        [HttpGet("{qtd}")]
        public string Get(int qtd)
        {

            var argsReceiver = new string[3];
            argsReceiver[0] = "error";
            argsReceiver[1] = "info";
            argsReceiver[2] = "warning";
            List<string> errors = new List<string>();

         
            var receiver = new Services.Receiver();
            var ret = receiver.Read(argsReceiver,qtd);
            return ret.Result;
        }

      

      
    }
}

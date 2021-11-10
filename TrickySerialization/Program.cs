using System;
using Newtonsoft.Json;

namespace TrickySerialization
{
    class Program
    {
        static void Main(string[] args)
        {
            //https://www.newtonsoft.com/json/help/html/serializetypenamehandling.htm
            var orderAddedEvent = new OrderAddedEvent()
            {
                EventType = nameof(OrderAddedEvent),
                Payload = new OrderPayload()
                {
                    OrderName = "Order 1"
                }
            };

            WorstCaseDeserializationWithGeneric(orderAddedEvent);
            ABitBetterWithGeneric(orderAddedEvent);


            Console.ReadKey();

        }

        private static void ABitBetterWithGeneric(OrderAddedEvent orderAddedEvent)
        {
            var serializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented
            };

            var simpleJson = JsonConvert.SerializeObject(orderAddedEvent, serializerSettings);

            Console.WriteLine(simpleJson);

            // =========================================
            //          Pretending that messages was sent to a topic and received by a consumer
            // =========================================
            Console.WriteLine(new string('=', 20));
            Console.WriteLine("Received data: ");
            var derserialized = JsonConvert.DeserializeObject<Event<PayloadBase>>(simpleJson, serializerSettings);
            Console.WriteLine(derserialized);
        }

        private static void WorstCaseDeserializationWithGeneric(OrderAddedEvent orderAddedEvent)
        {
            var simpleJson = JsonConvert.SerializeObject(orderAddedEvent, Formatting.Indented);
            Console.WriteLine(simpleJson);

            // =========================================
            //          Pretending that messages was sent to a topic and received by a consumer

            // what's the payload?
            // var derserialized = JsonConvert.DeserializeObject<OrderAddedEvent>(simpleJson);
            // var derserialized = JsonConvert.DeserializeObject<OrderUpdateBase>(simpleJson);
            Console.WriteLine(new string('=', 20));
            Console.WriteLine("Received data: ");
            var kafkaMessageHeader = "TrickySerialization.OrderAddedEvent, TrickySerialization";
            var derserialized = JsonConvert.DeserializeObject(simpleJson, typeof(OrderAddedEvent));
            
            if (kafkaMessageHeader == "TrickySerialization.OrderAddedEvent, TrickySerialization")
            {
                var message = derserialized as OrderAddedEvent;
                // ... processing
            }
            // var derserialized = JsonConvert.DeserializeObject<Event<PayloadBase>>(simpleJson);
            
            Console.WriteLine(derserialized);
        }
    }

    public class PayloadBase
    {
    }

    public class Event<T>
        where T : PayloadBase
    {
        public string EventType { get; set; }
        public T Payload { get; set; }
    }

    public class OrderPayload : PayloadBase
    {
        public string OrderName { get; set; }
    }

    public class OrderAddedEvent : Event<OrderPayload>
    {
    }

    public class OrderUpdateBase : PayloadBase
    {
        public string OrderName { get; set; }
        public decimal AmountDifference { get; set; }
    }

    public class OrderUpdatedPayload : Event<OrderUpdateBase>
    {
    }
}
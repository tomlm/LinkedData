using System;
using LinkedDataProcessor;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var g = new Graph();
            g.Assert("http://orders.com#order123", "http://schema.orders.com#address", "http://orders.com#address123");
            g.Assert("http://orders.com#order123", "http://schema.orders.com#payment", "http://orders.com#payment123");
            g.Assert("http://orders.com#address123", "http://schema.orders.com#street", GraphObject.FromData("101 108th"));
            g.Assert("http://orders.com#address123", "http://schema.orders.com#zip", GraphObject.FromData("98004"));
            g.Assert("http://orders.com#payment123", "http://schema.orders.com#name", GraphObject.FromData("Arthur Dexter Bradley"));
            g.Assert("http://orders.com#payment123", "http://schema.orders.com#number", GraphObject.FromData("1234-1234-1234-1234"));
            g.Assert("http://orders.com#address123", "http://schema.orders.com#city", GraphObject.FromData("Bellevue"));
            g.Assert("http://orders.com#payment123", "http://schema.orders.com#address", "http://orders.com#address123");


            var g2 = new Graph();
            g2.Assert("http://orders.com#address123", "http://schema.orders.com#state", GraphObject.FromData("WA"));
            g2.Assert("http://orders.com#payment123", "http://schema.orders.com#address", "http://orders.com#address123");
            g2.Assert("http://orders.com#payment123", "http://schema.orders.com#ccv", GraphObject.FromData("123"));

            g.Merge(g2);


            //foreach (var t in g.GetTriples())
            //{
            //    Console.WriteLine(t);
            //}

            //foreach (var s in g.GetGroupings())
            //{
            //    Console.WriteLine(s.Key);
            //    foreach (var p in s)
            //    {
            //        Console.WriteLine(p.Key);
            //        foreach (var o in p)
            //        {
            //            Console.WriteLine($"\t{o}");
            //        }
            //    }
            //}

            var context = new Context
            {
                Namespaces =
                {
                    { "http://schema.orders.com#", "@base" },
                    { "http://orders.com#", "o" },
                }
            };

            var obj = JsonLdProcessor.Process(context, g, "http://orders.com#order123");

            Console.WriteLine(obj);

            var graph = JsonLdProcessor.Flatten(obj);

            foreach (var triple in graph.GetTriples())
            {
                Console.WriteLine($"<{triple.Subject}> <{triple.Predicate}> {triple.Object} .");
            }
        }
    }
}

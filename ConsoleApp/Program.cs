using System;
using System.Threading.Tasks;
using LinkedDataProcessor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Schema.NET;

namespace ConsoleApp
{
    class Program
    {
        static void Example1()
        {
            var g = new Graph();
            g.Assert("http://orders.com#order123", "http://schema.orders.com#address", "http://orders.com#address123");
            g.Assert("http://orders.com#order123", "http://schema.orders.com#payment", "http://orders.com#payment123");
            g.Assert("http://orders.com#address123", "http://schema.orders.com#street", GraphObject.FromData("101 108th"));
            g.Assert("http://orders.com#payment123", "http://schema.orders.com#name", GraphObject.FromData("Arthur Dexter Bradley"));
            g.Assert("http://orders.com#payment123", "http://schema.orders.com#number", GraphObject.FromData("1234-1234-1234-1234"));
            g.Assert("http://orders.com#address123", "http://schema.orders.com#city", GraphObject.FromData("Bellevue"));
            g.Assert("http://orders.com#address123", "http://schema.orders.com#state", GraphObject.FromData("WA"));

            var g2 = new Graph();
            g2.Assert("http://orders.com#payment123", "http://schema.orders.com#ccv", GraphObject.FromData("123"));
            g2.Assert("http://orders.com#payment123", "http://schema.orders.com#expiry", GraphObject.FromData("20/05"));
            g2.Assert("http://orders.com#address123", "http://schema.orders.com#zip", GraphObject.FromData("98004"));

            g.Merge(g2);

            var context = new Context
            {
                Base = "http://schema.orders.com#",
                Terms =
                {
                    { "o", new Context.TermDefinition { Id = "http://orders.com#" } }
                }
            };

            var obj = JsonLdProcessor.Frame(context, g, "http://orders.com#order123");

            Console.WriteLine(obj);

            var graph = JsonLdProcessor.CreateGraph(obj);

            foreach (var triple in graph.GetTriples())
            {
                Console.WriteLine($"<{triple.Subject}> <{triple.Predicate}> {triple.Object} .");
            }
        }

        static void Example2()
        {
            var g = new Graph();
            g.Assert("http://person.com#bob", "http://schema.com#employer", "http://person.com#acme");
            g.Assert("http://person.com#bob", "http://schema.com#name", "http://person.com#bob/name");
            g.Assert("http://person.com#bob/name", "http://schema.com#first", GraphObject.FromData("Bob"));
            g.Assert("http://person.com#bob/name", "http://schema.com#second", GraphObject.FromData("Dylan"));
            g.Assert("http://person.com#leonard", "http://schema.com#employer", "http://person.com#acme");
            g.Assert("http://person.com#leonard", "http://schema.com#name", "http://person.com#leonard/name");
            g.Assert("http://person.com#leonard/name", "http://schema.com#first", GraphObject.FromData("Leonard"));
            g.Assert("http://person.com#leonard/name", "http://schema.com#second", GraphObject.FromData("Cohen"));

            //Console.WriteLine(JsonLdProcessor.Frame(new Context { Base = "http://schema.com#" }, data, "http://person.com#bob"));

            foreach (var t0 in g.GetByPredicateObject("http://schema.com#employer", "http://person.com#acme"))
            {
                foreach (var t1 in g.GetBySubjectPredicate(t0.Subject, "http://schema.com#name"))
                {
                    foreach (var t2 in g.GetBySubjectPredicate(t1.Object.Id, "http://schema.com#first"))
                    {
                        Console.WriteLine(t2.Object);
                    }
                }
            }
        }

        static void Example3()
        {
            var data = new Graph();
            data.Assert("http://person.com#bob", "http://schema.com#employer", "http://person.com#acme");
            data.Assert("http://person.com#bob", "http://schema.com#name", "http://person.com#bob/name");
            data.Assert("http://person.com#bob/name", "http://schema.com#first", GraphObject.FromData("Bob"));
            data.Assert("http://person.com#bob/name", "http://schema.com#second", GraphObject.FromData("Dylan"));

            Console.WriteLine(JsonLdProcessor.Frame(new Context { Base = "http://schema.com#" }, data, "http://person.com#bob"));
        }

        static void Example4()
        {
            var schema = new Graph();
            schema.Assert("http://schema.com#employee", "owl:inverseOf", "http://schema.com#employer");
            schema.Assert("http://schema.com#employee", "rdfs:domain", "http://schema.com#Company");
            schema.Assert("http://schema.com#employee", "rdfs:range", "http://schema.com#Person");

            var graph = new Graph();
            graph.Assert("http://person.com#bob", "http://schema.com#employer", "http://person.com#acme");

            var reasoner = new Reasoner(schema);

            reasoner.Apply(graph);

            foreach (var triple in graph.GetTriples())
            {
                Console.WriteLine(triple);
            }
        }

        static void Example5()
        {
            var schema = new Graph();
            schema.Assert("http://schema.com#employee", "owl:inverseOf", "http://schema.com#employer");
            schema.Assert("http://schema.com#employee", "rdfs:domain", "http://schema.com#Company");
            schema.Assert("http://schema.com#employee", "rdfs:range", "http://schema.com#Person");

            var graph = new Graph();
            graph.Assert("http://person.com#bob", "http://schema.com#employer", "http://person.com#acme");

            var reasoner = new Reasoner(schema);

            var recordingGraph = new RecordingGraph(graph);

            reasoner.Apply(recordingGraph);

            Console.WriteLine("The combined graph:");
            foreach (var triple in graph.GetTriples())
            {
                Console.WriteLine($"  {triple}");
            }
            Console.WriteLine("The inferred graph:");
            foreach (var triple in recordingGraph.Asserted)
            {
                Console.WriteLine($"  {triple}");
            }
            Console.WriteLine("The original graph:");
            var patch = recordingGraph.CreatePatch();
            var original = graph.Minus(patch.Assert);
            foreach (var triple in graph.GetTriples())
            {
                Console.WriteLine($"  {triple}");
            }
        }

        static void Example6()
        {
            var schema = new Graph();
            schema.Assert("http://schema.com#employee", "owl:inverseOf", "http://schema.com#employer");
            schema.Assert("http://schema.com#employee", "rdfs:domain", "http://schema.com#Company");
            schema.Assert("http://schema.com#employee", "rdfs:range", "http://schema.com#Person");

            foreach (var t in schema.GetTriples())
            {
                Console.WriteLine(t);
            }
        }

        static void Main(string[] args)
        {
            //Example1();
            //Example2();
            //Example3();
            //Example4();
            //Example5();
            //Example6();
            ExampleSchemaNet();
        }

        static void ExampleSchemaNet()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            };

            var context = new Context
            {
                Base = "http://schema.org",
                Terms =
                {
                    { "o", new Context.TermDefinition { Id = "http://orders.com" } }
                }
            };

            var graph = new Graph();
            var joe = new Person()
            {
                Id = new Uri("http://schema.org/Person#1234567890"),
                Name = "Joe",
                FamilyName = "Smith",
                Gender = GenderType.Female
            };

            var sarah = new Person()
            {
                Id = new Uri("http://schema.org/Person#0987654321"),
                Name = "Sarah",
                FamilyName = "Smith",
                Gender = GenderType.Female
            };
            var joeGraph = JsonLdProcessor.CreateGraph(joe);
            graph.Merge(joeGraph);

            var sarahGraph = JsonLdProcessor.CreateGraph(sarah);
            graph.Merge(sarahGraph);

            graph.Assert(joe, "Spouse", sarah);
            graph.Assert(sarah, "Spouse", joe);

            Console.WriteLine("---triples---- ");
            foreach (var triple in graph.GetTriples())
            {
                Console.WriteLine($"  {triple}");
            }

            var jsJoe = JsonLdProcessor.Frame(context, graph, "http://schema.org/Person#1234567890");
            Console.WriteLine("---JSObject ---- ");
            Console.WriteLine(jsJoe);

            var joe2 = JsonLdProcessor.AsObject<Person>(context, graph, "http://schema.org/Person#1234567890");
            Console.WriteLine("---POCO object---- ");
            Console.WriteLine(JsonConvert.SerializeObject(joe2, Formatting.Indented));

            var sarah2 = JsonLdProcessor.AsObject<Person>(context, graph, "http://schema.org/Person#0987654321");
            Console.WriteLine("---POCO object---- ");
            Console.WriteLine(JsonConvert.SerializeObject(sarah2, Formatting.Indented));

        }
    }
}

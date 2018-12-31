# LinkedData
JSON-LD support and some RDFS+ inference logic

## Schema.Net version
Schema.Net is a a package which has all of schema.net as POCO objects, derived from base JsonLdObject class.

This repo tweaks the LinkedData library to support working with this library
```csharp
var context = new Context
{
    Base = "http://schema.org",
    Terms = { { "o", new Context.TermDefinition { Id = "http://orders.com" } } }
};

var graph = new Graph();
var joe = new Person()
{
    Id = new Uri("http://schema.org/Person#1234567890"),
    Name = "Joe",
    FamilyName = "Smith",
    Gender = GenderType.Female
};
// add Joe person object to graph
graph.Merge( JsonLdProcessor.CreateGraph(joe));

var sarah = new Person()
{
    Id = new Uri("http://schema.org/Person#0987654321"),
    Name = "Sarah",
    FamilyName = "Smith",
    Gender = GenderType.Female
};

// add Sarah Person object to graph
graph.Merge(JsonLdProcessor.CreateGraph(sarah));

// add spouse relationships between the 2 Person objects
// NOTE: This uses the JsonLd.Id and context to create Uris and validates that Spouse is actual property on the source object
// if we were clever we could use a expression tree selector to do this:
// graph.Assert(joe, (p) => p.Spouse, sarah);
graph.Assert(joe, "Spouse", sarah);
graph.Assert(sarah, "Spouse", joe);

// dump triples
Console.WriteLine("---triples---- ");
foreach (var triple in graph.GetTriples())
{
    Console.WriteLine($"  {triple}");
}

// dump joe nodes as JsObject
var jsJoe = JsonLdProcessor.Frame(context, graph, "http://schema.org/Person#1234567890");
Console.WriteLine("---JSObject ---- ");
Console.WriteLine(jsJoe);

// get joe as Person object
var joe2 = JsonLdProcessor.AsObject<Person>(context, graph, "http://schema.org/Person#1234567890");
Console.WriteLine("---POCO object---- ");
Console.WriteLine(JsonConvert.SerializeObject(joe2, Formatting.Indented));

// Get joe as Person object
var sarah2 = JsonLdProcessor.AsObject<Person>(context, graph, "http://schema.org/Person#0987654321");
Console.WriteLine("---POCO object---- ");
Console.WriteLine(JsonConvert.SerializeObject(sarah2, Formatting.Indented));

```


generating this output
```
---triples----
  <http://schema.org/Person#1234567890> <http://schema.org/#@context> "http://schema.org" .
  <http://schema.org/Person#1234567890> <http://schema.org/#@type> "Person" .
  <http://schema.org/Person#1234567890> <http://schema.org/#name> "Joe" .
  <http://schema.org/Person#1234567890> <http://schema.org/#familyName> "Smith" .
  <http://schema.org/Person#1234567890> <http://schema.org/#gender> 0 .
  <http://schema.org/Person#1234567890> <http://schema.org/#spouse> <http://schema.org/Person#0987654321> .
  <http://schema.org/Person#0987654321> <http://schema.org/#@context> "http://schema.org" .
  <http://schema.org/Person#0987654321> <http://schema.org/#@type> "Person" .
  <http://schema.org/Person#0987654321> <http://schema.org/#name> "Sarah" .
  <http://schema.org/Person#0987654321> <http://schema.org/#familyName> "Smith" .
  <http://schema.org/Person#0987654321> <http://schema.org/#gender> 0 .
  <http://schema.org/Person#0987654321> <http://schema.org/#spouse> <http://schema.org/Person#1234567890> .
---JSObject ----
{
  "@id": "http://schema.org/Person#1234567890",
  "@context": "http://schema.org",
  "@type": "Person",
  "name": "Joe",
  "familyName": "Smith",
  "gender": 0,
  "spouse": {
    "@id": "http://schema.org/Person#0987654321",
    "@context": "http://schema.org",
    "@type": "Person",
    "name": "Sarah",
    "familyName": "Smith",
    "gender": 0,
    "spouse": {
      "@id": "http://schema.org/Person#1234567890"
    }
  }
}
---POCO object----
{
  "@context": "http://schema.org",
  "@type": "Person",
  "@id": "http://schema.org/Person#1234567890",
  "name": "Joe",
  "familyName": "Smith",
  "gender": 0,
  "spouse": {
    "@context": "http://schema.org",
    "@type": "Person",
    "@id": "http://schema.org/Person#0987654321",
    "name": "Sarah",
    "familyName": "Smith",
    "gender": 0,
    "spouse": {
      "@context": "http://schema.org",
      "@type": "Person",
      "@id": "http://schema.org/Person#1234567890"
    }
  }
}
---POCO object----
{
  "@context": "http://schema.org",
  "@type": "Person",
  "@id": "http://schema.org/Person#0987654321",
  "name": "Sarah",
  "familyName": "Smith",
  "gender": 0,
  "spouse": {
    "@context": "http://schema.org",
    "@type": "Person",
    "@id": "http://schema.org/Person#1234567890",
    "name": "Joe",
    "familyName": "Smith",
    "gender": 0,
    "spouse": {
      "@context": "http://schema.org",
      "@type": "Person",
      "@id": "http://schema.org/Person#0987654321"
    }
  }
}
```

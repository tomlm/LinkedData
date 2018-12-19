// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkedDataProcessor
{
    /// <summary>
    /// Multiple implementation with different optimizations of the flattened graph structure could be used hence this interface
    /// </summary>
    public interface IGraph : ICloneable
    {
        /// <summary>
        /// Gets the Name of the graph
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the Count of triples in this graph - this should be efficient
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Assert adds a triple to the graph if its not already there
        /// </summary>
        bool Assert(Triple t);

        /// <summary>
        /// Assert adds a triple to the graph if its not already there
        /// </summary>
        bool Assert(string s, string p, GraphObject o);

        /// <summary>
        /// Merge another graph into this one: Assert all the triples in that graph into this graph
        /// </summary>
        IGraph Merge(IGraph g);

        /// <summary>
        /// Retracts a triple from the graph if its there
        /// </summary>
        bool Retract(Triple t);

        /// <summary>
        /// Retracts a triple from the graph if its there
        /// </summary>
        bool Retract(string s, string p, GraphObject o);

        /// <summary>
        /// Subtracts a whole graph from the graph: Retract all the triples in that graph from this graph
        /// </summary>
        IGraph Minus(IGraph g);

        /// <summary>
        /// GetBySubject returns all the triples with this subject
        /// </summary>
        IEnumerable<Triple> GetBySubject(string s);

        /// <summary>
        /// GetByPredicate returns all the triples with this predicate
        /// </summary>
        IEnumerable<Triple> GetByPredicate(string p);

        /// <summary>
        /// GetByObject returns all the triples with this object
        /// </summary>
        IEnumerable<Triple> GetByObject(GraphObject o);

        /// <summary>
        /// GetBySubjectPredicate returns all the triples with this subject and this predicate
        /// </summary>
        IEnumerable<Triple> GetBySubjectPredicate(string s, string p);

        /// <summary>
        /// GetBySubjectPredicate returns all the triples with this predicate and this object
        /// </summary>
        IEnumerable<Triple> GetByPredicateObject(string p, GraphObject o);

        /// <summary>
        /// GetByObjectSubject returns all the triples with this object and subject
        /// </summary>
        IEnumerable<Triple> GetByObjectSubject(GraphObject o, string s);

        /// <summary>
        /// Returns all the triples
        /// </summary>
        IEnumerable<Triple> GetTriples();

        /// <summary>
        /// Tests whether the triple exists in the graph
        /// </summary>
        bool Exists(Triple t);

        /// <summary>
        /// Tests whether the triple exists in the graph
        /// </summary>
        bool Exists(string s, string p, GraphObject o);

        /// <summary>
        /// Efficiently enumerate over the whole graph
        /// </summary>
        IEnumerable<IGrouping<string, IGrouping<string, GraphObject>>> GetGroupings();

        /// <summary>
        /// Efficiently enumerate over a particular subject
        /// </summary>
        IEnumerable<IGrouping<string, GraphObject>> GetSubjectGroupings(string s);
    }
}

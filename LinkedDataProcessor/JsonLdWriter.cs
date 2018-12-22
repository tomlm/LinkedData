// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace LinkedDataProcessor
{
    public class JsonLdWriter : IGraphWriter, IDisposable
    {
        private JsonWriter _writer;
        private List<GraphObject> _currentObjects;

        public JsonLdWriter(JsonWriter writer)
        {
            _writer = writer;
            _currentObjects = new List<GraphObject>();
        }

        public void WriteStart()
        {
            _writer.WriteStartObject();
            InnerWriteContext();
        }

        public void WriteEnd()
        {
            _writer.WriteEndArray();
            _writer.WriteEndObject();
        }

        public void WriteName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                _writer.WritePropertyName("@id");
                _writer.WriteValue(name);
            }
            _writer.WritePropertyName("@graph");
            _writer.WriteStartArray();
        }

        public void WriteStartSubject(string s)
        {
            _writer.WriteStartObject();
            _writer.WritePropertyName("@id");
            _writer.WriteValue(s);
        }

        public void WriteEndSubject()
        {
            _writer.WriteEndObject();
        }

        public void WriteStartPredicate(string p)
        {
            _writer.WritePropertyName(p);
            _currentObjects.Clear();
        }

        public void WriteEndPredicate()
        {
            if (_currentObjects.Count > 1)
            {
                _writer.WriteStartArray();
                foreach (var o in _currentObjects)
                {
                    InnerWriteObject(o);
                }
                _writer.WriteEndArray();
            }
            else
            {
                InnerWriteObject(_currentObjects.First());
            }
        }

        public void WriteObject(GraphObject o)
        {
            _currentObjects.Add(o);
        }

        public void Dispose()
        {
            _writer.Flush();
            ((IDisposable)_writer).Dispose();
        }

        private void InnerWriteObject(GraphObject o)
        {
            if (o.IsID)
            {
                _writer.WriteStartObject();
                _writer.WritePropertyName("@id");
                _writer.WriteValue(o.Id);
                _writer.WriteEndObject();
            }
            else
            {
                _writer.WriteRawValue(o.ToJSON());
            }
        }

        private void InnerWriteContext()
        {
            _writer.WritePropertyName("@context");
            _writer.WriteStartObject();
            _writer.WritePropertyName("@vocab");
            _writer.WriteValue("http://microsoft.com/sets/terms#");
            _writer.WriteEndObject();
        }
    }
}

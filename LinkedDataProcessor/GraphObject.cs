using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LinkedDataProcessor
{
    public class GraphObject
    {
        // TODO: switch to object
        private string _value;

        public GraphObject(string id)
            : this(id, true)
        {
        }

        internal GraphObject(JValue data)
        {
            GraphObject t = null;
            switch (data.Type)
            {
                case JTokenType.String:
                case JTokenType.Date:
                case JTokenType.TimeSpan:
                case JTokenType.Guid:
                case JTokenType.Uri:
                    t = GraphObject.FromData(data.ToString());
                    break;
                case JTokenType.Integer:
                    t = GraphObject.FromData(data.Value<long>());
                    break;
                case JTokenType.Boolean:
                    t = GraphObject.FromData(data.Value<bool>());
                    break;
                case JTokenType.Float:
                    t = GraphObject.FromData(data.Value<double>());
                    break;
                default:
                    throw new InvalidOperationException($"{data.Type} not support as object");
            }
            IsID = t.IsID;
            _value = t._value;
        }

        protected GraphObject(string id, bool isID)
        {
            _value = id;
            IsID = isID;
        }

        [JsonIgnore]
        public string Id
        {
            get
            {
                if (IsID)
                {
                    return _value;
                }
                throw new InvalidOperationException("object is not an Id");
            }
        }

        public bool IsID { get; set; }

        [JsonIgnore]
        public bool IsNull
        {
            get
            {
                return _value == null;
            }
        }

        /// <summary>
        /// actual property data is stored as JSON
        /// </summary>
        public static implicit operator GraphObject(JToken jToken)
        {
            return new GraphObject((JValue)jToken);
        }

        /// <summary>
        /// all reference types are held as strings (not Uri because they often differ only in fragment)
        /// </summary>
        public static implicit operator GraphObject(string id)
        {
            return new GraphObject(id, true);
        }

        public static GraphObject FromData(string s)
        {
            // TODO: avoid JValue - which would mean avoiding JSON for the internal rep in this class - we need to use JValue here because it escapes strings
            return new GraphObject(Stringify(new JValue(s)), false);
        }

        /// <summary>
        /// create avoiding JValue
        /// </summary>
        public static GraphObject FromData(long n)
        {
            return new GraphObject($"{n}", false);
        }

        /// <summary>
        /// create avoiding JValue
        /// </summary>
        public static GraphObject FromData(double n)
        {
            return new GraphObject($"{n}", false);
        }

        /// <summary>
        /// create avoiding JValue
        /// </summary>
        public static GraphObject FromData(bool f)
        {
            return new GraphObject(f ? "true" : "false", false);
        }

        /// <summary>
        /// create avoiding JValue
        /// </summary>
        public static GraphObject FromRaw(string json)
        {
            return new GraphObject(json, false);
        }

        public static string Stringify(JValue jValue)
        {
            using (var textWriter = new StringWriter())
            {
                using (var jsonWriter = new JsonTextWriter(textWriter))
                {
                    jValue.WriteTo(jsonWriter);
                    jsonWriter.Flush();
                    return textWriter.ToString();
                }
            }
        }

        public override string ToString()
        {
            if (IsID)
            {
                return $"<{_value}>";
            }
            else
            {
                return _value;
            }
        }

        public string ToJSON()
        {
            return _value;
        }

        public string DataAsString()
        {
            if (IsID)
            {
                throw new InvalidOperationException("object is an id but should be data");
            }
            return JValue.Parse(_value).ToString();
        }

        public override bool Equals(object obj)
        {
            return ((GraphObject)obj).IsID == IsID && ((GraphObject)obj)._value.Equals(_value);
        }

        public override int GetHashCode()
        {
            return (IsID ? "1" : "0" + _value).GetHashCode();
        }
    }
}


namespace LinkedDataProcessor
{
    /// <summary>
    /// interface for efficient serialization of the whole graph
    /// TODO: decided how graph level properties fit into this
    /// </summary>
    public interface IGraphWriter
    {
        /// <summary>
        /// called at the start of the serialization
        /// </summary>
        void WriteStart();

        /// <summary>
        /// called at the end of the serialization
        /// </summary>
        void WriteEnd();

        /// <summary>
        /// write the graph name
        /// </summary>
        void WriteName(string name);

        /// <summary>
        /// write the current subject
        /// </summary>
        void WriteStartSubject(string s);

        /// <summary>
        /// end of the current subject
        /// </summary>
        void WriteEndSubject();

        /// <summary>
        /// write the current predicate for the current subject
        /// </summary>
        void WriteStartPredicate(string p);

        /// <summary>
        /// end of the current predicate
        /// </summary>
        void WriteEndPredicate();

        /// <summary>
        /// write an object corresponding to the current predicate and current subject
        /// </summary>
        void WriteObject(GraphObject o);
    }
}

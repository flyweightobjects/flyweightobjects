using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace FlyweightObjects
{
    /// <summary>
    /// Performs XML serialization routines.
    /// </summary>
    /// <example>
    /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
    /// <code>
    /// <![CDATA[
    /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
    /// {
    ///     // Retrieve a product from storage
    ///     Product p1 = context.Select<Product>(1).ToSingle();
    ///     Console.WriteLine("ProductID: {0}", p1.Name);
    ///            
    ///     // Serialize the object as XML
    ///     string xml = XmlSerializer.Serialize<Product>(p1);
    ///     
    ///     // Deserialize the product 
    ///     Product p2 = XmlSerializer.Deserialize<Product>(xml);
    ///     Console.WriteLine("ProductID: {0}", p2.Name);
    /// }
    /// ]]>
    /// </code>
    /// </example>   
    public sealed class XmlSerializer
    {
        /// <summary>
        /// Serializes an object into an XML string.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        public static string Serialize(object source)
        {
            if (source == null)
            {
                return null;
            }
            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializer.XmlWriter xmlWriter = new XmlSerializer.XmlWriter(stream, Encoding.UTF8);
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(source.GetType());
                serializer.Serialize(xmlWriter, source);
                stream.Position = 0;
                return UTF8Encoding.UTF8.GetString(stream.GetBytes()).TrimNull();
            }
        }

        /// <summary>
        /// Serializes an object into an XML string.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <param name="includeDeclaration">Determines whether the declaration should appear at the top of the returned XML.</param>
        /// <param name="namespaces">The <see cref="XmlSerializerNamespaces"/> to add.</param>
        public static string Serialize(object source, bool includeDeclaration, XmlSerializerNamespaces namespaces)
        {
            if (source == null)
            {
                return null;
            }
            if (namespaces == null)
            {
                namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);
            }
            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializer.XmlWriter xmlWriter = new XmlSerializer.XmlWriter(stream, Encoding.UTF8);
                xmlWriter.IncludeDeclaration = includeDeclaration;
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(source.GetType());
                serializer.Serialize(xmlWriter, source, namespaces);
                stream.Position = 0;
                return UTF8Encoding.UTF8.GetString(stream.GetBytes()).TrimNull();
            }
        }

        /// <summary>
        /// Deserializes an object into the type of T.
        /// </summary>
        /// <typeparam name="T">The type of object expected to be deserialized.</typeparam>
        /// <param name="xml">The xml string of the obejct.</param>
        /// <returns></returns>
        public static T Deserialize<T>(string xml) where T : class, new()
        {
            if (string.IsNullOrEmpty(xml))
            {
                return default(T);
            }
            using (MemoryStream stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(xml)))
            {
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                return serializer.Deserialize(stream) as T;
            }
        }

        private class XmlWriter : System.Xml.XmlTextWriter
        {
            public bool IncludeDeclaration { get; set; }
            
            public XmlWriter(Stream stream, Encoding encoding)
                : base(stream, encoding)
            {
                Initialize();
            }

            public XmlWriter(TextWriter writer)
                : base(writer)
            {
                Initialize();
            }

            private void Initialize()
            {
                this.IncludeDeclaration = true;
                base.Formatting = Formatting.None;
            }

            public override void WriteStartDocument()
            {
                if (this.IncludeDeclaration)
                    base.WriteStartDocument();
            }
        }
    }
}

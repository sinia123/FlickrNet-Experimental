﻿using System.Xml;

namespace FlickrNet
{
    /// <summary>
    /// A machine tag namespace. "namespace:predicate=value".
    /// </summary>
    public sealed class Namespace : IFlickrParsable
    {
        /// <summary>
        /// The name of the namespace.
        /// </summary>
        public string NamespaceName { get; set; }

        /// <summary>
        /// The usage of the namespace.
        /// </summary>
        public int Usage { get; set; }

        /// <summary>
        /// The number of unique predicates within this namespace.
        /// </summary>
        public int Predicates { get; set; }

        #region IFlickrParsable Members

        void IFlickrParsable.Load(XmlReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.LocalName)
                {
                    case "usage":
                        Usage = reader.ReadContentAsInt();
                        break;
                    case "predicates":
                        Predicates = reader.ReadContentAsInt();
                        break;
                }
            }

            reader.Read();

            if (reader.NodeType == XmlNodeType.Text)
                NamespaceName = reader.ReadContentAsString();

            reader.Read();
        }

        #endregion
    }
}
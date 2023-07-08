namespace ExBuddy.OrderBotTags.Fish
{
    using Clio.XmlEngine;
    using System;
    using System.ComponentModel;

    [XmlElement("TripleHook")]
    public class TripleHook
    {
        [XmlAttribute("TugType")]
        public string TugType { get; set; }

        public override string ToString()
        {
            return this.DynamicToString();
        }
    }
}
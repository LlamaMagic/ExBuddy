namespace ExBuddy.OrderBotTags.Fish
{
    using Clio.XmlEngine;
    using System;
    using System.ComponentModel;

    [XmlElement("DoubleHook")]
    public class DoubleHook
    {
        [XmlAttribute("TugType")]
        public string TugType { get; set; }

        public override string ToString()
        {
            return this.DynamicToString();
        }
    }
}
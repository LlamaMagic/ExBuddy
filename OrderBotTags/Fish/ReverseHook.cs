namespace ExBuddy.OrderBotTags.Fish
{
    using Clio.XmlEngine;
    using System;
    using System.ComponentModel;

    [XmlElement("ReverseHook")]
    public class ReverseHook
    {
        [XmlAttribute("MoochLevel")]
        public int MoochLevel { get; set; }

        [XmlAttribute("TugType")]
        public string TugType { get; set; }

        public override string ToString()
        {
            return this.DynamicToString();
        }
    }
}
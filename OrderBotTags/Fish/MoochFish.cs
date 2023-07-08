namespace ExBuddy.OrderBotTags.Fish
{
    using Clio.XmlEngine;
    using System;
    using System.ComponentModel;

    [XmlElement("MoochFish")]
    public class MoochFish
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        public override string ToString()
        {
            return this.DynamicToString();
        }
    }
}
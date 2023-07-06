namespace ExBuddy.OrderBotTags.Fish
{
    using Clio.XmlEngine;
    using ExBuddy.Enumerations;
    using System.ComponentModel;

    [XmlElement("DoubleHook")]
    public class DoubleHook
    {
        [DefaultValue(DoubleHookAction.KeepAll)]
        [XmlAttribute("Action")]
        public DoubleHookAction Action { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        public override string ToString()
        {
            return this.DynamicToString();
        }
    }
}
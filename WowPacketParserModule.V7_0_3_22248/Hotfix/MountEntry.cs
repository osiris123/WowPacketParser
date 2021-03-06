using WowPacketParser.Enums;
using WowPacketParser.Hotfix;

namespace WowPacketParserModule.V7_0_3_22248.Hotfix
{
    [HotfixStructure(DB2Hash.Mount)]
    public class MountEntry
    {
        public uint SpellId { get; set; }
        public uint DisplayId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SourceDescription { get; set; }
        public float CameraPivotMultiplier { get; set; }
        public ushort MountTypeId { get; set; }
        public ushort Flags { get; set; }
        public ushort PlayerConditionId { get; set; }
        public byte Source { get; set; }
        public uint ID { get; set; }
    }
}
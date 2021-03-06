using WowPacketParser.Enums;
using WowPacketParser.Hotfix;

namespace WowPacketParserModule.V7_0_3_22248.Hotfix
{
    [HotfixStructure(DB2Hash.QuestPackageItem, HasIndexInData = false)]
    public class QuestPackageItemEntry
    {
        public uint ItemID { get; set; }
        public ushort QuestPackageID { get; set; }
        public byte ItemCount { get; set; }
        public byte FilterType { get; set; }
    }
}
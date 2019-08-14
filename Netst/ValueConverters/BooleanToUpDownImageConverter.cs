
namespace Netst.ValueConverters
{
    public class BooleanToUpDownImageConverter : BooleanToImageConverter
    {
        public override string TrueResource { get; set; } = "Resources/up.png";
        public override string FalseResource { get; set; } = "Resources/down.png";
    }
}

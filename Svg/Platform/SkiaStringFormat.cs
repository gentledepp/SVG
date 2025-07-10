using System;
using System.Drawing;


namespace Svg.Platform
{
    public class SkiaStringFormat : StringFormat
    {
        public SkiaStringFormat()
        {
        }

        public StringFormatFlags FormatFlags { get; set; }
        public void SetMeasurableCharacterRanges(CharacterRange[] characterRanges)
        {
            if (characterRanges == null) throw new ArgumentNullException(nameof(characterRanges));
            MeasurableCharacterRanges = characterRanges;
        }

        public CharacterRange[] MeasurableCharacterRanges { get; private set; }
    }
}
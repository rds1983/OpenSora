namespace OpenSora.Scenarios
{
	public enum ScpStringType
	{
		SCPSTR_CODE_STRING = -1,
		SCPSTR_CODE_ITEM = 0x1F,
		SCPSTR_CODE_LINE_FEED = 0x01,
		SCPSTR_CODE_ENTER = 0x02,
		SCPSTR_CODE_CLEAR = 0x03,
		SCPSTR_CODE_05 = 0x05,
		SCPSTR_CODE_COLOR = 0x07,
		SCPSTR_CODE_09 = 0x09
	}

	public class ScpString
	{
		public ScpStringType Type { get; }
		public string String { get; }
		public int Value { get; }

		public ScpString(string str)
		{
			Type = ScpStringType.SCPSTR_CODE_STRING;
			String = str;
		}

		public ScpString(ScpStringType type, int value = 0)
		{
			Type = type;
			Value = value;
		}
	}
}


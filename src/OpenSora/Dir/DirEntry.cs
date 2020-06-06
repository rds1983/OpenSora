namespace OpenSora.Dir
{
	public class DirEntry
	{
		public string DatFilePath;
		public string Name;
		public int Index;
		public int Timestamp2;
		public int CompressedSize;
		public int DecompressedSize;
		public int Unused;
		public int Timestamp;
		public int Offset;

		public override string ToString()
		{
			return string.Format("Name: {0}, Timestamp2: {1}, CompressedSize: {2}, " +
				"UncompressedSize = {3}, Unused = {4}, Timestamp = {5}, " +
				"Offset: {6}",
				Name, Timestamp2, CompressedSize,
				DecompressedSize, Unused, Timestamp,
				Offset);
		}
	}
}

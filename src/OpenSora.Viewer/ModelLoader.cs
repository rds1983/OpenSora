using System;
using System.IO;

namespace OpenSora.Viewer
{
	static class ModelLoader
	{
		private static byte[] Decompress(BinaryReader reader)
		{
			var output = new byte[65535];
			byte bits = 8; //8 to start off with, then 16
			var flags = reader.ReadUInt16();
			flags >>= 8;
			ushort outputoffset = 0;

			// Setup / reset generator
			Func<bool> getflag = () =>
			{
				if (bits == 0)
				{
					flags = reader.ReadUInt16();
					bits = 16;
				}

				var flag = flags & 1;
				flags >>= 1;
				bits -= 1;
				return flag != 0;
			};

			Action<int> setup_run = (int prev_u_buffer_pos) =>
			{
				ushort run = 2;
				if (!getflag())
				{
					run += 1;
					if (!getflag())
					{
						run += 1;
						if (!getflag())
						{
							run += 1;
							if (!getflag())
							{
								if (!getflag())
								{
									run = reader.ReadByte();
									run += 0xE;
								}
								else
								{
									run = 0;
									for (var i = 1; i <= 3; ++i)
									{
										run = (ushort)((run << 1) | (getflag() ? 1 : 0));
									}
									run += 0x6;
								}
							}
						}
					}
				}
				// Does the 'copy from buffer' thing
				for (var i = 1; i <= run; ++i)
				{
					output[outputoffset] = output[outputoffset - prev_u_buffer_pos];
					outputoffset += 1;
				}
			};

			while (true)
			{
				if (getflag())
				{
					//Call next method to process next flag
					var prev_u_buffer_pos = 0;
					if (getflag())  // Long look-back distance or exit program or repeating sequence(flags = 11)
					{
						ushort run = 0;
						for (var i = 1; i <= 5; ++i) // Load high - order distance from flags (max = 0x31)
						{
							run = (ushort)((run << 1) | (getflag() ? 1 : 0));
						}

						prev_u_buffer_pos = reader.ReadByte();// Load low - order distance(max = 0xFF)
															  // Also acts as flag byte
															  // run = 0 and byte = 0->exit program
															  // run = 0 and byte = 1->sequence of repeating bytes
						if (run != 0)
						{
							prev_u_buffer_pos = prev_u_buffer_pos | (run << 8);// Add high and low order distance(max distance = 0x31FF)
							setup_run(prev_u_buffer_pos);// Get run length and finish unpacking(write to output)
						}
						else if (prev_u_buffer_pos > 2)// Is this used? Seems inefficient.
						{
							setup_run(prev_u_buffer_pos);
						}
						else if (prev_u_buffer_pos == 0)// Decompression complete. End program.
						{
							break;
						}
						else // Repeating byte
						{
							var branch = getflag();// True = long repeating sequence(> 30)
							for (var i = 1; i <= 4; ++i)
							{// Load run length from flags
								run = (ushort)((run << 1) | (getflag() ? 1 : 0));
							}
							if (branch)
							{// Long run length
								run = (ushort)((run << 0x8) | reader.ReadByte());// Load run length from byte and add high-order run length(max = 0xFFF + 0xE)
							}
							run += 0xE;
							var b = reader.ReadByte();
							for (var i = 0; i < run; ++i)
							{
								output[outputoffset++] = b;
							}
						}
					}
					else
					{// Short look - back distance(flags = 10)
						prev_u_buffer_pos = reader.ReadByte();// Get the look - back distance(max = 0xFF)
						setup_run(prev_u_buffer_pos);// Get run length and finish unpacking(write to output)

					}
				}
				else
				{ // Copy byte(flags = 0)
					output[outputoffset] = reader.ReadByte();
					outputoffset += 1;
				}
			}
			return output;

		}

		public static void LoadModel(byte[] data)
		{
			using (var stream = new MemoryStream(data))
			using(var reader = new BinaryReader(stream))
			{
				var size = reader.ReadUInt16();

				var method = reader.ReadByte();
				if (method == 0)
				{
					data = Decompress(reader);
				} else
				{
					throw new NotImplementedException();
				}
			}
		}
	}
}

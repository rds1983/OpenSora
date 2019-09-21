using System;
using System.IO;

namespace OpenSora.Viewer
{
	static class ModelLoader
	{
		private class Decompressor
		{
			byte _bits = 9; //8 to start off with, then 16
			ushort _flags;
			BinaryReader _reader;
			private readonly byte[] _output = new byte[65536];
			int _outputOffset;

			public byte[] Output
			{
				get
				{
					return _output;
				}
			}

			public int OutputSize
			{
				get
				{
					return _outputOffset;
				}
			}


			private bool getflag()
			{
				if (_bits == 0)
				{
					_flags = _reader.ReadUInt16();
					_bits = 16;
				}

				var flag = _flags & 1;
				_flags >>= 1;
				_bits -= 1;
				return flag != 0;
			}

			void setup_run(int prev_u_buffer_pos)
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
									run = _reader.ReadByte();
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
					_output[_outputOffset] = _output[_outputOffset - prev_u_buffer_pos];
					_outputOffset += 1;
				}
			}

			public void Decompress(BinaryReader r)
			{
				_reader = r;
				_bits = 8; //8 to start off with, then 16
				_flags = _reader.ReadUInt16();

				_flags >>= 8;
				_outputOffset = 0;

				while (true)
				{
					if (getflag())
					{
						// Call next method to process next flag
						if (getflag()) // Long look-back distance or exit program or repeating sequence(flags = 11)
						{
							ushort run = 0;
							for (var i = 1; i <= 5; ++i) // Load high - order distance from flags (max = 0x31)
							{
								run = (ushort)((run << 1) | (getflag() ? 1 : 0));
							}

							int prev_u_buffer_pos = _reader.ReadByte();// Load low - order distance(max = 0xFF)
																	  // Also acts as flag byte
																	  // run = 0 and byte = 0->exit program
																	  // run = 0 and byte = 1->sequence of repeating bytes
							if (run != 0)
							{
								prev_u_buffer_pos = prev_u_buffer_pos | (run << 8); // Add high and low order distance(max distance = 0x31FF)
								setup_run(prev_u_buffer_pos); // Get run length and finish unpacking(write to output)
							}
							else if (prev_u_buffer_pos > 2) // Is this used? Seems inefficient.
							{
								setup_run(prev_u_buffer_pos);
							}
							else if (prev_u_buffer_pos == 0) // Decompression complete. End program.
							{
								break;
							}
							else // Repeating byte
							{
								var branch = getflag(); // True = long repeating sequence(> 30)
								for (var i = 1; i <= 4; ++i)
								{
									// Load run length from flags
									run = (ushort)((run << 1) | (getflag() ? 1 : 0));
								}
								if (branch)
								{
									// Long run length
									run = (ushort)((run << 0x8) | _reader.ReadByte());// Load run length from byte and add high-order run length(max = 0xFFF + 0xE)
								}
								run += 0xE;
								var b = _reader.ReadByte();
								for (var i = 0; i < run; ++i)
								{
									_output[_outputOffset++] = b;
								}
							}
						}
						else
						{
							// Short look - back distance(flags = 10)
							var prev_u_buffer_pos = _reader.ReadByte();// Get the look - back distance(max = 0xFF)
							setup_run(prev_u_buffer_pos);// Get run length and finish unpacking(write to output)
						}
					}
					else
					{
						// Copy byte(flags = 0)
						_output[_outputOffset] = _reader.ReadByte();
						_outputOffset += 1;
					}
				}
			}
		}

		public static void LoadModel(byte[] data)
		{
			byte[] result;
			using (var output = new MemoryStream())
			{
				var decompressor = new Decompressor();
				using (var stream = new MemoryStream(data))
				using (var reader = new BinaryReader(stream))
				{
					while (true)
					{
						var size = reader.ReadUInt16();
						var method = reader.ReadByte();
						stream.Seek(-1, SeekOrigin.Current);
						if (method == 0)
						{
							decompressor.Decompress(reader);

							output.Write(decompressor.Output, 0, decompressor.OutputSize);
						}
						else
						{
							throw new NotImplementedException();
						}

						var flag = reader.ReadByte();
						if (flag == 0)
						{
							break;
						}
					}
				}

				result = output.ToArray();
			}

			File.WriteAllBytes("d:\\temp\\output.x2", result);
		}
	}
}

using OpenSora.Tests.Fixtures;
using System.IO;
using Xunit;

namespace OpenSora.Tests
{
	public class SpriteFixtureTests
	{
		[Fact]
		public void AnimationLoader_LoadsSyntheticCpInfo()
		{
			ushort?[][,] info;
			using (var stream = new MemoryStream(SyntheticCh.CreateAnimationInfo(2)))
			{
				info = AnimationLoader.LoadInfo(stream);
			}

			Assert.Equal(2, info.Length);
			Assert.Equal(16, info[0].GetLength(0));
			Assert.Equal(16, info[0].GetLength(1));

			Assert.Equal(SyntheticCh.Default4444Color, info[0][0, 0].Value);
			Assert.Equal(0x1234, info[0][0, 1].Value);
			Assert.Null(info[0][0, 2]);
			Assert.Equal(0xAAAA, info[1][0, 0].Value);
			Assert.Null(info[1][1, 1]);
		}

		[Fact]
		public void AnimationLoader_DecodesSyntheticChSheet()
		{
			var pixels = AnimationLoader.LoadImageData(new MemoryStream(SyntheticCh.CreateAnimationImage()));

			Assert.Equal(AnimationLoader.TextureSize * AnimationLoader.TextureSize, pixels.Length);
			Assert.Equal(0xFF, (int)pixels[0].B);
			Assert.Equal(0x00, (int)pixels[0].G);
			Assert.Equal(0x88, (int)pixels[0].R);
			Assert.Equal(0xFF, (int)pixels[0].A);
		}

		[Fact]
		public void ChLoader_DecodesDefaultSyntheticImage()
		{
			ChImageData image;
			using (var stream = new MemoryStream(SyntheticCh.CreateImage()))
			{
				image = ChLoader.LoadImageData("ED6_DT00.dat", "C_TEST._CH", stream);
			}

			Assert.Equal(SyntheticCh.DefaultChWidth, image.Width);
			Assert.Equal(SyntheticCh.DefaultChHeight, image.Height);
			Assert.Equal(ChFormat.BGRA4444, image.Format);
			Assert.Equal(SyntheticCh.DefaultChWidth * SyntheticCh.DefaultChHeight, image.Pixels.Length);

			Assert.Equal(0xFF, (int)image.Pixels[0].B);
			Assert.Equal(0x00, (int)image.Pixels[0].G);
			Assert.Equal(0x88, (int)image.Pixels[0].R);
			Assert.Equal(0xFF, (int)image.Pixels[0].A);
		}
	}
}

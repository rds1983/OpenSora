using Imaging.DDSReader;
using OpenSora.Dir;
using OpenSora.ModelLoading;
using OpenSora.Scenarios;
using OpenSora.Tests.Fixtures;
using System.IO;
using Xunit;

namespace OpenSora.Tests
{
	public class FixtureToolkitTests
	{
		[Fact]
		public void Fixtures_CanBeWrittenToTempFolderAndLoaded()
		{
			using (var temp = new TempDirectory())
			{
				var ddsPath = Path.Combine(temp.Path, "TEST_DS.dds");
				File.WriteAllBytes(ddsPath, SyntheticDds.CreateUncompressedRgba(1, 1, new byte[] { 0x11, 0x22, 0x33, 0x44 }));
				using (var stream = File.OpenRead(ddsPath))
				{
					var image = DDS.LoadImage(stream);
					Assert.Equal(1, image.Width);
					Assert.Equal(1, image.Height);
					Assert.Equal(0x33, (int)image.Data[0].R);
				}

				var chPath = Path.Combine(temp.Path, "C_TEST._CH");
				File.WriteAllBytes(chPath, SyntheticCh.CreateImage());
				using (var stream = File.OpenRead(chPath))
				{
					var image = ChLoader.LoadImageData("ED6_DT00.dat", "C_TEST._CH", stream);
					Assert.Equal(SyntheticCh.DefaultChWidth, image.Width);
					Assert.Equal(SyntheticCh.DefaultChHeight, image.Height);
				}

				var cpPath = Path.Combine(temp.Path, "CH00000P._CP");
				File.WriteAllBytes(cpPath, SyntheticCh.CreateAnimationInfo(1));
				using (var stream = File.OpenRead(cpPath))
				{
					var info = AnimationLoader.LoadInfo(stream);
					Assert.Single(info);
					Assert.Equal(SyntheticCh.Default4444Color, info[0][0, 0].Value);
				}

				var x2Path = Path.Combine(temp.Path, "TEST_X2");
				File.WriteAllBytes(x2Path, SyntheticModel.CreateX2());
				using (var stream = File.OpenRead(x2Path))
				{
					var root = ModelLoader.Load(stream, 2);
					Assert.Equal("Frame_SCENE_ROOT", root.Id);
					Assert.Single(root.Children);
				}

				var x3Path = Path.Combine(temp.Path, "TEST_X3");
				File.WriteAllBytes(x3Path, SyntheticModel.CreateX3());
				using (var stream = File.OpenRead(x3Path))
				{
					var root = ModelLoader.Load(stream, 3);
					Assert.Equal("Frame_SCENE_ROOT", root.Id);
					Assert.Single(root.Children);
				}

				var scenarioPath = Path.Combine(temp.Path, "TEST_SN");
				File.WriteAllBytes(scenarioPath, SyntheticScenario.CreateFc());
				using (var stream = File.OpenRead(scenarioPath))
				{
					var scenario = Scenario.FromFCStream(stream);
					Assert.Equal("TESTMAP", scenario.MapName);
					Assert.Single(scenario.Functions);
				}

				var datPath = SyntheticArchive.Write(temp.Path, "ED6_TEST", ("TEST0001._DS", SyntheticDds.CreateUncompressedRgba(1, 1, new byte[] { 0, 0, 0, 0xFF })));
				Assert.True(File.Exists(datPath));
				Assert.Single(DirProcessor.BuildEntries(temp.Path));
			}
		}
	}
}

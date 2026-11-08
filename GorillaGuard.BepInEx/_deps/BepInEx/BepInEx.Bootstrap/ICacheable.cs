using System.IO;

namespace BepInEx.Bootstrap;

public interface ICacheable
{

	void Save(BinaryWriter bw);

	void Load(BinaryReader br);
}

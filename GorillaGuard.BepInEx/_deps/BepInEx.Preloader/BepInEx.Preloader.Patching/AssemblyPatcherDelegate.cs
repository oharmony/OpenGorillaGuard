using Mono.Cecil;

namespace BepInEx.Preloader.Patching;

public delegate void AssemblyPatcherDelegate(ref AssemblyDefinition assembly);

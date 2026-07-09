# Running Samples on macOS

Use the Stride template pipeline. The template preprocessor injects the macOS
platform head from `samples/NewGame/NewGame/MyTemplate.macOS`, so samples and
starters use the same generated project shape as `dotnet new` and GameStudio.

```sh
dotnet pack sources/templates/Stride.Templates.Samples/Stride.Templates.Samples.csproj -c Debug
dotnet new install bin/packages/Stride.Templates.Samples.*.nupkg
dotnet new stride-particles -n ParticlesSample -o /tmp/stride-samples/ParticlesSample --platforms macos
dotnet build /tmp/stride-samples/ParticlesSample/ParticlesSample.macOS/ParticlesSample.macOS.csproj -c Debug
```

For starter templates, pack and install `Stride.Templates.Games.Starters` and
use the starter short name, for example:

```sh
dotnet pack sources/templates/Stride.Templates.Games.Starters/Stride.Templates.Games.Starters.csproj -c Debug
dotnet new install bin/packages/Stride.Templates.Games.Starters.*.nupkg
dotnet new stride-fps -n FirstPersonShooter -o /tmp/stride-samples/FirstPersonShooter --platforms macos
dotnet build /tmp/stride-samples/FirstPersonShooter/FirstPersonShooter.macOS/FirstPersonShooter.macOS.csproj -c Debug
```

Generated macOS projects build with `StridePlatform=macOS` and
`StrideGraphicsApi=Vulkan`. Runtime Vulkan/MoltenVK assets come from the Stride
packages, not from a system-wide Vulkan install.

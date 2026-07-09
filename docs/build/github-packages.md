# GitHub Packages Feed

This fork can publish the full compatible Stride package set to GitHub Packages from a manually dispatched workflow.

## Publish

Run **Publish GitHub Packages** from GitHub Actions on the branch you want to test.

By default, the workflow publishes a unique prerelease version:

```text
4.4.0-github-<run>.<attempt>
```

You can also pass a custom prerelease suffix, without the leading dash. Do not reuse a suffix unless you intentionally want `--skip-duplicate` to keep the already-published package version.

The workflow uses the repository `GITHUB_TOKEN` with `packages: write`; no personal access token is needed for publishing from Actions.

## Consume

GitHub Packages requires authentication for restore. Add the package source with a personal access token classic that has at least `read:packages`:

```bash
dotnet nuget add source \
  --username GITHUB_USERNAME \
  --password GITHUB_PAT \
  --store-password-in-clear-text \
  --name gurdasnijor-stride \
  "https://nuget.pkg.github.com/gurdasnijor/index.json"
```

For projects that use both nuget.org and this fork, package source mapping avoids accidental restores from the wrong feed:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="gurdasnijor-stride" value="https://nuget.pkg.github.com/gurdasnijor/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="nuget.org">
      <package pattern="*" />
      <package pattern="Stride.Dependencies.*" />
      <package pattern="Stride.GNU.*" />
      <package pattern="Stride.Mono.*" />
      <package pattern="Stride.GraphX.*" />
      <package pattern="Stride.Metrics" />
      <package pattern="Stride.QuickGraph" />
    </packageSource>
    <packageSource key="gurdasnijor-stride">
      <package pattern="Stride" />
      <package pattern="Stride.*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

Then pin generated projects to the published version:

```xml
<PackageReference Include="Stride.CommunityToolkit.CodeOnly" Version="4.4.0-github-123.1" />
```

The code-only template package is published to the same feed. Install the exact version you want to test:

```bash
dotnet new install Stride.Templates.CodeOnly::4.4.0-github-123.1
dotnet new stride-macos-fsharp -n MyFSharpGame
```

The generated project will already reference `Stride.CommunityToolkit.CodeOnly` at that template's engine version.

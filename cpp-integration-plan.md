# C++ Community Toolkit Integration Plan

## Goal

Create a hosting integration for C++ applications that fits the existing Community Toolkit and Aspire patterns used by Java, Rust, and Perl.

This plan assumes:

- V1 is a hosting integration, not a client integration.
- The first supported language standards are C++20 and C++23.
- The first supported compiler families are MSVC, Clang/LLVM, and GCC.
- The first supported package managers are vcpkg and Conan.
- The package name should be `CommunityToolkit.Aspire.Hosting.Cpp`.
- The first example should be a minimal hello-world style process rather than an HTTP service.
- vcpkg and Conan do not need to land in the first implementation slice, but both should be present by release.
- Container publish and Dockerfile support should be tracked as a roadmap item and revisited as the core integration takes shape.
- The design should align with Aspire custom hosting guidance and this repo's existing layout, tests, examples, and documentation flow.

## High-Level Recommendation

The closest fit for C++ is not Rust's `cargo run` model. It is a combination of:

- Java's explicit build-step pattern for artifact-producing projects.
- Perl's installer-resource pattern for dependency preparation.
- Rust's simple executable resource shape for the final running process.

The recommended V1 design is:

1. Model the running C++ service as an `ExecutableResource`.
2. Treat dependency restore, configure, and build as separate child resources when Aspire is responsible for preparing the binary.
3. Support a prebuilt executable path as a first-class mode.
4. Use CMake as the build orchestration layer for V1, with compiler and package-manager helpers layered on top.

## Why CMake Should Be The V1 Build Surface

C++ is materially different from Rust and Go in this repo:

- There is no single universal `cpp run` command that is portable across toolchains.
- Directly generating `cl`, `clang++`, or `g++` invocations from Aspire would be brittle for anything beyond trivial single-file apps.
- vcpkg and Conan both integrate naturally with CMake.
- CMake is the most practical abstraction for supporting MSVC, Clang, and GCC without encoding compiler-specific link and include rules into Aspire.

For that reason, the V1 integration should treat CMake as the supported build frontend, while still allowing users to point Aspire at an already-built executable.

## Other Build-Surface Options

CMake is still the current recommendation, but it is not the only viable option. The realistic alternatives are:

### 1. Direct compiler command orchestration

Aspire would generate `cl`, `clang++`, or `g++` commands directly.

Pros:

- Smallest dependency surface.
- Easy to understand for trivial single-file examples.

Cons:

- Breaks down quickly for multi-file projects, include paths, link flags, and third-party libraries.
- Pushes platform and compiler complexity into the integration itself.
- Poor fit for vcpkg and Conan compared to a build-system-oriented approach.

Assessment:

- Useful only as a very small sample or escape hatch, not as the main integration model.

### 2. CMake Presets-only integration

Aspire would require `CMakePresets.json` or `CMakeUserPresets.json` and mostly orchestrate named presets.

Pros:

- Strong alignment with modern CMake workflows.
- Lets the user's existing project define the generator, toolchain, cache variables, and output layout.
- Keeps the Aspire package thin.

Cons:

- More opinionated than necessary for the first release.
- Excludes simpler projects that do not already use presets.

Assessment:

- A strong long-term direction, but too restrictive as the only V1 entrypoint.

### 3. Meson-first integration

Aspire would target Meson and Ninja rather than CMake.

Pros:

- Clean build descriptions.
- Good cross-platform support.
- Compiler and dependency handling are less awkward than raw compiler command generation.

Cons:

- Considerably less common than CMake across mixed-platform C++ projects.
- Weaker fit for a first Community Toolkit release intended to meet users where they already are.

Assessment:

- Credible technically, but a worse default than CMake for adoption.

### 4. xmake-first integration

Aspire would target xmake as the primary build and package experience.

Pros:

- Good developer ergonomics.
- More batteries-included than raw CMake.

Cons:

- Not nearly as standard as CMake in the broader C++ ecosystem.
- Would make the toolkit opinionated in a way that does not match the Java, Rust, or Perl integrations in this repo.

Assessment:

- Interesting future exploration, but not a good default for the first release.

### 5. Generic custom build commands

Aspire would expose a thin `WithConfigureCommand(...)` and `WithBuildCommand(...)` surface and avoid choosing a build system.

Pros:

- Maximum flexibility.
- Supports any build system the user already has.

Cons:

- The least opinionated option is also the least helpful one.
- Harder to provide strong docs, tests, generated polyglot APIs, and consistent examples.

Assessment:

- Worth keeping in mind as an advanced escape hatch, but weak as the core public story.

## Current Recommendation

The best default remains:

- CMake as the main build surface.
- Explicit executable path support for already-built binaries.
- Child resources for restore, configure, and build work.

That gives the integration a stable default while leaving room for preset-based and custom-command extension points later.

## What Existing Integrations Are Doing Today

### Summary chart

| Integration | Language/runtime | Requires build or compile step? | Does Aspire handle it? | How it is modeled today | Recommended lesson for C++ |
| --- | --- | --- | --- | --- | --- |
| Java | Java | Yes | Both | Runs a prebuilt JAR, swaps the resource command to Maven or Gradle in run mode, or adds a child build resource and waits for completion | C++ should copy Java's explicit build-resource pattern for artifact-producing projects |
| Rust | Rust | Yes | Yes, implicitly | `AddRustApp` runs `cargo run`, which compiles and launches in one tool-owned flow | Good example of a simple executable resource, but not a good direct model for C++ build orchestration |
| Golang | Go | Yes | Yes, implicitly | `AddGolangApp` runs `go run`; dependency cleanup can be a separate installer resource; Docker publish does a multistage build | Useful reference for a language with compile-on-run plus optional dependency preparation |
| Perl | Perl | No compiler, but dependency installation matters | Partially | Uses annotations plus installer resources for `cpan`, `cpanm`, and Carton before the main app starts | C++ package-manager support should borrow Perl's installer-resource and annotation approach |
| SQL Database Projects | SQL project to `.dacpac` artifact | Yes | Mostly artifact-oriented | Resource tracks a `.dacpac` path and deployment metadata rather than compiling at launch | Good example of artifact-first design when Aspire should deploy a built output |
| Bun and Deno | JavaScript/TypeScript runtimes | Sometimes transpilation, not a native compile pipeline | Mostly tool-owned runtime flow | Runtime commands and optional install resources, but no universal explicit compile artifact resource | Adjacent pattern only; less directly applicable than Java, Rust, and Perl |

### Evidence from the repo

- Java executable and build-step behavior: [src/CommunityToolkit.Aspire.Hosting.Java/JavaAppHostingExtension.Executable.cs](src/CommunityToolkit.Aspire.Hosting.Java/JavaAppHostingExtension.Executable.cs)
- Java executable resource shape: [src/CommunityToolkit.Aspire.Hosting.Java/JavaAppExecutableResource.cs](src/CommunityToolkit.Aspire.Hosting.Java/JavaAppExecutableResource.cs)
- Rust executable entrypoint: [src/CommunityToolkit.Aspire.Hosting.Rust/RustAppHostingExtension.cs](src/CommunityToolkit.Aspire.Hosting.Rust/RustAppHostingExtension.cs)
- Rust executable resource shape: [src/CommunityToolkit.Aspire.Hosting.Rust/RustAppExecutableResource.cs](src/CommunityToolkit.Aspire.Hosting.Rust/RustAppExecutableResource.cs)
- Golang compile-on-run pattern: [src/CommunityToolkit.Aspire.Hosting.Golang/GolangAppHostingExtension.cs](src/CommunityToolkit.Aspire.Hosting.Golang/GolangAppHostingExtension.cs)
- Perl package-manager and installer model: [src/CommunityToolkit.Aspire.Hosting.Perl/PerlAppResourceBuilderExtensions.PackageManager.cs](src/CommunityToolkit.Aspire.Hosting.Perl/PerlAppResourceBuilderExtensions.PackageManager.cs)
- Perl executable resource shape: [src/CommunityToolkit.Aspire.Hosting.Perl/PerlAppResource.cs](src/CommunityToolkit.Aspire.Hosting.Perl/PerlAppResource.cs)
- SQL project artifact pattern: [src/CommunityToolkit.Aspire.Hosting.SqlDatabaseProjects/SqlProjectBuilderExtensions.cs](src/CommunityToolkit.Aspire.Hosting.SqlDatabaseProjects/SqlProjectBuilderExtensions.cs)

## How Compiled Integrations Currently Fall Into Buckets

### 1. Tool-owned build and run

Aspire calls the language's standard tool and lets that tool compile and execute the app.

- Rust: `cargo run`
- Golang: `go run`

Strengths:

- Simple API surface.
- Minimal Aspire-specific orchestration.
- Good developer ergonomics when the language ecosystem already has a canonical run command.

Weaknesses for C++:

- C++ does not have one standard command that spans MSVC, Clang, and GCC.
- Package management and toolchain selection are too fragmented to hide behind a single run verb.

### 2. Explicit prerequisite resource plus artifact execution

Aspire adds one or more child resources that prepare a runnable artifact, then the main resource launches that artifact.

- Java: `WithMavenBuild` and `WithGradleBuild` add child build resources and use wait annotations.

Strengths:

- Clear dashboard behavior.
- Explicit startup ordering.
- Easier to reason about failures in configure, restore, or build steps.
- Works well when the runtime command is different from the build command.

This is the best fit for C++.

### 3. Artifact-first resource model

Aspire assumes the artifact exists or can be pointed to explicitly and focuses on deployment or execution rather than compilation.

- Java with a prebuilt JAR path.
- SQL Database Projects with a `.dacpac`.

Strengths:

- Smallest possible integration surface.
- Stable for CI and publishing.
- Useful fallback even if build-tool integration exists.

This should be supported in C++ V1.

### 4. Installer-resource model for dependency preparation

Aspire creates child resources for package restore or dependency installation before the main app runs.

- Perl package installs.
- Golang `go mod` support.

Strengths:

- Good dashboard visibility.
- Natural place to expose links, validation, and failure messages.
- Keeps the running resource focused on execution.

This is the right pattern for vcpkg and Conan support.

## Recommended C++ Product Shape

## Package naming

Package name locked for this effort:

- `CommunityToolkit.Aspire.Hosting.Cpp`

Reasoning:

- It matches the short, ecosystem-recognizable naming style used by `Java`, `Rust`, and `Golang`.
- It avoids special characters in package and namespace names.

Alternative considered but not selected:

- `CommunityToolkit.Aspire.Hosting.CPlusPlus`

## Resource model

Recommended primary resource:

- `CppAppExecutableResource : ExecutableResource, IResourceWithServiceDiscovery, IResourceWithWaitSupport`

Recommended child resources:

- `CMakeConfigureResource`
- `CMakeBuildResource`
- `VcpkgInstallResource`
- `ConanInstallResource`

Recommended annotations and option types:

- `CppCompilerAnnotation`
- `CppLanguageStandardAnnotation`
- `CppPackageManagerAnnotation`
- `CMakePresetAnnotation`
- `CMakeGeneratorAnnotation`
- `CppBuildOutputAnnotation`

The main resource should represent the executable that actually runs. The child resources should model everything needed to make that executable available.

## Public API direction

Recommended shape:

```csharp
var api = builder.AddCppApp(
        name: "cpp-api",
        workingDirectory: "../cpp-api",
        executablePath: "out/build/debug/cpp-api")
    .WithCMakeBuild()
    .WithCppStandard(CppLanguageStandard.Cpp23)
    .WithCompiler(CppCompiler.Clang)
    .WithVcpkg()
    .WithHttpEndpoint(env: "PORT")
    .WithHttpHealthCheck("/health");
```

Recommended core methods:

- `AddCppApp(string name, string workingDirectory, string executablePath, string[]? args = null)`
- `WithCMakeBuild(...)`
- `WithCMakeConfigure(...)`
- `WithConfigurePreset(string preset)`
- `WithBuildPreset(string preset)`
- `WithBuildTarget(string target)`
- `WithCompiler(CppCompiler compiler, string? compilerPath = null)`
- `WithCppStandard(CppLanguageStandard standard)`
- `WithVcpkg(...)`
- `WithConan(...)`

Optional but useful fallback method:

- `AddCppExecutable(string name, string executablePath, string? workingDirectory = null, string[]? args = null)`

## Key design decisions behind that API

- The executable path should be explicit. Unlike Java's `java -jar` model, C++ output names and folders vary heavily across generators and build systems.
- Build steps should be opt-in. Some users will want Aspire to orchestrate the build; others will already have a compiled binary.
- Compiler and standard choices should configure the build step, not replace the run command.
- Package-manager helpers should create prerequisite resources, not silently hide restore work inside the main process.

## Proposed repository layout

### Source project

- `src/CommunityToolkit.Aspire.Hosting.Cpp/CommunityToolkit.Aspire.Hosting.Cpp.csproj`
- `src/CommunityToolkit.Aspire.Hosting.Cpp/CppAppHostingExtension.cs`
- `src/CommunityToolkit.Aspire.Hosting.Cpp/CppAppResourceBuilderExtensions.Build.cs`
- `src/CommunityToolkit.Aspire.Hosting.Cpp/CppAppResourceBuilderExtensions.PackageManager.cs`
- `src/CommunityToolkit.Aspire.Hosting.Cpp/CppCompiler.cs`
- `src/CommunityToolkit.Aspire.Hosting.Cpp/CppLanguageStandard.cs`
- `src/CommunityToolkit.Aspire.Hosting.Cpp/CppAppExecutableResource.cs`
- `src/CommunityToolkit.Aspire.Hosting.Cpp/CMakeBuildResource.cs`
- `src/CommunityToolkit.Aspire.Hosting.Cpp/CMakeConfigureResource.cs`
- `src/CommunityToolkit.Aspire.Hosting.Cpp/VcpkgInstallResource.cs`
- `src/CommunityToolkit.Aspire.Hosting.Cpp/ConanInstallResource.cs`
- `src/CommunityToolkit.Aspire.Hosting.Cpp/Annotations/...`
- `src/CommunityToolkit.Aspire.Hosting.Cpp/README.md`

### Tests

- `tests/CommunityToolkit.Aspire.Hosting.Cpp.Tests/CommunityToolkit.Aspire.Hosting.Cpp.Tests.csproj`
- Unit tests for validation, annotations, command construction, and wait relationships
- Integration tests for at least one real CMake-backed example
- A TypeScript AppHost test if the package exposes a polyglot surface through `AspireExport`

### Examples

- `examples/cpp/...`
- One minimal AppHost example
- One tiny hello-world C++ executable example with as little framework code as possible
- A richer HTTP example can follow after the core build and execution flow is stable

## Phased implementation plan

## Phase 0: decisions already captured

- Package name: `CommunityToolkit.Aspire.Hosting.Cpp`.
- First example: a simple hello-world process.
- Release target: both vcpkg and Conan should be supported before release, even if they do not land in the first implementation slice.
- Roadmap item: container publish and Dockerfile generation stay visible, but they are not part of the first execution-focused slice.
- Remaining architecture recommendation: proceed with CMake as the default build surface unless later implementation work uncovers a strong reason to pivot.

## Phase 1: smallest useful vertical slice

Deliver a prebuilt-executable flow first, along with a minimal hello-world example.

Scope:

- Create the C++ hosting package.
- Add `CppAppExecutableResource`.
- Add `AddCppApp` or `AddCppExecutable` for an already-built binary.
- Add a tiny hello-world example under `examples/cpp` to prove the AppHost and executable resource lifecycle with minimal moving parts.
- Add XML docs and generated API surface.
- Add basic unit tests for validation and command construction.

Why this first:

- It proves the package shape.
- It keeps the first slice close to Rust's simple executable model.
- It gives a stable fallback even if build orchestration evolves later.

## Phase 2: CMake configure and build orchestration

Add explicit build-step resources.

Scope:

- Add `WithCMakeConfigure` and `WithCMakeBuild`.
- Create `CMakeConfigureResource` and `CMakeBuildResource` as child resources.
- Use wait relationships so the executable resource does not start before build completion.
- Support explicit configure args, build args, presets, generator, and target selection.

Expected result:

- Aspire can prepare a CMake-based C++ app and then run the built artifact.

## Phase 3: compiler and language-standard helpers

Layer compiler and standard selection on top of CMake.

Scope:

- Add `CppCompiler` enum with `Msvc`, `Clang`, and `Gcc`.
- Add `CppLanguageStandard` enum with `Cpp20` and `Cpp23`.
- Map these choices to configure arguments or environment variables used by CMake.
- Keep raw escape hatches available through explicit configure args.

Important note:

- MSVC support should likely assume a valid Visual Studio build environment or a CMake preset that already encodes the generator and toolset.

## Phase 4: package-manager integration

Expose vcpkg and Conan as explicit prerequisite flows. These do not need to be in the first PR, but both are part of the release target.

### vcpkg

Recommended first behavior:

- Add `WithVcpkg(...)`.
- Detect manifest mode when `vcpkg.json` is present.
- Add an install resource when needed.
- Wire the relevant toolchain path or environment into the configure step.

### Conan

Recommended first behavior:

- Add `WithConan(...)`.
- Create a `conan install` resource that runs before configure.
- Feed the generated Conan toolchain file into CMake.

Design rule:

- Both package managers should be modeled visibly in the dashboard as prerequisite resources rather than invisible side effects.

## Phase 5: examples, integration tests, and docs

Scope:

- Expand the hello-world example into a stable example AppHost baseline under `examples/cpp`.
- Add a test project under `tests/CommunityToolkit.Aspire.Hosting.Cpp.Tests`.
- Add a TypeScript AppHost example and test if the package exports a polyglot surface.
- Update the root `README.md` integration table.
- Prepare the package `README.md` so it can later seed an `aspire.dev` docs PR.
- Add the new test project to CI using `./eng/testing/generate-test-list-for-workflow.sh` and the workflow matrix update.

## Testing strategy

The current repo sets a clear bar through Java, Rust, and Perl:

- Validation tests for null and empty arguments
- Resource-shape tests for command, working directory, annotations, and generated args
- Composition tests for multiple resources and prerequisite resources
- TypeScript AppHost tests where `AspireExport` is used
- End-to-end example-based tests where practical

Recommended C++ test breakdown:

### Unit tests

- `AddCppApp` validation
- executable path handling
- CMake configure and build arg generation
- compiler and standard annotation behavior
- wait relationship creation for configure, install, and build resources
- vcpkg and Conan annotation/resource composition

### Integration tests

- A minimal CMake-based example app can build and start
- The hello-world sample reaches a known ready state, either by process exit behavior or by a stable startup log line
- The package-manager prerequisite resources run before the main resource

### TypeScript AppHost tests

If the package is exported for polyglot AppHosts, include a test similar to Java and Rust.

Important CI implication:

- The TypeScript AppHost test should declare the actual required commands for the chosen example. For C++, this is likely at least `cmake` plus one compiler toolchain command, and possibly `vcpkg` or `conan` for the package-manager-focused example.

## Practical constraints and risks

### Biggest technical risk

Toolchain variability is the main challenge, not the Aspire resource model.

The Aspire-side architecture is straightforward. The difficult part is keeping the build story reliable across:

- Windows vs Linux
- MSVC vs Clang vs GCC
- different CMake generators
- package-manager-specific toolchain setup

### Recommended mitigation

- Make CMake the supported orchestration layer.
- Keep the prebuilt executable mode available as an escape hatch.
- Prefer explicit outputs, presets, and toolchain files over clever auto-detection.
- Start with one stable example and one stable compiler path in CI before expanding the matrix.

### Scope to defer unless it becomes essential

- direct `cl` or `g++` command generation without CMake
- Meson or Bazel support
- automatic container publish and generated Dockerfile support in the first slice
- advanced IDE-specific project formats beyond what CMake already abstracts

## Roadmap candidates after the base plan

- container publish and generated Dockerfile support for C++ resources
- a richer HTTP sample after the hello-world example proves out the core build and run flow
- more opinionated CMake Presets support if the initial CMake surface feels too loose or too repetitive
- optional advanced escape hatches for custom build commands if real scenarios justify them

## Recommendation on initial delivery order

If the goal is to get a useful package into review quickly, the best sequence is:

1. Prebuilt executable support
2. hello-world example and baseline tests
3. CMake build-step support
4. compiler and standard helpers
5. vcpkg support
6. Conan support
7. broader examples, polyglot tests, and CI expansion
8. roadmap review for container publish and richer samples

That order keeps the integration shippable at each step and matches the iterative style already used successfully in this repo.

## Decisions captured from this discussion

1. The package name is `CommunityToolkit.Aspire.Hosting.Cpp`.
2. The first example should be a simple hello-world process.
3. vcpkg and Conan are release requirements, but they do not need to be in the first PR.
4. Container publish and Dockerfile generation should stay on the roadmap and be reconsidered as the implementation matures.
5. The remaining build-surface recommendation is CMake, with the alternatives documented above for comparison.

## Repo guidance used for this plan

- Community Toolkit integration authoring guide: [docs/create-integration.md](docs/create-integration.md)
- Aspire custom hosting integration guidance, mirrored in the local Aspire skill notes: [.agents/skills/aspire/SKILL.md](.agents/skills/aspire/SKILL.md)
- Java guide package README: [src/CommunityToolkit.Aspire.Hosting.Java/README.md](src/CommunityToolkit.Aspire.Hosting.Java/README.md)
- Rust guide package README: [src/CommunityToolkit.Aspire.Hosting.Rust/README.md](src/CommunityToolkit.Aspire.Hosting.Rust/README.md)
- Perl guide package README: [src/CommunityToolkit.Aspire.Hosting.Perl/README.md](src/CommunityToolkit.Aspire.Hosting.Perl/README.md)

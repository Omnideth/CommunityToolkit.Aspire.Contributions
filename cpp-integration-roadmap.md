# C++ Community Toolkit Integration Roadmap

As we work on this project, we will make sure to update the Roadmap as we complete items in it.

This roadmap turns [cpp-integration-plan.md](cpp-integration-plan.md) into a sequential delivery order for `CommunityToolkit.Aspire.Hosting.Cpp`.

The default operating rule for every feature slice in this roadmap is:

1. Add or extend prerequisite guidance, required-command validation, and installation/help links before we rely on a tool.
2. Add or extend tests for the slice.
3. Implement the feature.
4. Review whether the feature requires updates to [.devcontainer/devcontainer.json](.devcontainer/devcontainer.json), [.devcontainer/post-create.sh](.devcontainer/post-create.sh), and [docs/setup.md](docs/setup.md).
5. Verify the work still follows repo expectations from [docs/create-integration.md](docs/create-integration.md).

For active C++ integration work, tests should pre-empt the next intended C++ integration behavior before implementation lands. Avoid placeholder tests that only prove generic Aspire behavior when a C++-specific assertion is possible.

The roadmap is intentionally sequential. If we learn something that changes the order, we should update this file first and then continue.

## Roadmap Chart

| Step | Status | Blockers | Task | Depends on | Ordered work inside the step | Devcontainer checkpoint | Primary deliverable |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | In Progress | Need to expand the first prerequisite-warning scaffold beyond the initial CMake-oriented helper and capture the remaining tool-specific guidance | [Foundation and prerequisite guidance scaffolding](#step-1-foundation-and-prerequisite-guidance-scaffolding) | None | Install guidance -> test scaffolding -> package scaffolding | Inventory whether the current devcontainer already covers `cmake`, `gcc`, `g++`, `clang`, `ninja`, and useful editor extensions | Shared validation/help-link pattern plus source and test project skeletons |
| 2 | In Progress | Need a real C++ example to exercise the base executable flow in a meaningful AppHost scenario | [Base executable resource and prebuilt binary flow](#step-2-base-executable-resource-and-prebuilt-binary-flow) | Step 1 | Install guidance -> unit tests -> implementation | Confirm whether the devcontainer needs anything beyond the current .NET base to build and run the first source project | `CppAppExecutableResource`, `AddCppApp` or `AddCppExecutable`, and unit coverage |
| 3 | Planned | Step 2 must expose a runnable base resource | [Hello-world example and baseline integration coverage](#step-3-hello-world-example-and-baseline-integration-coverage) | Step 2 | Install guidance -> integration tests -> example implementation | Check whether the devcontainer should gain basic C++ editing/debugging support before example work grows | Minimal `examples/cpp` AppHost and hello-world executable with repeatable tests |
| 4 | Planned | Need the base executable flow and example path in place first | [CMake configure and build orchestration](#step-4-cmake-configure-and-build-orchestration) | Steps 2-3 | Install guidance -> tests -> implementation | Verify whether to add `cmake` and optionally `ninja` to the devcontainer as part of this slice | `WithCMakeConfigure`, `WithCMakeBuild`, child resources, and wait relationships |
| 5 | Planned | Step 4 must define the CMake execution shape | [Compiler and C++ standard helpers](#step-5-compiler-and-c-standard-helpers) | Step 4 | Install guidance -> tests -> implementation | Review whether the devcontainer needs explicit compiler packages or toolchain activation notes | `CppCompiler`, `CppLanguageStandard`, and mapping to CMake inputs |
| 6 | Planned | Step 4 must exist; Step 5 should define compiler interaction points | [vcpkg integration](#step-6-vcpkg-integration) | Steps 4-5 | Install guidance -> tests -> implementation | Decide whether the devcontainer should install `vcpkg`, bootstrap it, or document manual setup first | `WithVcpkg`, install resource, and CMake toolchain wiring |
| 7 | Planned | Step 4 must exist; Step 5 should define compiler interaction points | [Conan integration](#step-7-conan-integration) | Steps 4-5 | Install guidance -> tests -> implementation | Decide whether the devcontainer should install `conan` through Python tooling or document manual setup first | `WithConan`, install resource, and Conan toolchain/profile wiring |
| 8 | Planned | Core execution, build, and package-manager slices should be stable first | [Docs, polyglot surface, CI, and release readiness](#step-8-docs-polyglot-surface-ci-and-release-readiness) | Steps 1-7 | Install guidance audit -> tests -> implementation and repo wiring | Add only the tools and extensions the completed feature set actually needs | README updates, CI list updates, TypeScript AppHost work if exported, and release-level polish |
| 9 | Roadmap | Release-target scope should be stable before expanding | [Post-release roadmap candidates](#step-9-post-release-roadmap-candidates) | Step 8 | Re-evaluate install guidance -> tests -> implementation | Revisit devcontainer needs only if new features become committed scope | Container publish, richer HTTP sample, preset-heavy APIs, and other follow-on work |
| 10 | Planned | The feature set should be substantially complete first | [Full repo cleanup and test hardening](#step-10-full-repo-cleanup-and-test-hardening) | Steps 1-9 | Brittleness review -> C++-behavior audit -> cleanup -> final verification | Re-run the final devcontainer and setup audit against the finished feature set | Final cleanup pass, test hardening, and repo consistency review |

## Step 1: Foundation And Prerequisite Guidance Scaffolding

Objective:

Build the shared prerequisite-validation and help-link pattern before feature work starts depending on external tools.

Detailed breakdown:

1. Define the common guidance approach for missing commands, using the Perl integration as the behavioral reference for install/help messaging.
2. Decide which tools need first-class guidance entries up front: `cmake`, `cl` or MSVC environment activation notes, `clang`, `g++`, `vcpkg`, and `conan`.
3. Scaffold the source project and test project so subsequent steps can add tests before implementation rather than after implementation.
4. Add initial unit tests for the guidance and validation helpers themselves.
5. Review the current devcontainer definition in [.devcontainer/devcontainer.json](.devcontainer/devcontainer.json) and post-create flow in [.devcontainer/post-create.sh](.devcontainer/post-create.sh) to record what is already present and what is still missing for C++ work.
6. Keep the project skeleton aligned with repo expectations from [docs/create-integration.md](docs/create-integration.md), including naming, XML docs, example placement, and test placement.

Implementation notes:

- This step is about scaffolding behavior and project shape, not feature completeness.
- The output should make later feature slices easier to implement safely.
- If there is uncertainty around exact installation links, capture the intended guidance shape first and refine the URLs during the specific feature step.

Progress so far:

- Added `CommunityToolkit.Aspire.Hosting.Cpp` source, test, and example AppHost scaffolding.
- Added the first reusable prerequisite helper, `WithRequiredCppCommand`, plus install-link constants for CMake, MSVC, Clang, GCC, vcpkg, and Conan.
- Expanded the prerequisite scaffold into concrete CMake, compiler, and package-manager prerequisite helpers with validation callbacks.
- Added initial unit tests covering the prerequisite helper and install-link scaffold.
- Shifted the Cpp tests toward TDD-style feature shaping so the next intended prerequisite behaviors are described before implementation.
- Added focused VS Code tasks for building the Cpp source project, building the Cpp example AppHost, running Cpp unit tests, and running the dedicated Cpp integration test.
- Recorded the current devcontainer baseline: [.devcontainer/post-create.sh](.devcontainer/post-create.sh) already installs `cmake` and `g++`, while `clang`, `ninja`, `vcpkg`, and `conan` remain open decisions for later steps.

Exit criteria:

- A reusable missing-tool guidance pattern exists.
- The `CommunityToolkit.Aspire.Hosting.Cpp` source project and companion test project are scaffolded.
- The roadmap has a recorded view of the current devcontainer gap for C++ work.

[Back to roadmap chart](#roadmap-chart)

## Step 2: Base Executable Resource And Prebuilt Binary Flow

Objective:

Establish the smallest useful hosting integration that can run an already-built C++ executable.

Detailed breakdown:

1. Add or extend prerequisite guidance for the minimum tools needed to run the first slice.
2. Write unit tests for resource creation, validation, working-directory handling, command handling, and argument wiring.
3. Implement `CppAppExecutableResource` and the first public entrypoint, either `AddCppApp` or `AddCppExecutable`.
4. Add XML docs and generated API surface expectations as part of the slice rather than as cleanup later.
5. Re-check whether the devcontainer needs changes just to support building and validating the new .NET source project.
6. Confirm that naming, namespaces, and public-surface style still match the repo conventions in [docs/create-integration.md](docs/create-integration.md).

Implementation notes:

- This slice should stay intentionally narrow.
- The value of this step is proving the package shape and public API without taking on build-system complexity too early.

Progress so far:

- Added `CppAppExecutableResource`.
- Added the initial `AddCppExecutable` entrypoint for the prebuilt executable flow.
- Added unit tests for validation and command-shape behavior.
- Tightened the executable tests to cover rooted executable paths and stricter public-API validation behavior.
- Replaced the first AppHost smoke assertion with a C++-specific assertion so the integration test no longer only checks generic Aspire model availability.

Exit criteria:

- A consumer can add a prebuilt C++ executable resource to an AppHost.
- Validation and command-shape unit tests exist for the exposed API.

[Back to roadmap chart](#roadmap-chart)

## Step 3: Hello-World Example And Baseline Integration Coverage

Objective:

Back the base resource with the smallest possible end-to-end example before adding more feature depth.

Detailed breakdown:

1. Add or extend prerequisite guidance for the tools needed by the hello-world sample.
2. Write integration tests for the example and any basic AppHost composition tests needed to prove startup behavior.
3. Create a minimal `examples/cpp` layout with an AppHost and a tiny hello-world executable.
4. Prefer a sample that is simple to build and easy to validate through process state or a stable startup log line, rather than introducing HTTP or framework concerns immediately.
5. Review whether the devcontainer should add C++ editing, linting, or debugging support now that there is a real sample in the repo.
6. Keep the example structure compatible with future reuse by CI and by later TypeScript AppHost work if needed.

Implementation notes:

- The sample should be intentionally boring.
- It exists to reduce uncertainty in later CMake and package-manager work, not to show off runtime features.

Exit criteria:

- A minimal hello-world example exists under `examples/cpp`.
- The example can be used as a stable baseline for future integration tests.

[Back to roadmap chart](#roadmap-chart)

## Step 4: CMake Configure And Build Orchestration

Objective:

Add the first build-system-aware feature slice using CMake as the default orchestration layer.

Detailed breakdown:

1. Add or extend missing-tool guidance for `cmake` and any immediate companion tooling we decide to require in this slice.
2. Write tests for configure/build argument generation, child resource composition, and wait relationships.
3. Implement `WithCMakeConfigure`, `WithCMakeBuild`, `CMakeConfigureResource`, and `CMakeBuildResource`.
4. Make the executable resource wait on the CMake steps rather than hiding build work inside the main process.
5. Decide how much preset support belongs in this slice versus a later follow-up.
6. Review the devcontainer for `cmake` and possibly `ninja`, but avoid adding tools until this step proves they are required for active work.
7. Keep the public API small and explicit so the first build-system slice remains understandable.

Implementation notes:

- This is the first slice where the integration starts to look like Java's build-step model.
- The step should preserve the prebuilt executable path rather than replacing it.

Exit criteria:

- A CMake-backed example can be configured, built, and then executed through Aspire-managed resources.
- The dashboard model makes configure and build steps visible and ordered.

[Back to roadmap chart](#roadmap-chart)

## Step 5: Compiler And C++ Standard Helpers

Objective:

Add explicit, typed support for the targeted compiler families and the first two C++ language standards.

Detailed breakdown:

1. Add or extend prerequisite guidance for MSVC environment expectations, Clang/LLVM, and GCC.
2. Write tests that verify compiler and language-standard selections are translated into the expected CMake inputs.
3. Implement `CppCompiler` and `CppLanguageStandard`, plus the annotations or configuration helpers needed to wire them into the CMake flow.
4. Document where we rely on the developer's environment versus where the integration sets explicit configure inputs.
5. Review whether the devcontainer should explicitly add compiler packages now, based on what the completed CMake slice actually needs.
6. Keep raw escape hatches available so the typed helpers do not paint the integration into a corner.

Implementation notes:

- This step should remain CMake-oriented.
- It should not turn into a direct compiler-command orchestration feature.

Exit criteria:

- The integration exposes typed compiler and standard choices for MSVC, Clang, GCC, C++20, and C++23.
- The choices are validated by tests and flow into the configured build path.

[Back to roadmap chart](#roadmap-chart)

## Step 6: vcpkg Integration

Objective:

Introduce vcpkg as the first package-manager-aware prerequisite flow.

Detailed breakdown:

1. Add or extend prerequisite guidance for `vcpkg`, including clear help messaging for missing installs and any bootstrap expectations.
2. Write tests for manifest detection, install resource composition, and CMake toolchain wiring.
3. Implement `WithVcpkg`, `VcpkgInstallResource`, and the annotations needed to thread vcpkg state into CMake configure/build work.
4. Decide how much automatic detection we want versus how much explicit configuration we require from users.
5. Review whether the devcontainer should install or bootstrap `vcpkg`, or whether the first version should document manual setup and defer container changes until the workflow proves out.
6. Keep the dashboard experience explicit so dependency install work is visible and diagnosable.

Implementation notes:

- This slice should behave more like Perl's installer-resource model than like hidden restore logic.
- The first version should prefer predictable manifest-mode behavior over ambitious auto-detection.

Exit criteria:

- A consumer can opt into vcpkg support through a visible prerequisite resource path.
- Tests cover the expected install and toolchain composition behavior.

[Back to roadmap chart](#roadmap-chart)

## Step 7: Conan Integration

Objective:

Add Conan as the second package-manager-aware prerequisite flow and reach the agreed release target for package managers.

Detailed breakdown:

1. Add or extend prerequisite guidance for `conan`, including installation and profile expectations.
2. Write tests for `conan install` composition, toolchain-file output handling, and integration with the existing CMake flow.
3. Implement `WithConan`, `ConanInstallResource`, and the annotations needed to pass Conan output into configure/build work.
4. Decide how much profile management belongs in V1 versus how much should stay explicit for users.
5. Review whether the devcontainer should install Conan as part of the Python toolchain or whether that should remain a documented manual setup for the initial release path.
6. Re-check the consistency of the vcpkg and Conan user experiences so they feel like two variations of the same design language.

Implementation notes:

- This step should finish the agreed package-manager release requirement.
- Avoid making Conan significantly more magical than vcpkg or vice versa.

Exit criteria:

- The integration supports both vcpkg and Conan before release.
- Conan support is validated by tests and follows the same dashboard-first prerequisite model.

[Back to roadmap chart](#roadmap-chart)

## Step 8: Docs, Polyglot Surface, CI, And Release Readiness

Objective:

Complete the repo-facing work needed to make the integration feel native to this codebase and ready for broader use.

Detailed breakdown:

1. Audit installation guidance across all completed features and close any gaps before release.
2. Add or extend tests for the finished public surface, including TypeScript AppHost coverage if the integration exposes `AspireExport`-based polyglot APIs.
3. Update the package README, the repo root README integration table, and any other documentation needed to reflect the final supported surface.
4. Add CI wiring for the new test project using the repo's existing test-list and workflow patterns.
5. Review the devcontainer only for the features that are actually in scope by this point, rather than pre-installing speculative tools.
6. Confirm that XML docs, namespaces, examples, tests, and manifest behavior still fit the standards in [docs/create-integration.md](docs/create-integration.md).

Implementation notes:

- This step is where the integration stops being only locally coherent and starts being repo-coherent.
- If any completed feature still lacks clear install guidance at this stage, that is a release blocker.

Exit criteria:

- The integration is documented, tested, and wired into the repo in the same style as the other language integrations.
- Release-target features are covered by the roadmap, examples, tests, and documentation.

[Back to roadmap chart](#roadmap-chart)

## Step 9: Post-Release Roadmap Candidates

Objective:

Keep follow-on work visible without diluting the initial release path.

Detailed breakdown:

1. Re-evaluate installation guidance needs for any newly proposed feature before adding implementation work.
2. Add tests before implementation for any post-release feature that graduates into active scope.
3. Revisit container publish and generated Dockerfile support once the local execution story is stable.
4. Consider a richer HTTP example after the hello-world path has served its purpose.
5. Revisit whether stronger CMake Presets support or custom-command escape hatches are justified by real usage.
6. Review devcontainer expansion only when a post-release feature becomes committed work.

Candidates currently tracked here:

- container publish and generated Dockerfile support for C++ resources
- a richer HTTP sample
- stronger CMake Presets support
- advanced custom configure or build command escape hatches

Exit criteria:

- None for now. This section remains a backlog and should only be promoted into active steps by updating this roadmap first.

[Back to roadmap chart](#roadmap-chart)

## Step 10: Full Repo Cleanup And Test Hardening

Objective:

Do a final cleanup and hardening pass once the intended feature set is in place.

Detailed breakdown:

1. Review the full C++ test suite for brittleness and either remove or rewrite tests that overfit temporary implementation details without protecting meaningful C++ integration behavior.
2. Audit the test suite to ensure tests are asserting `CommunityToolkit.Aspire.Hosting.Cpp` behavior rather than merely proving generic Aspire behavior.
3. Remove temporary scaffolding, placeholder assertions, or transitional helpers that no longer belong once the real feature set exists.
4. Re-check docs, examples, tasks, and project wiring for stale names, stale paths, or leftover roadmap-era shortcuts.
5. Re-run the devcontainer and setup audit to confirm the final feature set is supported intentionally rather than accidentally.
6. Run the focused C++ tasks and any broader repo checks needed to confirm the cleaned-up state still holds.

Implementation notes:

- This step is where we pay down the shortcuts taken while iterating.
- If a test still primarily validates Aspire infrastructure rather than the C++ integration, this is the step where it should be fixed or removed.

Exit criteria:

- The final C++ test suite is not obviously brittle.
- The tests primarily validate C++ integration behavior.
- The surrounding repo wiring is consistent and cleaned up.

[Back to roadmap chart](#roadmap-chart)

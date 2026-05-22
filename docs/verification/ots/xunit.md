## xUnit

### Required Functionality

xUnit (`ReqStream-OTS-XUnit`) shall execute unit tests and report results. The
xUnit framework (xunit.v3 and xunit.runner.visualstudio) discovers and runs all test
methods. Passing tests confirm the framework is functioning correctly.

### Verification Approach

xUnit is verified by integration test evidence. The test suite is executed with `dotnet test`
as part of the CI pipeline. Passing test methods demonstrate that xUnit discovered and ran
the tests correctly.

### Test Scenarios

**Context Argument Parsing**: Verifies that xUnit discovers and executes Context unit tests
correctly. This scenario is tested by `Context_Create_NoArguments_ReturnsDefaultContext` and
`Context_Create_VersionFlag_SetsVersionProperty` and `Context_Create_HelpFlags_SetsHelpProperty`.

**Section Parsing**: Verifies that xUnit discovers and executes Section unit tests correctly.
This scenario is tested by `Section_Load_SimpleRequirement_ParsesCorrectly`.

**Requirement Properties**: Verifies that xUnit discovers and executes Requirement unit tests
correctly. This scenario is tested by `Requirement_Properties_DefaultValues`.

**TraceMatrix Construction**: Verifies that xUnit discovers and executes TraceMatrix unit tests
correctly. This scenario is tested by `TraceMatrix_Constructor_WithNoFiles_CreatesEmptyMatrix`.

**Program Execution**: Verifies that xUnit discovers and executes Program unit tests correctly.
This scenario is tested by `Program_Run_WithVersionFlag_PrintsVersion`.

**Validation Execution**: Verifies that xUnit discovers and executes Validation unit tests
correctly. This scenario is tested by `Validation_Run_WithSilentContext_CompletesSuccessfully`.

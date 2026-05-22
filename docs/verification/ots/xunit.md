## xUnit

### Required Functionality

xUnit (`ReqStream-OTS-XUnit-Execute`, `ReqStream-OTS-XUnit-Report`) provides the unit-testing
framework for the project. xUnit (xunit.v3 and xunit.runner.visualstudio) discovers and runs
all test methods and produces test result output consumed by the CI pipeline and ReqStream.
Passing tests confirm the framework is functioning correctly.

### Verification Approach

xUnit is verified by integration test evidence. The test suite is executed with `dotnet test`
as part of the CI pipeline. Passing test methods demonstrate that xUnit discovered and ran
the tests correctly and reported the results.

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

### Requirements Coverage

| Requirement | Scenario | Test Method(s) |
| --- | --- | --- |
| ReqStream-OTS-XUnit-Execute | Context Argument Parsing | `Context_Create_NoArguments_ReturnsDefaultContext`, `Context_Create_VersionFlag_SetsVersionProperty`, `Context_Create_HelpFlags_SetsHelpProperty` |
| ReqStream-OTS-XUnit-Execute | Section Parsing | `Section_Load_SimpleRequirement_ParsesCorrectly` |
| ReqStream-OTS-XUnit-Execute | Requirement Properties | `Requirement_Properties_DefaultValues` |
| ReqStream-OTS-XUnit-Execute | TraceMatrix Construction | `TraceMatrix_Constructor_WithNoFiles_CreatesEmptyMatrix` |
| ReqStream-OTS-XUnit-Report | Program Execution | `Program_Run_WithVersionFlag_PrintsVersion` |
| ReqStream-OTS-XUnit-Report | Validation Execution | `Validation_Run_WithSilentContext_CompletesSuccessfully` |

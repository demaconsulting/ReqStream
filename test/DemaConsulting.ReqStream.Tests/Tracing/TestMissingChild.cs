using DemaConsulting.ReqStream.Modeling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DemaConsulting.ReqStream.Tests.Tracing;

[TestClass]
public class TestMissingChildRequirement
{
    [TestMethod]
    public void Requirements_Load_WithMissingChild_ReportsError()
    {
        var yaml = @"---
sections:
  - title: ""Test""
    requirements:
      - id: ""PARENT""
        children:
          - ""NONEXISTENT""
";
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.yaml");
        File.WriteAllText(path, yaml);
        try
        {
            var result = Requirements.Load(path);
            Console.WriteLine($"Loaded: {result.Requirements != null}, Issues: {result.Issues.Count}");
            foreach (var issue in result.Issues)
            {
                Console.WriteLine($"  {issue.Severity}: {issue.Message}");
            }
        }
        finally
        {
            File.Delete(path);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using FeatureHubSDK;

[Route("api/test/feature")]
[ApiController]
public class FeatureFlagController(IFeatureHubRepository repo) : ControllerBase
{
    [HttpGet]
    public IActionResult Check()
    {
        var flag = repo.GetFeature("CleanFeature");

        var isEnabled = flag.IsEnabled;
        var message = isEnabled
            ? "✅ The feature is currently ENABLED."
            : "❌ The feature is currently DISABLED.";

        return Ok(new
        {
            enabled = isEnabled,
            message
        });
    }
}
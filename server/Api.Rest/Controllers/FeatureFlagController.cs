using Microsoft.AspNetCore.Mvc;
using FeatureHubSDK;

[Route("api/test/feature")]
[ApiController]
public class FeatureFlagController(IFeatureHubRepository repo) : ControllerBase
{
    [HttpGet]
    public IActionResult Check()
    {
        try
        {
            var flag = repo.GetFeature("CleanFeature");
            return Ok(new
            {
                enabled = flag.IsEnabled,
                message = flag.IsEnabled
                    ? "✅ Feature is ENABLED."
                    : "🚫 Feature is DISABLED."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
        }
    }
}

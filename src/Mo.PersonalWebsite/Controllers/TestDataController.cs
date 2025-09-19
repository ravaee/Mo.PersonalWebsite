using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mo.PersonalWebsite.Features.TestData.Services;

namespace Mo.PersonalWebsite.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Protect these endpoints
public class TestDataController : ControllerBase
{
    private readonly ITestDataService _testDataService;
    private readonly ILogger<TestDataController> _logger;

    public TestDataController(ITestDataService testDataService, ILogger<TestDataController> logger)
    {
        _testDataService = testDataService;
        _logger = logger;
    }

    /// <summary>
    /// Generate test articles (public endpoint for testing)
    /// GET /api/testdata/generate-public?count=10
    /// </summary>
    [HttpPost("generate-public")]
    [AllowAnonymous] // Remove auth requirement for testing
    public async Task<ActionResult<object>> GenerateTestDataPublic([FromQuery] int count = 10)
    {
        try
        {
            if (count <= 0 || count > 1000) // Limit to 1000 for public endpoint
            {
                return BadRequest(new { error = "Count must be between 1 and 1,000 for public endpoint" });
            }

            _logger.LogInformation("Starting public test data generation for {Count} articles", count);
            var startTime = DateTime.UtcNow;

            var articlesCreated = await _testDataService.GenerateTestArticlesAsync(count);

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            var result = new
            {
                success = true,
                articlesCreated = articlesCreated,
                requestedCount = count,
                duration = duration.ToString(@"mm\:ss\.fff"),
                startTime = startTime,
                endTime = endTime,
                message = $"Successfully generated {articlesCreated} test articles"
            };

            _logger.LogInformation("Public test data generation completed: {Result}", result.message);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating test data");
            return StatusCode(500, new { error = "An error occurred while generating test data", details = ex.Message });
        }
    }

    /// <summary>
    /// Generate test articles
    /// GET /api/testdata/generate?count=1000
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<object>> GenerateTestData([FromQuery] int count = 1000)
    {
        try
        {
            if (count <= 0 || count > 100000)
            {
                return BadRequest(new { error = "Count must be between 1 and 100,000" });
            }

            _logger.LogInformation("Starting test data generation for {Count} articles", count);
            var startTime = DateTime.UtcNow;

            var articlesCreated = await _testDataService.GenerateTestArticlesAsync(count);

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            var result = new
            {
                success = true,
                articlesCreated = articlesCreated,
                requestedCount = count,
                duration = duration.ToString(@"mm\:ss\.fff"),
                startTime = startTime,
                endTime = endTime,
                message = $"Successfully generated {articlesCreated} test articles"
            };

            _logger.LogInformation("Test data generation completed: {Result}", result.message);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating test data");
            return StatusCode(500, new { error = "An error occurred while generating test data", details = ex.Message });
        }
    }

    /// <summary>
    /// Clear all test data
    /// DELETE /api/testdata/clear
    /// </summary>
    [HttpDelete("clear")]
    public async Task<ActionResult<object>> ClearTestData()
    {
        try
        {
            _logger.LogInformation("Starting test data cleanup");
            var startTime = DateTime.UtcNow;

            var recordsDeleted = await _testDataService.ClearAllTestDataAsync();

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            var result = new
            {
                success = true,
                recordsDeleted = recordsDeleted,
                duration = duration.ToString(@"mm\:ss\.fff"),
                startTime = startTime,
                endTime = endTime,
                message = $"Successfully cleared {recordsDeleted} records"
            };

            _logger.LogInformation("Test data cleanup completed: {Result}", result.message);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing test data");
            return StatusCode(500, new { error = "An error occurred while clearing test data", details = ex.Message });
        }
    }

    /// <summary>
    /// Get test data statistics
    /// GET /api/testdata/stats
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetTestDataStats()
    {
        try
        {
            var categories = await _testDataService.EnsureTestCategoriesAsync();
            
            var result = new
            {
                success = true,
                totalCategories = categories.Count,
                categories = categories.Select(c => new { c.Name, c.Slug }).ToList(),
                message = "Test data statistics retrieved successfully"
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting test data stats");
            return StatusCode(500, new { error = "An error occurred while getting test data stats", details = ex.Message });
        }
    }
}

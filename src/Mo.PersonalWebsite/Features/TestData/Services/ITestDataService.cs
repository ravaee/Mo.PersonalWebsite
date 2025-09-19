using Mo.PersonalWebsite.Infrastructure.Entities;

namespace Mo.PersonalWebsite.Features.TestData.Services;

public interface ITestDataService
{
    Task<int> GenerateTestArticlesAsync(int count = 100000);
    Task<int> ClearAllTestDataAsync();
    Task<List<Category>> EnsureTestCategoriesAsync();
}

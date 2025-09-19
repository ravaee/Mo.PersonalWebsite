using Mo.PersonalWebsite.Infrastructure.Entities;

namespace Mo.PersonalWebsite.Features.Categories.Services;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<Category?> GetBySlugAsync(string slug);
    Task<Category> CreateAsync(Category category);
    Task<Category> UpdateAsync(Category category);
    Task DeleteAsync(int id);
    Task<Category> GetOrCreateAsync(string name);
}

using Claims.Domain;

namespace Claims.Services;

public interface ICoverRepository
{
    Task<IEnumerable<Cover>> GetAllAsync();

    Task<Cover?> GetByIdAsync(string id);

    Task AddAsync(Cover cover);

    Task DeleteAsync(string id);
}

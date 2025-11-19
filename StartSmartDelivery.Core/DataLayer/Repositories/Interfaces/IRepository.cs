namespace StartSmartDelivery.Core.DataLayer.Repositories.Interfaces
{
    // Follows composite key implementation based off: https://medium.com/@martinstm/repository-pattern-net-core-78d0646b6045

    // Includes IQueryable, as suggested here: https://stackoverflow.com/questions/4528712/what-is-a-irepository-and-what-is-it-used-for

    // Utilizes Unit of work for transactions as seen here: https://antondevtips.com/blog/implementing-unit-of-work-pattern-in-ef-core

    // Should queries using GetQueryable become complex, and need to be optimized - Implement
    // https://www.infoworld.com/article/2335344/how-to-use-the-specification-design-pattern-in-c-sharp.html
    // Will be between Repository and Services
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(params object[] keyValues);
        IQueryable<T> GetQueryable(); // This replaces GetByPredicateAsync that was going to be used in CampusLearn 
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(params object[] keyValues);
    }
}

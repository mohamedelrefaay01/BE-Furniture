namespace Home_furnishings.Repository
{
    public interface IRepository<T> where T : class
    {
        List<T> GetAll();
        T GetById(int id);
        void Insert(T entity);
        void Update(int id, T entity);
        void Delete(int id);
    }
}
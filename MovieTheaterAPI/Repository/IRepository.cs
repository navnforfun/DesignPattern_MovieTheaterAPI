﻿namespace MovieTheaterAPI.Repository
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(int id);
        Task Add(T entity);
        Task Update(T entity);
        Task Delete(T entity);
        Task Detached(T entity);
        Task<bool> IsExists(int id);
    }
}
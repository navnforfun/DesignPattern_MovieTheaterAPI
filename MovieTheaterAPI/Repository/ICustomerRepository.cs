﻿using MovieTheaterAPI.Entities;

namespace MovieTheaterAPI.Repository
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer> Login(string Username, string password);
    }
}

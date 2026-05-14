using ASC.DataAccess.Interfaces;
using ASC.Model.BaseTypes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace ASC.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private Dictionary<string, object> _repositories = new Dictionary<string, object>();
        private DbContext _dbContext;

        public UnitOfWork(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public int CommitTransaction()
        {
            return _dbContext.SaveChanges();
        }

        public IRepository<T> Repository<T>() where T : BaseEntity
        {
            var type = typeof(T).Name;

            if (_repositories.ContainsKey(type))
                return (IRepository<T>)_repositories[type];

            var repositoryType = typeof(Repository<>);

            var repositoryInstance =
                Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _dbContext);

            _repositories.Add(type, repositoryInstance);

            return (IRepository<T>)_repositories[type];
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
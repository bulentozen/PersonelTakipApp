using PT.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.BLL.Repository
{
    public class RepositoryBase<T,TID> where T :class
    {
        protected internal static MyContext dbContext;
        public virtual List<T> GetAll()
        {
            try
            {
                dbContext = new MyContext();
                return dbContext.Set<T>().ToList();
            }
            catch (Exception exp)
            {

                throw exp;
            }
        }

        public virtual T GetById(TID Id)
        {
            try
            {
                dbContext = new MyContext();
                return dbContext.Set<T>().Find(Id);
            }
            catch (Exception exp)
            {

                throw exp;
            }
        }

        public virtual int Insert(T entity)
        {
            try
            {
                dbContext = dbContext ?? new MyContext();
                dbContext.Set<T>().Add(entity);
                return dbContext.SaveChanges();
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public virtual int Delete(T entity)
        {
            try
            {
                dbContext = dbContext ?? new MyContext();
                dbContext.Set<T>().Remove(entity);
                return dbContext.SaveChanges();
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public virtual int Update()
        {
            try
            {
                dbContext = dbContext ?? new MyContext();
                return dbContext.SaveChanges();
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }
    }
}

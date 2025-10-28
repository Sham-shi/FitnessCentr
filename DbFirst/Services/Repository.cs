using Microsoft.EntityFrameworkCore;

namespace DbFirst.Services;

public class Repository<T> where T : class
{
    public List<T> GetAll()
    {
        using var ctx = DatabaseService.CreateContext();
        return ctx.Set<T>().AsNoTracking().ToList();
    }

    //public void Add(T entity)
    //{
    //    using var ctx = DatabaseService.CreateContext();
    //    ctx.Set<T>().Add(entity);
    //    ctx.SaveChanges();
    //}
    public void Add(T entity)
    {
        using var ctx = DatabaseService.CreateContext();
        //ctx.Entry(entity).Property("BranchID").IsTemporary = true;
        ctx.Set<T>().Add(entity);
        ctx.SaveChanges(); // EF обновит entity.BranchID после этого вызова
        //return entity;     // возвращаем тот же объект с заполненным ID
    }

    public void Update(T entity)
    {
        using var ctx = DatabaseService.CreateContext();
        ctx.Set<T>().Update(entity);
        ctx.SaveChanges();
    }

    public void Delete(T entity)
    {
        using var ctx = DatabaseService.CreateContext();
        ctx.Set<T>().Remove(entity);
        ctx.SaveChanges();
    }
}

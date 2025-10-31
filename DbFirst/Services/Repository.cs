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

    //public void Update(T entity)
    //{
    //    using var ctx = DatabaseService.CreateContext();

    //    ctx.Set<T>().Update(entity);
    //    ctx.SaveChanges();
    //}

    public void Delete(T entity)
    {
        using var ctx = DatabaseService.CreateContext();
        ctx.Set<T>().Remove(entity);
        ctx.SaveChanges();
    }

    public void Save(T entity)
    {
        using var ctx = DatabaseService.CreateContext();

        // Попробуем найти описание сущности в модели EF
        var entityTypeInfo = ctx.Model.FindEntityType(typeof(T));
        if (entityTypeInfo == null)
            throw new InvalidOperationException($"Тип {typeof(T).Name} не зарегистрирован в контексте EF.");

        // Находим первичный ключ (например, TrainerID)
        var keyProp = entityTypeInfo.FindPrimaryKey()?.Properties?.FirstOrDefault();
        if (keyProp == null)
            throw new InvalidOperationException($"У типа {typeof(T).Name} не найден первичный ключ.");

        var idProp = typeof(T).GetProperty(keyProp.Name);
        if (idProp == null)
            throw new InvalidOperationException($"Свойство первичного ключа '{keyProp.Name}' отсутствует у типа {typeof(T).Name}.");

        // Проверяем — новая ли сущность
        var idValue = idProp.GetValue(entity);
        bool isNew = idValue == null || (idValue is int i && i == 0);

        // Устанавливаем состояние и сохраняем
        var entry = ctx.Entry(entity);
        entry.State = isNew ? EntityState.Added : EntityState.Modified;

        ctx.SaveChanges();
    }
}

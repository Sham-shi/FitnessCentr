using DbFirst.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DbFirst.Services;

public static class DatabaseService
{
    private static IConfigurationRoot? _configuration;
    private static string _currentDb = "SQLite"; // default MSSQL
    private static string? _mssqlConn;
    private static string? _sqliteConn;

    public static void Initialize(IConfigurationRoot configuration)
    {
        _configuration = configuration;
        _mssqlConn = configuration.GetConnectionString("MSSQL");
        _sqliteConn = configuration.GetConnectionString("SQLite");
    }

    public static void UseDatabase(string dbType)
    {
        if (dbType != "MSSQL" && dbType != "SQLite")
            throw new ArgumentException("dbType must be 'MSSQL' or 'SQLite'");

        _currentDb = dbType;
    }

    public static List<T> GetAll<T>() where T : class
    {
        using var context = CreateContext();
        // AsNoTracking() для эффективности, т.к. мы только читаем
        return context.Set<T>().AsNoTracking().ToList();
    }

    public static FitnessCenterContext CreateContext()
    {
        if (_configuration == null)
            throw new InvalidOperationException("DatabaseService not initialized. Call Initialize(...) on startup.");

        var optionsBuilder = new DbContextOptionsBuilder<FitnessCenterContext>();

        if (_currentDb == "SQLite")
        {
            if (string.IsNullOrWhiteSpace(_sqliteConn))
                throw new InvalidOperationException("SQLite connection string is empty. Check appsettings.json.");

            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FitnessCenter.db");
            // Если пользователь записал просто имя файла или относительный путь, нормализуем:
            var conn = NormalizeSqliteConnectionString(_sqliteConn);
            optionsBuilder.UseSqlite(conn);
        }
        else // MSSQL
        {
            if (string.IsNullOrWhiteSpace(_mssqlConn))
                throw new InvalidOperationException("MSSQL connection string is empty. Check appsettings.json.");

            optionsBuilder.UseSqlServer(_mssqlConn);
        }

        return new FitnessCenterContext(optionsBuilder.Options);
    }

    private static string NormalizeSqliteConnectionString(string raw)
    {
        // если строка уже в формате "Data Source=..." — возвращаем как есть
        if (raw.TrimStart().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
            return raw;

        // если передали просто имя/путь файла (например "fitness.db" или "/data/fitness.db")
        // нужно собрать корректную строку подключения.
        // Поддерживаем относительные пути: "Photos/.." или "/db/fitness.db" -> приведём к абсолютному
        var path = raw.Trim();

        // если строка начинается с "file:" — оставляем
        if (path.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            return path;

        // убираем ведущие слеши
        path = path.TrimStart('\\', '/');

        var fullPath = Path.IsPathRooted(path)
            ? path
            : Path.Combine(Directory.GetCurrentDirectory(), path);

        // Убедимся, что директория существует (необязательно)
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        return $"Data Source={fullPath}";
    }

    /// <summary>
    /// Проверяет, создана ли БД. Если нет - создает и заполняет данными.
    /// Выбрасывает исключение в случае ошибки.
    /// </summary>
    public static void InitializeDatabase()
    {
        // Создаем контекст
        using var context = CreateContext();

        // EnsureCreated() вернет true, если БД была создана, 
        // и false, если она уже существовала.
        // Если произойдет ошибка (например, нет прав на запись) - метод выбросит исключение.
        bool wasCreated = context.Database.EnsureCreated();

        if (wasCreated)
        {
            // БД была только что создана, заполняем ее тестовыми данными
            SeedDatabase(context);
        }
    }

    /// <summary>
    /// Заполняет пустую БД тестовыми данными.
    /// </summary>
    private static void SeedDatabase(FitnessCenterContext context)
    {
        // (Проверка, что мы не пытаемся заполнить данными уже заполненную БД)
        if (context.Branches.Any() || context.Clients.Any() || context.Trainers.Any())
        {
            return; // БД не пустая
        }

        // 1. Создаем Филиалы
        var branch1 = new Branch 
        { 
            BranchName = "Gains & Pains - Центральный", 
            Address = "ул. Тверская, 25, г. Москва", 
            Phone = "+7 (495) 111-2233", 
            Email = "central@gains-pains.ru"
        };
        var branch2 = new Branch
        {
            BranchName = "Gains & Pains - Северный",
            Address = "пр. Мира, 18, г. Москва",
            Phone = "+7 (495) 222-3344",
            Email = "north@gains-pains.ru"
        };
        var branch3 = new Branch
        {
            BranchName = "Gains & Pains - Южный",
            Address = "Ленинский пр-т, 67, г. Москва",
            Phone = "+7 (495) 333-4455",
            Email = "south@gains-pains.ru"
        };
        context.Branches.AddRange(branch1, branch2, branch3);
        context.SaveChanges(); // Сохраняем, чтобы получить BranchID

        // 2. Создаем Тренеров (ссылаясь на филиалы)
        var trainer1 = new Trainer
        {
            BranchID = branch1.BranchID,
            FullName = "Петров Иван Сергеевич",
            Phone = "+7 (916) 111-2233",
            Email = "ivan.petrov@gains-pains.ru",
            Specialization = "Йога, Пилатес, Стретчинг",
            Education = "МГАФК, сертифицированный инструктор по йоге RYT 500",
            WorkExperience = "8 лет в фитнес-индустрии, работал в сети фитнес-клубов \"World Class\"",
            SportsAchievements = "КМС по спортивной гимнастике, призер чемпионата России по йоге (2020)",
            Salary = 85000,
            PhotoPath = "/Photos/petrov_ivan.jpg"
        };
        var trainer2 = new Trainer
        {
            BranchID = branch1.BranchID,
            FullName = "Сидорова Мария Андреевна",
            Phone = "+7 (916) 222-3344",
            Email = "maria.sidorova@gains-pains.ru",
            Specialization = "Функциональный тренинг, TRX",
            Education = "РГУФК, FPA, Functional Training Specialist",
            WorkExperience = "6 лет опыта, специализация - реабилитация после травм, подготовка спортсменов",
            SportsAchievements = "1 разряд по легкой атлетике, участница соревнований по кроссфиту",
            Salary = 90000,
            PhotoPath = "/Photos/sidorova_maria.jpg"
        };
        var trainer3 = new Trainer
        {
            BranchID = branch1.BranchID,
            FullName = "Козлов Алексей Владимирович",
            Phone = "+7 (916) 333-4455",
            Email = "alexey.kozlov@gains-pains.ru",
            Specialization = "Бодибилдинг, Пауэрлифтинг",
            Education = "МГАФК, NSCA-CPT",
            WorkExperience = "10 лет в силовых видах спорта, подготовка к соревнованиям по бодибилдингу",
            SportsAchievements = "КМС по пауэрлифтингу, чемпион Москвы по бодибилдингу (2019)",
            Salary = 95000,
            PhotoPath = "/Photos/kozlov_alexey.jpg"
        };
        var trainer4 = new Trainer
        {
            BranchID = branch2.BranchID,
            FullName = "Николаева Елена Дмитриевна",
            Phone = "+7 (916) 444-5566",
            Email = "elena.nikolaeva@gains-pains.ru",
            Specialization = "Кардио, Стретчинг, Аэробика",
            Education = "РГУФК, сертификаты по стретчингу и ЛФК",
            WorkExperience = "7 лет работы с клиентами разного уровня подготовки, специализация - программы для снижения веса",
            SportsAchievements = "КМС по художественной гимнастике",
            Salary = 82000,
            PhotoPath = "/Photos/nikolaeva_elena.jpg"
        };
        var trainer5 = new Trainer
        {
            BranchID = branch2.BranchID,
            FullName = "Волков Дмитрий Игоревич",
            Phone = "+7 (916) 555-6677",
            Email = "dmitry.volkov@gains-pains.ru",
            Specialization = "CrossFit, Functional Training",
            Education = "CrossFit Level 2, FPA",
            WorkExperience = "5 лет в кроссфите, тренер команды по функциональному многоборью",
            SportsAchievements = "Участник чемпионата России по CrossFit, призер региональных соревнований",
            Salary = 88000,
            PhotoPath = "/Photos/volkov_dmitry.jpg"
        };
        var trainer6 = new Trainer
        {
            BranchID = branch2.BranchID,
            FullName = "Орлова Анна Викторовна",
            Phone = "+7 (916) 666-7788",
            Email = "anna.orlova@gains-pains.ru",
            Specialization = "Пилатес, Йога, Body&Mind",
            Education = "МГАФК, сертифицированный инструктор по пилатесу",
            WorkExperience = "9 лет преподавания, специализация - программы для женщин, постнатальное восстановление",
            SportsAchievements = "Инструктор йоги международного класса, ведущий преподаватель пилатеса",
            Salary = 87000,
            PhotoPath = "/Photos/orlova_anna.jpg"
        };
        var trainer7 = new Trainer
        {
            BranchID = branch3.BranchID,
            FullName = "Семенов Артем Олегович",
            Phone = "+7 (916) 777-8899",
            Email = "artem.semenov@gains-pains.ru",
            Specialization = "Бокс, MMA, Functional",
            Education = "РГУФК, сертификаты по единоборствам",
            WorkExperience = "12 лет тренировочного опыта, подготовка спортсменов для соревнований",
            SportsAchievements = "КМС по боксу, победитель международных турниров по ММА",
            Salary = 92000,
            PhotoPath = "/Photos/semenov_artem.jpg"
        };
        var trainer8 = new Trainer
        {
            BranchID = branch3.BranchID,
            FullName = "Федорова Ольга Сергеевна",
            Phone = "+7 (916) 888-9900",
            Email = "olga.fedorova@gains-pains.ru",
            Specialization = "Танцы, Zumba, Cardio",
            Education = "Институт современного искусства, Zumba Instructor",
            WorkExperience = "8 лет преподавания танцев, проведение мастер-классов и воркшопов",
            SportsAchievements = "Чемпионка России по современным танцам, сертифицированный инструктор Zumba",
            Salary = 79000,
            PhotoPath = "/Photos/fedorova_olga.jpg"
        };
        var trainer9 = new Trainer
        {
            BranchID = branch3.BranchID,
            FullName = "Громов Михаил Александрович",
            Phone = "+7 (916) 999-0011",
            Email = "mikhail.gromov@gains-pains.ru",
            Specialization = "Силовой тренинг, Коррекция осанки",
            Education = "МГАФК, сертификат по кинезиотейпированию",
            WorkExperience = "7 лет реабилитационной практики, работа с нарушениями осанки",
            SportsAchievements = "Мастер спорта по тяжелой атлетике, чемпион области по пауэрлифтингу",
            Salary = 91000,
            PhotoPath = "/Photos/gromov_mikhail.jpg"
        };
        context.Trainers.AddRange(trainer1, trainer2, trainer3,
                                    trainer4, trainer5, trainer6,
                                    trainer7, trainer8, trainer9);

        // 3. Создаем Услуги
        var service1 = new Service
        { 
            ServiceName = "Индивидуальная тренировка", 
            ServiceType = "Индивидуальное", 
            DurationMinutes = 60,
            MaxParticipants = 1,
            BasePrice = 2500, 
            Description = "Персональная тренировка с тренером"
        };
        var service2 = new Service 
        { ServiceName = "Йога для начинающих", 
            ServiceType = "Групповое", 
            DurationMinutes = 90,
            MaxParticipants = 15,
            BasePrice = 1200, 
            Description = "Групповое занятие йогой для новичков"
        };
        var service3 = new Service
        {
            ServiceName = "Functional Training",
            ServiceType = "Групповое",
            DurationMinutes = 60,
            MaxParticipants = 12,
            BasePrice = 1500,
            Description = "Функциональный тренинг в группе"
        };
        var service4 = new Service
        {
            ServiceName = "CrossFit",
            ServiceType = "Групповое",
            DurationMinutes = 75,
            MaxParticipants = 10,
            BasePrice = 1800,
            Description = "Групповое занятие CrossFit"
        };
        var service5 = new Service
        {
            ServiceName = "Силовой тренинг",
            ServiceType = "Индивидуальное",
            DurationMinutes = 60,
            MaxParticipants = 1,
            BasePrice = 2800,
            Description = "Индивидуальная силовая тренировка"
        };
        var service6 = new Service
        {
            ServiceName = "Пилатес",
            ServiceType = "Групповое",
            DurationMinutes = 60,
            MaxParticipants = 12,
            BasePrice = 1300,
            Description = "Групповое занятие пилатесом"
        };
        var service7 = new Service
        {
            ServiceName = "Танцевальная аэробика",
            ServiceType = "Групповое",
            DurationMinutes = 60,
            MaxParticipants = 20,
            BasePrice = 1000,
            Description = "Танцевальное кардио занятие"
        };
        var service8 = new Service
        {
            ServiceName = "Бокс для начинающих",
            ServiceType = "Индивидуальное",
            DurationMinutes = 45,
            MaxParticipants = 1,
            BasePrice = 2200,
            Description = "Индивидуальное занятие по боксу"
        };
        var service9 = new Service
        {
            ServiceName = "Стретчинг",
            ServiceType = "Групповое",
            DurationMinutes = 45,
            MaxParticipants = 15,
            BasePrice = 900,
            Description = "Групповое занятие по растяжке"
        };
        context.Services.AddRange(service1, service2, service3,
                                    service4, service5, service6,
                                    service7, service8, service9);

        // 4. Создаем Клиентов
        var client1 = new Client
        { 
            FullName = "Иванова Анна Сергеевна",
            Phone = "+7 (915) 111-2233",
            Email = "anna.ivanova@mail.ru",
            BirthDate = new DateOnly(1990, 5, 15),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-5))
        };
        var client2 = new Client
        {
            FullName = "Смирнов Дмитрий Александрович",
            Phone = "+7 (915) 222-3344",
            Email = "dmitry.smirnov@gmail.com",
            BirthDate = new DateOnly(1985, 08, 20),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-10))
        };
        var client3 = new Client
        {
            FullName = "Кузнецова Ольга Викторовна",
            Phone = "+7 (915) 333-4455",
            Email = "olga.kuznetsova@yandex.ru",
            BirthDate = new DateOnly(1992, 12, 10),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-9))
        };
        var client4 = new Client
        {
            FullName = "Попов Сергей Игоревич",
            Phone = "+7 (915) 444-5566",
            Email = "sergey.popov@mail.ru",
            BirthDate = new DateOnly(1988, 03, 25),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-8))
        };
        var client5 = new Client
        {
            FullName = "Новикова Екатерина Дмитриевна",
            Phone = "+7 (915) 555-6677",
            Email = "ekaterina.novikova@gmail.com",
            BirthDate = new DateOnly(1995, 07, 30),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-7))
        };
        var client6 = new Client
        {
            FullName = "Лебедев Алексей Петрович",
            Phone = "+7 (915) 666-7788",
            Email = "alexey.lebedev@yandex.ru",
            BirthDate = new DateOnly(1987, 11, 14),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-6))
        };
        var client7 = new Client
        {
            FullName = "Соколова Марина Олеговна",
            Phone = "+7 (915) 777-8899",
            Email = "marina.sokolova@mail.ru",
            BirthDate = new DateOnly(1993, 02, 28),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-4))
        };
        var client8 = new Client
        {
            FullName = "Комаров Андрей Викторович",
            Phone = "+7 (915) 888-9900",
            Email = "andrey.komarov@gmail.com",
            BirthDate = new DateOnly(1982, 09, 05),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3))
        };
        var client9 = new Client
        {
            FullName = "Павлова Ирина Александровна",
            Phone = "+7 (915) 999-0011",
            Email = "irina.pavlova@yandex.ru",
            BirthDate = new DateOnly(1991, 06, 18),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2))
        };
        context.Clients.AddRange(client1, client2, client3,
                                    client4, client5, client6,
                                    client7, client8, client9);

        // Сохраняем все, чтобы получить ID тренеров, услуг и клиентов
        context.SaveChanges();

        // 5. Создаем Записи (Bookings), ссылаясь на все остальное
        var booking1 = new Booking
        {
            ClientID = client1.ClientID,
            ServiceID = service1.ServiceID,
            TrainerID = trainer1.TrainerID,
            BookingDateTime = DateTime.Now.AddDays(3).Date.AddHours(10),
            SessionsCount = 1,
            TotalPrice = service1.BasePrice,
            Status = "Запланировано",
            Notes = "Первое занятие по йоге",
            CreatedDate = DateTime.Now
        };

        var booking2 = new Booking
        {
            ClientID = client2.ClientID,
            ServiceID = service5.ServiceID,
            TrainerID = trainer3.TrainerID,
            BookingDateTime = DateTime.Now.AddDays(1).Date.AddHours(18),
            SessionsCount = 1,
            TotalPrice = service5.BasePrice,
            Status = "Запланировано",
            Notes = "Силовая тренировка",
            CreatedDate = DateTime.Now
        };

        var booking3 = new Booking
        {
            ClientID = client3.ClientID,
            ServiceID = service2.ServiceID,
            TrainerID = trainer4.TrainerID,
            BookingDateTime = DateTime.Now.AddDays(-2).Date.AddHours(9),
            SessionsCount = 5,
            TotalPrice = service2.BasePrice * 5,
            Status = "Завершено",
            Notes = "Групповая йога",
            CreatedDate = DateTime.Now.AddDays(-5)
        };

        var booking4 = new Booking
        {
            ClientID = client4.ClientID,
            ServiceID = service4.ServiceID,
            TrainerID = trainer5.TrainerID,
            BookingDateTime = DateTime.Now.AddDays(5).Date.AddHours(17),
            SessionsCount = 10,
            TotalPrice = service4.BasePrice * 10,
            Status = "Запланировано",
            Notes = "CrossFit занятие",
            CreatedDate = DateTime.Now
        };

        var booking5 = new Booking
        {
            ClientID = client5.ClientID,
            ServiceID = service6.ServiceID,
            TrainerID = trainer6.TrainerID,
            BookingDateTime = DateTime.Now.AddDays(-1).Date.AddHours(11),
            SessionsCount = 1,
            TotalPrice = service6.BasePrice,
            Status = "Отменено",
            Notes = "Клиент заболел",
            CreatedDate = DateTime.Now.AddDays(-3)
        };

        var booking6 = new Booking
        {
            ClientID = client6.ClientID,
            ServiceID = service8.ServiceID,
            TrainerID = trainer7.TrainerID,
            BookingDateTime = DateTime.Now.AddDays(7).Date.AddHours(16),
            SessionsCount = 5,
            TotalPrice = service8.BasePrice * 5,
            Status = "Запланировано",
            Notes = "Первое занятие по боксу",
            CreatedDate = DateTime.Now
        };

        var booking7 = new Booking
        {
            ClientID = client7.ClientID,
            ServiceID = service7.ServiceID,
            TrainerID = trainer8.TrainerID,
            BookingDateTime = DateTime.Now.AddDays(2).Date.AddHours(19),
            SessionsCount = 7,
            TotalPrice = service7.BasePrice * 7,
            Status = "Запланировано",
            Notes = "Танцевальная аэробика",
            CreatedDate = DateTime.Now
        };

        var booking8 = new Booking
        {
            ClientID = client8.ClientID,
            ServiceID = service3.ServiceID,
            TrainerID = trainer2.TrainerID,
            BookingDateTime = DateTime.Now.AddDays(-3).Date.AddHours(14),
            SessionsCount = 1,
            TotalPrice = service3.BasePrice,
            Status = "Перенесено",
            Notes = "Перенос на следующую неделю",
            CreatedDate = DateTime.Now.AddDays(-7)
        };

        var booking9 = new Booking
        {
            ClientID = client9.ClientID,
            ServiceID = service9.ServiceID,
            TrainerID = trainer9.TrainerID,
            BookingDateTime = DateTime.Now.AddDays(4).Date.AddHours(12),
            SessionsCount = 1,
            TotalPrice = service9.BasePrice,
            Status = "Запланировано",
            Notes = "Стретчинг",
            CreatedDate = DateTime.Now
        };

        var booking10 = new Booking
        {
            ClientID = client1.ClientID,
            ServiceID = service2.ServiceID,
            TrainerID = trainer1.TrainerID,
            BookingDateTime = DateTime.Now.AddDays(10).Date.AddHours(15),
            SessionsCount = 3,
            TotalPrice = service2.BasePrice * 3,
            Status = "Запланировано",
            Notes = "Повторное занятие йогой",
            CreatedDate = DateTime.Now
        };

        context.Bookings.AddRange(booking1, booking2, booking3, booking4, booking5,
                                 booking6, booking7, booking8, booking9, booking10);

        // Финальное сохранение
        context.SaveChanges();
    }
}
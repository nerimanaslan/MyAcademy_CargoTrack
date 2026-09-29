using System;
using System.Collections.Generic;
using System.Linq;
using CargoTrack.Business;
using CargoTrack.Business.Services.Abouts;
using CargoTrack.Business.Services.AuditLogs;
using CargoTrack.Business.Services.Branches;
using CargoTrack.Business.Services.CargoPrices;
using CargoTrack.Business.Services.Cargos;
using CargoTrack.Business.Services.Cities;
using CargoTrack.Business.Services.Dashboards;
using CargoTrack.Business.Services.Employees;
using CargoTrack.Business.Services.TransferCenters;
using CargoTrack.DataAccess.Context;
using CargoTrack.DataAccess.Repositories.Abouts;
using CargoTrack.DataAccess.Repositories.AuditLogs;
using CargoTrack.DataAccess.Repositories.Branches;
using CargoTrack.DataAccess.Repositories.CargoMovements;
using CargoTrack.DataAccess.Repositories.CargoPrices;
using CargoTrack.DataAccess.Repositories.Cargos;
using CargoTrack.DataAccess.Repositories.Cities;
using CargoTrack.DataAccess.Repositories.Customers;
using CargoTrack.DataAccess.Repositories.Deliveries;
using CargoTrack.DataAccess.Repositories.DeliveryExceptions;
using CargoTrack.DataAccess.Repositories.Employees;
using CargoTrack.DataAccess.Repositories.GenericRepositories;
using CargoTrack.DataAccess.Repositories.TransferCenters;
using CargoTrack.Entity.Entities;
using CargoTrack.Entity.Entities.Enums;
using CargoTrack.WebUI.Consts;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// FluentValidation
builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters()
                .AddValidatorsFromAssemblyContaining<BusinessAssembly>();

// Generic Repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

// Specialized Repositories
builder.Services.AddScoped<IAboutRepository, AboutRepository>();
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ICargoRepository, CargoRepository>();
builder.Services.AddScoped<ICargoMovementRepository, CargoMovementRepository>();
builder.Services.AddScoped<ITransferCenterRepository, TransferCenterRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IDeliveryExceptionRepository, DeliveryExceptionRepository>();
builder.Services.AddScoped<ICargoPriceRepository, CargoPriceRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// Business Services (NO GenericService as per Case Study requirement)
builder.Services.AddScoped<IAboutService, AboutService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<ICargoService, CargoService>();
builder.Services.AddScoped<ICargoPricingService, CargoPricingService>();
builder.Services.AddScoped<ITransferCenterService, TransferCenterService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<CargoTrack.Business.Services.Auths.IAuthService, CargoTrack.Business.Services.Auths.AuthService>();
builder.Services.AddScoped<CargoTrack.Business.Services.Auths.IUserRoleService, CargoTrack.Business.Services.Auths.UserRoleService>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseLazyLoadingProxies();
});

builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(config =>
{
    config.LoginPath = "/Login/Index";
    config.LogoutPath = "/Login/Logout";
    config.AccessDeniedPath = "/ErrorPages/AccessDenied";
    config.Cookie.Name = "CargoTrackCookie";
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Default}/{action=Index}/{id?}");

// Database Initialization and Comprehensive Seed Data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

    context.Database.EnsureCreated();

    // 1. Roles
    string[] roleNames = [Roles.Admin, Roles.Manager, Roles.User];
    foreach (var roleName in roleNames)
    {
        if (!context.Roles.Any(r => r.Name == roleName))
        {
            await roleManager.CreateAsync(new AppRole { Name = roleName, NormalizedName = roleName.ToUpper() });
        }
    }

    // 2. Cities
    if (!context.Cities.Any())
    {
        var cities = new List<City>
        {
            new() { Id = Guid.NewGuid(), Name = "İstanbul" },
            new() { Id = Guid.NewGuid(), Name = "Ankara" },
            new() { Id = Guid.NewGuid(), Name = "İzmir" },
            new() { Id = Guid.NewGuid(), Name = "Bursa" },
            new() { Id = Guid.NewGuid(), Name = "Antalya" },
            new() { Id = Guid.NewGuid(), Name = "Adana" },
            new() { Id = Guid.NewGuid(), Name = "Konya" },
            new() { Id = Guid.NewGuid(), Name = "Kocaeli" }
        };
        context.Cities.AddRange(cities);
        context.SaveChanges();
    }

    var istanbul = context.Cities.First(c => c.Name == "İstanbul");
    var ankara = context.Cities.First(c => c.Name == "Ankara");
    var izmir = context.Cities.First(c => c.Name == "İzmir");
    var bursa = context.Cities.First(c => c.Name == "Bursa");

    // 3. Branches
    if (!context.Branches.Any())
    {
        var branches = new List<Branch>
        {
            new() { Id = Guid.NewGuid(), Name = "Kadıköy Şubesi", CityId = istanbul.Id, AddressDetail = "Moda Cad. No:12 Kadıköy", Phone = "0216 450 11 00" },
            new() { Id = Guid.NewGuid(), Name = "Ataşehir Şubesi", CityId = istanbul.Id, AddressDetail = "Atatürk Mah. No:45 Ataşehir", Phone = "0216 455 22 11" },
            new() { Id = Guid.NewGuid(), Name = "Beşiktaş Şubesi", CityId = istanbul.Id, AddressDetail = "Barbaros Bulvarı No:88 Beşiktaş", Phone = "0212 260 33 22" },
            new() { Id = Guid.NewGuid(), Name = "Çankaya Şubesi", CityId = ankara.Id, AddressDetail = "Tunalı Hilmi Cad. No:34 Çankaya", Phone = "0312 425 44 33" },
            new() { Id = Guid.NewGuid(), Name = "Kızılay Şubesi", CityId = ankara.Id, AddressDetail = "Atatürk Bulvarı No:110 Kızılay", Phone = "0312 430 55 44" },
            new() { Id = Guid.NewGuid(), Name = "Konak Şubesi", CityId = izmir.Id, AddressDetail = "Cumhuriyet Bulvarı No:77 Konak", Phone = "0232 480 66 55" },
            new() { Id = Guid.NewGuid(), Name = "Nilüfer Şubesi", CityId = bursa.Id, AddressDetail = "FSM Bulvarı No:23 Nilüfer", Phone = "0224 240 77 66" }
        };
        context.Branches.AddRange(branches);
        context.SaveChanges();
    }

    var branchKadikoy = context.Branches.First(b => b.Name.Contains("Kadıköy"));
    var branchAtasehir = context.Branches.First(b => b.Name.Contains("Ataşehir"));
    var branchCankaya = context.Branches.First(b => b.Name.Contains("Çankaya"));
    var branchKonak = context.Branches.First(b => b.Name.Contains("Konak"));
    var branchNilufer = context.Branches.First(b => b.Name.Contains("Nilüfer"));

    // 4. Transfer Centers
    if (!context.TransferCenters.Any())
    {
        var tcenters = new List<TransferCenter>
        {
            new() { Id = Guid.NewGuid(), Name = "İstanbul Anadolu Transfer Merkezi", Code = "TM-IST-AND", CityId = istanbul.Id, DailyCapacity = 20000, AddressDetail = "Tuzla Lojistik Köyü" },
            new() { Id = Guid.NewGuid(), Name = "İstanbul Avrupa Transfer Merkezi", Code = "TM-IST-AVR", CityId = istanbul.Id, DailyCapacity = 25000, AddressDetail = "Hadımköy Lojistik Üssü" },
            new() { Id = Guid.NewGuid(), Name = "Ankara Lojistik Aktarma Merkezi", Code = "TM-ANK-01", CityId = ankara.Id, DailyCapacity = 18000, AddressDetail = "Kazan Lojistik Bölgesi" },
            new() { Id = Guid.NewGuid(), Name = "Ege Bölge Transfer Merkezi", Code = "TM-IZM-01", CityId = izmir.Id, DailyCapacity = 15000, AddressDetail = "Torbalı Aktarma" }
        };
        context.TransferCenters.AddRange(tcenters);
        context.SaveChanges();
    }

    var tmIst = context.TransferCenters.First(t => t.Code == "TM-IST-AND");
    var tmAnk = context.TransferCenters.First(t => t.Code == "TM-ANK-01");

    // 5. Pricing Policy
    if (!context.CargoPrices.Any())
    {
        context.CargoPrices.Add(new CargoPrice
        {
            Name = "Standart Kurumsal Fiyat Politikası",
            BasePrice = 45.00m,
            PerKgRate = 12.00m,
            PerDesiRate = 10.00m,
            InterCityMultiplier = 1.35m,
            ExpressMultiplier = 1.50m,
            SameDayMultiplier = 2.00m,
            FragileHandlingFee = 25.00m,
            IsActive = true
        });
        context.SaveChanges();
    }

    // 6. Users
    // Admin: Neriman Aslan
    var defaultEmail = "neriman.aslan045@gmail.com";
    var defaultUser = await userManager.FindByEmailAsync(defaultEmail);
    if (defaultUser == null)
    {
        defaultUser = new AppUser
        {
            FirstName = "Neriman",
            LastName = "Aslan",
            UserName = "nerimanaslan",
            Email = defaultEmail,
            EmailConfirmed = true
        };
        var res = await userManager.CreateAsync(defaultUser, "Neriman123*");
        if (res.Succeeded)
        {
            await userManager.AddToRoleAsync(defaultUser, Roles.Admin);
            await userManager.AddToRoleAsync(defaultUser, Roles.User);
        }
    }

    // Manager: Şube Yöneticisi (Assigned to Kadıköy)
    var managerEmail = "manager@cargotrack.com";
    var managerUserName = "manager";
    var managerPassword = "123456";
    var managerUser = await userManager.FindByEmailAsync(managerEmail);
    if (managerUser == null)
    {
        managerUser = new AppUser
        {
            FirstName = "Serkan",
            LastName = "Yönetici",
            UserName = managerUserName,
            Email = managerEmail,
            EmailConfirmed = true,
            BranchId = branchKadikoy.Id
        };
        var res = await userManager.CreateAsync(managerUser, managerPassword);
        if (res.Succeeded)
        {
            await userManager.AddToRoleAsync(managerUser, Roles.Manager);
        }
    }
    else
    {
        bool userChanged = false;
        if (managerUser.UserName != managerUserName)
        {
            managerUser.UserName = managerUserName;
            userChanged = true;
        }
        if (managerUser.BranchId == null)
        {
            managerUser.BranchId = branchKadikoy.Id;
            userChanged = true;
        }
        if (userChanged)
        {
            await userManager.UpdateAsync(managerUser);
        }
        if (!await userManager.CheckPasswordAsync(managerUser, managerPassword))
        {
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(managerUser);
            await userManager.ResetPasswordAsync(managerUser, resetToken, managerPassword);
        }
        if (!await userManager.IsInRoleAsync(managerUser, Roles.Manager))
        {
            await userManager.AddToRoleAsync(managerUser, Roles.Manager);
        }
    }

    // User: Ali Yıldız
    var aliEmail = "ali@gmail.com";
    var aliUser = await userManager.FindByEmailAsync(aliEmail);
    if (aliUser == null)
    {
        aliUser = new AppUser
        {
            FirstName = "Ali",
            LastName = "Yıldız",
            UserName = "ali01",
            Email = aliEmail,
            EmailConfirmed = true
        };
        var res = await userManager.CreateAsync(aliUser, "Ali1234*");
        if (res.Succeeded)
        {
            await userManager.AddToRoleAsync(aliUser, Roles.User);
        }
    }

    // User: Ayşe Yılmaz
    var ayseEmail = "ayse@gmail.com";
    var ayseUser = await userManager.FindByEmailAsync(ayseEmail);
    if (ayseUser == null)
    {
        ayseUser = new AppUser
        {
            FirstName = "Ayşe",
            LastName = "Yılmaz",
            UserName = "ayse01",
            Email = ayseEmail,
            EmailConfirmed = true
        };
        var res = await userManager.CreateAsync(ayseUser, "Ayse1234*");
        if (res.Succeeded)
        {
            await userManager.AddToRoleAsync(ayseUser, Roles.User);
        }
    }

    // 7. Employees
    if (!context.Employees.Any())
    {
        var emps = new List<Employee>
        {
            new() { FirstName = "Ahmet", LastName = "Kaya", Title = "Dağıtım Kuryesi", BranchId = branchCankaya.Id, Phone = "0532 100 20 30", Email = "ahmet.kaya@cargotrack.com" },
            new() { FirstName = "Mehmet", LastName = "Öz", Title = "Dağıtım Kuryesi", BranchId = branchKadikoy.Id, Phone = "0532 200 30 40", Email = "mehmet.oz@cargotrack.com" },
            new() { FirstName = "Canan", LastName = "Demir", Title = "Operasyon Yetkilisi", BranchId = branchAtasehir.Id, Phone = "0532 300 40 50", Email = "canan.demir@cargotrack.com" },
            new() { FirstName = "Burak", LastName = "Aydın", Title = "Transfer Sorumlusu", TransferCenterId = tmIst.Id, Phone = "0532 400 50 60", Email = "burak.aydin@cargotrack.com" }
        };
        context.Employees.AddRange(emps);
        context.SaveChanges();
    }

    var courierAhmet = context.Employees.First(e => e.FirstName == "Ahmet");
    var courierMehmet = context.Employees.First(e => e.FirstName == "Mehmet");

    // 8. Sample Cargos with Full Lifecycles
    if (!context.Cargos.Any(c => c.TrackCode == "MYC-2026-849251" || c.TrackCode == "CGT2026001458"))
    {
        // 1. Cargo: OutForDelivery (Matching PDF & Template!)
        var cargo1 = new Cargo
        {
            TrackCode = "MYC-2026-849251",
            ShipmentDate = DateTime.Now.AddDays(-2),
            EstimatedDeliveryDate = DateTime.Today,
            Weight = 2.4,
            Width = 20,
            Height = 25,
            Length = 15,
            Desi = 2.5,
            Price = 95.00m,
            CargoType = CargoType.Standard,
            CargoStatus = CargoStatus.OutForDelivery,
            SenderId = defaultUser.Id,
            ReceiverId = aliUser.Id,
            ReceiverName = "Ahmet Demir",
            ReceiverPhone = "0532 999 88 77",
            ReceiverAddress = "Tunalı Hilmi Cad. No:34 Çankaya / Ankara",
            OriginBranchId = branchAtasehir.Id,
            DestinationBranchId = branchCankaya.Id,
            CurrentBranchId = branchCankaya.Id,
            DeliveryPinCode = "849251",
            DeliveredByEmployeeId = courierAhmet.Id
        };
        context.Cargos.Add(cargo1);
        context.SaveChanges();

        // Also add CGT2026001458 so direct search for html template sample works:
        var cargoTemplate = new Cargo
        {
            TrackCode = "CGT2026001458",
            ShipmentDate = DateTime.Now.AddDays(-2),
            EstimatedDeliveryDate = DateTime.Today,
            Weight = 2.4,
            Width = 20,
            Height = 25,
            Length = 15,
            Desi = 2.5,
            Price = 95.00m,
            CargoType = CargoType.Standard,
            CargoStatus = CargoStatus.OutForDelivery,
            SenderId = defaultUser.Id,
            ReceiverId = aliUser.Id,
            ReceiverName = "Ahmet Demir",
            ReceiverPhone = "0532 999 88 77",
            ReceiverAddress = "Çankaya / Ankara",
            OriginBranchId = branchAtasehir.Id,
            DestinationBranchId = branchCankaya.Id,
            CurrentBranchId = branchCankaya.Id,
            DeliveryPinCode = "849251",
            DeliveredByEmployeeId = courierAhmet.Id
        };
        context.Cargos.Add(cargoTemplate);
        context.SaveChanges();

        // Movements for Cargo 1
        var movements1 = new List<CargoMovement>
        {
            new() { CargoId = cargo1.Id, PreviousStatus = null, NewStatus = CargoStatus.Created, Timestamp = DateTime.Now.AddDays(-2).AddHours(9), LocationName = "Ataşehir Şubesi", Description = "Gönderi Teslim Alındı", BranchId = branchAtasehir.Id },
            new() { CargoId = cargo1.Id, PreviousStatus = CargoStatus.Created, NewStatus = CargoStatus.AtOriginBranch, Timestamp = DateTime.Now.AddDays(-2).AddHours(11), LocationName = "Ataşehir Şubesi", Description = "Gönderici Şubesinde Kabul Edildi", BranchId = branchAtasehir.Id },
            new() { CargoId = cargo1.Id, PreviousStatus = CargoStatus.AtOriginBranch, NewStatus = CargoStatus.InTransferCenter, Timestamp = DateTime.Now.AddDays(-2).AddHours(16), LocationName = "İstanbul Anadolu Transfer Merkezi", Description = "Transfer Merkezine Gönderildi", TransferCenterId = tmIst.Id },
            new() { CargoId = cargo1.Id, PreviousStatus = CargoStatus.InTransferCenter, NewStatus = CargoStatus.InTransferCenter, Timestamp = DateTime.Now.AddDays(-1).AddHours(4), LocationName = "Ankara Lojistik Aktarma Merkezi", Description = "Ankara Aktarma Merkezinden Çıktı", TransferCenterId = tmAnk.Id },
            new() { CargoId = cargo1.Id, PreviousStatus = CargoStatus.InTransferCenter, NewStatus = CargoStatus.AtDestinationBranch, Timestamp = DateTime.Now.AddDays(-1).AddHours(8), LocationName = "Çankaya Şubesi", Description = "Varış Dağıtım Şubesine Ulaştı", BranchId = branchCankaya.Id },
            new() { CargoId = cargo1.Id, PreviousStatus = CargoStatus.AtDestinationBranch, NewStatus = CargoStatus.OutForDelivery, Timestamp = DateTime.Now.AddHours(-2), LocationName = "Çankaya Dağıtım Bölgesi", Description = "Kurye tarafından dağıtıma çıkarıldı. Teslimat Kodu üretildi.", EmployeeId = courierAhmet.Id }
        };
        context.CargoMovements.AddRange(movements1);

        // Movements for template cargo
        foreach (var m in movements1)
        {
            context.CargoMovements.Add(new CargoMovement
            {
                CargoId = cargoTemplate.Id,
                PreviousStatus = m.PreviousStatus,
                NewStatus = m.NewStatus,
                Timestamp = m.Timestamp,
                LocationName = m.LocationName,
                Description = m.Description,
                BranchId = m.BranchId,
                TransferCenterId = m.TransferCenterId,
                EmployeeId = m.EmployeeId
            });
        }

        // 2. Cargo: InTransferCenter (Matching CGT2026001287 from template)
        var cargo2 = new Cargo
        {
            TrackCode = "CGT2026001287",
            ShipmentDate = DateTime.Now.AddDays(-1),
            EstimatedDeliveryDate = DateTime.Today.AddDays(1),
            Weight = 4.2,
            Width = 30,
            Height = 30,
            Length = 20,
            Desi = 6.0,
            Price = 135.00m,
            CargoType = CargoType.Express,
            CargoStatus = CargoStatus.InTransferCenter,
            SenderId = aliUser.Id,
            ReceiverName = "Kemal Sunal",
            ReceiverPhone = "0533 111 22 33",
            ReceiverAddress = "Nilüfer / Bursa",
            OriginBranchId = branchKonak.Id,
            DestinationBranchId = branchNilufer.Id,
            CurrentTransferCenterId = tmIst.Id
        };
        context.Cargos.Add(cargo2);
        context.SaveChanges();

        context.CargoMovements.AddRange(new List<CargoMovement>
        {
            new() { CargoId = cargo2.Id, PreviousStatus = null, NewStatus = CargoStatus.Created, Timestamp = DateTime.Now.AddDays(-1).AddHours(10), LocationName = "Konak Şubesi", Description = "Gönderi kabulü yapıldı.", BranchId = branchKonak.Id },
            new() { CargoId = cargo2.Id, PreviousStatus = CargoStatus.Created, NewStatus = CargoStatus.InTransferCenter, Timestamp = DateTime.Now.AddDays(-1).AddHours(18), LocationName = "Ege Transfer Merkezi", Description = "Transfer Merkezinde Tasnif Edildi.", TransferCenterId = tmIst.Id }
        });

        // 3. Cargo: Delivered (Matching CGT2026000974 from template)
        var cargo3 = new Cargo
        {
            TrackCode = "CGT2026000974",
            ShipmentDate = DateTime.Now.AddDays(-4),
            ArrivalDate = DateTime.Now.AddDays(-1),
            EstimatedDeliveryDate = DateTime.Now.AddDays(-1),
            Weight = 1.8,
            Desi = 1.5,
            Price = 65.00m,
            CargoType = CargoType.Standard,
            CargoStatus = CargoStatus.Delivered,
            SenderId = ayseUser.Id,
            ReceiverId = defaultUser.Id,
            ReceiverName = "Neriman Aslan",
            ReceiverPhone = "0535 000 11 22",
            ReceiverAddress = "Kadıköy / İstanbul",
            OriginBranchId = branchCankaya.Id,
            DestinationBranchId = branchKadikoy.Id,
            CurrentBranchId = branchKadikoy.Id,
            ReceivedBy = "Neriman Aslan",
            DeliveredByEmployeeId = courierMehmet.Id,
            DeliveryPinCode = "974512"
        };
        context.Cargos.Add(cargo3);
        context.SaveChanges();

        context.Deliveries.Add(new Delivery
        {
            CargoId = cargo3.Id,
            DeliveryDate = DateTime.Now.AddDays(-1),
            ReceivedBy = "Neriman Aslan",
            ReceiverRelationship = "Kendisi",
            DeliveryPinCodeVerified = true,
            DeliveredByEmployeeId = courierMehmet.Id,
            Notes = "Adreste elden teslim edildi."
        });

        context.CargoMovements.AddRange(new List<CargoMovement>
        {
            new() { CargoId = cargo3.Id, PreviousStatus = null, NewStatus = CargoStatus.Created, Timestamp = DateTime.Now.AddDays(-4), LocationName = "Çankaya Şubesi", Description = "Kargo oluşturuldu.", BranchId = branchCankaya.Id },
            new() { CargoId = cargo3.Id, PreviousStatus = CargoStatus.Created, NewStatus = CargoStatus.InTransferCenter, Timestamp = DateTime.Now.AddDays(-3), LocationName = "Ankara Lojistik Aktarma", Description = "Transfer merkezine ulaştı.", TransferCenterId = tmAnk.Id },
            new() { CargoId = cargo3.Id, PreviousStatus = CargoStatus.InTransferCenter, NewStatus = CargoStatus.AtDestinationBranch, Timestamp = DateTime.Now.AddDays(-2), LocationName = "Kadıköy Şubesi", Description = "Kadıköy varış şubesine ulaştı.", BranchId = branchKadikoy.Id },
            new() { CargoId = cargo3.Id, PreviousStatus = CargoStatus.AtDestinationBranch, NewStatus = CargoStatus.OutForDelivery, Timestamp = DateTime.Now.AddDays(-1).AddHours(9), LocationName = "Kadıköy Dağıtım", Description = "Dağıtıma çıkarıldı.", BranchId = branchKadikoy.Id },
            new() { CargoId = cargo3.Id, PreviousStatus = CargoStatus.OutForDelivery, NewStatus = CargoStatus.Delivered, Timestamp = DateTime.Now.AddDays(-1).AddHours(14), LocationName = "Kadıköy", Description = "Alıcıya teslim edildi.", BranchId = branchKadikoy.Id, EmployeeId = courierMehmet.Id }
        });

        // 4. Cargo: DeliveryFailed with DeliveryException
        var cargo4 = new Cargo
        {
            TrackCode = "MYC-2026-339102",
            ShipmentDate = DateTime.Now.AddDays(-3),
            EstimatedDeliveryDate = DateTime.Now.AddDays(-1),
            Weight = 3.5,
            Desi = 4.0,
            Price = 110.00m,
            CargoType = CargoType.Standard,
            CargoStatus = CargoStatus.DeliveryFailed,
            SenderId = aliUser.Id,
            ReceiverName = "Selim Varol",
            ReceiverPhone = "0534 888 77 66",
            ReceiverAddress = "Caddebostan No:19 Kadıköy / İstanbul",
            OriginBranchId = branchNilufer.Id,
            DestinationBranchId = branchKadikoy.Id,
            CurrentBranchId = branchKadikoy.Id,
            FailedDeliveryAttempts = 1
        };
        context.Cargos.Add(cargo4);
        context.SaveChanges();

        context.DeliveryExceptions.Add(new DeliveryException
        {
            CargoId = cargo4.Id,
            ExceptionDate = DateTime.Now.AddHours(-5),
            Reason = "Alıcı Adreste Bulunamadı",
            AttemptNumber = 1,
            RecordedByEmployeeId = courierMehmet.Id,
            Notes = "Zil çalındı, telefona ulaşılamadı. Yeniden dağıtıma planlanacak."
        });

        // 5. Cargo: ReturnProcess (3 failed attempts!)
        var cargo5 = new Cargo
        {
            TrackCode = "MYC-2026-551029",
            ShipmentDate = DateTime.Now.AddDays(-5),
            EstimatedDeliveryDate = DateTime.Now.AddDays(-3),
            Weight = 5.0,
            Desi = 6.0,
            Price = 150.00m,
            CargoType = CargoType.Fragile,
            CargoStatus = CargoStatus.ReturnProcess,
            SenderId = defaultUser.Id,
            ReceiverName = "Engin Altan",
            ReceiverPhone = "0536 777 66 55",
            ReceiverAddress = "Moda No:10 Kadıköy",
            OriginBranchId = branchCankaya.Id,
            DestinationBranchId = branchKadikoy.Id,
            CurrentBranchId = branchKadikoy.Id,
            FailedDeliveryAttempts = 3
        };
        context.Cargos.Add(cargo5);
        context.SaveChanges();

        context.DeliveryExceptions.AddRange(new List<DeliveryException>
        {
            new() { CargoId = cargo5.Id, ExceptionDate = DateTime.Now.AddDays(-3), Reason = "Alıcı Yok", AttemptNumber = 1 },
            new() { CargoId = cargo5.Id, ExceptionDate = DateTime.Now.AddDays(-2), Reason = "Alıcı Yok", AttemptNumber = 2 },
            new() { CargoId = cargo5.Id, ExceptionDate = DateTime.Now.AddDays(-1), Reason = "Alıcı Kargoyu Kabul Etmedi (Reddetti)", AttemptNumber = 3 }
        });

        context.CargoMovements.Add(new CargoMovement
        {
            CargoId = cargo5.Id,
            PreviousStatus = CargoStatus.DeliveryFailed,
            NewStatus = CargoStatus.ReturnProcess,
            Timestamp = DateTime.Now.AddDays(-1),
            LocationName = "Kadıköy Şubesi",
            Description = "3 başarısız deneme sonucu kargo İade Sürecine alındı."
        });

        // Initial Audit Logs
        context.AuditLogs.AddRange(new List<AuditLog>
        {
            new() { ActionType = "Create", EntityName = "Cargo", EntityId = cargo1.Id.ToString(), Description = "Kargo oluşturuldu: MYC-2026-849251", UserName = "nerimanaslan", Timestamp = DateTime.Now.AddDays(-2) },
            new() { ActionType = "StatusChange", EntityName = "Cargo", EntityId = cargo1.Id.ToString(), OldValue = "AtDestinationBranch", NewValue = "OutForDelivery", Description = "Kargo dağıtıma çıktı, 6 haneli kod üretildi.", UserName = "manager01", Timestamp = DateTime.Now.AddHours(-2) },
            new() { ActionType = "Delivery", EntityName = "Cargo", EntityId = cargo3.Id.ToString(), OldValue = "OutForDelivery", NewValue = "Delivered", Description = "Kargo güvenlik kodu onaylanarak teslim edildi.", UserName = "manager01", Timestamp = DateTime.Now.AddDays(-1) }
        });

        context.SaveChanges();
    } // End of previous if block

    // --- 15 SAHTE KARGO VE HAREKET EKLENİYOR ---
    if (!context.Cargos.Any(c => c.ReceiverName == "Zeynep Yılmaz"))
    {
        var extraCargos = new List<Cargo>();
        var extraMovements = new List<CargoMovement>();
        var rnd = new Random();

        string[] names = { "Zeynep Yılmaz", "Murat Özkan", "Elif Polat", "Burak Şahin", "Merve Çelik", "Kemal Sunal", "Haluk Bilginer", "Şener Şen", "Adile Naşit", "Gülşen Bubikoğlu", "Ahmet Kaya", "Barış Manço", "Sezen Aksu", "Tarkan Tevetoğlu", "Müzeyyen Senar" };
        string[] phones = { "05551112233", "05324445566", "05447778899", "05051234567", "05339876543" };
        var availableBranches = new[] { branchKadikoy, branchAtasehir, branchCankaya, branchKonak, branchNilufer };
        var baseDate = DateTime.Now.AddDays(-10);

        for (int i = 0; i < 15; i++)
        {
            var origin = availableBranches[rnd.Next(availableBranches.Length)];
            var dest = availableBranches[rnd.Next(availableBranches.Length)];
            if (origin.Id == dest.Id) dest = availableBranches[(rnd.Next(availableBranches.Length) + 1) % availableBranches.Length];

            var statusValue = rnd.Next(1, 7); // 1 to 6 (Created to Delivered)
            var status = (CargoStatus)statusValue;

            var cargo = new Cargo
            {
                TrackCode = $"MYC-2026-{rnd.Next(100000, 999999)}",
                ShipmentDate = baseDate.AddDays(i),
                EstimatedDeliveryDate = baseDate.AddDays(i + 2),
                Weight = rnd.Next(1, 10),
                Width = rnd.Next(10, 50),
                Height = rnd.Next(10, 50),
                Length = rnd.Next(10, 50),
                Desi = rnd.Next(1, 15),
                Price = rnd.Next(50, 300),
                CargoType = CargoType.Standard,
                CargoStatus = status,
                SenderId = defaultUser.Id,
                ReceiverName = names[i],
                ReceiverPhone = phones[rnd.Next(phones.Length)],
                OriginBranchId = origin.Id,
                DestinationBranchId = dest.Id,
                CurrentBranchId = status >= CargoStatus.AtDestinationBranch ? dest.Id : origin.Id,
            };

            extraCargos.Add(cargo);

            // Create Movements
            extraMovements.Add(new CargoMovement { CargoId = cargo.Id, PreviousStatus = null, NewStatus = CargoStatus.Created, Timestamp = cargo.ShipmentDate, LocationName = origin.Name, BranchId = origin.Id, Description = "Kargo oluşturuldu" });
            
            if (status >= CargoStatus.AtOriginBranch)
                extraMovements.Add(new CargoMovement { CargoId = cargo.Id, PreviousStatus = CargoStatus.Created, NewStatus = CargoStatus.AtOriginBranch, Timestamp = cargo.ShipmentDate.AddHours(2), LocationName = origin.Name, BranchId = origin.Id, Description = "Şubede teslim alındı" });

            if (status >= CargoStatus.InTransferCenter)
                extraMovements.Add(new CargoMovement { CargoId = cargo.Id, PreviousStatus = CargoStatus.AtOriginBranch, NewStatus = CargoStatus.InTransferCenter, Timestamp = cargo.ShipmentDate.AddHours(10), LocationName = "Transfer Merkezi", Description = "Transfer Merkezinde işleniyor" });

            if (status >= CargoStatus.AtDestinationBranch)
                extraMovements.Add(new CargoMovement { CargoId = cargo.Id, PreviousStatus = CargoStatus.InTransferCenter, NewStatus = CargoStatus.AtDestinationBranch, Timestamp = cargo.ShipmentDate.AddDays(1).AddHours(2), LocationName = dest.Name, BranchId = dest.Id, Description = "Varış şubesine ulaştı" });

            if (status >= CargoStatus.OutForDelivery)
                extraMovements.Add(new CargoMovement { CargoId = cargo.Id, PreviousStatus = CargoStatus.AtDestinationBranch, NewStatus = CargoStatus.OutForDelivery, Timestamp = cargo.ShipmentDate.AddDays(1).AddHours(8), LocationName = dest.Name, BranchId = dest.Id, Description = "Dağıtıma çıkarıldı" });

            if (status == CargoStatus.Delivered)
                extraMovements.Add(new CargoMovement { CargoId = cargo.Id, PreviousStatus = CargoStatus.OutForDelivery, NewStatus = CargoStatus.Delivered, Timestamp = cargo.ShipmentDate.AddDays(1).AddHours(12), LocationName = dest.Name, BranchId = dest.Id, Description = "Teslim edildi" });
        }

        context.Cargos.AddRange(extraCargos);
        context.CargoMovements.AddRange(extraMovements);
        context.SaveChanges();
    }
}

app.Run();

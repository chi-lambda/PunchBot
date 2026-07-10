using System.Diagnostics.CodeAnalysis;
using AspNetCoreInjection.TypedFactories;
using Microsoft.EntityFrameworkCore;
using PunchBotCore2.Data;
using PunchBotCore2.Util;

namespace PunchBotCore2;

[ExcludeFromCodeCoverage]
public class Program
{
    private const string dbFilename = "times.db";

    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddSingleton<IDateTimeService, DateTimeService>();
        builder.Services.AddSingleton<LiteDB.ILiteDatabase>(new LiteDB.LiteDatabase(dbFilename));
        builder.Services.AddDbContextFactory<PunchContext>(
            options => options.UseSqlite(builder.Configuration.GetConnectionString("PunchContextSQLite")));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddTransient<DataAggregator>();
        builder.Services.RegisterTypedFactory<IDataAggregatorFactory>().ForConcreteType<DataAggregator>();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();
        }

        using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
        {
            IServiceProvider services = scope.ServiceProvider;
            PunchContext context = services.GetRequiredService<PunchContext>();
            context.Database.EnsureCreated();
            LiteDB.ILiteDatabase liteDB = services.GetRequiredService<LiteDB.ILiteDatabase>();
            await context.Migrate(liteDB);
        }

        // app.UseHttpsRedirection();

        app.UseRouting();
        app.UsePathBase(new PathString("/punch"));
        app.UseStaticFiles();

        // app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}

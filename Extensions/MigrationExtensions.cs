using Microsoft.EntityFrameworkCore;
using VinhUni_Educator_API.Context;

namespace VinhUni_Educator_API.Extensions
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            using ApplicationDBContext dbContext =
                scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            dbContext.Database.Migrate();
        }
    }
}

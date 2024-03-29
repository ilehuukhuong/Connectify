using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,
            IConfiguration config)
        {
            services.AddCors();
            services.AddHttpClient();
            services.AddScoped<ITokenService, TokenService>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.Configure<ComputervisionSettings>(config.GetSection("ComputervisionSettings"));
            services.Configure<OneDriveSettings>(config.GetSection("OneDriveSettings"));
            services.Configure<ContentModeratorSettings>(config.GetSection("ContentModeratorSettings"));
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            services.Configure<OutlookMailSettings>(config.GetSection("OutlookMailSettings"));
            services.Configure<ReCaptchaSettings>(config.GetSection("ReCaptchaSettings"));
            services.AddScoped<IOneDriveService, OneDriveService>();
            services.AddScoped<INSFWChecker, NSFWChecker>();
            services.AddScoped<ICaptchaService, CaptchaService>();
            services.AddScoped<IContentModeratorService, ContentModeratorService>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<LogUserActivity>();
            services.AddSignalR();
            services.AddSingleton<PresenceTracker>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
using Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Identity
{
    public class Settings
    {

        private static Settings _settings;

        public IConfiguration Configuration { get; private set; }

        public Settings(IConfiguration config)
        {
            Configuration = config;

            // Now set Current
            _settings = this;
        }

        public static Settings Current
        {
            get
            {
                if (_settings == null)
                {
                    _settings = GetConfigurationSettings();
                }

                return _settings;
            }
        }

        private static string AssemblyDirectory
        {
            get
            {
                Assembly thisAssembly = Assembly.GetAssembly(typeof(Settings));
                string codeBase = thisAssembly != null ? thisAssembly.CodeBase : ""; //Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private static Settings GetConfigurationSettings()
        {

            var builder = new ConfigurationBuilder()
                            .SetBasePath(AssemblyDirectory)
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddEnvironmentVariables();

            IConfigurationRoot configuration = builder.Build();

            return new Settings(configuration);

        }



        /// <summary>
        /// Sets up the application's jwt bearer tokens and applies policies to the service collection
        /// </summary>
        /// <param name="services"></param>
        public void AddAuthScheme(IServiceCollection services)
        {
            
            services.AddAuthentication(opts =>
            {
                opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                cfg.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = Current.Configuration.GetSection("TokenSettings:ValidIssuer").Value,
                    ValidAudience = Current.Configuration.GetSection("TokenSettings:ValidAudience").Value,
                    IssuerSigningKey = TokenService.GetKey(Current.Configuration.GetSection("TokenSettings:SecurityKey").Value),
                    ClockSkew = TimeSpan.Zero,

                    // security switches
                    RequireExpirationTime = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = true
                };
            });
        }
    }
}

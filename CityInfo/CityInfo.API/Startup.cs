﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Entities;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;

namespace CityInfo.API
{
    public class Startup
    {
        #region 1.0 way of adding configuration to asp core
        //public static IConfigurationRoot Configuration;

        //public Startup(IHostingEnvironment env)
        //{
        //    /**
        //     * How to target appSettings.json. Note settings for dynamically applying changes and optionality
        //     */
        //    var builder = new ConfigurationBuilder()
        //        .SetBasePath(env.ContentRootPath)
        //        .AddJsonFile("appSettings.json", optional:false, reloadOnChange:true);

        //    Configuration = builder.Build();
        //}
        #endregion

        #region 2.0 way of adding configuration to asp core
        
        public static IConfiguration Configuration { get; private set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        #endregion

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddMvcOptions(o => o.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter()))//add support for xml serialization. can also add or remove note json is default);
                ////this is how to access and manipulate the json serializer settings if you need to for some reasons. But in most cases the defaults are fine
                //.AddJsonOptions(o =>
                //{
                //    if (o.SerializerSettings.ContractResolver != null)
                //    {
                //        var castedResolver = o.SerializerSettings.ContractResolver
                //        as DefaultContractResolver;
                //        castedResolver.NamingStrategy = null;//adding this means that json . net will take property names as defined (it won't convert them to lower case)
                //    }
                //})
                ;


            #if DEBUG
                services.AddTransient<IMailService,LocalMailService>();//light and stateless 
            #else
                services.AddTransient<IMailService,CloudMailService>();//light and stateless / one off
            #endif

            services.AddDbContext<CityInfoContext>(o => o.UseSqlServer(Startup.Configuration["connectionStrings:cityInfoDBConnectionString"]));
            services.AddScoped<ICityInfoRepository, CityInfoRepository>(); //for repos scoped makes the most sense
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, CityInfoContext cityInfoContext)
        {

            loggerFactory.AddConsole();
            loggerFactory.AddDebug(/*can choose a specific log level but default is fine*/);//=> add an instance to any controller to log whatever is desired

            //supplying seed data if there isn't any
            cityInfoContext.EnsureSeedDataForContext();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            
            /*
             * Auto Mapper is Convention based so it'll map same names
             * //(source, destination) 
             */
            AutoMapper.Mapper.Initialize(cfg => 
            {
                cfg.CreateMap<Entities.City, Models.CityWithoutPointOfInterestDto>();
                cfg.CreateMap<Entities.City, Models.CityDto>();
                cfg.CreateMap<Entities.PointOfInterest, Models.PointOfInterestDto>();
                cfg.CreateMap<Models.PointOfInterestForCreationDto, Entities.PointOfInterest>();
                cfg.CreateMap<Models.PointOfInterestForUpdateDto, Entities.PointOfInterest>();
                cfg.CreateMap<Entities.PointOfInterest, Models.PointOfInterestForUpdateDto>();
            });


            app.UseMvc();


            //app.Run((context) =>
            //{
            //    throw new Exception("Example Throwing an Exception!");
            //});

            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Hello World!");
            //});
        }
    }
}

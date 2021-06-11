﻿using System;
using System.IO;
using eCommerce.Adapters;
using eCommerce.Auth;
using eCommerce.Business;
using eCommerce.Business.Repositories;
using eCommerce.Common;
using eCommerce.Statistics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace eCommerce.Service
{
    public class SystemService: ISystemService
    {

        private MarketState _marketState;
        
        public SystemService()
        {
            _marketState = MarketState.GetInstance();
        }
        
        public bool IsSystemValid()
        {
            return _marketState.ValidState;
        }

        public bool GetErrMessageIfValidSystem(out string message)
        {
            return _marketState.TryGetErrMessage(out message);
        }

        public void Start(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        public bool InitSystem(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: <Config file>");
                return false;
            }
            
            AppConfig config = AppConfig.GetInstance();
            if (!config.Init(args[0]))
            {
                Console.WriteLine($"Usage: Invalid config file {args[0]}");
                return false;
            }

            MarketFacade marketFacade;
            IUserAuth authService;
            IRepository<User> userRepo = null;
            AbstractStoreRepo storeRepo = null;

            authService = InitAuth(config);
            InitStatistics(config);
            InitPaymentAdapter(config);
            InitSupplyAdapter(config);
            
            string memoryAs = config.GetData("Memory");
            switch (memoryAs)
            {
                case "InMemory":
                {
                    userRepo = new InMemoryRegisteredUsersRepository();
                    storeRepo = new InMemoryStoreRepo();
                    break;
                }
                case "Persistence":
                {
                    userRepo = new PersistenceRegisteredUsersRepo();
                    //TODO update to persistennce store repo
                    storeRepo = new InMemoryStoreRepo();
                    break;   
                }
                case null:
                {
                    config.ThrowErrorOfData("Memory", "missing");
                    break;
                }
                default:
                {
                    config.ThrowErrorOfData("Memory", "invalid");
                    break;
                }
            }

            marketFacade = MarketFacade.GetInstance();
            marketFacade.Init(authService, userRepo, storeRepo);

            string initFilePath;
            if (config.GetData("InitWithData").Equals("True"))
            {
                initFilePath = config.GetData("InitDataFile");
                if (initFilePath != null)
                {
                    InitSystemWithData initSystemWithData = new InitSystemWithData(
                        new AuthService(),
                        new UserService(),
                        new InStoreService());
                    initSystemWithData.Init(initFilePath);
                }
            }

            return true;
        }

        private IUserAuth InitAuth(AppConfig config)
        {
            IUserAuth authService = UserAuth.GetInstance();
            authService.Init(config);
            return authService;
        }
        
        private IStatisticsService InitStatistics(AppConfig config)
        {
            IStatisticsService statisticsService = Statistics.Statistics.GetInstance();
            statisticsService.Init(config);
            return statisticsService;
        }

        public Result<Services> GetInstanceForTests(string configFile)
        {
            if (!File.Exists(configFile))
            {
                return Result.Fail<Services>("No config file");
            }

            IAuthService authService;
            IUserService userService;
            INStoreService inStoreService;
            ICartService cartService;
            IMarketFacade marketFacade;
            
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile(configFile)
                .Build();

            string authKey = config["JWTKey"];
            if (authKey == null)
            {
                return Result.Fail<Services>("Missing JWTKey in config");
            }
            
            string memoryAs = config["Memory"];
            switch (memoryAs)
            {
                case "InMemory":
                {
                    InMemoryRegisteredUserRepo RP = new InMemoryRegisteredUserRepo();
                    UserAuth UA = UserAuth.CreateInstanceForTests(RP, authKey);
                    InMemoryStoreRepo SR = new InMemoryStoreRepo();
                    IRepository<User> UR = new InMemoryRegisteredUsersRepository();
                    marketFacade = MarketFacade.CreateInstanceForTests(UA,UR, SR);
                    break;
                }
                case "Persistence":
                {
                    IRegisteredUserRepo RP = new PersistentRegisteredUserRepo();
                    UserAuth UA = UserAuth.CreateInstanceForTests(RP, authKey);
                    AbstractStoreRepo SR = new PersistenceStoreRepo();
                    IRepository<User> UR = new PersistenceRegisteredUsersRepo();
                    marketFacade = MarketFacade.CreateInstanceForTests(UA,UR, SR);
                    break;   
                }
                default:
                {
                    return Result.Fail<Services>("Invalid value in config");
                    break;
                }
            }

            authService = AuthService.CreateUserServiceForTests(marketFacade);
            userService = UserService.CreateUserServiceForTests(marketFacade);
            inStoreService = InStoreService.CreateUserServiceForTests(marketFacade);
            cartService = CartService.CreateUserServiceForTests(marketFacade);

            return Result.Ok(new Services(authService, userService, inStoreService, cartService));

        }
        
        private void InitPaymentAdapter(AppConfig config)
        {
            string paymentAdapter = config.GetData("PaymentAdapter");
            switch (paymentAdapter)
            {
                case "WSEP":
                {
                    PaymentProxy.AssignPaymentService(new WSEPPaymentAdapter());
                    break;
                }
                case null:
                {
                    break;
                }
                default:
                {
                    config.ThrowErrorOfData("PaymentAdapter", "invalid");
                    break;
                }
                    
            }
        }
        
        private void InitSupplyAdapter(AppConfig config)
        {
            string paymentAdapter = config.GetData("SupplyAdapter");
            switch (paymentAdapter)
            {
                case "WSEP":
                {
                    SupplyProxy.AssignSupplyService(new WSEPSupplyAdapter());
                    break;
                }
                case null:
                {
                    break;
                }
                default:
                {
                    config.ThrowErrorOfData("SupplyAdapter", "invalid");
                    break;
                }
                    
            }
        }
    }
}
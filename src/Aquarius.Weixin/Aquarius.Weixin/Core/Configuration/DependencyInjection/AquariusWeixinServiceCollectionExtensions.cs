﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using System;
using Aquarius.Weixin.Cache;
using Aquarius.Weixin.Core.Authentication;
using Aquarius.Weixin.Core.Configuration.DependencyInjection.Options;
using Aquarius.Weixin.Core.Exceptions;
using Aquarius.Weixin.Core.InterfaceCaller;
using Aquarius.Weixin.Core.JsApi;
using Aquarius.Weixin.Core.MaintainContainer;
using Aquarius.Weixin.Core.Message;
using Aquarius.Weixin.Core.Message.Handler;
using Aquarius.Weixin.Core.Message.Processer;
using Aquarius.Weixin.Core.Message.Reply;
using Aquarius.Weixin.Core.Middleware;
using Aquarius.Weixin.Entity.Configuration;
using Aquarius.Weixin.Entity.Enums;
using Aquarius.Weixin.Entity.WeixinMessage;
using Aquarius.Weixin.Core.Message.Handler.DefaultHandler;

namespace Aquarius.Weixin.Core.Configuration.DependencyInjection
{
    /// <summary>
    /// 添加Aquarius.Weixin的DI扩展
    /// </summary>
    public static class AquariusWeixinServiceCollectionExtensions
    {
        private static AquariusWeixinOptions options = new AquariusWeixinOptions()
        {
            CacheType = CacheType.InMemory,
            MsgMiddlewareType = MessageMiddlewareType.Plain,
            BaseSetting = new BaseSettings()
            {
                Debug = true,
                IsRepetValid = false,
                AppId = string.Empty,
                AppSecret = string.Empty,
                Token = string.Empty,
                EncodingAESKey = string.Empty,
                ApiKey = string.Empty,
                CertPass = string.Empty,
                CertRoot = string.Empty,
                MchId = string.Empty
            },
            RedisConfig = null
        };

        /// <summary>
        /// 添加 Aquarius.Weixin
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddAquariusWeixin(this IServiceCollection services)
        {
            //RestSharp
            services.AddScoped<IRestClient, RestClient>();

            //接口调用
            services.AddScoped<DisposableMessageInterfaceCaller, DisposableMessageInterfaceCaller>();
            services.AddScoped<MenuInterfaceCaller, MenuInterfaceCaller>();
            services.AddScoped<OAuthInterfaceCaller, OAuthInterfaceCaller>();
            services.AddScoped<TemplateMessageInterfaceCaller, TemplateMessageInterfaceCaller>();
            services.AddScoped<TicketInterfaceCaller, TicketInterfaceCaller>();
            services.AddScoped<UserTagManageInterfaceCaller, UserTagManageInterfaceCaller>();
            services.AddScoped<WxPayInterfaceCaller, WxPayInterfaceCaller>();

            //容器
            services.AddScoped<AccessTokenContainer, AccessTokenContainer>();
            services.AddScoped<AuthorizationContainer, AuthorizationContainer>();
            services.AddScoped<TicketContainer, TicketContainer>();

            //签名与验证
            services.AddScoped<SignatureGenerater, SignatureGenerater>();
            services.AddScoped<Verifyer, Verifyer>();

            //消息
            //消息重复处理
            services.AddScoped<MessageRepetHandler, MessageRepetHandler>();
            //消息解析器
            services.AddScoped<MessageParser, MessageParser>();
            //消息处理器
            services.AddScoped<MessageProcesser, MessageProcesser>();
            //消息处理方法
            services.AddScoped<IClickEvtMessageHandler, DefaultClickEvtMessageHandler>();
            services.AddScoped<IImageMessageHandler, DefaultImageMessageHandler>();
            services.AddScoped<ILinkMessageHandler, DefaultLinkMessageHandler>();
            services.AddScoped<ILocationEvtMessageHandler, DefaultLocationEvtMessageHandler>();
            services.AddScoped<ILocationMessageHandler, DefaultLocationMessageHandler>();
            services.AddScoped<IScanEvtMessageHandler, DefaultScanEvtMessageHandler>();
            services.AddScoped<IScanSubscribeEvtMessageHandler, DefaultScanSubscribeEvtMessageHandler>();
            services.AddScoped<IShortVideoMessageHandler, DefaultShortVideoMessageHandler>();
            services.AddScoped<ISubscribeEvtMessageHandler, DefaultSubscribeEvtMessageHandler>();
            services.AddScoped<ITextMessageHandler, DefaultTextMessageHandler>();
            services.AddScoped<IUnsubscribeEvtMessageHandler, DefaultUnsubscribeEvtMessageHandler>();
            services.AddScoped<IVideoMessageHandler, DefaultVideoMessageHandler>();
            services.AddScoped<IViewEvtMessageHandler, DefaultViewEvtMessageHandler>();
            services.AddScoped<IVoiceMessageHandler, DefaultVoiceMessageHandler>();
            //消息回复
            services.AddScoped<IMessageReply<ImageMessage>, ImageMessageReply>();
            services.AddScoped<IMessageReply<MusicMessage>, MusicMessageReply>();
            services.AddScoped<IMessageReply<NewsMessage>, NewsMessageReply>();
            services.AddScoped<IMessageReply<TextMessage>, TextMessageReply>();
            services.AddScoped<IMessageReply<VideoMessage>, VideoMessageReply>();
            services.AddScoped<IMessageReply<VoiceMessage>, VoiceMessageReply>();

            //JS-API
            services.AddScoped<ConfigGenerater, ConfigGenerater>();

            //缓存
            switch (options.CacheType)
            {
                case CacheType.InMemory:
                    //InMemory
                    services.AddMemoryCache();
                    services.AddScoped<ICache, InMemoryCache>();
                    break;
                case CacheType.Redis:
                    //Redis
                    if (options.RedisConfig == null)
                        throw new RedisNotConfiguredExpection("Redis未配置");
                    services.AddDistributedRedisCache(opt =>
                    {
                        opt.Configuration = $"{options.RedisConfig.Host}:{options.RedisConfig.Port}{(string.IsNullOrEmpty(options.RedisConfig.Password) ? string.Empty : string.Concat(",password=", options.RedisConfig.Password))}";
                    });
                    services.AddScoped<ICache, RedisCache>();
                    break;
                default:
                    break;
            }

            //微信设置
            services.AddScoped<BaseSettings, BaseSettings>(provider =>
                options.BaseSetting);

            //消息中间件
            switch(options.MsgMiddlewareType)
            {
                case MessageMiddlewareType.Plain:
                    //明文
                    services.AddScoped<IMessageMiddleware, MessageMiddlePlain>();
                    break;
                case MessageMiddlewareType.Cipher:
                    //密文
                    services.AddScoped<IMessageMiddleware, MessageMiddleCipher>();
                    break;
                default:
                    break;
            }

            return services;
        }

        /// <summary>
        /// 添加 Aquarius.Weixin
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="setupAction">The setup action.</param>
        /// <returns></returns>
        public static IServiceCollection AddAquariusWeixin(this IServiceCollection services, Action<AquariusWeixinOptions> setupAction)
        {
            setupAction.Invoke(options);
            return services.AddAquariusWeixin();
        }

        /// <summary>
        /// 添加 Aquarius.Weixin
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public static IServiceCollection AddAquariusWeixin(this IServiceCollection services, IConfiguration configuration)
        {
            options = configuration.Get<AquariusWeixinOptions>();
            return services.AddAquariusWeixin();
        }
    }
}

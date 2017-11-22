﻿namespace Weixin.Netcore.Model.WeixinMessage
{
    /// <summary>
    /// 接收消息
    /// </summary>
    public interface IMessageReceive
    {
        /// <summary>
        /// 转化实体
        /// </summary>
        /// <param name="xml"></param>
        void ConvertEntity(string xml);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RequestId
{
    /**
     * 不受服务器消息队列限制的心跳
     */
    NO_QUEUE_HEART = 0,
    /**
         * 登录
         */
    LOGIN = 1,
    /**
     * 登录完成后通知后端的消息
     */
    LOGIN_OVER = 82,
}

using com.gateway.log;
using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using STC.Chrome.Plugin.Data;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.QQ;
using STC.Common.Utils;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using HttpMethod = DotNetty.Codecs.Http.HttpMethod;

namespace STC.Chrome.Plugin.Handler
{
    public class HttpServerHandler : SimpleChannelInboundHandler<IFullHttpRequest>
    {
        private IHttpRequest _request;
        private StringBuilder _bodyBuffer = new StringBuilder();
        protected override void ChannelRead0(IChannelHandlerContext ctx, IFullHttpRequest msg)
        {   

            string uri = msg.Uri;
            HttpMethod method = msg.Method;
            LogHelper.Info($"收到请求: {msg.Method} {msg.Uri}");
            if (uri == "/" && method.Equals(HttpMethod.Get))
            {
                string responseContent = "Hello, this is STC/BDB local web server!";
                // 构造 HTTP 响应
                IByteBuffer buffer = Unpooled.CopiedBuffer(responseContent, Encoding.UTF8);
                IFullHttpResponse response = new DefaultFullHttpResponse(
                    HttpVersion.Http11, HttpResponseStatus.OK, buffer);
                response.Headers.Set(HttpHeaderNames.ContentType, "text/plain; charset=UTF-8");
                // ---- CORS 跨域支持 ----
                response.Headers.Set(HttpHeaderNames.AccessControlAllowOrigin, "*");
                // 如果你需要携带 cookie 或身份凭据，将上面改为具体域名，并添加：
                response.Headers.Set(HttpHeaderNames.AccessControlAllowCredentials, "true");
                // 允许的 HTTP 方法
                response.Headers.Set(HttpHeaderNames.AccessControlAllowMethods, "GET, POST, PUT, DELETE, OPTIONS");
                // 允许前端携带的自定义头
                response.Headers.Set(HttpHeaderNames.AccessControlAllowHeaders, "Origin, X-Requested-With, Content-Type, Accept, Authorization, sessionId");
                // 允许前端访问的响应头（可选）
                response.Headers.Set(HttpHeaderNames.AccessControlExposeHeaders, "Content-Disposition, Content-Length, Content-Type");
                // 预检请求缓存时间（单位：秒）
                response.Headers.Set(HttpHeaderNames.AccessControlMaxAge, "3600");

                ctx.WriteAndFlushAsync(response);

                LogHelper.Info("处理 GET /");
            }
            else if (uri == "/qbBindTokenCallBack")
            {
                if (method.Equals(HttpMethod.Options))
                {
                    var response = GetOptionsHttpResponse();
                    ctx.WriteAndFlushAsync(response).ContinueWith(t => ctx.CloseAsync());
                    LogHelper.Info("收到预检请求（CORS）。");
                }
                else if (method.Equals(HttpMethod.Post))
                {
                    // 确认是否 FullHttpRequest
                    string requestBody = null;
                    byte[] bodyBytes = new byte[msg.Content.ReadableBytes];
                    msg.Content.ReadBytes(bodyBytes);

                    // 没有压缩，直接读取
                    requestBody = Encoding.UTF8.GetString(bodyBytes);

                    LogHelper.Info("收到 qbBindTokenCallBack 回调: " + requestBody);

                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        FQBindTokenResponse fQBindTokenResponse = CommonUtils.DeserializeObject<FQBindTokenResponse>(requestBody);
                        CefHandler.Instance.FQClientToken = fQBindTokenResponse;
                    }
                    // 构造返回 JSON
                    string responseJson = "{\"status\":\"success\",\"message\":\"数据已收到\"}";

                    // 构造 HTTP 响应
                    var responseContent = Unpooled.CopiedBuffer(responseJson, Encoding.UTF8);
                    var response = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK, responseContent);
                    response.Headers.Set(HttpHeaderNames.ContentType, "application/json; charset=UTF-8");
                    // ---- CORS 跨域支持 ----
                    response.Headers.Set(HttpHeaderNames.AccessControlAllowOrigin, "*");
                    // 如果你需要携带 cookie 或身份凭据，将上面改为具体域名，并添加：
                    response.Headers.Set(HttpHeaderNames.AccessControlAllowCredentials, "true");
                    // 允许的 HTTP 方法
                    response.Headers.Set(HttpHeaderNames.AccessControlAllowMethods, "GET, POST, PUT, DELETE, OPTIONS");
                    // 允许前端携带的自定义头
                    response.Headers.Set(HttpHeaderNames.AccessControlAllowHeaders, "Origin, X-Requested-With, Content-Type, Accept, Authorization, sessionId");
                    // 允许前端访问的响应头（可选）
                    response.Headers.Set(HttpHeaderNames.AccessControlExposeHeaders, "Content-Disposition, Content-Length, Content-Type");
                    // 预检请求缓存时间（单位：秒）
                    response.Headers.Set(HttpHeaderNames.AccessControlMaxAge, "3600");

                    response.Headers.Set(HttpHeaderNames.ContentLength, responseContent.ReadableBytes);
                    ctx.WriteAndFlushAsync(response);
                }
            }
            else if (uri == "/finqClientToken")
            {
                if (method.Equals(HttpMethod.Options))
                {
                    var response = GetOptionsHttpResponse();
                    ctx.WriteAndFlushAsync(response).ContinueWith(t => ctx.CloseAsync());
                    LogHelper.Info("收到预检请求（CORS）。");
                }
                else if (method.Equals(HttpMethod.Post))
                {
                    // 确认是否 FullHttpRequest
                    string requestBody = null;
                    byte[] bodyBytes = new byte[msg.Content.ReadableBytes];
                    msg.Content.ReadBytes(bodyBytes);

                    // 没有压缩，直接读取
                    requestBody = Encoding.UTF8.GetString(bodyBytes);

                    LogHelper.Info("收到 finqClientToken 回调: " + requestBody);

                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        FQBindTokenResponse TR = CommonUtils.DeserializeObject<FQBindTokenResponse>(requestBody);
                        if (TR != null)
                        {
                            if ((TR.userInfo.qb ?? "") == LocalEnv.UserInfo.UserName)
                            {
                                //如果是自己的，则除了记录外还要重新发送
                                CefHandler.Instance.FQTokenFanout = requestBody;
                            }
                            else
                            {
                                Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "FQTokenFanout", Info3 = requestBody });
                                LogHelper.Info("Qeubee 账号或 QQ 不一致, 忽略!");
                            }
                        }
                    }

                    // 构造返回 JSON
                    string responseJson = "{\"status\":\"success\",\"message\":\"数据已收到\"}";
                    // 构造 HTTP 响应
                    var responseContent = Unpooled.CopiedBuffer(responseJson, Encoding.UTF8);
                    var response = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK, responseContent);
                    response.Headers.Set(HttpHeaderNames.ContentType, "application/json; charset=UTF-8");
                    // ---- CORS 跨域支持 ----
                    response.Headers.Set(HttpHeaderNames.AccessControlAllowOrigin, "*");
                    // 如果你需要携带 cookie 或身份凭据，将上面改为具体域名，并添加：
                    response.Headers.Set(HttpHeaderNames.AccessControlAllowCredentials, "true");
                    // 允许的 HTTP 方法
                    response.Headers.Set(HttpHeaderNames.AccessControlAllowMethods, "GET, POST, PUT, DELETE, OPTIONS");
                    // 允许前端携带的自定义头
                    response.Headers.Set(HttpHeaderNames.AccessControlAllowHeaders, "Origin, X-Requested-With, Content-Type, Accept, Authorization, sessionId");
                    // 允许前端访问的响应头（可选）
                    response.Headers.Set(HttpHeaderNames.AccessControlExposeHeaders, "Content-Disposition, Content-Length, Content-Type");
                    // 预检请求缓存时间（单位：秒）
                    response.Headers.Set(HttpHeaderNames.AccessControlMaxAge, "3600");

                    response.Headers.Set(HttpHeaderNames.ContentLength, responseContent.ReadableBytes);
                    ctx.WriteAndFlushAsync(response);
                }
            }
            else
            {
                // 确认是否 FullHttpRequest
                string requestBody = null;
                byte[] bodyBytes = new byte[msg.Content.ReadableBytes];
                msg.Content.ReadBytes(bodyBytes);

                // 没有压缩，直接读取
                requestBody = Encoding.UTF8.GetString(bodyBytes);

                LogHelper.Info("收到 其它 请求，内容: " + requestBody);

                var response = new DefaultFullHttpResponse(DotNetty.Codecs.Http.HttpVersion.Http11, HttpResponseStatus.NotFound, Unpooled.CopiedBuffer("404 Not Found", Encoding.UTF8));

                response.Headers.Set(HttpHeaderNames.ContentType, "text/plain; charset=UTF-8");
                // ---- CORS 跨域支持 ----
                response.Headers.Set(HttpHeaderNames.AccessControlAllowOrigin, "*");
                // 如果你需要携带 cookie 或身份凭据，将上面改为具体域名，并添加：
                response.Headers.Set(HttpHeaderNames.AccessControlAllowCredentials, "true");
                // 允许的 HTTP 方法
                response.Headers.Set(HttpHeaderNames.AccessControlAllowMethods, "GET, POST, PUT, DELETE, OPTIONS");
                // 允许前端携带的自定义头
                response.Headers.Set(HttpHeaderNames.AccessControlAllowHeaders, "Origin, X-Requested-With, Content-Type, Accept, Authorization, sessionId");
                // 允许前端访问的响应头（可选）
                response.Headers.Set(HttpHeaderNames.AccessControlExposeHeaders, "Content-Disposition, Content-Length, Content-Type");
                // 预检请求缓存时间（单位：秒）
                response.Headers.Set(HttpHeaderNames.AccessControlMaxAge, "3600");

                ctx.WriteAndFlushAsync(response);
                LogHelper.Error("未匹配的请求: " + uri);
            }
        }

        /// <summary>
        /// 获取Options回应
        /// </summary>
        /// <returns></returns>
        private DefaultFullHttpResponse GetOptionsHttpResponse()
        {
            // 处理预检请求（CORS）
            DefaultFullHttpResponse response = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK);
            // ---- 基本响应头 ----
            response.Headers.Set(HttpHeaderNames.ContentType, "application/json; charset=UTF-8");
            // ---- CORS 跨域支持 ----
            response.Headers.Set(HttpHeaderNames.AccessControlAllowOrigin, "*");
            // 如果你需要携带 cookie 或身份凭据，将上面改为具体域名，并添加：
            response.Headers.Set(HttpHeaderNames.AccessControlAllowCredentials, "true");
            // 允许的 HTTP 方法
            response.Headers.Set(HttpHeaderNames.AccessControlAllowMethods, "GET, POST, PUT, DELETE, OPTIONS");
            // 允许前端携带的自定义头
            response.Headers.Set(HttpHeaderNames.AccessControlAllowHeaders, "Origin, X-Requested-With, Content-Type, Accept, Authorization, sessionId");
            // 允许前端访问的响应头（可选）
            response.Headers.Set(HttpHeaderNames.AccessControlExposeHeaders, "Content-Disposition, Content-Length, Content-Type");
            // 预检请求缓存时间（单位：秒）
            response.Headers.Set(HttpHeaderNames.AccessControlMaxAge, "3600");

            return response;
        }
    }
}

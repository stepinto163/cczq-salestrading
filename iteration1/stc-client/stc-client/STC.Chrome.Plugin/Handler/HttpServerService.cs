using com.gateway.log;
using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STC.Common.Model.QQ;
using STC.Common.Utils;
using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Media.Animation;


namespace STC.Chrome.Plugin.Handler
{
    public class HttpServerService
    {
        private static readonly Lazy<HttpServerService> _instance = new Lazy<HttpServerService>(() => new HttpServerService());

        public static HttpServerService Instance => _instance.Value;

        private MultithreadEventLoopGroup _bossGroup;
        private MultithreadEventLoopGroup _workerGroup;
        private IChannel _boundChannel;

        private HttpServerService() { }

        public async Task StartAsync(int port)
        {
            if (_boundChannel != null)
            {
                LogHelper.Info($"Server already running at {port}");
                return;
            }

            _bossGroup = new MultithreadEventLoopGroup(1);
            _workerGroup = new MultithreadEventLoopGroup();

            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap.Group(_bossGroup, _workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        var pipeline = channel.Pipeline;
                        pipeline.AddLast(new HttpServerCodec());
                        pipeline.AddLast(new HttpObjectAggregator(6553600)); // 聚合成 FullHttpRequest
                        pipeline.AddLast(new HttpServerHandler());
                    }));

                
                _boundChannel = await bootstrap.BindAsync(port);

                LogHelper.Info($"DotNetty HTTP server started at http://0.0.0.0:{port}");

            }
            catch (Exception ex)
            {
                LogHelper.Error("服务器启动失败: " + ex.Message);
            }
        }

        public async Task StopAsync()
        {
            try
            {
                if (_bossGroup != null) await _bossGroup.ShutdownGracefullyAsync();
                if (_workerGroup != null) await _workerGroup.ShutdownGracefullyAsync();

                LogHelper.Info("服务器已停止");
            }
            catch (Exception ex)
            {
                LogHelper.Error("服务器停止错误: " + ex.Message);
            }
        }
    }
}

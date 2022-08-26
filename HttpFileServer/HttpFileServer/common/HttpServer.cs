using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static System.Net.WebRequestMethods;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net.Http;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.Metadata;

namespace HttpFileServer.common
{
    /// <summary>
    /// 对TCP进行封装，构造HTTP服务
    /// </summary>
    public class HttpServer
    {
        /// <summary>
        /// TCP是否启动
        /// </summary>
        public bool isRunning { get; private set; }

        /// <summary>
        /// 服务端Socet
        /// </summary>
        private TcpListener? serverListen;

        /// <summary>
        /// 主窗体 TODO: 这个属性或者叫做字段不应该放在这个类中，放在公共类或者工具类中比较合适
        /// </summary>
        public static Form1? mainForm;

        /// <summary>
        /// 请求方式
        /// </summary>
        public string requestMethod { get; private set; }

        public string statusCode { get; private set; }

        /// <summary>
        /// TCP服务启动
        /// </summary>
        public void Start()
        {
            if (isRunning) return;

            // TcpListener 是对TCP的一种封装，创建TCP协议的Socket
            serverListen = new TcpListener(IPAddress.Any, 80);
            // 开始监听
            serverListen.Start();

            mainForm.ShowMsg("开始监听");

            // 委托
            //Action action = new Action();

            isRunning = true;
            try
            {
                // 开启新线程接收客户端连接，不开启新线程，主窗体无法加载，被卡死在等待连接这了
                Thread awaitThread = new Thread(() => { AwaitClientConnect(); });
                awaitThread.IsBackground = true;
                awaitThread.Start();
            }
            catch
            { }

        }

        /// <summary>
        /// TCP服务关闭
        /// </summary>
        public void Stop()
        {
            if (!isRunning) return;

            isRunning = false;
            if (serverListen != null)
                serverListen.Stop();
        }

        /// <summary>
        /// 循环等待客户端连接
        /// </summary>
        private void AwaitClientConnect()
        {
            try
            {
                while (isRunning && serverListen != null)
                {
                    TcpClient client = serverListen.AcceptTcpClient();
                    mainForm.ShowMsg(client.Client.RemoteEndPoint + ":" + "连接成功");

                    // 处理客户端请求
                    ProcessRequest(client);

                    // 新建立一个子线程处理客户端请求?  如果是页面请求，应该不用建立新线程来处理
                    // TODO: 处理客户端的请求线程，用线程池来处理？
                    //Thread requestThread = new Thread(() => { ProcessRequest(client.GetStream()); });
                    //requestThread.IsBackground = true;
                    //requestThread.Start();
                }
            }
            catch (SocketException ex)
            {
                /*
                    因为TcpListener.AcceptTcpClient()是阻塞式等待客户端连接，
                    直到接到一个TcpClient或者出错才会进行下一步，
                    此时我们调用TcpListener.Stop()必然会出错
                 */
                Debug.WriteLine("可以忽略的异常: " + ex.Message);
            }
        }

        /// <summary>
        /// 处理客户端的请求
        /// </summary>
        private void ProcessRequest(TcpClient client)
        {

#if false
            // 浏览器地址栏输入127.0.0.1后 client.GetStream()获得的字符串
            /**
                GET / HTTP/1.1
                Host: 127.0.0.1
                Connection: keep-alive
                Cache-Control: max-age=0
                Upgrade-Insecure-Requests: 1
                User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.0.0 Safari/537.36
                Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*; q = 0.8,application / signed - exchange; v = b3; q = 0.9
                Accept - Encoding: gzip, deflate
                Accept - Language: zh - CN,zh; q = 0.9
             **/
#endif

            // 该方法 GetStream 返回一个 NetworkStream 可用于发送和接收数据的方法。
            // 该 NetworkStream 类继承自 Stream 类，该类提供丰富的方法和属性集合，用于促进网络通信。
            NetworkStream networkStream = client.GetStream();
            byte[] receiveBuffer = new byte[1024 * 1024 * 2];
            int bytesLength = networkStream.Read(receiveBuffer);
            string data = Encoding.UTF8.GetString(receiveBuffer, 0, bytesLength);
            Debug.WriteLine("Get 请求连接=========： " + data);

            // 按行处理
            String[] rows = Regex.Split(data, Environment.NewLine);

            // 按空格分割 去掉头尾空格后按空格分割，
            // Where函数  相当于sql中的where条件？很有可能，前置条件之类的
            String[] firstRow = Regex.Split(rows[0], @"(\s+)")
                .Where(e => e.Trim() != string.Empty).ToArray();

            // 透过分解字符串，可以获取访问方式，地址URL以及HTTP的version
            //Debug.WriteLine("Get Row=========： " + firstRow[0]);
            //Debug.WriteLine("Get Row=========： " + firstRow[1]);
            //Debug.WriteLine("Get Row=========： " + firstRow[2]);

            requestMethod = firstRow[0];

            OnGet(networkStream);

            // 如果GET或者POST有参数，再分解URL


        }

        private void OnGet( NetworkStream networkStream)
        {
            try
            {
                // 构建响应头并且写回客户端
                String header = BuildHeader();
                byte[] headerBytes = Encoding.UTF8.GetBytes(header);
                networkStream.Write(headerBytes, 0, headerBytes.Length);

                // 发送空行
                byte[] lineBytes = Encoding.UTF8.GetBytes(System.Environment.NewLine);
                networkStream.Write(lineBytes, 0, lineBytes.Length);

                byte[] responseContent = Encoding.UTF8.GetBytes("<html><body><h1>Hello World!</h1></body></html>");

                // 发送内容
                networkStream.Write(responseContent, 0, responseContent.Length);

                // 刷新流中的数据并关闭连接
                networkStream.Flush();
                networkStream.Close();
            }
            catch
            { }

        }

        protected string BuildHeader()
        {
            StringBuilder builder = new StringBuilder();

            statusCode = "200";

            if (!string.IsNullOrEmpty(statusCode))
                builder.Append("HTTP/1.1 " + statusCode + "\r\n");

            String contentType = "text/html; charset=utf-8";

            if (!string.IsNullOrEmpty(contentType))
                builder.AppendLine("Content-Type:" + contentType);
            return builder.ToString();
        }



        private void HttpRequest(Stream clientStream)
        {

        }

        private void GetRequestData(Stream clientStream)
        {

        }
    }
}

using HttpFileServer.common;
using System.Net;
using System.Net.Sockets;
using static System.Net.WebRequestMethods;

namespace HttpFileServer
{


    public partial class Form1 : Form
    {
        private HttpServer? httpServer;

        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            // 本窗体
            HttpServer.mainForm = this;

            // 主窗体启动时，启动服务
            httpServer = new HttpServer();
            httpServer.Start();
        }


        public void ShowMsg(String msg)
        {
            textBox1.AppendText(msg + "\r\n");
        }
    }
}
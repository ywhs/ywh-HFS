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
            // ������
            HttpServer.mainForm = this;

            // ����������ʱ����������
            httpServer = new HttpServer();
            httpServer.Start();
        }


        public void ShowMsg(String msg)
        {
            textBox1.AppendText(msg + "\r\n");
        }
    }
}
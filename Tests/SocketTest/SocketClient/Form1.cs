using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketClient
{
    public partial class Form1 : Form
    {

        private Socket? socketSend;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 进行连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt1_Click(object sender, EventArgs e)
        {
            try
            {
                // 创建发送的socket  stream 是TCP  Dgram是UDP
                socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // 获取服务器IP
                IPAddress ip = IPAddress.Parse(textBox2.Text);

                // 绑定IP和端口号
                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(textBox3.Text));

                // 进行连接
                socketSend.Connect(point);

                ShowMsg("连接成功");

                // 开始接收服务端的消息
                Thread thread = new Thread(Receive);
                thread.IsBackground = true;
                thread.Start();
            }
            catch { }
        }


        /// <summary>
        /// 接收消息
        /// </summary>
        private void Receive()
        {

            while (true)
            {
                try
                {
                    if (socketSend != null)
                    {
                        // 接收消息
                        byte[] buffer = new byte[1024 * 1024 * 5];
                        int length = socketSend.Receive(buffer);
                        if (length == 0)
                        {
                            break;
                        }

                        if (buffer[0] == 0)
                        {
                            // 接收的字节转换为字符串
                            String receiveTxt = Encoding.UTF8.GetString(buffer, 1, length - 1);
                            ShowMsg(socketSend.RemoteEndPoint + ":" + receiveTxt);
                        }
                        else if (buffer[0] == 1)
                        {
                            SaveFileDialog saveFileDialog = new SaveFileDialog();
                            saveFileDialog.InitialDirectory = @"C:\Users\14594\Desktop";
                            saveFileDialog.Title = "请选择要保存的路径";
                            saveFileDialog.Filter = "所有文件|*.*";
                            saveFileDialog.ShowDialog(this);

                            String filePath = saveFileDialog.FileName;
                            // 开始写入文件
                            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                fileStream.Write(buffer, 1, length - 1);
                            }
                            MessageBox.Show("保存成功");
                        }
                        
                    }
                }
                catch
                { }

            }
        
        }


        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取客户端发送的数据
                string str = textBox4.Text.Trim();
                // 把字符串转换为字节
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(str);
                if (socketSend != null)
                {
                    socketSend.Send(buffer);
                }
                
            }
            catch
            {
                MessageBox.Show("发送失败");
            }

        }


        private void ShowMsg(string str)
        {
            textBox1.AppendText(str + "\r\n");
        }
    }
}
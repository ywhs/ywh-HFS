using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketTest1
{
    public partial class Form1 : Form
    {
        // 接收连接的客户端socket对象
        private Socket? socketClientSend;

        // 存储客户端socket对象
        private Dictionary<string, Socket> sockets = new Dictionary<string, Socket>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // 创建Socket
                Socket socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // 获取本机IP地址
                IPAddress ip = IPAddress.Any;

                // 把IP地址和端口进行绑定
                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(textBox3.Text));

                // Socket和IP、端口进行绑定
                socketWatch.Bind(point);

                ShowMsg("监听成功");

                // 设置监听数
                socketWatch.Listen(10);

                // 创建线程对象
                Thread thread = new Thread(Listen);
                // 线程后台运行
                thread.IsBackground = true;
                // 线程启动，线程执行的方法的参数，需要再Start函数中传入，并且参数需要为Object类型
                thread.Start(socketWatch);
            }
            catch { }
            

        }


        /// <summary>
        /// 循环等待客户端连接
        /// </summary>
        /// <param name="o">IP和端口绑定后的Socket类型对象</param>
        private void Listen(Object? o)
        {
            Socket? socketWatch = o as Socket;

            while (true)
            {
                try{
                    if (socketWatch != null)
                    {
                        // 等待客户端连接并且返回一个负责通信的socket
                        socketClientSend = socketWatch.Accept();

                        if (socketClientSend.RemoteEndPoint != null)
                        {
                            // 把客户端socket进行存储 key为客户端ip+端口，value为socket
                            sockets.Add(socketClientSend.RemoteEndPoint.ToString(), socketClientSend);
                            // 添加客户端ip+端口到下拉框中
                            comboBox1.Items.Add(socketClientSend.RemoteEndPoint.ToString());
                        }

                        ShowMsg(socketClientSend.RemoteEndPoint + " :连接成功");

                        // 新建立一个子线程进行接收客户端数据
                        Thread thread = new Thread(Receive);
                        thread.IsBackground = true;
                        thread.Start(socketClientSend);
                    }
                    
                }
                catch {
                }
            }
            
        }

        /// <summary>
        /// 循环接收客户端数据
        /// </summary>
        /// <param name="o">与客户端已经建立连接的Socket对象</param>
        private void Receive(Object? o)
        {
            // 在接收数据时，每一个线程中，socket都是独立的
            Socket? socketSend = o as Socket;

            while (true) 
            {
                try
                {
                    // 准备缓冲数组
                    byte[] buffer = new byte[1024 * 1024 * 2];

                    // 接收客户端数据
                    if (socketSend != null)
                    {
                        int len = socketSend.Receive(buffer);

                        // 判断如果发送的数据为0，则退出
                        if (len == 0)
                        {
                            break;
                        }

                        String s = Encoding.UTF8.GetString(buffer, 0, len);

                        ShowMsg(socketSend.RemoteEndPoint + ":" + s);
                    }
                    
                }
                catch {
                }
            }
        }


        private void ShowMsg(String txt)
        {
            // 追加文本信息
            textBox1.AppendText(txt + "\r\n");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 在程序加载时，取消跨线程的检查
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        /// <summary>
        /// 服务端发送消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            string txt = textBox4.Text.Trim ();

            // 规定客户端判断第一个字节是发送的文件还是消息   0 发送的消息， 1 发送的文件
            List<byte> list = new List<byte>();

            list.Add(0);
            list.AddRange(System.Text.Encoding.UTF8.GetBytes(txt));
            byte[] buffer = list.ToArray();

            // 根据选择的客户端进行发送消息
            if (comboBox1.Items.Count != 0)
            {
                // 默认没有选择的话，与连接的第一个客户端通信
                comboBox1.SelectedIndex = comboBox1.SelectedIndex == -1 ? 0 : comboBox1.SelectedIndex;

                String? ip = comboBox1.SelectedItem == null ? null : comboBox1.SelectedItem.ToString();

                if (ip != null)
                {
                    sockets[ip].Send(buffer);
                }
                else 
                {
                    MessageBox.Show("发送数据失败");
                }
            }
            else
            {
                MessageBox.Show("发送数据失败");
            }
            
        }

        /// <summary>
        /// 选择要发送的文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // 设置初始目录
            openFileDialog.InitialDirectory = @"C:\Users\14594\Desktop";
            openFileDialog.Title = "请选择要发送的文件";
            openFileDialog.Filter = "所有文件|*.*";
            openFileDialog.ShowDialog();

            // 把选择的文件赋值到文本框中
            textBox5.Text = openFileDialog.FileName;
        }


        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            // 获取文件路径
            String filePath = textBox5.Text;

            // using 相当于 try-finally，最终会自动执行file资源的关闭
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // 开始读取文件内容
                byte[] fileBuffer = new byte[1024 * 1024 * 5];

                // TODO: 此处应该循环读取并且发送，现在只能发送小于5M的文件
                int fileLength = file.Read(fileBuffer, 0, fileBuffer.Length);

                // 定义协议头，0 发送消息  1 发送文件
                List<byte> list = new List<byte>();
                list.Add(1);
                list.AddRange(fileBuffer);

                byte[] newFileBuffer = list.ToArray();

                // 开始发送数据
                // 根据选择的客户端进行发送消息
                if (comboBox1.Items.Count != 0)
                {
                    // 默认没有选择的话，与连接的第一个客户端通信
                    comboBox1.SelectedIndex = comboBox1.SelectedIndex == -1 ? 0 : comboBox1.SelectedIndex;

                    String? ip = comboBox1.SelectedItem == null ? null : comboBox1.SelectedItem.ToString();

                    if (ip != null)
                    {
                        // 发送的数据+1 是因为在第一个字节加了一个标志位
                        sockets[ip].Send(newFileBuffer, 0, fileLength + 1, SocketFlags.None);
                    }
                    else
                    {
                        MessageBox.Show("发送数据失败");
                    }
                }
                else
                {
                    MessageBox.Show("发送数据失败");
                }

            }

        }
    }
}
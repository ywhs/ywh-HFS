using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketTest1
{
    public partial class Form1 : Form
    {
        // �������ӵĿͻ���socket����
        private Socket? socketClientSend;

        // �洢�ͻ���socket����
        private Dictionary<string, Socket> sockets = new Dictionary<string, Socket>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // ����Socket
                Socket socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // ��ȡ����IP��ַ
                IPAddress ip = IPAddress.Any;

                // ��IP��ַ�Ͷ˿ڽ��а�
                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(textBox3.Text));

                // Socket��IP���˿ڽ��а�
                socketWatch.Bind(point);

                ShowMsg("�����ɹ�");

                // ���ü�����
                socketWatch.Listen(10);

                // �����̶߳���
                Thread thread = new Thread(Listen);
                // �̺߳�̨����
                thread.IsBackground = true;
                // �߳��������߳�ִ�еķ����Ĳ�������Ҫ��Start�����д��룬���Ҳ�����ҪΪObject����
                thread.Start(socketWatch);
            }
            catch { }
            

        }


        /// <summary>
        /// ѭ���ȴ��ͻ�������
        /// </summary>
        /// <param name="o">IP�Ͷ˿ڰ󶨺��Socket���Ͷ���</param>
        private void Listen(Object? o)
        {
            Socket? socketWatch = o as Socket;

            while (true)
            {
                try{
                    if (socketWatch != null)
                    {
                        // �ȴ��ͻ������Ӳ��ҷ���һ������ͨ�ŵ�socket
                        socketClientSend = socketWatch.Accept();

                        if (socketClientSend.RemoteEndPoint != null)
                        {
                            // �ѿͻ���socket���д洢 keyΪ�ͻ���ip+�˿ڣ�valueΪsocket
                            sockets.Add(socketClientSend.RemoteEndPoint.ToString(), socketClientSend);
                            // ��ӿͻ���ip+�˿ڵ���������
                            comboBox1.Items.Add(socketClientSend.RemoteEndPoint.ToString());
                        }

                        ShowMsg(socketClientSend.RemoteEndPoint + " :���ӳɹ�");

                        // �½���һ�����߳̽��н��տͻ�������
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
        /// ѭ�����տͻ�������
        /// </summary>
        /// <param name="o">��ͻ����Ѿ��������ӵ�Socket����</param>
        private void Receive(Object? o)
        {
            // �ڽ�������ʱ��ÿһ���߳��У�socket���Ƕ�����
            Socket? socketSend = o as Socket;

            while (true) 
            {
                try
                {
                    // ׼����������
                    byte[] buffer = new byte[1024 * 1024 * 2];

                    // ���տͻ�������
                    if (socketSend != null)
                    {
                        int len = socketSend.Receive(buffer);

                        // �ж�������͵�����Ϊ0�����˳�
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
            // ׷���ı���Ϣ
            textBox1.AppendText(txt + "\r\n");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // �ڳ������ʱ��ȡ�����̵߳ļ��
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        /// <summary>
        /// ����˷�����Ϣ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            string txt = textBox4.Text.Trim ();

            // �涨�ͻ����жϵ�һ���ֽ��Ƿ��͵��ļ�������Ϣ   0 ���͵���Ϣ�� 1 ���͵��ļ�
            List<byte> list = new List<byte>();

            list.Add(0);
            list.AddRange(System.Text.Encoding.UTF8.GetBytes(txt));
            byte[] buffer = list.ToArray();

            // ����ѡ��Ŀͻ��˽��з�����Ϣ
            if (comboBox1.Items.Count != 0)
            {
                // Ĭ��û��ѡ��Ļ��������ӵĵ�һ���ͻ���ͨ��
                comboBox1.SelectedIndex = comboBox1.SelectedIndex == -1 ? 0 : comboBox1.SelectedIndex;

                String? ip = comboBox1.SelectedItem == null ? null : comboBox1.SelectedItem.ToString();

                if (ip != null)
                {
                    sockets[ip].Send(buffer);
                }
                else 
                {
                    MessageBox.Show("��������ʧ��");
                }
            }
            else
            {
                MessageBox.Show("��������ʧ��");
            }
            
        }

        /// <summary>
        /// ѡ��Ҫ���͵��ļ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // ���ó�ʼĿ¼
            openFileDialog.InitialDirectory = @"C:\Users\14594\Desktop";
            openFileDialog.Title = "��ѡ��Ҫ���͵��ļ�";
            openFileDialog.Filter = "�����ļ�|*.*";
            openFileDialog.ShowDialog();

            // ��ѡ����ļ���ֵ���ı�����
            textBox5.Text = openFileDialog.FileName;
        }


        /// <summary>
        /// �����ļ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            // ��ȡ�ļ�·��
            String filePath = textBox5.Text;

            // using �൱�� try-finally�����ջ��Զ�ִ��file��Դ�Ĺر�
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // ��ʼ��ȡ�ļ�����
                byte[] fileBuffer = new byte[1024 * 1024 * 5];

                // TODO: �˴�Ӧ��ѭ����ȡ���ҷ��ͣ�����ֻ�ܷ���С��5M���ļ�
                int fileLength = file.Read(fileBuffer, 0, fileBuffer.Length);

                // ����Э��ͷ��0 ������Ϣ  1 �����ļ�
                List<byte> list = new List<byte>();
                list.Add(1);
                list.AddRange(fileBuffer);

                byte[] newFileBuffer = list.ToArray();

                // ��ʼ��������
                // ����ѡ��Ŀͻ��˽��з�����Ϣ
                if (comboBox1.Items.Count != 0)
                {
                    // Ĭ��û��ѡ��Ļ��������ӵĵ�һ���ͻ���ͨ��
                    comboBox1.SelectedIndex = comboBox1.SelectedIndex == -1 ? 0 : comboBox1.SelectedIndex;

                    String? ip = comboBox1.SelectedItem == null ? null : comboBox1.SelectedItem.ToString();

                    if (ip != null)
                    {
                        // ���͵�����+1 ����Ϊ�ڵ�һ���ֽڼ���һ����־λ
                        sockets[ip].Send(newFileBuffer, 0, fileLength + 1, SocketFlags.None);
                    }
                    else
                    {
                        MessageBox.Show("��������ʧ��");
                    }
                }
                else
                {
                    MessageBox.Show("��������ʧ��");
                }

            }

        }
    }
}
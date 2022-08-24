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
        /// ��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt1_Click(object sender, EventArgs e)
        {
            try
            {
                // �������͵�socket  stream ��TCP  Dgram��UDP
                socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // ��ȡ������IP
                IPAddress ip = IPAddress.Parse(textBox2.Text);

                // ��IP�Ͷ˿ں�
                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(textBox3.Text));

                // ��������
                socketSend.Connect(point);

                ShowMsg("���ӳɹ�");

                // ��ʼ���շ���˵���Ϣ
                Thread thread = new Thread(Receive);
                thread.IsBackground = true;
                thread.Start();
            }
            catch { }
        }


        /// <summary>
        /// ������Ϣ
        /// </summary>
        private void Receive()
        {

            while (true)
            {
                try
                {
                    if (socketSend != null)
                    {
                        // ������Ϣ
                        byte[] buffer = new byte[1024 * 1024 * 5];
                        int length = socketSend.Receive(buffer);
                        if (length == 0)
                        {
                            break;
                        }

                        if (buffer[0] == 0)
                        {
                            // ���յ��ֽ�ת��Ϊ�ַ���
                            String receiveTxt = Encoding.UTF8.GetString(buffer, 1, length - 1);
                            ShowMsg(socketSend.RemoteEndPoint + ":" + receiveTxt);
                        }
                        else if (buffer[0] == 1)
                        {
                            SaveFileDialog saveFileDialog = new SaveFileDialog();
                            saveFileDialog.InitialDirectory = @"C:\Users\14594\Desktop";
                            saveFileDialog.Title = "��ѡ��Ҫ�����·��";
                            saveFileDialog.Filter = "�����ļ�|*.*";
                            saveFileDialog.ShowDialog(this);

                            String filePath = saveFileDialog.FileName;
                            // ��ʼд���ļ�
                            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                fileStream.Write(buffer, 1, length - 1);
                            }
                            MessageBox.Show("����ɹ�");
                        }
                        
                    }
                }
                catch
                { }

            }
        
        }


        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // ��ȡ�ͻ��˷��͵�����
                string str = textBox4.Text.Trim();
                // ���ַ���ת��Ϊ�ֽ�
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(str);
                if (socketSend != null)
                {
                    socketSend.Send(buffer);
                }
                
            }
            catch
            {
                MessageBox.Show("����ʧ��");
            }

        }


        private void ShowMsg(string str)
        {
            textBox1.AppendText(str + "\r\n");
        }
    }
}
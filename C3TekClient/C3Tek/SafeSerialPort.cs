using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSafeSerialPort.Serial;
namespace C3TekClient.C3Tek
{
    public class SafeSerialPort : SerialPort
    {
        //public int ReadBufferSize { get; set; }
        public bool ENABLE_LOOP_BACK_SERIAL = false;
        public bool ENABLE_WRITE_LOGGER = false;
        public bool ENABLE_WRITE_BOTH_LOGGER = false;

        byte[] buffer_writer_logger;
        string file_writer_logger = "writter.log";
        UInt64 current_pos = 0;
        public SafeSerialPort()
        {
            this.AutoReconnect = true;
            SerialPort.UnlockKey = "BC3450FEA2344897EFC34325BA3910X2";
        }
        public bool IsOpen
        {
            get
            {
                return this.IsConnected();
            }
        }
        public void Write(byte b)
        {
            this.SendByte(b);
            if (ENABLE_WRITE_LOGGER)
            {

                buffer_writer_logger[current_pos++] = b;

            }
        }
        public void StartLoggerWrite(string fn, bool accept_read)
        {
            buffer_writer_logger = new byte[1024 * 1024 * 3];
            ENABLE_WRITE_LOGGER = true;
            this.file_writer_logger = fn;
            current_pos = 0;
            ENABLE_WRITE_BOTH_LOGGER = accept_read;
        }
        public void EndLoggerWrite()
        {
            File.WriteAllBytes(this.file_writer_logger, buffer_writer_logger);
            ENABLE_WRITE_LOGGER = false;
            buffer_writer_logger = null;
            ENABLE_WRITE_BOTH_LOGGER = false; 

        }
      
        public int Read(byte[] buffer, int start, int len)
        {
            buffer = this.ReadBytes(len);

            if (ENABLE_WRITE_BOTH_LOGGER)
            {
                foreach (byte b in buffer)
                {
                    buffer_writer_logger[current_pos++] = b;
                }
            }

            return buffer.Length;
        }
        public void WriteLine(string text)
        {
            text = text + Environment.NewLine;
            this.SendAsciiString(text);
            if (ENABLE_WRITE_LOGGER)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(text);
                foreach (byte b in bytes)
                {
                    buffer_writer_logger[current_pos++] = b;
                }
            }
        }
        public void Write(string text)
        {
            this.SendAsciiString(text);
            if (ENABLE_WRITE_LOGGER)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(text);
                foreach (byte b in bytes)
                {
                    buffer_writer_logger[current_pos++] = b;
                }
            }
        }
        public string ReadExisting()
        {
            string res = "";
            try
            {
                res = this.ReadBufferToString();
                if (ENABLE_WRITE_BOTH_LOGGER)
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(res);
                    foreach (byte b in bytes)
                    {
                        buffer_writer_logger[current_pos++] = b;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SafeSerialPort] ReadExisting {ex.Message}");
            }
            finally
            {

            }
            return res;
        }


    }
}

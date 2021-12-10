using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;

namespace CustomHttp
{
    public class CustomHttpClient : IDisposable
    {
        public CustomHttpClient(string cIpAddress, int cPort)
        {
            ipAddress = cIpAddress;
            port = cPort;

            headers = new Dictionary<string, string>();

            host = ipAddress + ":" + port.ToString();
            userAgent = CustomHttpDefine.UNITY_PLAYER;
            accept = CustomHttpDefine.ACCEPT_ACC;
            acceptEncoding = CustomHttpDefine.ACCEPT_ENCODING_IDENTITY;
            contentType = CustomHttpDefine.CONTENT_TYPE_OCTETSTREAM;
            contentLength = 0;

            HeaderEnd = Encoding.Default.GetBytes("\r\n\r\n");

            initHeader();
        }

        public void Dispose()
        {
            headers.Clear();
            headers = null;

            requestData = null;
            responseData = null;

            //GC.Collect();
        }

        public void request(string reqUrl, RequestMethod reqMethod, byte[] reqBody = null)
        {
            isDone = false;

            bool combineData = false;

            url = reqUrl;

            method = reqMethod;

            int bodySize = 0;

            if (reqBody != null)
            {
                method = RequestMethod.POST;

                bodySize = reqBody.Length;

                combineData = true;
            }

            contentLength = bodySize;

            byte[] headerBytes = Encoding.Default.GetBytes(generateHeader());

            if (combineData)
            {
                requestData = combineBytes(ref headerBytes, ref reqBody);
                headerBytes = null;
                reqBody = null;
            }
            else
            {
                requestData = headerBytes;
                headerBytes = null;
            }

            Thread t = new Thread(request);
            t.Start();
        }

        public void connect()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Connect(ipAddress, port);
            }
            catch (System.Exception ex)
            {
                socket = null;
                Debug.Log("connect fail " + ex.ToString());
            }
        }

        public void recv()
        {
            if (socket == null)
            {
                return;
            }

            try
            {
                int bufferSize = 4096;

                int totalSize = 0;
                int totalRecvSize = -1;

                bool isHeader = false;

                byte[] headerData = null;
                List<byte> bodyData = new List<byte>();

                byte[] recvBuffer = new byte[bufferSize];

                while (totalRecvSize < totalSize)
                {
                    int recvSize = socket.Receive(recvBuffer);

                    totalRecvSize += recvSize;

                    if (!isHeader)
                    {
                        int searchVal = search(recvBuffer, HeaderEnd, true);

                        if (searchVal != -1)
                        {
                            addBytes(ref headerData, ref recvBuffer, 0, searchVal);
                            isHeader = true;

                            string[] parseData = Encoding.UTF8.GetString(headerData).Split(new char[] { '\r', '\n' });

                            for (int i = 0; i < parseData.Length; i++)
                            {
                                if (parseData[i].Contains(CustomHttpDefine.HEADER_CONTENT_LENGTH))
                                {
                                    string[] conLen = parseData[i].Split(new char[] { ':', ' ' });
                                    totalSize = int.Parse(conLen[2]);
                                    totalRecvSize = 0;
                                }
                            }

                            headerData = null;

                            addBytes(ref recvBuffer, searchVal);
                            totalRecvSize = bufferSize - searchVal;
                        }
                        else
                        {
                            addBytes(ref headerData, ref recvBuffer);
                        }
                    }
                    else
                    {
                        addBytes(ref recvBuffer, 0, recvSize);
                    }
                }
            }
            catch (System.Exception ex)
            {
                socket = null;
                Debug.Log("receive fail " + ex.ToString());
            }
        }

        public void send()
        {
            if (socket == null)
            {
                return;
            }

            try
            {
                int sendTotalSize = requestData.Length;
                int sendSize = 0;

                while (sendSize < sendTotalSize)
                {
                    sendSize += socket.Send(requestData, sendSize, sendTotalSize - sendSize, SocketFlags.None);
                }
            }
            catch (System.Exception ex)
            {
                socket = null;
                Debug.Log("sending fail " + ex.ToString());
            }
        }

        public void disconnect()
        {
            if (socket == null)
            {
                return;
            }

            try
            {
                socket.Disconnect(false);
                socket.Close();
                socket = null;
            }
            catch (System.Exception ex)
            {
                socket = null;
                Debug.Log("disconnect fail " + ex.ToString());
            }
        }

        private void initHeader()
        {
            headers.Add(CustomHttpDefine.HEADER_COMMANDLINE, method.ToString() + " " + url + " " + CustomHttpDefine.HEADER_VERSION11);
            headers.Add(CustomHttpDefine.HEADER_HOST, host);
            headers.Add(CustomHttpDefine.HEADER_USER_AGENT, userAgent);
            headers.Add(CustomHttpDefine.HEADER_ACCEPT, accept);
            headers.Add(CustomHttpDefine.HEADER_ACCEPT_ENCODING, acceptEncoding);
            headers.Add(CustomHttpDefine.HEADER_CONTENT_TYPE, contentType);
            headers.Add(CustomHttpDefine.HEADER_CONTENT_LENGTH, contentLength.ToString());
        }

        private void setHeader()
        {
            if (!url.StartsWith("/"))
            {
                url = "/" + url;
            }

            headers[CustomHttpDefine.HEADER_COMMANDLINE] = method.ToString() + " " + url + " " + CustomHttpDefine.HEADER_VERSION11;
            headers[CustomHttpDefine.HEADER_HOST] = host;
            headers[CustomHttpDefine.HEADER_USER_AGENT] = userAgent;
            headers[CustomHttpDefine.HEADER_ACCEPT] = accept;
            headers[CustomHttpDefine.HEADER_ACCEPT_ENCODING] = acceptEncoding;
            headers[CustomHttpDefine.HEADER_CONTENT_TYPE] = contentType;
            headers[CustomHttpDefine.HEADER_CONTENT_LENGTH] = contentLength.ToString();
        }

        private void addHeader(string key, string value)
        {
            if (headers.ContainsKey(key))
            {
                headers[key] = value;
            }
            else
            {
                headers.Add(key, value);
            }
        }

        private void request()
        {
            connect();
            send();
            recv();
            disconnect();

            isDone = true;
        }

        private int search(byte[] src, byte[] pattern, bool endIndex = false)
        {
            int maxFirstCharSlot = src.Length - pattern.Length + 1;
            for (int i = 0; i < maxFirstCharSlot; i++)
            {
                if (src[i] != pattern[0])
                    continue;

                for (int j = pattern.Length - 1; j >= 1; j--)
                {
                    if (src[i + j] != pattern[j]) break;
                    if (j == 1) return i + (endIndex ? pattern.Length : 0);
                }
            }
            return -1;
        }

        private void addBytes(ref byte[] addData, int startIndex = 0, int length = -1)
        {
            if (length == -1)
            {
                length = addData.Length - startIndex;
            }

            int baseIndex = 0;

            if (responseData == null)
            {
                responseData = new byte[length];
            }
            else
            {
                baseIndex = responseData.Length;
                Array.Resize(ref responseData, baseIndex + length);
            }

            Array.Copy(addData, startIndex, responseData, baseIndex, length);
        }

        private void addBytes(ref byte[] leftData, ref byte[] addData, int startIndex = 0, int length = -1)
        {
            if (length == -1)
            {
                length = addData.Length - startIndex;
            }

            int baseIndex = 0;

            if (leftData == null)
            {
                leftData = new byte[length];
            }
            else
            {
                baseIndex = leftData.Length;
                Array.Resize(ref leftData, baseIndex + length);
            }

            Array.Copy(addData, startIndex, leftData, baseIndex, length);
        }

        private byte[] combineBytes(ref byte[] left, ref byte[] right)
        {
            int leftSize = left.Length;
            int rightSize = right.Length;
            byte[] returnBytes = new byte[leftSize + rightSize];

            Array.Copy(left, 0, returnBytes, 0, leftSize);
            Array.Copy(right, 0, returnBytes, leftSize, rightSize);

            return returnBytes;
        }

        private string generateHeader()
        {
            setHeader();

            string returnVal = "";

            Dictionary<string, string>.Enumerator headerEnum = headers.GetEnumerator();

            while (headerEnum.MoveNext())
            {
                string key = headerEnum.Current.Key;
                string value = headerEnum.Current.Value;

                if (key.Equals(CustomHttpDefine.HEADER_COMMANDLINE))
                {
                    returnVal += value;
                    returnVal += CustomHttpDefine.LINEEND;
                }
                else
                {
                    returnVal += key + ": " + value;
                    returnVal += CustomHttpDefine.LINEEND;
                }
            }

            returnVal += CustomHttpDefine.LINEEND;

            return returnVal;
        }

        public RequestMethod method = RequestMethod.GET;
        public string url = "/";

        string ipAddress = CustomHttpDefine.LOCALHOST;
        int port = 8080;

        public string host = "";
        public string userAgent = "";
        public string accept = "";
        public string acceptEncoding = "";
        public string contentType = "";
        public int contentLength = 0;

        public bool isDone = false;

        public byte[] requestData = null;
        public byte[] responseData = null;

        private byte[] HeaderEnd = null;

        private Socket socket;

        Dictionary<string, string> headers = null;
    }
}
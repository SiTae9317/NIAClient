namespace CustomHttp
{
    public enum RequestMethod
    {
        GET,
        POST,
        DELETE,
        UPDATE
    }

    public class CustomHttpDefine
    {
        public const string HEADER_COMMANDLINE = "CommandLine";

        #region Header Version
        public const string HEADER_VERSION10 = "HTTP/1.0";
        public const string HEADER_VERSION11 = "HTTP/1.1";
        public const string HEADER_VERSION20 = "HTTP/2.0";
        #endregion

        #region Header Key
        public const string HEADER_HOST = "Host";
        public const string HEADER_USER_AGENT = "User-Agent";
        public const string HEADER_ACCEPT = "Accept";
        public const string HEADER_ACCEPT_ENCODING = "Accept-Encoding";
        public const string HEADER_CONTENT_TYPE = "Content-Type";
        public const string HEADER_CONTENT_LENGTH = "Content-Length";
        public const string HEADER_CONNECTION = "Connection";
        #endregion

        #region User Agent
        public const string CUSTOM_CLIENT = "CustomClient";
        public const string UNITY_PLAYER = "UnityPlayer/2019.4.18f1 (UnityWebRequest/1.0, libcurl/7.52.0-DEV)";
        #endregion

        #region Accept
        public const string ACCEPT_ACC = "*/*";
        public const string ACCEPT_TEXT_HTML = "text/html";
        public const string ACCEPT_IMAGE = "image/*";
        #endregion

        #region Accept Encoding
        public const string ACCEPT_ENCODING_BR = "br";
        public const string ACCEPT_ENCODING_IDENTITY = "identity";
        public const string ACCEPT_ENCODING_GZIP = "gzip";
        public const string ACCEPT_ENCODING_COMPRESS = "compress";
        public const string ACCEPT_ENCODING_DEFLATE = "deflate";
        #endregion

        #region Connection
        public const string CONNECTION_KEEP_ALIVE = "keep-alive";
        public const string CONNECTION_CLOSE = "close";
        #endregion

        #region Content Type
        public const string CONTENT_TYPE_OCTETSTREAM = "application/octet-stream";
        public const string CONTENT_TYPE_TEXT_HTML = "text/html";
        public const string CONTENT_TYPE_MULTIPART = "multipart/form-data";
        #endregion

        #region Default Words
        public const string EQUAL = "=";
        public const string SEMICOLON = ";";
        public const string UTF8 = "utf-8";
        public const string LINEEND = "\r\n";

        public const string BOUNDARY = "boundary";
        public const string CHARSET = "charset";

        public const string LOCALHOST = "127.0.0.1";
        #endregion
    }
}
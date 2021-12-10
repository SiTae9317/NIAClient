using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using CustomHttp;

public class ExampleClient : MonoBehaviour
{
    #region Public Variable
    [Header("Server IP")]
    public string ipAddress = "127.0.0.1";

    [Header("Server Port")]
    public int port = 9999;

    [Header("Custom Handler")]
    public List<string> requestUrl;

    [Header("Input RenderTexture")]
    public RenderTexture inputCam;

    [Header("Output Image Object")]
    public GameObject displayObj;

    [Header("Output FPS")]
    public Text fpsObj;

    [Header("Output Request Status")]
    public Text requestStatusObj;
    #endregion

    #region Private Variable
    private float fps = 0.0f;

    private Texture2D outputTex;
    private Texture2D captureTex;
    private Color[] receiveColorData;
    private byte[] saveReceiveData;

    private const int imageSize = 160 * 120;
    //const int dataSize = imageSize * sizeof(Int64);
    private const int dataSize = imageSize * 4;

    private Rect defRect;

    private bool isRun = false;
    private bool isRequest = false;
    #endregion

    void Start()
    {
        outputTex = new Texture2D(160, 120);
        receiveColorData = new Color[imageSize];
        saveReceiveData = new byte[dataSize];

        defRect = new Rect(0, 0, inputCam.width, inputCam.height);

        captureTex = new Texture2D(inputCam.width, inputCam.height);

        displayObj.GetComponent<Image>().material.mainTexture = outputTex;

        isRun = true;

        StartCoroutine(gcCor());

        StartCoroutine(setBinaryData());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isRequest = !isRequest;
            if(isRequest)
            {
                StartCoroutine(socketCor());
            }
        }
        requestStatusObj.text = isRequest.ToString();
    }

    IEnumerator gcCor()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1.0f);
            GC.Collect();
        }
    }

    IEnumerator socketCor()
    {
        long saveTime = DateTime.Now.Ticks;

        var old_rt = RenderTexture.active;
        RenderTexture.active = inputCam;

        captureTex.ReadPixels(defRect, 0, 0);
        captureTex.Apply();

        RenderTexture.active = old_rt;

        byte[] capBytes = captureTex.EncodeToJPG();

        using (CustomHttpClient chc = new CustomHttpClient(ipAddress, port))
        {
            string reqUrl = "/";
            if (requestUrl.Count != 0)
            {
                reqUrl += requestUrl[0];

            }

            chc.request(reqUrl, RequestMethod.POST, capBytes);

            while (!chc.isDone)
            {
                yield return null;
            }

            try
            {
                GCHandle pinnedArray = GCHandle.Alloc(saveReceiveData, GCHandleType.Pinned);
                IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                Marshal.Copy(chc.responseData, 0, pointer, dataSize);
                pinnedArray.Free();
            }
            catch (System.Exception ex)
            {
                isRequest = false;
                Debug.Log("memcopy error " + ex.ToString());
            }
        }

        if (isRequest)
        {
            StartCoroutine(socketCor());
        }

        float avgTime = (float)(((double)(DateTime.Now.Ticks - saveTime)) / 10000000.0);
        fps = (1.0f / avgTime);
        fpsObj.text = string.Format("{0:f2}", fps);
    }

    //IEnumerator WWWCor()
    //{
    //    long saveTime = DateTime.Now.Ticks;

    //    var old_rt = RenderTexture.active;
    //    RenderTexture.active = inputCam;

    //    captureTex.ReadPixels(defRect, 0, 0);
    //    captureTex.Apply();

    //    RenderTexture.active = old_rt;

    //    byte[] capBytes = captureTex.EncodeToJPG();

    //    string baseUrl = "http://" + ipAddress + ":" + port.ToString() + "/";

    //    if (requestUrl.Count != 0)
    //    {
    //        baseUrl += requestUrl[0];

    //    }
        
    //    using(WWW www = new WWW(baseUrl, capBytes))

    //    while (!www.isDone)
    //    {
    //        yield return null;
    //    }

    //    Debug.Log(www.bytes.Length);

    //    GCHandle pinnedArray = GCHandle.Alloc(saveReceiveData, GCHandleType.Pinned);
    //    IntPtr pointer = pinnedArray.AddrOfPinnedObject();
    //    Marshal.Copy(www.bytes, 0, pointer, dataSize);
    //    pinnedArray.Free();

    //    Debug.Log("cor end");

    //    StartCoroutine(WWWCor());

    //    float avgTime = (float)(((double)(DateTime.Now.Ticks - saveTime)) / 10000000.0);

    //    Debug.Log(string.Format("{0:f2}", (1.0 / avgTime)) + " fps");
    //}

    //IEnumerator WebRequestCor()
    //{
    //    long saveTime = DateTime.Now.Ticks;

    //    var old_rt = RenderTexture.active;
    //    RenderTexture.active = inputCam;

    //    captureTex.ReadPixels(defRect, 0, 0);
    //    captureTex.Apply();

    //    RenderTexture.active = old_rt;

    //    string baseUrl = ipAddress + ":" + port.ToString() + "/";
    //    if (requestUrl.Count != 0)
    //    {
    //        baseUrl += requestUrl[0];

    //    }
    //    byte[] requestData = captureTex.EncodeToJPG();
    //    Debug.Log(requestData.Length);
    //    UploadHandlerRaw uploadHandler = new UploadHandlerRaw(requestData);

    //    UnityWebRequest request = UnityWebRequest.Get(baseUrl);
    //    request.method = "POST";
    //    request.uploadHandler = uploadHandler;

    //    yield return request.SendWebRequest();

    //    if (request.isNetworkError)
    //    {
    //        Debug.Log(request.error);
    //    }
    //    else
    //    {
    //        GCHandle pinnedArray = GCHandle.Alloc(saveReceiveData, GCHandleType.Pinned);
    //        IntPtr pointer = pinnedArray.AddrOfPinnedObject();
    //        Marshal.Copy(request.downloadHandler.data, 0, pointer, dataSize);
    //        pinnedArray.Free();
    //    }

    //    request.Dispose();
    //    request = null;
    //    uploadHandler.Dispose();
    //    uploadHandler = null;

    //    StartCoroutine(WebRequestCor());

    //    float avgTime = (float)(((double)(DateTime.Now.Ticks - saveTime)) / 10000000.0);

    //    Debug.Log(string.Format("{0:f2}", avgTime) + " fps");
    //}

    IEnumerator setBinaryData()
    {
        StartCoroutine(setTextureCor());

        MemoryStream ms = new MemoryStream(saveReceiveData);
        BinaryReader br = new BinaryReader(ms);

        Color curColor = Color.black;

        int width = 160;
        int height = 120;

        while (isRun)
        {
            int curIndex = 0;
            ms.Position = 0;

            int x = 0;
            int y = height - 1;

            while (ms.Position < ms.Length)
            {
                curIndex = y * width + x;
                Color keepColor = receiveColorData[curIndex];
                keepColor.r = keepColor.g = keepColor.b = ((float)br.ReadUInt16() / 65536.0f);
                receiveColorData[curIndex] = keepColor;
                br.ReadUInt16();

                x = (x + 1) % width;
                y = (x == 0 ? y - 1 : y);
            }

            yield return null;
        }

        br.Close();
        ms.Close();
    }

    IEnumerator setTextureCor()
    {
        while (isRun)
        {
            outputTex.SetPixels(receiveColorData);
            outputTex.Apply();

            yield return null;
        }
    }
}

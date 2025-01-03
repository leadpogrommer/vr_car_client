using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Godot;

namespace vr_test;

public class CameraFeedReceiver {
    private string name;
    private string host;
    private int port;
    private ImageTexture texture;
    private Thread thread;
    private volatile bool isRunning;

    public CameraFeedReceiver(string name, string host, int port, ImageTexture texture) {
        this.name = name;
        this.host = host;
        this.port = port;
        this.texture = texture;

        thread = new Thread(() => { receiveLoop(); });
    }

    public void Start() {
        isRunning = true;
        thread.Start();
    }

    public void Stop() {
        isRunning = false;
    }

    private void receiveLoop() {
        GD.Print($"{name} Receiver thread started");
        
        var server = new UdpClient(port);
        server.Client.ReceiveTimeout = 1000;

        var alist = Dns.GetHostAddresses(host);
        var ep = new IPEndPoint(alist[0], 55555);
        
        GD.Print($"{name} Connecting to {alist[0]}:{port}");
        server.Connect(ep);

        
        byte[] msg;
        List<byte> currentFrame = new List<byte>();

        var startMarker = new byte[] { 0xff, 0xd8, 0xff };
        var endMarker = new byte[] { 0xff, 0xd9 };

        while (isRunning) {
            try {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                msg = server.Receive(ref sender);
            }
            catch (SocketException se) {
                GD.Print($"{name} Failed to receive data from cam, sending trigger");
                server.Send(new byte[] { 0x55 });
                continue;
            }


            for (int i = 0; i < msg.Length; i++) {
                currentFrame.Add(msg[i]);
                var cnt = currentFrame.Count;
                if (cnt >= 5 && currentFrame[cnt - 1] == 0xd9 && currentFrame[cnt - 2] == 0xff) {
                    // TODO: this is horrible and ineficient
                    var ind = currentFrame.StartingIndex(startMarker.ToList());

                    if (ind < 0) {
                        GD.PrintErr($"{name} Invalid frame");
                        currentFrame.Clear();
                        continue;
                    }
                    // GD.Print("Got frame");

                    var img = new Image();
                    var err = img.LoadJpgFromBuffer(currentFrame.ToArray());
                    currentFrame.Clear();
                    if (err != 0) {
                        GD.PrintErr($"{name} Failed to load image:", err);
                        continue;
                    }

                    texture.CallDeferred("set_image", img);

                }
            }
        }
        
        GD.Print($"{name} Exited");
    }
}
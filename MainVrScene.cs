using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public static class ArrayUtils {
	public static bool IsSubArrayEqual(List<byte> x, List<byte> y, int start) {
		for (int i = 0; i < y.Count; i++) {
			if (x[start++] != y[i]) return false;
		}
		return true;
	}
	public static int StartingIndex(this List<byte> x, List<byte> y) {
		int max = 1 + x.Count - y.Count;
		for(int i = 0 ; i < max ; i++) {
			if(IsSubArrayEqual(x,y,i)) return i;
		}
		return -1;
	}
	// public static int RStartingIndex(this byte[] x, byte[] y) {
	// 	int max = x.Length - y.Length;
	// 	for(int i = max ; i >= 0 ; i--) {
	// 		if(IsSubArrayEqual(x,y,i)) return i;
	// 	}
	// 	return -1;
	// }
}


public partial class MainVrScene : Node3D
{
	private XRInterface _xrInterface;
	private System.Threading.Thread _leftTeceiverThread;
	private System.Threading.Thread _rightReceiverThread;
	private bool _isRunning;
	private ImageTexture _leftTexture;
	private ImageTexture _rightTexture;

	public override void _Ready() {
		_isRunning = true;
		
		_leftTexture = new ImageTexture();
		_rightTexture = new ImageTexture();
		
		var mat = this.GetNode<MeshInstance3D>("Screen").GetActiveMaterial(0) as ShaderMaterial;
		mat.SetShaderParameter("left_texture", _leftTexture);
		mat.SetShaderParameter("right_texture", _rightTexture);
		
		// 00 and 60
		_leftTeceiverThread = new Thread(() => {
			ReceiverThread("cam-00.local", _leftTexture, 55556);	
		});
		_leftTeceiverThread.Start();
		
		_rightReceiverThread = new Thread(() => {
			ReceiverThread("cam-60.local", _rightTexture, 55557);	
		});
		_rightReceiverThread.Start();
		
		_xrInterface = XRServer.FindInterface("OpenXR");
		if (_xrInterface != null && _xrInterface.IsInitialized()) {
			GD.Print("OpenXR initialized successfully");

			// Turn off v-sync!
			DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);

			// Change our main viewport to output to the HMD
			GetViewport().UseXR = true;
		}
		else {
			GD.Print("OpenXR not initialized, please check if your headset is connected");
		}
	}

	private void ReceiverThread(String address, ImageTexture texture, int port) {
		var server = new UdpClient(port);
		server.Client.ReceiveTimeout = 1000;

		var alist = Dns.GetHostAddresses(address);
		
		// var ep = new IPEndPoint(new IPAddress(new byte[] { 192, 168, 2, 105 }), 55555);
		var ep = new IPEndPoint(alist[0], 55555);
		server.Connect(ep);
		
		GD.Print("Receiver thread started");
		byte[] msg;
		List<byte> currentFrame = new List<byte>();
		// currentFrame.Res

		var startMarker = new byte[] { 0xff, 0xd8, 0xff };
		var endMarker = new byte[] { 0xff, 0xd9 };
		
		while (_isRunning) {
			try {
				IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
				msg = server.Receive(ref sender);
			}
			catch (SocketException se) {
				GD.Print("Failed to receive data from cam, sending trigger");
				server.Send(new byte[]{0x55});
				continue;
			}

			
			// int soi = msg.StartingIndex(startMarker);
			// int eoi = msg.RStartingIndex(endMarker);
			//
			// // TODO: do this better
			// if (soi >= 0) {
			// 	chunks = msg.Skip(soi).ToList();
			// }else if (eoi >= 0) {
			// 	chunks = chunks.Concat(msg.Take(eoi + 3)).ToList();
			// 	GD.Print("Got image");
			// }
			// else {
			// 	chunks = chunks.Concat(msg).ToList();
			// }

			for (int i = 0; i < msg.Length; i++) {
				currentFrame.Add(msg[i]);
				var cnt = currentFrame.Count;
				if (cnt >= 5 && currentFrame[cnt - 1] == 0xd9 && currentFrame[cnt - 2] == 0xff) {
					// TODO: this is horrible and ineficient
					var ind = currentFrame.StartingIndex(startMarker.ToList());
					
					if (ind < 0) {
						GD.PrintErr("Invalid frame");
						currentFrame.Clear();
						continue;
					}
					
					// currentFrame.RemoveRange(0, ind);
					
					GD.Print("Got frame");

					var img = new Image();
					var err = img.LoadJpgFromBuffer(currentFrame.ToArray());
					currentFrame.Clear();
					if (err != 0) {
						GD.PrintErr("Failed to load image:", err);
						continue;
					}


					CallDeferred("UpdateTexture", img, texture);
					
					// mat.SetShaderParameter();
					
					
					
				}
			}
			



		}
	}

	public void UpdateTexture(Image img, ImageTexture texture) {
		// var t = new ImageTexture();
		// TODO: update image, not set
		texture.SetImage(img);
		
		// mat.SetShaderParameter();
	}


	protected override void Dispose(bool disposing) {
		_isRunning = false;
		base.Dispose(disposing);
	}
}

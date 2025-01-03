using Godot;
using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using vr_test;



public partial class MainVrScene : Node3D {

	private Object _mlock;
	private XRInterface _xrInterface;
	private CameraFeedReceiver _leftReceiver;
	private CameraFeedReceiver _rightReceiver;
	private ImageTexture _leftTexture;
	private ImageTexture _rightTexture;

	// private string leftAddr = "cam-00.local";
	// private string rightAddr = "cam-60.local";
	
	private string leftAddr = "192.168.2.49";
	private string rightAddr = "192.168.2.105";

	public override void _Ready() {
		InitXr();
		
		_leftTexture = new ImageTexture();
		_rightTexture = new ImageTexture();
		
		var mat = this.GetNode<MeshInstance3D>("Screen").GetActiveMaterial(0) as ShaderMaterial;
		mat.SetShaderParameter("left_texture", _leftTexture);
		mat.SetShaderParameter("right_texture", _rightTexture);
		
		// 00 and 60
		_leftReceiver = new CameraFeedReceiver("[Left  cam]", leftAddr, 55556, _leftTexture);
		_leftReceiver.Start();
		
		_rightReceiver = new CameraFeedReceiver("[Right cam]", rightAddr, 55557, _rightTexture);
		_rightReceiver.Start();
	}

	private void InitXr() {
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
	


	protected override void Dispose(bool disposing) {
		_leftReceiver.Stop();
		_rightReceiver.Stop();
		base.Dispose(disposing);
	}
	
}

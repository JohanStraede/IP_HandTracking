
using UnityEngine;
using System.Text;
using OscJack;

/// <summary>
/// Simple OSC listener using OscJack package.
/// Attach to any GameObject and set the listen port (default 9000).
/// It will print incoming OSC messages to the Unity console.
/// </summary>
public class oscListnerClient : MonoBehaviour
{
	[Tooltip("UDP port to listen on for incoming OSC messages")]
	public int listenPort = 9000;

	OscServer _server;
	// Action queue to marshal callbacks from OscJack's background thread to Unity main thread
	readonly System.Collections.Generic.Queue<System.Action> _mainThreadQueue = new System.Collections.Generic.Queue<System.Action>();
	readonly object _queueLock = new object();

	// Public state properties other scripts can read safely (they are updated on main thread)
	public bool SpellOn { get; private set; }
	public bool RightMoving { get; private set; }
	public float RightSpeed { get; private set; }

	// Optional events other scripts can subscribe to for change notifications
	public event System.Action<bool> SpellChanged;
	public event System.Action<bool> RightMovingChanged;
	public event System.Action<float> RightSpeedChanged;

	void Start()
	{
		try
		{
			_server = new OscServer(listenPort);
			// Use empty address as a monitor callback to receive all messages
			_server.MessageDispatcher.AddCallback(string.Empty, OnOscMessage);
			Debug.Log($"OSC listener started on port {listenPort}");
		}
		catch (System.Exception e)
		{
			Debug.LogError($"Failed to start OSC listener on port {listenPort}: {e}");
		}
	}

	void OnOscMessage(string address, OscDataHandle data)
	{
		var count = data.GetElementCount();

		// If this is one of the known addresses, parse and publish the state to main thread.
		if (address == "/spell")
		{
			// Expecting an int/bool (1/0) or boolean-like value
			bool val = false;
			if (count > 0)
			{
				// Prefer integer/bool
				int asInt = data.GetElementAsInt(0);
				float asFloat = data.GetElementAsFloat(0);
				string asString = data.GetElementAsString(0);
				if (!string.IsNullOrEmpty(asString) && (asString.Equals("true", System.StringComparison.OrdinalIgnoreCase) || asString.Equals("1")))
					val = true;
				else if (asInt != 0)
					val = true;
				else if (Mathf.Abs(asFloat) > Mathf.Epsilon)
					val = true;
			}

			EnqueueOnMainThread(() => {
				if (SpellOn != val)
				{
					SpellOn = val;
					SpellChanged?.Invoke(val);
				}
				Debug.Log($"OSC /spell => {SpellOn}");
			});
			return;
		}

		if (address == "/right/moving")
		{
			bool val = false;
			if (count > 0)
			{
				int asInt = data.GetElementAsInt(0);
				float asFloat = data.GetElementAsFloat(0);
				string asString = data.GetElementAsString(0);
				if (!string.IsNullOrEmpty(asString) && (asString.Equals("true", System.StringComparison.OrdinalIgnoreCase) || asString.Equals("1")))
					val = true;
				else if (asInt != 0)
					val = true;
				else if (Mathf.Abs(asFloat) > Mathf.Epsilon)
					val = true;
			}

			EnqueueOnMainThread(() => {
				if (RightMoving != val)
				{
					RightMoving = val;
					RightMovingChanged?.Invoke(val);
				}
				Debug.Log($"OSC /right/moving => {RightMoving}");
			});
			return;
		}

		if (address == "/right/speed")
		{
			float speed = 0f;
			if (count > 0)
				speed = data.GetElementAsFloat(0);

			EnqueueOnMainThread(() => {
				RightSpeed = speed;
				RightSpeedChanged?.Invoke(speed);
				Debug.Log($"OSC /right/speed => {RightSpeed}");
			});
			return;
		}

		// Generic logging for other addresses
		var sb = new StringBuilder();
		sb.AppendFormat("OSC {0} args[{1}]:", address, count);
		for (int i = 0; i < count; i++)
		{
			string asString = data.GetElementAsString(i);
			float asFloat = data.GetElementAsFloat(i);
			int asInt = data.GetElementAsInt(i);
			if (!string.IsNullOrEmpty(asString) && !(asString == asInt.ToString()))
				sb.AppendFormat(" [{0}]:\"{1}\"", i, asString);
			else
				sb.AppendFormat(" [{0}]:int={1}, float={2}", i, asInt, asFloat);
		}
		Debug.Log(sb.ToString());
	}

	void EnqueueOnMainThread(System.Action act)
	{
		lock (_queueLock)
		{
			_mainThreadQueue.Enqueue(act);
		}
	}

	void OnDestroy()
	{
		if (_server != null)
		{
			_server.Dispose();
			_server = null;
			Debug.Log("OSC listener stopped.");
		}
	}

	void Update()
	{
		// Execute queued actions from OSC background thread on Unity main thread
	System.Action act = null;
		while (true)
		{
			lock (_queueLock)
			{
				if (_mainThreadQueue.Count == 0) break;
				act = _mainThreadQueue.Dequeue();
			}

			try
			{
				act?.Invoke();
			}
			catch (System.Exception e)
			{
				Debug.LogError($"Error executing queued OSC action: {e}");
			}
		}
	}
}


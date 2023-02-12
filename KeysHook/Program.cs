using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace KeysHook {
	internal class Program {
		static void Main(string[] args) {
			KeyPressHook hook = new KeyPressHook();
			hook.KeyPresed += KeyPresedHandler;
			hook.Start();
		}

		protected static void KeyPresedHandler(object sender, KeyPressedEventArgs e) {
			string logText = $"Клавiшу {e.Key} заблоковано!";
			Console.WriteLine(logText);
			using (StreamWriter stream = new StreamWriter("Log.txt", true)) {
				stream.WriteLine(logText);
			}
		}
	}

	public class KeyPressHook {
		protected const int WH_KEYBOARD_LL = 13;
		protected const int WM_KEYDOWN = 0x0100;

		protected HookProc proc;
		protected IntPtr hook = IntPtr.Zero;

		protected delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

		public delegate void KeyPresedHandler(object sender, KeyPressedEventArgs e);
		public event KeyPresedHandler KeyPresed;

		public KeyPressHook() {
			proc = HookCallback;
		}

		public void Start() {
			hook = SetHook(proc);
			Application.Run();
			UnhookWindowsHookEx(hook);
		}

		protected IntPtr SetHook(HookProc proc) {
			using (Process p = Process.GetCurrentProcess())
			using (ProcessModule pm = p.MainModule) {
				IntPtr x = GetModuleHandle(pm.ModuleName);
				return SetWindowsHookEx(WH_KEYBOARD_LL, proc, (IntPtr)null, 0);
			}
		}

		protected IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
			if ((nCode >= 0) && (wParam == (IntPtr)WM_KEYDOWN)) {
				Keys keyPressed = (Keys)Marshal.ReadInt32(lParam);

				foreach (Keys key in Enum.GetValues(typeof(Keys))) {
					if (keyPressed == key) {
						KeyPresed?.Invoke(this, new KeyPressedEventArgs(key));
						return (IntPtr)1;
					}
				}
			}
			return CallNextHookEx(hook, nCode, wParam, lParam);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		protected static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, UInt32 dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		protected static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		protected static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		protected static extern IntPtr GetModuleHandle(string lpModuleName);
	}

	public class KeyPressedEventArgs : EventArgs {
		protected Keys key;

		public KeyPressedEventArgs(Keys key) {
			this.key = key;
		}

		public Keys Key { get => key; }
	}
}

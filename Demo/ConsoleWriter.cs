using System;
using System.Linq;
using System.Threading;

namespace AlphaOmega.Debug
{
	internal class ConsoleWriter
	{
		private volatile Boolean _pause = false;
		private readonly ManualResetEvent _pauseEvent = new ManualResetEvent(false);
		private readonly ManualResetEvent _exitEvent = new ManualResetEvent(false);

		private readonly Boolean _pauseOnDir;
		private Boolean Pause
		{
			get { return this._pause; }
			set
			{
				this._pause = value;
				if(value)
					this._pauseEvent.Reset();
				else
					this._pauseEvent.Set();
			}
		}

		public ConsoleWriter(Boolean pauseOnDir)
			=> this._pauseOnDir = pauseOnDir;

		public void StartThreadAsync(Action func)
		{
			ThreadPool.QueueUserWorkItem((e) => this.Start(func));
			this.WaitForWritingToStop();
		}

		public void Start(Action func)
		{
			try
			{
				func();
				this.Stop();
			} catch(Exception exc)
			{
				this.ConsoleWriteError(exc, exc.GetType().Name, true);
				this.Stop();
				throw;
			}
		}

		private void WaitForWritingToStop()
		{
			do
			{
				Console.ReadKey();
				this.Pause = !this.Pause;
			}
			while(this._exitEvent.WaitOne(100) == false);
		}

		public void Stop()
			=> this._exitEvent.Set();

		public void PauseOnDir()
		{
			if(this._pauseOnDir)
				this.Pause = true;
		}

		public void WriteLine(String message)
		{
			if(this._pause)
				this._pauseEvent.WaitOne();
			Console.WriteLine(message);
		}

		public void Write(String message)
		{
			if(this._pause)
				this._pauseEvent.WaitOne();
			Console.Write(message);
		}

		public void ConsoleWriteError(Exception exc, String title, Boolean waitForInput = false)
			=> this.ConsoleWriteError(waitForInput,
				title + ": " + exc.Message,
				"========================",
				exc.Data == null ? String.Empty : String.Join(Environment.NewLine, exc.Data.Keys.Cast<Object>().Select(k => k.ToString() + ": " + exc.Data[k].ToString()).ToArray()),
				exc.StackTrace);

		public void ConsoleWriteError(Boolean waitForInput, params String[] lines)
		{
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			foreach(String line in lines)
				this.WriteLine(line);
			Console.ForegroundColor = color;
			if(waitForInput)
				this.Pause = true;
		}

		public void ConsolWriteInstruction(Int32 line, System.Reflection.Emit.OpCode il, String code)
		{
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Gray;
			this.Write("IL_" + line.ToString("X").PadLeft(4, '0') + ": " + il.Name);
			Console.ForegroundColor = color;
			this.WriteLine(code);
		}

		public void ConsoleWriteMembers(Object obj)
			=> this.ConsoleWriteMembers(null, obj);

		public void ConsoleWriteMembers(String title, Object obj)
		{
			if(!String.IsNullOrEmpty(title))
				this.Write(title + ": ");
			this.WriteLine(Utils.GetReflectedMembers(obj));
		}
	}
}
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GorillaGuard.Security;

internal static class DesktopShell
{
	private sealed class GuardWindow : Form
	{
		private readonly Label status = new Label
		{
			AutoSize = false,
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft
		};

		private readonly TextBox activity = new TextBox
		{
			Multiline = true,
			ReadOnly = true,
			ScrollBars = ScrollBars.Vertical,
			Dock = DockStyle.Fill,
			BorderStyle = BorderStyle.None
		};

		private readonly Button action = new Button
		{
			Text = _"Start protected session",
			AutoSize = true,
			Padding = new Padding(18, 10, 18, 10),
			FlatStyle = FlatStyle.Flat
		};

		private readonly Button scan = new Button
		{
			Text = _"Scan only",
			AutoSize = true,
			Padding = new Padding(18, 10, 18, 10),
			FlatStyle = FlatStyle.Flat
		};

		private readonly ProgressBar progress = new ProgressBar
		{
			Dock = DockStyle.Fill,
			Style = ProgressBarStyle.Blocks
		};

		private readonly CancellationTokenSource stop = new CancellationTokenSource();

		private readonly ConcurrentQueue<string> messages = new ConcurrentQueue<string>();

		private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer
		{
			Interval = 150
		};

		private readonly Func<string[], Task<int>> run;

		private readonly string[] args;

		private readonly string purpose;

		private readonly TextWriter previousOut = Console.Out;

		private readonly TextWriter previousError = Console.Error;

		private bool running;

		private bool finished;

		private bool closing;

		private bool activeScan;

		private int pending;

		internal int Result { get; private set; }

		internal GuardWindow(string[] arguments, Func<string[], Task<int>> backend, string purpose, Action<CancellationToken> setToken)
		{
			args = arguments;
			run = backend;
			this.purpose = purpose;
			setToken(stop.Token);
			Text = _"Gorilla Guard";
			base.ClientSize = new Size(820, 620);
			MinimumSize = new Size(700, 540);
			base.StartPosition = FormStartPosition.CenterScreen;
			base.AutoScaleMode = AutoScaleMode.Dpi;
			BackColor = Color.FromArgb(12, 20, 32);
			ForeColor = Color.FromArgb(235, 242, 248);
			Font = new Font(_"Segoe UI", 10f);
			base.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath);
			TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(28),
				ColumnCount = 1,
				RowCount = 7
			};
			int[] array = new int[5] { 110, 40, 52, 10, 18 };
			foreach (int num in array)
			{
				tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, num));
			}
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 64f));
			TableLayoutPanel tableLayoutPanel2 = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 2
			};
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108f));
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			PictureBox logo = new PictureBox
			{
				Dock = DockStyle.Fill,
				SizeMode = PictureBoxSizeMode.Zoom
			};
			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(_"GorillaGuard.Logo.png"))
			{
				if (stream != null)
				{
					using Image original = Image.FromStream(stream);
					logo.Image = new Bitmap(original);
				}
			}
			Label control = new Label
			{
				Text = _"GORILLA GUARD\nLocal protection. One session at a time.",
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleLeft,
				Font = new Font(_"Segoe UI Semibold", 18f)
			};
			tableLayoutPanel2.Controls.Add(logo, 0, 0);
			tableLayoutPanel2.Controls.Add(control, 1, 0);
			tableLayoutPanel.Controls.Add(tableLayoutPanel2, 0, 0);
			tableLayoutPanel.Controls.Add(new Label
			{
				Text = purpose + _"  •  v1.12.31",
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleLeft,
				ForeColor = Color.FromArgb(101, 215, 211)
			}, 0, 1);
			status.Text = ((purpose == _"Secure installer") ? _"Ready to download and verify Gorilla Guard." : _"Ready to check your installation and launch Gorilla Tag.");
			tableLayoutPanel.Controls.Add(status, 0, 2);
			tableLayoutPanel.Controls.Add(progress, 0, 3);
			tableLayoutPanel.Controls.Add(new Label
			{
				Text = _"ACTIVITY",
				Dock = DockStyle.Fill,
				Font = new Font(_"Segoe UI", 8f)
			}, 0, 4);
			activity.BackColor = Color.FromArgb(19, 30, 45);
			activity.ForeColor = ForeColor;
			tableLayoutPanel.Controls.Add(activity, 0, 5);
			FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				FlowDirection = FlowDirection.RightToLeft,
				Padding = new Padding(0, 10, 0, 0)
			};
			action.BackColor = Color.FromArgb(65, 218, 205);
			action.ForeColor = Color.FromArgb(8, 22, 32);
			scan.BackColor = Color.FromArgb(28, 45, 63);
			scan.ForeColor = ForeColor;
			if (purpose == _"Secure installer")
			{
				action.Text = _"Install Gorilla Guard";
			}
			flowLayoutPanel.Controls.Add(action);
			if (purpose != _"Secure installer")
			{
				flowLayoutPanel.Controls.Add(scan);
			}
			tableLayoutPanel.Controls.Add(flowLayoutPanel, 0, 6);
			base.Controls.Add(tableLayoutPanel);
			UiWriter uiWriter = new UiWriter(A);
			Console.SetOut(uiWriter);
			Console.SetError(uiWriter);
			timer.Tick += delegate
			{
				A();
			};
			timer.Start();
			action.Click += async delegate
			{
				if (finished)
				{
					Close();
				}
				else if (running)
				{
					a();
				}
				else
				{
					await A(false);
				}
			};
			scan.Click += async delegate
			{
				if (!finished && !running)
				{
					await A(true);
				}
			};
			base.FormClosing += delegate(object? _, FormClosingEventArgs P_1)
			{
				if (running)
				{
					P_1.Cancel = true;
					closing = true;
					a();
				}
			};
			base.FormClosed += delegate
			{
				timer.Stop();
				Console.SetOut(previousOut);
				Console.SetError(previousError);
				logo.Image?.Dispose();
				stop.Dispose();
				timer.Dispose();
			};
			if (!args.Contains(_"--ui-preview"))
			{
				return;
			}
			base.Shown += delegate
			{
				using Bitmap bitmap = new Bitmap(base.Width, base.Height);
				DrawToBitmap(bitmap, new Rectangle(0, 0, base.Width, base.Height));
				bitmap.Save(Path.Combine(AppContext.BaseDirectory, _"GorillaGuard.UI.png"));
				Close();
			};
		}

		private void A(string P_0)
		{
			if (Interlocked.Increment(ref pending) > 256)
			{
				Interlocked.Decrement(ref pending);
			}
			else
			{
				messages.Enqueue((P_0.Length > 1200) ? P_0.Substring(0, 1200) : P_0);
			}
		}

		private void A()
		{
			for (int i = 0; i < 40; i++)
			{
				if (!messages.TryDequeue(out string result))
				{
					break;
				}
				Interlocked.Decrement(ref pending);
				if (activity.TextLength > 24000)
				{
					TextBox textBox = activity;
					string text = activity.Text;
					textBox.Text = text.Substring(text.Length - 12000);
				}
				activity.AppendText(result + Environment.NewLine);
				if (running && !stop.IsCancellationRequested)
				{
					if (result.Contains(_"[ERROR]") || result.Contains(_"STOPPED:"))
					{
						status.Text = _"An issue needs attention. Review the details below.";
					}
					else if (result.Contains(_"Protected game process found"))
					{
						status.Text = _"Game started. Waiting for the guard to verify the protected session.";
					}
					else if (result.Contains(_"GorillaGuard.dll authenticated to the launcher."))
					{
						status.Text = _"Protected session connected. Keep this window open while playing.";
					}
					else if (result.Contains(_"Downloading"))
					{
						status.Text = _"Downloading and verifying the signed release…";
					}
				}
			}
		}

		private void a()
		{
			stop.Cancel();
			action.Enabled = false;
			scan.Enabled = false;
			status.Text = (activeScan ? _"Stopping the scan safely…" : _"Stopping safely… The protected game will close and cleanup will follow.");
		}

		private async Task A(bool P_0)
		{
			activeScan = P_0;
			running = true;
			scan.Enabled = false;
			action.Text = (P_0 ? _"Cancel scan" : ((purpose == _"Secure installer") ? _"Cancel installation" : _"Stop session"));
			progress.Style = ProgressBarStyle.Marquee;
			status.Text = (P_0 ? _"Scanning only… No quarantine, repair, settings change, or game launch will occur." : _"Working… Checking files, updates, and your game environment.");
			string[] runArgs = (P_0 ? args.Append(_"--scan-only").ToArray() : args);
			try
			{
				Result = await Task.Run(() => run(runArgs));
			}
			catch (OperationCanceledException)
			{
				Result = 1;
			}
			catch (Exception ex2)
			{
				Result = 1;
				A(_"Could not complete this session: " + ex2.Message);
			}
			finally
			{
				running = false;
				finished = true;
				A();
				progress.Style = ProgressBarStyle.Blocks;
				status.Text = ((stop.IsCancellationRequested && P_0) ? _"Read-only scan stopped. Close this window when ready." : (stop.IsCancellationRequested ? _"Session stopped. Any scheduled cleanup runs after this window closes." : ((P_0 && Result == 0) ? _"Read-only scan passed. Review the activity below, then close this window." : ((P_0 && Result == 2) ? _"Read-only scan was incomplete. Review the activity below for details." : (P_0 ? _"Read-only scan found an issue. Review the activity below for details." : ((Result == 0) ? _"Finished. Close this window to allow any scheduled cleanup to complete." : _"Session could not complete. Review the activity below for details."))))));
				action.Enabled = true;
				action.Text = _"Close";
				if (closing || (!P_0 && Result == 0) || stop.IsCancellationRequested)
				{
					Close();
				}
			}
		}

				private void A(object? _, EventArgs P_1)
		{
			A();
		}

				private async void a(object? _, EventArgs P_1)
		{
			if (finished)
			{
				Close();
			}
			else if (running)
			{
				a();
			}
			else
			{
				await A(false);
			}
		}

				private async void B(object? _, EventArgs P_1)
		{
			if (!finished && !running)
			{
				await A(true);
			}
		}

				private void A(object? _, FormClosingEventArgs P_1)
		{
			if (running)
			{
				P_1.Cancel = true;
				closing = true;
				a();
			}
		}

				private void b(object? _, EventArgs P_1)
		{
			using Bitmap bitmap = new Bitmap(base.Width, base.Height);
			DrawToBitmap(bitmap, new Rectangle(0, 0, base.Width, base.Height));
			bitmap.Save(Path.Combine(AppContext.BaseDirectory, _"GorillaGuard.UI.png"));
			Close();
		}
	}

	private sealed class UiWriter(Action<string> write) : TextWriter()
	{
		public override Encoding Encoding => System.Text.Encoding.UTF8;

		public override void WriteLine(string? value)
		{
			write(value ?? _"");
		}

		public override void Write(char value)
		{
			if (value != '\r' && value != '\n')
			{
				write(value.ToString());
			}
		}
	}

	internal static int A(string[] P_0, Func<string[], Task<int>> P_1, string P_2, Action<CancellationToken> P_3)
	{
		if (P_0.Length != 0 && !P_0.Contains(_"--ui-preview"))
		{
			return P_1(P_0).GetAwaiter().GetResult();
		}
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		using GuardWindow guardWindow = new GuardWindow(P_0, P_1, P_2, P_3);
		Application.Run(guardWindow);
		return guardWindow.Result;
	}
}

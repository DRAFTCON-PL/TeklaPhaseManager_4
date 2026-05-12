// Tekla macro: Named pipe listener for debugging
// Place in Tekla macros folder and run. Ensures pipe communication works before implementing UI calls.

using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Windows.Forms;

public static class TeklaOpenPhaseManagerMacro
{
    // Entry point for Tekla macro runner
    public static void Run()
    {
        Thread t = new Thread(ListenPipe) { IsBackground = true };
        t.Start();
        MessageBox.Show("TPM Pipe listener started. Waiting for messages on pipe 'TPM_PIPE'.", "TPM Macro");
    }

    private static void ListenPipe()
    {
        const string pipeName = "TPM_PIPE"; // change if needed
        while (true)
        {
            try
            {
                using (var server = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.None))
                {
                    server.WaitForConnection();
                    if (!server.IsConnected) continue;

                    byte[] buffer = new byte[1024];
                    int read = server.Read(buffer, 0, buffer.Length);
                    if (read > 0)
                    {
                        string msg = Encoding.UTF8.GetString(buffer, 0, read);
                        try { MessageBox.Show("Received pipe message:\n" + msg, "TPM Macro"); } catch { }
                        // For debugging: if message contains OPEN_PHASE_MANAGER, show explicit acknowledgment
                        if (msg.IndexOf("OPEN_PHASE_MANAGER", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            try { MessageBox.Show("OPEN_PHASE_MANAGER received.", "TPM Macro"); } catch { }
                        }
                    }

                    try { server.Disconnect(); } catch { }
                }
            }
            catch (Exception ex)
            {
                try { MessageBox.Show("TPM pipe listener error: " + ex.Message, "TPM Macro Error"); } catch { }
                Thread.Sleep(1000);
            }
        }
    }
}

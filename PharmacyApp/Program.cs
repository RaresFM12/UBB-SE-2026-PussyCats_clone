using Microsoft.Windows.ApplicationModel.DynamicDependency;
using System;

namespace PharmacyApp;

//MY PROJECT DOES NOT WORK WITHOUT THIS. YOU CAN IGNORE ON MERGE 

public static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            Bootstrap.Initialize(0x00010008);
        }
        catch { /* already initialized, ignore */ }

        global::WinRT.ComWrappersSupport.InitializeComWrappers();
        global::Microsoft.UI.Xaml.Application.Start((p) =>
        {
            var context = new global::Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(
                global::Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
            global::System.Threading.SynchronizationContext.SetSynchronizationContext(context);
            new App();
        });
    }
}
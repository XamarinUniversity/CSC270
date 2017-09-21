using System;
using System.IO;

/*
 * GCAllocs - show allocations (physical addresses)
 * 
 * Run this app, it will produce a "gcallocs.txt" file on your desktop.
 * Open the file in Excel, select the entire 1st column and create a new Line Chart
 * from the Insert tab.
 * 
 * Results should show steady increase in addresses, then a drop off
 * repeating over and over. This is the GC at work!
 */

namespace GCAllocs
{
    class Dummy
    {
        // This is always allocated _inside_ the Dummy memory on the heap.
        public int intValue;
    }

    class MainClass
    {
        public static void Main (string [] args)
        {
            Console.WriteLine ("GCAllocs: running for 3 seconds.");

            string filename = Path.Combine (
                Environment.GetFolderPath (Environment.SpecialFolder.Desktop),
                "gcallocs.csv");

            using (StreamWriter writer = new StreamWriter(filename))
            {
                DateTime future = DateTime.Now + TimeSpan.FromSeconds(3);
                while (DateTime.Now <= future) {
                    var d = new Dummy ();
                    Console.WriteLine (GetAddressOfRef(ref d));
                }
            }

            Console.WriteLine ("Done. Data written to {0}", filename);
        }

        static long GetAddressOfRef (ref Dummy item)
        {
            unsafe
            {
                // Taking pointer addresses is generally a bad
                // idea -- don't do this sort of thing in production code!
                fixed (int* pd = &item.intValue)
                {
                    return ((IntPtr)pd).ToInt64 ();
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;

/*
 * GCAllocs V2 - show allocations (physical addresses)
 * 
 * Run this app, it will produce a "gcallocs.txt" file on your desktop.
 * Open the file in Excel, select the entire 1st column and create a new Line Chart
 * from the Insert tab.
 * 
 * Results should show steady increase in addresses, then a drop off
 * repeating over and over. This is the GC at work!
 */
using System.Diagnostics;

namespace GCAllocs
{
    class Dummy
    {
        // This is always allocated _inside_ the Dummy memory on the heap.
        public int intValue;
    }

    struct Holder
    {
        public Dummy theObject;
        public long nurseryAddr;
        public long majorHeapAddr;
    }

    class MainClass
    {
        public static void Main (string [] args)
        {
            Trace.Listeners.Add (new ConsoleTraceListener ());

            const int MAX_OBJECTS = 1000000;
            List<Holder> hold = new List<Holder> (MAX_OBJECTS);

            Console.WriteLine ("GCAllocs: allocating {0} objects.", MAX_OBJECTS);

            string filename = Path.Combine (
                Environment.GetFolderPath (Environment.SpecialFolder.Desktop),
                "gcallocs.csv");

            for (int count = 0; count < MAX_OBJECTS; count++) {
                var dummy = new Dummy ();
                hold.Add(new Holder {
                    theObject = dummy,
                    nurseryAddr = GetAddressOfRef (ref dummy)
                });
            }

            Console.WriteLine ("Did {0} nursery collections, {1} major collections.", 
                               GC.CollectionCount(0), GC.CollectionCount(1));

            Console.WriteLine ("Forcing collect to push everything to the major heap.");

            GC.Collect ();

            using (StreamWriter writer = new StreamWriter (filename)) {

                for (int i = 0; i < hold.Count; i++) {
                    var item = hold [i];
                    item.majorHeapAddr = GetAddressOfRef (ref item.theObject);
                    writer.WriteLine ("{0},{1}", item.nurseryAddr, item.majorHeapAddr);
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

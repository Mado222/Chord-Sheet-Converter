using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindControlLib
{
    public class CPIC24_IntelHex : CIntelHex
    {
        public override int MemoryMirror_size { get { return 2 * 0x1000000; } }

        public override byte[] Get_Memory_Area(uint StartAddress, uint numBytestoRead)
        {
            List<byte> mem_vals_list = new(base.Get_Memory_Area(StartAddress * 2, numBytestoRead * 2));

            //Remove Higher Word
            int cnt = 2;
            while (cnt < mem_vals_list.Count)
            {
                mem_vals_list.RemoveAt(cnt);
                mem_vals_list.RemoveAt(cnt);
                cnt += 2;
            }
            return [.. mem_vals_list];
        }
        public override void Add_to_MemoryMirror(byte[] Data, uint BeginAddress, bool Overwrite, byte Addedchar = 0)
        {
            //Add zero values
            List<byte> mem_vals_list = new();

            int cnt = 0;
            while (cnt < Data.Length - 1)
            {
                mem_vals_list.Add(Data[cnt]);
                mem_vals_list.Add(Data[cnt + 1]);
                mem_vals_list.Add(Addedchar);
                mem_vals_list.Add(Addedchar);
                cnt += 2;
            }
            base.Add_to_MemoryMirror([.. mem_vals_list], BeginAddress * 2, Overwrite);
        }

    }
}

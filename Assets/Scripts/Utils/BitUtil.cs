using System.Collections.Generic;

public class BitUtil
{
	/**
		* 位位置 
		*/		
	public static int[] BIT_POS = {0x01,0x02,0x04,0x08,
                                    0x10,0x20,0x40,0x80,
                                    0x100,0x200,0x400,0x800,
                                    0x1000,0x2000,0x4000,0x8000,
                                    0x10000,0x20000,0x40000,0x80000,
                                    0x100000,0x200000,0x400000,0x800000,
                                    0x1000000,0x2000000,0x4000000,0x8000000,
                                    0x10000000,0x20000000,0x40000000};
	

	public static int getBit(int value, int index)
	{
		return (value & BIT_POS[index]) >> index;
	}
}
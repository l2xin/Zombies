using System;
using System.IO;

public enum NetworkMessageType : byte
{
	InputBuffer = 0,
	RandomSeedSynchronization,
	RandomSeedSynchronized,
	Syncronization,
    AIState,
}
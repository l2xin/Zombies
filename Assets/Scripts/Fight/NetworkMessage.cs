using System;
using System.IO;

public abstract class NetworkMessage<T>
{
	#region public instance properties
	public NetworkMessageType MessageType{get; set;}
	public ulong PlayerIndex{get; set;}
	public ushort CurrentFrame{get; set;}

    public ushort currentNetworkFrame;

    public T Data{get; set;}
	#endregion
	
	#region protected instance constructors
	protected NetworkMessage(NetworkMessageType messageType, ulong playerIndex, ushort currentFrame, T data)
    {
		this.MessageType = messageType;
		this.PlayerIndex = playerIndex;
		this.CurrentFrame = currentFrame;
		this.Data = data;
	}
	
	protected NetworkMessage(byte[] serializedNetworkMessage)
    {
        using (MemoryStream stream = new MemoryStream(serializedNetworkMessage))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                this.MessageType = (NetworkMessageType)reader.ReadByte();
                this.CurrentFrame = reader.ReadUInt16();
                this.currentNetworkFrame = reader.ReadUInt16();
                this.Data = this.ReadFromStream(reader);
            }
        }
	}
    #endregion

    #region public instance methods

    private MemoryStream stream = new MemoryStream();
    private BinaryWriter writer = null;
    public byte[] Serialize()
    {
        stream = new MemoryStream();
        writer = new BinaryWriter(stream);
        stream.Seek(0, SeekOrigin.Begin);
        writer.Write((byte)this.MessageType);
        writer.Write(this.CurrentFrame);
        //writer.Write(UFE.currentNetworkFrame);
        this.AddToStream(writer, this.Data);
        writer.Flush();
        return stream.ToArray();
    }
	#endregion

	#region public override methods
	public override string ToString ()
    {
		return string.Format(
			"[{0} | messageType = {1} | playerIndex = {2} | currentFrame = {3} | data = {4}]",
			this.GetType().ToString(),
			this.MessageType,
			this.PlayerIndex,
			this.CurrentFrame,
			this.Data.ToString()
		);
	}
	#endregion
	
	#region protected instance methods
	protected abstract void AddToStream(BinaryWriter writer, T data);
	protected abstract T ReadFromStream(BinaryReader reader);
	#endregion
}
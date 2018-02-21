namespace SoftFX.Net.BotAgent
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using SoftFX.Net.Core;
    
    #pragma warning disable 164
    
    struct BoolArray
    {
        public BoolArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public bool this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetBool(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetBool(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct BoolNullArray
    {
        public BoolNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public bool? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetBoolNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetBoolNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct CharArray
    {
        public CharArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public char this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetChar(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetChar(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct CharNullArray
    {
        public CharNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public char? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetCharNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetCharNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct UCharArray
    {
        public UCharArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public char this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetUChar(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetUChar(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct UCharNullArray
    {
        public UCharNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public char? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetUCharNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetUCharNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct ByteArray
    {
        public ByteArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public byte this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetByte(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetByte(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct ByteNullArray
    {
        public ByteNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public byte? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetByteNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetByteNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct SByteArray
    {
        public SByteArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public sbyte this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetSByte(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetSByte(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct SByteNullArray
    {
        public SByteNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public sbyte? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                data_.SetSByteNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                return data_.GetSByteNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct ShortArray
    {
        public ShortArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 2);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public short this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 2);
                data_.SetShort(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 2);
                return data_.GetShort(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct ShortNullArray
    {
        public ShortNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 2);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public short? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 2);
                data_.SetShortNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 2);
                return data_.GetShortNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct UShortArray
    {
        public UShortArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 2);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public ushort this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 2);
                data_.SetUShort(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 2);
                return data_.GetUShort(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct UShortNullArray
    {
        public UShortNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 2);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public ushort? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 2);
                data_.SetUShortNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 2);
                return data_.GetUShortNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct IntArray
    {
        public IntArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public int this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetInt(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return data_.GetInt(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct IntNullArray
    {
        public IntNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public int? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetIntNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return data_.GetIntNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct UIntArray
    {
        public UIntArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public uint this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetUInt(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return data_.GetUInt(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct UIntNullArray
    {
        public UIntNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public uint? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetUIntNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return data_.GetUIntNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct LongArray
    {
        public LongArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public long this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetLong(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetLong(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct LongNullArray
    {
        public LongNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public long? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetLongNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetLongNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct ULongArray
    {
        public ULongArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public ulong this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetULong(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetULong(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct ULongNullArray
    {
        public ULongNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public ulong? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetULongNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetULongNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct DoubleArray
    {
        public DoubleArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public double this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetDouble(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetDouble(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct DoubleNullArray
    {
        public DoubleNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public double? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetDoubleNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetDoubleNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct DateTimeArray
    {
        public DateTimeArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public DateTime this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetDateTime(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetDateTime(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct DateTimeNullArray
    {
        public DateTimeNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public DateTime? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetDateTimeNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetDateTimeNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct StringArray
    {
        public StringArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public string this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetString(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetString(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct StringNullArray
    {
        public StringNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public string this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetStringNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetStringNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct UStringArray
    {
        public UStringArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public string this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetUString(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetUString(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct UStringNullArray
    {
        public UStringNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public string this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetUStringNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetUStringNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct BytesArray
    {
        public BytesArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public byte[] this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetBytes(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetBytes(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct BytesNullArray
    {
        public BytesNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 8);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public byte[] this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                data_.SetBytesNull(itemOffset, value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 8);
                return data_.GetBytesNull(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct LoginRequest
    {
        public static implicit operator Message(LoginRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public LoginRequest(int i)
        {
            info_ = BotAgent.Info.LoginRequest;
            data_ = new MessageData(24);
            
            data_.SetInt(4, 0);
        }
        
        public LoginRequest(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.LoginRequest))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string Username
        {
            set { data_.SetUString(8, value); }
            
            get { return data_.GetUString(8); }
        }
        
        public int GetUsernameLength()
        {
            return data_.GetUStringLength(8);
        }
        
        public void SetUsername(char[] value, int offset, int count)
        {
            data_.SetUString(8, value, offset, count);
        }
        
        public void GetUsername(char[] value, int offset)
        {
            data_.GetUString(8, value, offset);
        }
        
        public void ReadUsername(Stream stream, int size)
        {
            data_.ReadUString(8, stream, size);
        }
        
        public void WriteUsername(Stream stream)
        {
            data_.WriteUString(8, stream);
        }
        
        public string Password
        {
            set { data_.SetUString(16, value); }
            
            get { return data_.GetUString(16); }
        }
        
        public int GetPasswordLength()
        {
            return data_.GetUStringLength(16);
        }
        
        public void SetPassword(char[] value, int offset, int count)
        {
            data_.SetUString(16, value, offset, count);
        }
        
        public void GetPassword(char[] value, int offset)
        {
            data_.GetUString(16, value, offset);
        }
        
        public void ReadPassword(Stream stream, int size)
        {
            data_.ReadUString(16, stream, size);
        }
        
        public void WritePassword(Stream stream)
        {
            data_.WriteUString(16, stream);
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public LoginRequest Clone()
        {
            MessageData data = data_.Clone();
            
            return new LoginRequest(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct LoginReport_1
    {
        public static implicit operator Message(LoginReport_1 message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public LoginReport_1(int i)
        {
            info_ = BotAgent.Info.LoginReport_1;
            data_ = new MessageData(12);
            
            data_.SetInt(4, 1);
        }
        
        public LoginReport_1(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.LoginReport_1))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public int CurrentVersion
        {
            set { data_.SetInt(8, value); }
            
            get { return data_.GetInt(8); }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public LoginReport_1 Clone()
        {
            MessageData data = data_.Clone();
            
            return new LoginReport_1(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct LoginReport
    {
        public static implicit operator Message(LoginReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public LoginReport(int i)
        {
            info_ = BotAgent.Info.LoginReport;
            data_ = new MessageData(8);
            
            data_.SetInt(4, 20);
        }
        
        public LoginReport(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.LoginReport))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public LoginReport Clone()
        {
            MessageData data = data_.Clone();
            
            return new LoginReport(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    enum LoginRejectReason
    {
        InvalidCredentials = 0,
        VersionMismatch = 1,
        InternalServerError = 2,
    }
    
    struct LoginRejectReasonArray
    {
        public LoginRejectReasonArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public LoginRejectReason this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetUInt(itemOffset, (uint) value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return (LoginRejectReason) data_.GetUInt(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct LoginRejectReasonNullArray
    {
        public LoginRejectReasonNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public LoginRejectReason? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                if (value.HasValue)
                {
                    data_.SetUIntNull(itemOffset, (uint) value.Value);
                }
                else
                    data_.SetUIntNull(itemOffset, null);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                uint? value = data_.GetUIntNull(itemOffset);
                
                if (value.HasValue)
                    return (LoginRejectReason) value.Value;
                
                return null;
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct LoginReject
    {
        public static implicit operator Message(LoginReject message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public LoginReject(int i)
        {
            info_ = BotAgent.Info.LoginReject;
            data_ = new MessageData(20);
            
            data_.SetInt(4, 2);
        }
        
        public LoginReject(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.LoginReject))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public LoginRejectReason Reason
        {
            set { data_.SetUInt(8, (uint) value); }
            
            get { return (LoginRejectReason) data_.GetUInt(8); }
        }
        
        public string Text
        {
            set { data_.SetUStringNull(12, value); }
            
            get { return data_.GetUStringNull(12); }
        }
        
        public int? GetTextLength()
        {
            return data_.GetUStringNullLength(12);
        }
        
        public void SetText(char[] value, int offset, int count)
        {
            data_.SetUString(12, value, offset, count);
        }
        
        public void GetText(char[] value, int offset)
        {
            data_.GetUString(12, value, offset);
        }
        
        public void ReadText(Stream stream, int size)
        {
            data_.ReadUString(12, stream, size);
        }
        
        public void WriteText(Stream stream)
        {
            data_.WriteUString(12, stream);
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public LoginReject Clone()
        {
            MessageData data = data_.Clone();
            
            return new LoginReject(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct LogoutRequest
    {
        public static implicit operator Message(LogoutRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public LogoutRequest(int i)
        {
            info_ = BotAgent.Info.LogoutRequest;
            data_ = new MessageData(8);
            
            data_.SetInt(4, 3);
        }
        
        public LogoutRequest(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.LogoutRequest))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public LogoutRequest Clone()
        {
            MessageData data = data_.Clone();
            
            return new LogoutRequest(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    enum LogoutReason
    {
        ClientRequest = 0,
        ServerLogout = 1,
        InternalServerError = 2,
    }
    
    struct LogoutReasonArray
    {
        public LogoutReasonArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public LogoutReason this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetUInt(itemOffset, (uint) value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return (LogoutReason) data_.GetUInt(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct LogoutReasonNullArray
    {
        public LogoutReasonNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public LogoutReason? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                if (value.HasValue)
                {
                    data_.SetUIntNull(itemOffset, (uint) value.Value);
                }
                else
                    data_.SetUIntNull(itemOffset, null);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                uint? value = data_.GetUIntNull(itemOffset);
                
                if (value.HasValue)
                    return (LogoutReason) value.Value;
                
                return null;
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct LogoutReport
    {
        public static implicit operator Message(LogoutReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public LogoutReport(int i)
        {
            info_ = BotAgent.Info.LogoutReport;
            data_ = new MessageData(20);
            
            data_.SetInt(4, 4);
        }
        
        public LogoutReport(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.LogoutReport))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public LogoutReason Reason
        {
            set { data_.SetUInt(8, (uint) value); }
            
            get { return (LogoutReason) data_.GetUInt(8); }
        }
        
        public string Text
        {
            set { data_.SetUStringNull(12, value); }
            
            get { return data_.GetUStringNull(12); }
        }
        
        public int? GetTextLength()
        {
            return data_.GetUStringNullLength(12);
        }
        
        public void SetText(char[] value, int offset, int count)
        {
            data_.SetUString(12, value, offset, count);
        }
        
        public void GetText(char[] value, int offset)
        {
            data_.GetUString(12, value, offset);
        }
        
        public void ReadText(Stream stream, int size)
        {
            data_.ReadUString(12, stream, size);
        }
        
        public void WriteText(Stream stream)
        {
            data_.WriteUString(12, stream);
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public LogoutReport Clone()
        {
            MessageData data = data_.Clone();
            
            return new LogoutReport(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct Request
    {
        public static implicit operator Message(Request message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public Request(int i)
        {
            info_ = BotAgent.Info.Request;
            data_ = new MessageData(16);
            
            data_.SetInt(4, 5);
        }
        
        public Request(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.Request))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string Id
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public Request Clone()
        {
            MessageData data = data_.Clone();
            
            return new Request(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    enum RequestExecState
    {
        Completed = 0,
        InternalServerError = 1,
    }
    
    struct RequestExecStateArray
    {
        public RequestExecStateArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public RequestExecState this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetUInt(itemOffset, (uint) value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return (RequestExecState) data_.GetUInt(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct RequestExecStateNullArray
    {
        public RequestExecStateNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public RequestExecState? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                if (value.HasValue)
                {
                    data_.SetUIntNull(itemOffset, (uint) value.Value);
                }
                else
                    data_.SetUIntNull(itemOffset, null);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                uint? value = data_.GetUIntNull(itemOffset);
                
                if (value.HasValue)
                    return (RequestExecState) value.Value;
                
                return null;
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct Report
    {
        public static implicit operator Message(Report message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public Report(int i)
        {
            info_ = BotAgent.Info.Report;
            data_ = new MessageData(28);
            
            data_.SetInt(4, 6);
        }
        
        public Report(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.Report))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string RequestId
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetRequestIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetRequestId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetRequestId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadRequestId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteRequestId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public RequestExecState RequestState
        {
            set { data_.SetUInt(16, (uint) value); }
            
            get { return (RequestExecState) data_.GetUInt(16); }
        }
        
        public string Text
        {
            set { data_.SetUStringNull(20, value); }
            
            get { return data_.GetUStringNull(20); }
        }
        
        public int? GetTextLength()
        {
            return data_.GetUStringNullLength(20);
        }
        
        public void SetText(char[] value, int offset, int count)
        {
            data_.SetUString(20, value, offset, count);
        }
        
        public void GetText(char[] value, int offset)
        {
            data_.GetUString(20, value, offset);
        }
        
        public void ReadText(Stream stream, int size)
        {
            data_.ReadUString(20, stream, size);
        }
        
        public void WriteText(Stream stream)
        {
            data_.WriteUString(20, stream);
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public Report Clone()
        {
            MessageData data = data_.Clone();
            
            return new Report(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    enum UpdateType
    {
        Added = 0,
        Updated = 1,
        Removed = 2,
    }
    
    struct UpdateTypeArray
    {
        public UpdateTypeArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public UpdateType this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetUInt(itemOffset, (uint) value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return (UpdateType) data_.GetUInt(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct UpdateTypeNullArray
    {
        public UpdateTypeNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public UpdateType? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                if (value.HasValue)
                {
                    data_.SetUIntNull(itemOffset, (uint) value.Value);
                }
                else
                    data_.SetUIntNull(itemOffset, null);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                uint? value = data_.GetUIntNull(itemOffset);
                
                if (value.HasValue)
                    return (UpdateType) value.Value;
                
                return null;
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct Update
    {
        public static implicit operator Message(Update message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public Update(int i)
        {
            info_ = BotAgent.Info.Update;
            data_ = new MessageData(20);
            
            data_.SetInt(4, 7);
        }
        
        public Update(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.Update))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string Id
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public UpdateType Type
        {
            set { data_.SetUInt(16, (uint) value); }
            
            get { return (UpdateType) data_.GetUInt(16); }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public Update Clone()
        {
            MessageData data = data_.Clone();
            
            return new Update(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct PluginKey
    {
        public PluginKey(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public string PackageName
        {
            set { data_.SetUString(offset_ + 0, value); }
            
            get { return data_.GetUString(offset_ + 0); }
        }
        
        public int GetPackageNameLength()
        {
            return data_.GetUStringLength(offset_ + 0);
        }
        
        public void SetPackageName(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 0, value, offset, count);
        }
        
        public void GetPackageName(char[] value, int offset)
        {
            data_.GetUString(offset_ + 0, value, offset);
        }
        
        public void ReadPackageName(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 0, stream, size);
        }
        
        public void WritePackageName(Stream stream)
        {
            data_.WriteUString(offset_ + 0, stream);
        }
        
        public string DescriptorId
        {
            set { data_.SetUString(offset_ + 8, value); }
            
            get { return data_.GetUString(offset_ + 8); }
        }
        
        public int GetDescriptorIdLength()
        {
            return data_.GetUStringLength(offset_ + 8);
        }
        
        public void SetDescriptorId(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 8, value, offset, count);
        }
        
        public void GetDescriptorId(char[] value, int offset)
        {
            data_.GetUString(offset_ + 8, value, offset);
        }
        
        public void ReadDescriptorId(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 8, stream, size);
        }
        
        public void WriteDescriptorId(Stream stream)
        {
            data_.WriteUString(offset_ + 8, stream);
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginKeyNull
    {
        public PluginKeyNull(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void New()
        {
            data_.NewGroup(offset_, 16);
        }
        
        public bool HasValue
        {
            get { return data_.GetInt(offset_) != 0; }
        }
        
        public PluginKey Value
        {
            get { return new PluginKey(data_, GetDataOffset()); }
        }
        
        public string PackageName
        {
            set { data_.SetUString(GetDataOffset() + 0, value); }
            
            get { return data_.GetUString(GetDataOffset() + 0); }
        }
        
        public int GetPackageNameLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 0);
        }
        
        public void SetPackageName(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 0, value, offset, count);
        }
        
        public void GetPackageName(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 0, value, offset);
        }
        
        public void ReadPackageName(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 0, stream, size);
        }
        
        public void WritePackageName(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 0, stream);
        }
        
        public string DescriptorId
        {
            set { data_.SetUString(GetDataOffset() + 8, value); }
            
            get { return data_.GetUString(GetDataOffset() + 8); }
        }
        
        public int GetDescriptorIdLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 8);
        }
        
        public void SetDescriptorId(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 8, value, offset, count);
        }
        
        public void GetDescriptorId(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 8, value, offset);
        }
        
        public void ReadDescriptorId(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 8, stream, size);
        }
        
        public void WriteDescriptorId(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 8, stream);
        }
        
        int GetDataOffset()
        {
            int dataOffset = data_.GetInt(offset_);
            
            if (dataOffset == 0)
                throw new Exception("Group is not allocated");
            
            return dataOffset;
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginKeyArray
    {
        public PluginKeyArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 16);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public PluginKey this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 16);
                
                return new PluginKey(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginKeyNullArray
    {
        public PluginKeyNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public PluginKeyNull this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                return new PluginKeyNull(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    enum PluginType
    {
        Indicator = 0,
        Robot = 1,
        Unknown = 2,
    }
    
    struct PluginTypeArray
    {
        public PluginTypeArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public PluginType this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetUInt(itemOffset, (uint) value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return (PluginType) data_.GetUInt(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginTypeNullArray
    {
        public PluginTypeNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public PluginType? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                if (value.HasValue)
                {
                    data_.SetUIntNull(itemOffset, (uint) value.Value);
                }
                else
                    data_.SetUIntNull(itemOffset, null);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                uint? value = data_.GetUIntNull(itemOffset);
                
                if (value.HasValue)
                    return (PluginType) value.Value;
                
                return null;
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginDescriptor
    {
        public PluginDescriptor(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public string ApiVersion
        {
            set { data_.SetUStringNull(offset_ + 0, value); }
            
            get { return data_.GetUStringNull(offset_ + 0); }
        }
        
        public int? GetApiVersionLength()
        {
            return data_.GetUStringNullLength(offset_ + 0);
        }
        
        public void SetApiVersion(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 0, value, offset, count);
        }
        
        public void GetApiVersion(char[] value, int offset)
        {
            data_.GetUString(offset_ + 0, value, offset);
        }
        
        public void ReadApiVersion(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 0, stream, size);
        }
        
        public void WriteApiVersion(Stream stream)
        {
            data_.WriteUString(offset_ + 0, stream);
        }
        
        public string Id
        {
            set { data_.SetUString(offset_ + 8, value); }
            
            get { return data_.GetUString(offset_ + 8); }
        }
        
        public int GetIdLength()
        {
            return data_.GetUStringLength(offset_ + 8);
        }
        
        public void SetId(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 8, value, offset, count);
        }
        
        public void GetId(char[] value, int offset)
        {
            data_.GetUString(offset_ + 8, value, offset);
        }
        
        public void ReadId(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 8, stream, size);
        }
        
        public void WriteId(Stream stream)
        {
            data_.WriteUString(offset_ + 8, stream);
        }
        
        public string DisplayName
        {
            set { data_.SetUString(offset_ + 16, value); }
            
            get { return data_.GetUString(offset_ + 16); }
        }
        
        public int GetDisplayNameLength()
        {
            return data_.GetUStringLength(offset_ + 16);
        }
        
        public void SetDisplayName(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 16, value, offset, count);
        }
        
        public void GetDisplayName(char[] value, int offset)
        {
            data_.GetUString(offset_ + 16, value, offset);
        }
        
        public void ReadDisplayName(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 16, stream, size);
        }
        
        public void WriteDisplayName(Stream stream)
        {
            data_.WriteUString(offset_ + 16, stream);
        }
        
        public string UserDisplayName
        {
            set { data_.SetUString(offset_ + 24, value); }
            
            get { return data_.GetUString(offset_ + 24); }
        }
        
        public int GetUserDisplayNameLength()
        {
            return data_.GetUStringLength(offset_ + 24);
        }
        
        public void SetUserDisplayName(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 24, value, offset, count);
        }
        
        public void GetUserDisplayName(char[] value, int offset)
        {
            data_.GetUString(offset_ + 24, value, offset);
        }
        
        public void ReadUserDisplayName(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 24, stream, size);
        }
        
        public void WriteUserDisplayName(Stream stream)
        {
            data_.WriteUString(offset_ + 24, stream);
        }
        
        public string Category
        {
            set { data_.SetUStringNull(offset_ + 32, value); }
            
            get { return data_.GetUStringNull(offset_ + 32); }
        }
        
        public int? GetCategoryLength()
        {
            return data_.GetUStringNullLength(offset_ + 32);
        }
        
        public void SetCategory(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 32, value, offset, count);
        }
        
        public void GetCategory(char[] value, int offset)
        {
            data_.GetUString(offset_ + 32, value, offset);
        }
        
        public void ReadCategory(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 32, stream, size);
        }
        
        public void WriteCategory(Stream stream)
        {
            data_.WriteUString(offset_ + 32, stream);
        }
        
        public string Version
        {
            set { data_.SetUStringNull(offset_ + 40, value); }
            
            get { return data_.GetUStringNull(offset_ + 40); }
        }
        
        public int? GetVersionLength()
        {
            return data_.GetUStringNullLength(offset_ + 40);
        }
        
        public void SetVersion(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 40, value, offset, count);
        }
        
        public void GetVersion(char[] value, int offset)
        {
            data_.GetUString(offset_ + 40, value, offset);
        }
        
        public void ReadVersion(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 40, stream, size);
        }
        
        public void WriteVersion(Stream stream)
        {
            data_.WriteUString(offset_ + 40, stream);
        }
        
        public string Description
        {
            set { data_.SetUStringNull(offset_ + 48, value); }
            
            get { return data_.GetUStringNull(offset_ + 48); }
        }
        
        public int? GetDescriptionLength()
        {
            return data_.GetUStringNullLength(offset_ + 48);
        }
        
        public void SetDescription(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 48, value, offset, count);
        }
        
        public void GetDescription(char[] value, int offset)
        {
            data_.GetUString(offset_ + 48, value, offset);
        }
        
        public void ReadDescription(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 48, stream, size);
        }
        
        public void WriteDescription(Stream stream)
        {
            data_.WriteUString(offset_ + 48, stream);
        }
        
        public string Copyright
        {
            set { data_.SetUStringNull(offset_ + 56, value); }
            
            get { return data_.GetUStringNull(offset_ + 56); }
        }
        
        public int? GetCopyrightLength()
        {
            return data_.GetUStringNullLength(offset_ + 56);
        }
        
        public void SetCopyright(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 56, value, offset, count);
        }
        
        public void GetCopyright(char[] value, int offset)
        {
            data_.GetUString(offset_ + 56, value, offset);
        }
        
        public void ReadCopyright(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 56, stream, size);
        }
        
        public void WriteCopyright(Stream stream)
        {
            data_.WriteUString(offset_ + 56, stream);
        }
        
        public PluginType Type
        {
            set { data_.SetUInt(offset_ + 64, (uint) value); }
            
            get { return (PluginType) data_.GetUInt(offset_ + 64); }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginDescriptorNull
    {
        public PluginDescriptorNull(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void New()
        {
            data_.NewGroup(offset_, 68);
        }
        
        public bool HasValue
        {
            get { return data_.GetInt(offset_) != 0; }
        }
        
        public PluginDescriptor Value
        {
            get { return new PluginDescriptor(data_, GetDataOffset()); }
        }
        
        public string ApiVersion
        {
            set { data_.SetUStringNull(GetDataOffset() + 0, value); }
            
            get { return data_.GetUStringNull(GetDataOffset() + 0); }
        }
        
        public int? GetApiVersionLength()
        {
            return data_.GetUStringNullLength(GetDataOffset() + 0);
        }
        
        public void SetApiVersion(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 0, value, offset, count);
        }
        
        public void GetApiVersion(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 0, value, offset);
        }
        
        public void ReadApiVersion(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 0, stream, size);
        }
        
        public void WriteApiVersion(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 0, stream);
        }
        
        public string Id
        {
            set { data_.SetUString(GetDataOffset() + 8, value); }
            
            get { return data_.GetUString(GetDataOffset() + 8); }
        }
        
        public int GetIdLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 8);
        }
        
        public void SetId(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 8, value, offset, count);
        }
        
        public void GetId(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 8, value, offset);
        }
        
        public void ReadId(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 8, stream, size);
        }
        
        public void WriteId(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 8, stream);
        }
        
        public string DisplayName
        {
            set { data_.SetUString(GetDataOffset() + 16, value); }
            
            get { return data_.GetUString(GetDataOffset() + 16); }
        }
        
        public int GetDisplayNameLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 16);
        }
        
        public void SetDisplayName(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 16, value, offset, count);
        }
        
        public void GetDisplayName(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 16, value, offset);
        }
        
        public void ReadDisplayName(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 16, stream, size);
        }
        
        public void WriteDisplayName(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 16, stream);
        }
        
        public string UserDisplayName
        {
            set { data_.SetUString(GetDataOffset() + 24, value); }
            
            get { return data_.GetUString(GetDataOffset() + 24); }
        }
        
        public int GetUserDisplayNameLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 24);
        }
        
        public void SetUserDisplayName(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 24, value, offset, count);
        }
        
        public void GetUserDisplayName(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 24, value, offset);
        }
        
        public void ReadUserDisplayName(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 24, stream, size);
        }
        
        public void WriteUserDisplayName(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 24, stream);
        }
        
        public string Category
        {
            set { data_.SetUStringNull(GetDataOffset() + 32, value); }
            
            get { return data_.GetUStringNull(GetDataOffset() + 32); }
        }
        
        public int? GetCategoryLength()
        {
            return data_.GetUStringNullLength(GetDataOffset() + 32);
        }
        
        public void SetCategory(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 32, value, offset, count);
        }
        
        public void GetCategory(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 32, value, offset);
        }
        
        public void ReadCategory(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 32, stream, size);
        }
        
        public void WriteCategory(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 32, stream);
        }
        
        public string Version
        {
            set { data_.SetUStringNull(GetDataOffset() + 40, value); }
            
            get { return data_.GetUStringNull(GetDataOffset() + 40); }
        }
        
        public int? GetVersionLength()
        {
            return data_.GetUStringNullLength(GetDataOffset() + 40);
        }
        
        public void SetVersion(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 40, value, offset, count);
        }
        
        public void GetVersion(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 40, value, offset);
        }
        
        public void ReadVersion(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 40, stream, size);
        }
        
        public void WriteVersion(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 40, stream);
        }
        
        public string Description
        {
            set { data_.SetUStringNull(GetDataOffset() + 48, value); }
            
            get { return data_.GetUStringNull(GetDataOffset() + 48); }
        }
        
        public int? GetDescriptionLength()
        {
            return data_.GetUStringNullLength(GetDataOffset() + 48);
        }
        
        public void SetDescription(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 48, value, offset, count);
        }
        
        public void GetDescription(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 48, value, offset);
        }
        
        public void ReadDescription(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 48, stream, size);
        }
        
        public void WriteDescription(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 48, stream);
        }
        
        public string Copyright
        {
            set { data_.SetUStringNull(GetDataOffset() + 56, value); }
            
            get { return data_.GetUStringNull(GetDataOffset() + 56); }
        }
        
        public int? GetCopyrightLength()
        {
            return data_.GetUStringNullLength(GetDataOffset() + 56);
        }
        
        public void SetCopyright(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 56, value, offset, count);
        }
        
        public void GetCopyright(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 56, value, offset);
        }
        
        public void ReadCopyright(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 56, stream, size);
        }
        
        public void WriteCopyright(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 56, stream);
        }
        
        public PluginType Type
        {
            set { data_.SetUInt(GetDataOffset() + 64, (uint) value); }
            
            get { return (PluginType) data_.GetUInt(GetDataOffset() + 64); }
        }
        
        int GetDataOffset()
        {
            int dataOffset = data_.GetInt(offset_);
            
            if (dataOffset == 0)
                throw new Exception("Group is not allocated");
            
            return dataOffset;
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginDescriptorArray
    {
        public PluginDescriptorArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 68);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public PluginDescriptor this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 68);
                
                return new PluginDescriptor(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginDescriptorNullArray
    {
        public PluginDescriptorNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public PluginDescriptorNull this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                return new PluginDescriptorNull(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginInfo
    {
        public PluginInfo(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public PluginKey Key
        {
            get { return new PluginKey(data_, offset_ + 0); }
        }
        
        public PluginDescriptor Descriptor
        {
            get { return new PluginDescriptor(data_, offset_ + 16); }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginInfoNull
    {
        public PluginInfoNull(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void New()
        {
            data_.NewGroup(offset_, 84);
        }
        
        public bool HasValue
        {
            get { return data_.GetInt(offset_) != 0; }
        }
        
        public PluginInfo Value
        {
            get { return new PluginInfo(data_, GetDataOffset()); }
        }
        
        public PluginKey Key
        {
            get { return new PluginKey(data_, GetDataOffset() + 0); }
        }
        
        public PluginDescriptor Descriptor
        {
            get { return new PluginDescriptor(data_, GetDataOffset() + 16); }
        }
        
        int GetDataOffset()
        {
            int dataOffset = data_.GetInt(offset_);
            
            if (dataOffset == 0)
                throw new Exception("Group is not allocated");
            
            return dataOffset;
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginInfoArray
    {
        public PluginInfoArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 84);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public PluginInfo this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 84);
                
                return new PluginInfo(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginInfoNullArray
    {
        public PluginInfoNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public PluginInfoNull this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                return new PluginInfoNull(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PackageModel
    {
        public PackageModel(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public string Name
        {
            set { data_.SetUString(offset_ + 0, value); }
            
            get { return data_.GetUString(offset_ + 0); }
        }
        
        public int GetNameLength()
        {
            return data_.GetUStringLength(offset_ + 0);
        }
        
        public void SetName(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 0, value, offset, count);
        }
        
        public void GetName(char[] value, int offset)
        {
            data_.GetUString(offset_ + 0, value, offset);
        }
        
        public void ReadName(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 0, stream, size);
        }
        
        public void WriteName(Stream stream)
        {
            data_.WriteUString(offset_ + 0, stream);
        }
        
        public DateTime Created
        {
            set { data_.SetDateTime(offset_ + 8, value); }
            
            get { return data_.GetDateTime(offset_ + 8); }
        }
        
        public PluginInfoArray Plugins
        {
            get { return new PluginInfoArray(data_, offset_ + 16); }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PackageModelNull
    {
        public PackageModelNull(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void New()
        {
            data_.NewGroup(offset_, 24);
        }
        
        public bool HasValue
        {
            get { return data_.GetInt(offset_) != 0; }
        }
        
        public PackageModel Value
        {
            get { return new PackageModel(data_, GetDataOffset()); }
        }
        
        public string Name
        {
            set { data_.SetUString(GetDataOffset() + 0, value); }
            
            get { return data_.GetUString(GetDataOffset() + 0); }
        }
        
        public int GetNameLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 0);
        }
        
        public void SetName(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 0, value, offset, count);
        }
        
        public void GetName(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 0, value, offset);
        }
        
        public void ReadName(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 0, stream, size);
        }
        
        public void WriteName(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 0, stream);
        }
        
        public DateTime Created
        {
            set { data_.SetDateTime(GetDataOffset() + 8, value); }
            
            get { return data_.GetDateTime(GetDataOffset() + 8); }
        }
        
        public PluginInfoArray Plugins
        {
            get { return new PluginInfoArray(data_, GetDataOffset() + 16); }
        }
        
        int GetDataOffset()
        {
            int dataOffset = data_.GetInt(offset_);
            
            if (dataOffset == 0)
                throw new Exception("Group is not allocated");
            
            return dataOffset;
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PackageModelArray
    {
        public PackageModelArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 24);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public PackageModel this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 24);
                
                return new PackageModel(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PackageModelNullArray
    {
        public PackageModelNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public PackageModelNull this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                return new PackageModelNull(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct AccountKey
    {
        public AccountKey(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public string Login
        {
            set { data_.SetUString(offset_ + 0, value); }
            
            get { return data_.GetUString(offset_ + 0); }
        }
        
        public int GetLoginLength()
        {
            return data_.GetUStringLength(offset_ + 0);
        }
        
        public void SetLogin(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 0, value, offset, count);
        }
        
        public void GetLogin(char[] value, int offset)
        {
            data_.GetUString(offset_ + 0, value, offset);
        }
        
        public void ReadLogin(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 0, stream, size);
        }
        
        public void WriteLogin(Stream stream)
        {
            data_.WriteUString(offset_ + 0, stream);
        }
        
        public string Server
        {
            set { data_.SetUString(offset_ + 8, value); }
            
            get { return data_.GetUString(offset_ + 8); }
        }
        
        public int GetServerLength()
        {
            return data_.GetUStringLength(offset_ + 8);
        }
        
        public void SetServer(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 8, value, offset, count);
        }
        
        public void GetServer(char[] value, int offset)
        {
            data_.GetUString(offset_ + 8, value, offset);
        }
        
        public void ReadServer(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 8, stream, size);
        }
        
        public void WriteServer(Stream stream)
        {
            data_.WriteUString(offset_ + 8, stream);
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct AccountKeyNull
    {
        public AccountKeyNull(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void New()
        {
            data_.NewGroup(offset_, 16);
        }
        
        public bool HasValue
        {
            get { return data_.GetInt(offset_) != 0; }
        }
        
        public AccountKey Value
        {
            get { return new AccountKey(data_, GetDataOffset()); }
        }
        
        public string Login
        {
            set { data_.SetUString(GetDataOffset() + 0, value); }
            
            get { return data_.GetUString(GetDataOffset() + 0); }
        }
        
        public int GetLoginLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 0);
        }
        
        public void SetLogin(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 0, value, offset, count);
        }
        
        public void GetLogin(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 0, value, offset);
        }
        
        public void ReadLogin(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 0, stream, size);
        }
        
        public void WriteLogin(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 0, stream);
        }
        
        public string Server
        {
            set { data_.SetUString(GetDataOffset() + 8, value); }
            
            get { return data_.GetUString(GetDataOffset() + 8); }
        }
        
        public int GetServerLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 8);
        }
        
        public void SetServer(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 8, value, offset, count);
        }
        
        public void GetServer(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 8, value, offset);
        }
        
        public void ReadServer(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 8, stream, size);
        }
        
        public void WriteServer(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 8, stream);
        }
        
        int GetDataOffset()
        {
            int dataOffset = data_.GetInt(offset_);
            
            if (dataOffset == 0)
                throw new Exception("Group is not allocated");
            
            return dataOffset;
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct AccountKeyArray
    {
        public AccountKeyArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 16);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public AccountKey this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 16);
                
                return new AccountKey(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct AccountKeyNullArray
    {
        public AccountKeyNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public AccountKeyNull this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                return new AccountKeyNull(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginPermissions
    {
        public PluginPermissions(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public bool TradeAllowed
        {
            set { data_.SetBool(offset_ + 0, value); }
            
            get { return data_.GetBool(offset_ + 0); }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginPermissionsNull
    {
        public PluginPermissionsNull(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void New()
        {
            data_.NewGroup(offset_, 1);
        }
        
        public bool HasValue
        {
            get { return data_.GetInt(offset_) != 0; }
        }
        
        public PluginPermissions Value
        {
            get { return new PluginPermissions(data_, GetDataOffset()); }
        }
        
        public bool TradeAllowed
        {
            set { data_.SetBool(GetDataOffset() + 0, value); }
            
            get { return data_.GetBool(GetDataOffset() + 0); }
        }
        
        int GetDataOffset()
        {
            int dataOffset = data_.GetInt(offset_);
            
            if (dataOffset == 0)
                throw new Exception("Group is not allocated");
            
            return dataOffset;
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginPermissionsArray
    {
        public PluginPermissionsArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 1);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public PluginPermissions this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 1);
                
                return new PluginPermissions(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct PluginPermissionsNullArray
    {
        public PluginPermissionsNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public PluginPermissionsNull this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                return new PluginPermissionsNull(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    enum BotState
    {
        Offline = 0,
        Starting = 1,
        Faulted = 2,
        Online = 3,
        Stopping = 4,
        Broken = 5,
        Reconnecting = 6,
    }
    
    struct BotStateArray
    {
        public BotStateArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public BotState this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetUInt(itemOffset, (uint) value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return (BotState) data_.GetUInt(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct BotStateNullArray
    {
        public BotStateNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public BotState? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                if (value.HasValue)
                {
                    data_.SetUIntNull(itemOffset, (uint) value.Value);
                }
                else
                    data_.SetUIntNull(itemOffset, null);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                uint? value = data_.GetUIntNull(itemOffset);
                
                if (value.HasValue)
                    return (BotState) value.Value;
                
                return null;
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct BotModel
    {
        public BotModel(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public string InstanceId
        {
            set { data_.SetUString(offset_ + 0, value); }
            
            get { return data_.GetUString(offset_ + 0); }
        }
        
        public int GetInstanceIdLength()
        {
            return data_.GetUStringLength(offset_ + 0);
        }
        
        public void SetInstanceId(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 0, value, offset, count);
        }
        
        public void GetInstanceId(char[] value, int offset)
        {
            data_.GetUString(offset_ + 0, value, offset);
        }
        
        public void ReadInstanceId(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 0, stream, size);
        }
        
        public void WriteInstanceId(Stream stream)
        {
            data_.WriteUString(offset_ + 0, stream);
        }
        
        public bool Isolated
        {
            set { data_.SetBool(offset_ + 8, value); }
            
            get { return data_.GetBool(offset_ + 8); }
        }
        
        public BotState State
        {
            set { data_.SetUInt(offset_ + 9, (uint) value); }
            
            get { return (BotState) data_.GetUInt(offset_ + 9); }
        }
        
        public PluginPermissions Permissions
        {
            get { return new PluginPermissions(data_, offset_ + 13); }
        }
        
        public AccountKey Account
        {
            get { return new AccountKey(data_, offset_ + 14); }
        }
        
        public PluginKey Plugin
        {
            get { return new PluginKey(data_, offset_ + 30); }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct BotModelNull
    {
        public BotModelNull(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void New()
        {
            data_.NewGroup(offset_, 46);
        }
        
        public bool HasValue
        {
            get { return data_.GetInt(offset_) != 0; }
        }
        
        public BotModel Value
        {
            get { return new BotModel(data_, GetDataOffset()); }
        }
        
        public string InstanceId
        {
            set { data_.SetUString(GetDataOffset() + 0, value); }
            
            get { return data_.GetUString(GetDataOffset() + 0); }
        }
        
        public int GetInstanceIdLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 0);
        }
        
        public void SetInstanceId(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 0, value, offset, count);
        }
        
        public void GetInstanceId(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 0, value, offset);
        }
        
        public void ReadInstanceId(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 0, stream, size);
        }
        
        public void WriteInstanceId(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 0, stream);
        }
        
        public bool Isolated
        {
            set { data_.SetBool(GetDataOffset() + 8, value); }
            
            get { return data_.GetBool(GetDataOffset() + 8); }
        }
        
        public BotState State
        {
            set { data_.SetUInt(GetDataOffset() + 9, (uint) value); }
            
            get { return (BotState) data_.GetUInt(GetDataOffset() + 9); }
        }
        
        public PluginPermissions Permissions
        {
            get { return new PluginPermissions(data_, GetDataOffset() + 13); }
        }
        
        public AccountKey Account
        {
            get { return new AccountKey(data_, GetDataOffset() + 14); }
        }
        
        public PluginKey Plugin
        {
            get { return new PluginKey(data_, GetDataOffset() + 30); }
        }
        
        int GetDataOffset()
        {
            int dataOffset = data_.GetInt(offset_);
            
            if (dataOffset == 0)
                throw new Exception("Group is not allocated");
            
            return dataOffset;
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct BotModelArray
    {
        public BotModelArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 46);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public BotModel this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 46);
                
                return new BotModel(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct BotModelNullArray
    {
        public BotModelNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public BotModelNull this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                return new BotModelNull(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    enum ConnectionState
    {
        Offline = 0,
        Connecting = 1,
        Online = 2,
        Disconnecting = 3,
    }
    
    struct ConnectionStateArray
    {
        public ConnectionStateArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public ConnectionState this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetUInt(itemOffset, (uint) value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return (ConnectionState) data_.GetUInt(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct ConnectionStateNullArray
    {
        public ConnectionStateNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public ConnectionState? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                if (value.HasValue)
                {
                    data_.SetUIntNull(itemOffset, (uint) value.Value);
                }
                else
                    data_.SetUIntNull(itemOffset, null);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                uint? value = data_.GetUIntNull(itemOffset);
                
                if (value.HasValue)
                    return (ConnectionState) value.Value;
                
                return null;
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    enum ConnectionErrorCode
    {
        None = 0,
        Unknown = 1,
        NetworkError = 2,
        Timeout = 3,
        BlockedAccount = 4,
        ClientInitiated = 5,
        InvalidCredentials = 6,
        SlowConnection = 7,
        ServerError = 8,
        LoginDeleted = 9,
        ServerLogout = 10,
        Canceled = 11,
    }
    
    struct ConnectionErrorCodeArray
    {
        public ConnectionErrorCodeArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public ConnectionErrorCode this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                data_.SetUInt(itemOffset, (uint) value);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                return (ConnectionErrorCode) data_.GetUInt(itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct ConnectionErrorCodeNullArray
    {
        public ConnectionErrorCodeNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public ConnectionErrorCode? this[int index]
        {
            set
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                if (value.HasValue)
                {
                    data_.SetUIntNull(itemOffset, (uint) value.Value);
                }
                else
                    data_.SetUIntNull(itemOffset, null);
            }
            
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                uint? value = data_.GetUIntNull(itemOffset);
                
                if (value.HasValue)
                    return (ConnectionErrorCode) value.Value;
                
                return null;
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct ConnectionErrorModel
    {
        public ConnectionErrorModel(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public ConnectionErrorCode Code
        {
            set { data_.SetUInt(offset_ + 0, (uint) value); }
            
            get { return (ConnectionErrorCode) data_.GetUInt(offset_ + 0); }
        }
        
        public string Text
        {
            set { data_.SetUStringNull(offset_ + 4, value); }
            
            get { return data_.GetUStringNull(offset_ + 4); }
        }
        
        public int? GetTextLength()
        {
            return data_.GetUStringNullLength(offset_ + 4);
        }
        
        public void SetText(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 4, value, offset, count);
        }
        
        public void GetText(char[] value, int offset)
        {
            data_.GetUString(offset_ + 4, value, offset);
        }
        
        public void ReadText(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 4, stream, size);
        }
        
        public void WriteText(Stream stream)
        {
            data_.WriteUString(offset_ + 4, stream);
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct ConnectionErrorModelNull
    {
        public ConnectionErrorModelNull(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void New()
        {
            data_.NewGroup(offset_, 12);
        }
        
        public bool HasValue
        {
            get { return data_.GetInt(offset_) != 0; }
        }
        
        public ConnectionErrorModel Value
        {
            get { return new ConnectionErrorModel(data_, GetDataOffset()); }
        }
        
        public ConnectionErrorCode Code
        {
            set { data_.SetUInt(GetDataOffset() + 0, (uint) value); }
            
            get { return (ConnectionErrorCode) data_.GetUInt(GetDataOffset() + 0); }
        }
        
        public string Text
        {
            set { data_.SetUStringNull(GetDataOffset() + 4, value); }
            
            get { return data_.GetUStringNull(GetDataOffset() + 4); }
        }
        
        public int? GetTextLength()
        {
            return data_.GetUStringNullLength(GetDataOffset() + 4);
        }
        
        public void SetText(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 4, value, offset, count);
        }
        
        public void GetText(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 4, value, offset);
        }
        
        public void ReadText(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 4, stream, size);
        }
        
        public void WriteText(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 4, stream);
        }
        
        int GetDataOffset()
        {
            int dataOffset = data_.GetInt(offset_);
            
            if (dataOffset == 0)
                throw new Exception("Group is not allocated");
            
            return dataOffset;
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct ConnectionErrorModelArray
    {
        public ConnectionErrorModelArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 12);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public ConnectionErrorModel this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 12);
                
                return new ConnectionErrorModel(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct ConnectionErrorModelNullArray
    {
        public ConnectionErrorModelNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public ConnectionErrorModelNull this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                return new ConnectionErrorModelNull(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct AccountModel_1
    {
        public AccountModel_1(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public string Login
        {
            set { data_.SetUString(offset_ + 0, value); }
            
            get { return data_.GetUString(offset_ + 0); }
        }
        
        public int GetLoginLength()
        {
            return data_.GetUStringLength(offset_ + 0);
        }
        
        public void SetLogin(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 0, value, offset, count);
        }
        
        public void GetLogin(char[] value, int offset)
        {
            data_.GetUString(offset_ + 0, value, offset);
        }
        
        public void ReadLogin(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 0, stream, size);
        }
        
        public void WriteLogin(Stream stream)
        {
            data_.WriteUString(offset_ + 0, stream);
        }
        
        public string Server
        {
            set { data_.SetUString(offset_ + 8, value); }
            
            get { return data_.GetUString(offset_ + 8); }
        }
        
        public int GetServerLength()
        {
            return data_.GetUStringLength(offset_ + 8);
        }
        
        public void SetServer(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 8, value, offset, count);
        }
        
        public void GetServer(char[] value, int offset)
        {
            data_.GetUString(offset_ + 8, value, offset);
        }
        
        public void ReadServer(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 8, stream, size);
        }
        
        public void WriteServer(Stream stream)
        {
            data_.WriteUString(offset_ + 8, stream);
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct AccountModel_1Null
    {
        public AccountModel_1Null(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void New()
        {
            data_.NewGroup(offset_, 16);
        }
        
        public bool HasValue
        {
            get { return data_.GetInt(offset_) != 0; }
        }
        
        public AccountModel_1 Value
        {
            get { return new AccountModel_1(data_, GetDataOffset()); }
        }
        
        public string Login
        {
            set { data_.SetUString(GetDataOffset() + 0, value); }
            
            get { return data_.GetUString(GetDataOffset() + 0); }
        }
        
        public int GetLoginLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 0);
        }
        
        public void SetLogin(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 0, value, offset, count);
        }
        
        public void GetLogin(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 0, value, offset);
        }
        
        public void ReadLogin(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 0, stream, size);
        }
        
        public void WriteLogin(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 0, stream);
        }
        
        public string Server
        {
            set { data_.SetUString(GetDataOffset() + 8, value); }
            
            get { return data_.GetUString(GetDataOffset() + 8); }
        }
        
        public int GetServerLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 8);
        }
        
        public void SetServer(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 8, value, offset, count);
        }
        
        public void GetServer(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 8, value, offset);
        }
        
        public void ReadServer(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 8, stream, size);
        }
        
        public void WriteServer(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 8, stream);
        }
        
        int GetDataOffset()
        {
            int dataOffset = data_.GetInt(offset_);
            
            if (dataOffset == 0)
                throw new Exception("Group is not allocated");
            
            return dataOffset;
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct AccountModel_1Array
    {
        public AccountModel_1Array(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 16);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public AccountModel_1 this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 16);
                
                return new AccountModel_1(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct AccountModel_1NullArray
    {
        public AccountModel_1NullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public AccountModel_1Null this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                return new AccountModel_1Null(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct AccountModel
    {
        public AccountModel(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public string Login
        {
            set { data_.SetUString(offset_ + 0, value); }
            
            get { return data_.GetUString(offset_ + 0); }
        }
        
        public int GetLoginLength()
        {
            return data_.GetUStringLength(offset_ + 0);
        }
        
        public void SetLogin(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 0, value, offset, count);
        }
        
        public void GetLogin(char[] value, int offset)
        {
            data_.GetUString(offset_ + 0, value, offset);
        }
        
        public void ReadLogin(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 0, stream, size);
        }
        
        public void WriteLogin(Stream stream)
        {
            data_.WriteUString(offset_ + 0, stream);
        }
        
        public string Server
        {
            set { data_.SetUString(offset_ + 8, value); }
            
            get { return data_.GetUString(offset_ + 8); }
        }
        
        public int GetServerLength()
        {
            return data_.GetUStringLength(offset_ + 8);
        }
        
        public void SetServer(char[] value, int offset, int count)
        {
            data_.SetUString(offset_ + 8, value, offset, count);
        }
        
        public void GetServer(char[] value, int offset)
        {
            data_.GetUString(offset_ + 8, value, offset);
        }
        
        public void ReadServer(Stream stream, int size)
        {
            data_.ReadUString(offset_ + 8, stream, size);
        }
        
        public void WriteServer(Stream stream)
        {
            data_.WriteUString(offset_ + 8, stream);
        }
        
        public bool UseNewProtocol
        {
            set { data_.SetBool(offset_ + 16, value); }
            
            get { return data_.GetBool(offset_ + 16); }
        }
        
        public ConnectionState ConnectionState
        {
            set { data_.SetUInt(offset_ + 17, (uint) value); }
            
            get { return (ConnectionState) data_.GetUInt(offset_ + 17); }
        }
        
        public ConnectionErrorModel LastError
        {
            get { return new ConnectionErrorModel(data_, offset_ + 21); }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct AccountModelNull
    {
        public AccountModelNull(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void New()
        {
            data_.NewGroup(offset_, 33);
        }
        
        public bool HasValue
        {
            get { return data_.GetInt(offset_) != 0; }
        }
        
        public AccountModel Value
        {
            get { return new AccountModel(data_, GetDataOffset()); }
        }
        
        public string Login
        {
            set { data_.SetUString(GetDataOffset() + 0, value); }
            
            get { return data_.GetUString(GetDataOffset() + 0); }
        }
        
        public int GetLoginLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 0);
        }
        
        public void SetLogin(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 0, value, offset, count);
        }
        
        public void GetLogin(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 0, value, offset);
        }
        
        public void ReadLogin(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 0, stream, size);
        }
        
        public void WriteLogin(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 0, stream);
        }
        
        public string Server
        {
            set { data_.SetUString(GetDataOffset() + 8, value); }
            
            get { return data_.GetUString(GetDataOffset() + 8); }
        }
        
        public int GetServerLength()
        {
            return data_.GetUStringLength(GetDataOffset() + 8);
        }
        
        public void SetServer(char[] value, int offset, int count)
        {
            data_.SetUString(GetDataOffset() + 8, value, offset, count);
        }
        
        public void GetServer(char[] value, int offset)
        {
            data_.GetUString(GetDataOffset() + 8, value, offset);
        }
        
        public void ReadServer(Stream stream, int size)
        {
            data_.ReadUString(GetDataOffset() + 8, stream, size);
        }
        
        public void WriteServer(Stream stream)
        {
            data_.WriteUString(GetDataOffset() + 8, stream);
        }
        
        public bool UseNewProtocol
        {
            set { data_.SetBool(GetDataOffset() + 16, value); }
            
            get { return data_.GetBool(GetDataOffset() + 16); }
        }
        
        public ConnectionState ConnectionState
        {
            set { data_.SetUInt(GetDataOffset() + 17, (uint) value); }
            
            get { return (ConnectionState) data_.GetUInt(GetDataOffset() + 17); }
        }
        
        public ConnectionErrorModel LastError
        {
            get { return new ConnectionErrorModel(data_, GetDataOffset() + 21); }
        }
        
        int GetDataOffset()
        {
            int dataOffset = data_.GetInt(offset_);
            
            if (dataOffset == 0)
                throw new Exception("Group is not allocated");
            
            return dataOffset;
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct AccountModelArray
    {
        public AccountModelArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 33);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public AccountModel this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 33);
                
                return new AccountModel(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct AccountModelNullArray
    {
        public AccountModelNullArray(MessageData data, int offset)
        {
            data_ = data;
            offset_ = offset;
        }
        
        public void Resize(int length)
        {
            data_.ResizeArray(offset_, length, 4);
        }
        
        public int Length
        {
            get { return data_.GetArrayLength(offset_); }
        }
        
        public AccountModelNull this[int index]
        {
            get
            {
                int itemOffset = data_.GetArrayItemOffset(offset_, index, 4);
                
                return new AccountModelNull(data_, itemOffset);
            }
        }
        
        MessageData data_;
        int offset_;
    }
    
    struct AccountListRequest
    {
        public static implicit operator Request(AccountListRequest message)
        {
            return new Request(message.Info, message.Data);
        }
        
        public static implicit operator Message(AccountListRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public AccountListRequest(int i)
        {
            info_ = BotAgent.Info.AccountListRequest;
            data_ = new MessageData(16);
            
            data_.SetInt(4, 8);
        }
        
        public AccountListRequest(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.AccountListRequest))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string Id
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public AccountListRequest Clone()
        {
            MessageData data = data_.Clone();
            
            return new AccountListRequest(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct AccountListReport_1
    {
        public static implicit operator Report(AccountListReport_1 message)
        {
            return new Report(message.Info, message.Data);
        }
        
        public static implicit operator Message(AccountListReport_1 message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public AccountListReport_1(int i)
        {
            info_ = BotAgent.Info.AccountListReport_1;
            data_ = new MessageData(36);
            
            data_.SetInt(4, 9);
        }
        
        public AccountListReport_1(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.AccountListReport_1))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string RequestId
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetRequestIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetRequestId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetRequestId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadRequestId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteRequestId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public RequestExecState RequestState
        {
            set { data_.SetUInt(16, (uint) value); }
            
            get { return (RequestExecState) data_.GetUInt(16); }
        }
        
        public string Text
        {
            set { data_.SetUStringNull(20, value); }
            
            get { return data_.GetUStringNull(20); }
        }
        
        public int? GetTextLength()
        {
            return data_.GetUStringNullLength(20);
        }
        
        public void SetText(char[] value, int offset, int count)
        {
            data_.SetUString(20, value, offset, count);
        }
        
        public void GetText(char[] value, int offset)
        {
            data_.GetUString(20, value, offset);
        }
        
        public void ReadText(Stream stream, int size)
        {
            data_.ReadUString(20, stream, size);
        }
        
        public void WriteText(Stream stream)
        {
            data_.WriteUString(20, stream);
        }
        
        public AccountModel_1Array Accounts
        {
            get { return new AccountModel_1Array(data_, 28); }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public AccountListReport_1 Clone()
        {
            MessageData data = data_.Clone();
            
            return new AccountListReport_1(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct AccountListReport
    {
        public static implicit operator Report(AccountListReport message)
        {
            return new Report(message.Info, message.Data);
        }
        
        public static implicit operator Message(AccountListReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public AccountListReport(int i)
        {
            info_ = BotAgent.Info.AccountListReport;
            data_ = new MessageData(36);
            
            data_.SetInt(4, 21);
        }
        
        public AccountListReport(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.AccountListReport))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string RequestId
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetRequestIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetRequestId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetRequestId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadRequestId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteRequestId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public RequestExecState RequestState
        {
            set { data_.SetUInt(16, (uint) value); }
            
            get { return (RequestExecState) data_.GetUInt(16); }
        }
        
        public string Text
        {
            set { data_.SetUStringNull(20, value); }
            
            get { return data_.GetUStringNull(20); }
        }
        
        public int? GetTextLength()
        {
            return data_.GetUStringNullLength(20);
        }
        
        public void SetText(char[] value, int offset, int count)
        {
            data_.SetUString(20, value, offset, count);
        }
        
        public void GetText(char[] value, int offset)
        {
            data_.GetUString(20, value, offset);
        }
        
        public void ReadText(Stream stream, int size)
        {
            data_.ReadUString(20, stream, size);
        }
        
        public void WriteText(Stream stream)
        {
            data_.WriteUString(20, stream);
        }
        
        public AccountModelArray Accounts
        {
            get { return new AccountModelArray(data_, 28); }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public AccountListReport Clone()
        {
            MessageData data = data_.Clone();
            
            return new AccountListReport(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct BotListRequest
    {
        public static implicit operator Request(BotListRequest message)
        {
            return new Request(message.Info, message.Data);
        }
        
        public static implicit operator Message(BotListRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public BotListRequest(int i)
        {
            info_ = BotAgent.Info.BotListRequest;
            data_ = new MessageData(16);
            
            data_.SetInt(4, 10);
        }
        
        public BotListRequest(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.BotListRequest))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string Id
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public BotListRequest Clone()
        {
            MessageData data = data_.Clone();
            
            return new BotListRequest(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct BotListReport
    {
        public static implicit operator Report(BotListReport message)
        {
            return new Report(message.Info, message.Data);
        }
        
        public static implicit operator Message(BotListReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public BotListReport(int i)
        {
            info_ = BotAgent.Info.BotListReport;
            data_ = new MessageData(36);
            
            data_.SetInt(4, 11);
        }
        
        public BotListReport(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.BotListReport))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string RequestId
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetRequestIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetRequestId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetRequestId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadRequestId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteRequestId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public RequestExecState RequestState
        {
            set { data_.SetUInt(16, (uint) value); }
            
            get { return (RequestExecState) data_.GetUInt(16); }
        }
        
        public string Text
        {
            set { data_.SetUStringNull(20, value); }
            
            get { return data_.GetUStringNull(20); }
        }
        
        public int? GetTextLength()
        {
            return data_.GetUStringNullLength(20);
        }
        
        public void SetText(char[] value, int offset, int count)
        {
            data_.SetUString(20, value, offset, count);
        }
        
        public void GetText(char[] value, int offset)
        {
            data_.GetUString(20, value, offset);
        }
        
        public void ReadText(Stream stream, int size)
        {
            data_.ReadUString(20, stream, size);
        }
        
        public void WriteText(Stream stream)
        {
            data_.WriteUString(20, stream);
        }
        
        public BotModelArray Bots
        {
            get { return new BotModelArray(data_, 28); }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public BotListReport Clone()
        {
            MessageData data = data_.Clone();
            
            return new BotListReport(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct PackageListRequest
    {
        public static implicit operator Request(PackageListRequest message)
        {
            return new Request(message.Info, message.Data);
        }
        
        public static implicit operator Message(PackageListRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public PackageListRequest(int i)
        {
            info_ = BotAgent.Info.PackageListRequest;
            data_ = new MessageData(16);
            
            data_.SetInt(4, 12);
        }
        
        public PackageListRequest(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.PackageListRequest))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string Id
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public PackageListRequest Clone()
        {
            MessageData data = data_.Clone();
            
            return new PackageListRequest(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct PackageListReport
    {
        public static implicit operator Report(PackageListReport message)
        {
            return new Report(message.Info, message.Data);
        }
        
        public static implicit operator Message(PackageListReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public PackageListReport(int i)
        {
            info_ = BotAgent.Info.PackageListReport;
            data_ = new MessageData(36);
            
            data_.SetInt(4, 13);
        }
        
        public PackageListReport(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.PackageListReport))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string RequestId
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetRequestIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetRequestId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetRequestId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadRequestId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteRequestId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public RequestExecState RequestState
        {
            set { data_.SetUInt(16, (uint) value); }
            
            get { return (RequestExecState) data_.GetUInt(16); }
        }
        
        public string Text
        {
            set { data_.SetUStringNull(20, value); }
            
            get { return data_.GetUStringNull(20); }
        }
        
        public int? GetTextLength()
        {
            return data_.GetUStringNullLength(20);
        }
        
        public void SetText(char[] value, int offset, int count)
        {
            data_.SetUString(20, value, offset, count);
        }
        
        public void GetText(char[] value, int offset)
        {
            data_.GetUString(20, value, offset);
        }
        
        public void ReadText(Stream stream, int size)
        {
            data_.ReadUString(20, stream, size);
        }
        
        public void WriteText(Stream stream)
        {
            data_.WriteUString(20, stream);
        }
        
        public PackageModelArray Packages
        {
            get { return new PackageModelArray(data_, 28); }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public PackageListReport Clone()
        {
            MessageData data = data_.Clone();
            
            return new PackageListReport(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct SubscribeRequest
    {
        public static implicit operator Request(SubscribeRequest message)
        {
            return new Request(message.Info, message.Data);
        }
        
        public static implicit operator Message(SubscribeRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public SubscribeRequest(int i)
        {
            info_ = BotAgent.Info.SubscribeRequest;
            data_ = new MessageData(16);
            
            data_.SetInt(4, 14);
        }
        
        public SubscribeRequest(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.SubscribeRequest))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string Id
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public SubscribeRequest Clone()
        {
            MessageData data = data_.Clone();
            
            return new SubscribeRequest(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct SubscribeReport
    {
        public static implicit operator Report(SubscribeReport message)
        {
            return new Report(message.Info, message.Data);
        }
        
        public static implicit operator Message(SubscribeReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public SubscribeReport(int i)
        {
            info_ = BotAgent.Info.SubscribeReport;
            data_ = new MessageData(28);
            
            data_.SetInt(4, 15);
        }
        
        public SubscribeReport(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.SubscribeReport))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string RequestId
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetRequestIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetRequestId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetRequestId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadRequestId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteRequestId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public RequestExecState RequestState
        {
            set { data_.SetUInt(16, (uint) value); }
            
            get { return (RequestExecState) data_.GetUInt(16); }
        }
        
        public string Text
        {
            set { data_.SetUStringNull(20, value); }
            
            get { return data_.GetUStringNull(20); }
        }
        
        public int? GetTextLength()
        {
            return data_.GetUStringNullLength(20);
        }
        
        public void SetText(char[] value, int offset, int count)
        {
            data_.SetUString(20, value, offset, count);
        }
        
        public void GetText(char[] value, int offset)
        {
            data_.GetUString(20, value, offset);
        }
        
        public void ReadText(Stream stream, int size)
        {
            data_.ReadUString(20, stream, size);
        }
        
        public void WriteText(Stream stream)
        {
            data_.WriteUString(20, stream);
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public SubscribeReport Clone()
        {
            MessageData data = data_.Clone();
            
            return new SubscribeReport(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct AccountModelUpdate_1
    {
        public static implicit operator Update(AccountModelUpdate_1 message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static implicit operator Message(AccountModelUpdate_1 message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public AccountModelUpdate_1(int i)
        {
            info_ = BotAgent.Info.AccountModelUpdate_1;
            data_ = new MessageData(36);
            
            data_.SetInt(4, 16);
        }
        
        public AccountModelUpdate_1(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.AccountModelUpdate_1))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string Id
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public UpdateType Type
        {
            set { data_.SetUInt(16, (uint) value); }
            
            get { return (UpdateType) data_.GetUInt(16); }
        }
        
        public AccountModel_1 Item
        {
            get { return new AccountModel_1(data_, 20); }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public AccountModelUpdate_1 Clone()
        {
            MessageData data = data_.Clone();
            
            return new AccountModelUpdate_1(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct AccountModelUpdate
    {
        public static implicit operator Update(AccountModelUpdate message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static implicit operator Message(AccountModelUpdate message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public AccountModelUpdate(int i)
        {
            info_ = BotAgent.Info.AccountModelUpdate;
            data_ = new MessageData(53);
            
            data_.SetInt(4, 22);
        }
        
        public AccountModelUpdate(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.AccountModelUpdate))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string Id
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public UpdateType Type
        {
            set { data_.SetUInt(16, (uint) value); }
            
            get { return (UpdateType) data_.GetUInt(16); }
        }
        
        public AccountModel Item
        {
            get { return new AccountModel(data_, 20); }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public AccountModelUpdate Clone()
        {
            MessageData data = data_.Clone();
            
            return new AccountModelUpdate(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct BotModelUpdate
    {
        public static implicit operator Update(BotModelUpdate message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static implicit operator Message(BotModelUpdate message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public BotModelUpdate(int i)
        {
            info_ = BotAgent.Info.BotModelUpdate;
            data_ = new MessageData(66);
            
            data_.SetInt(4, 17);
        }
        
        public BotModelUpdate(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.BotModelUpdate))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string Id
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public UpdateType Type
        {
            set { data_.SetUInt(16, (uint) value); }
            
            get { return (UpdateType) data_.GetUInt(16); }
        }
        
        public BotModel Item
        {
            get { return new BotModel(data_, 20); }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public BotModelUpdate Clone()
        {
            MessageData data = data_.Clone();
            
            return new BotModelUpdate(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct PackageModelUpdate
    {
        public static implicit operator Update(PackageModelUpdate message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static implicit operator Message(PackageModelUpdate message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public PackageModelUpdate(int i)
        {
            info_ = BotAgent.Info.PackageModelUpdate;
            data_ = new MessageData(44);
            
            data_.SetInt(4, 18);
        }
        
        public PackageModelUpdate(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.PackageModelUpdate))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string Id
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public UpdateType Type
        {
            set { data_.SetUInt(16, (uint) value); }
            
            get { return (UpdateType) data_.GetUInt(16); }
        }
        
        public PackageModel Item
        {
            get { return new PackageModel(data_, 20); }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public PackageModelUpdate Clone()
        {
            MessageData data = data_.Clone();
            
            return new PackageModelUpdate(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct BotStateUpdate
    {
        public static implicit operator Update(BotStateUpdate message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static implicit operator Message(BotStateUpdate message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public BotStateUpdate(int i)
        {
            info_ = BotAgent.Info.BotStateUpdate;
            data_ = new MessageData(32);
            
            data_.SetInt(4, 19);
        }
        
        public BotStateUpdate(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.BotStateUpdate))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string Id
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public UpdateType Type
        {
            set { data_.SetUInt(16, (uint) value); }
            
            get { return (UpdateType) data_.GetUInt(16); }
        }
        
        public string BotId
        {
            set { data_.SetUString(20, value); }
            
            get { return data_.GetUString(20); }
        }
        
        public int GetBotIdLength()
        {
            return data_.GetUStringLength(20);
        }
        
        public void SetBotId(char[] value, int offset, int count)
        {
            data_.SetUString(20, value, offset, count);
        }
        
        public void GetBotId(char[] value, int offset)
        {
            data_.GetUString(20, value, offset);
        }
        
        public void ReadBotId(Stream stream, int size)
        {
            data_.ReadUString(20, stream, size);
        }
        
        public void WriteBotId(Stream stream)
        {
            data_.WriteUString(20, stream);
        }
        
        public BotState State
        {
            set { data_.SetUInt(28, (uint) value); }
            
            get { return (BotState) data_.GetUInt(28); }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public BotStateUpdate Clone()
        {
            MessageData data = data_.Clone();
            
            return new BotStateUpdate(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    struct AccountStateUpdate
    {
        public static implicit operator Update(AccountStateUpdate message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static implicit operator Message(AccountStateUpdate message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public AccountStateUpdate(int i)
        {
            info_ = BotAgent.Info.AccountStateUpdate;
            data_ = new MessageData(52);
            
            data_.SetInt(4, 23);
        }
        
        public AccountStateUpdate(MessageInfo info, MessageData data)
        {
            if (! info.Is(BotAgent.Info.AccountStateUpdate))
                throw new Exception("Invalid message type cast operation");
            
            info_ = info;
            data_ = data;
        }
        
        public string Id
        {
            set { data_.SetString(8, value); }
            
            get { return data_.GetString(8); }
        }
        
        public int GetIdLength()
        {
            return data_.GetStringLength(8);
        }
        
        public void SetId(char[] value, int offset, int count)
        {
            data_.SetString(8, value, offset, count);
        }
        
        public void GetId(char[] value, int offset)
        {
            data_.GetString(8, value, offset);
        }
        
        public void ReadId(Stream stream, int size)
        {
            data_.ReadString(8, stream, size);
        }
        
        public void WriteId(Stream stream)
        {
            data_.WriteString(8, stream);
        }
        
        public UpdateType Type
        {
            set { data_.SetUInt(16, (uint) value); }
            
            get { return (UpdateType) data_.GetUInt(16); }
        }
        
        public AccountKey Account
        {
            get { return new AccountKey(data_, 20); }
        }
        
        public ConnectionState ConnectionState
        {
            set { data_.SetUInt(36, (uint) value); }
            
            get { return (ConnectionState) data_.GetUInt(36); }
        }
        
        public ConnectionErrorModel LastError
        {
            get { return new ConnectionErrorModel(data_, 40); }
        }
        
        public int Size
        {
            get { return data_.GetInt(0); }
        }
        
        public MessageInfo Info
        {
            get { return info_; }
        }
        
        public MessageData Data
        {
            get { return data_; }
        }
        
        public AccountStateUpdate Clone()
        {
            MessageData data = data_.Clone();
            
            return new AccountStateUpdate(info_, data);
        }
        
        public void Reset()
        {
            data_.Reset(info_.MinSize);
        }
        
        public override string ToString()
        {
            return data_.ToString(info_);
        }
        
        MessageInfo info_;
        MessageData data_;
    }
    
    class Is
    {
        public static bool LoginRequest(Message message)
        {
            return message.Info.Is(Info.LoginRequest);
        }
        
        public static bool LoginReport_1(Message message)
        {
            return message.Info.Is(Info.LoginReport_1);
        }
        
        public static bool LoginReport(Message message)
        {
            return message.Info.Is(Info.LoginReport);
        }
        
        public static bool LoginReject(Message message)
        {
            return message.Info.Is(Info.LoginReject);
        }
        
        public static bool LogoutRequest(Message message)
        {
            return message.Info.Is(Info.LogoutRequest);
        }
        
        public static bool LogoutReport(Message message)
        {
            return message.Info.Is(Info.LogoutReport);
        }
        
        public static bool Request(Message message)
        {
            return message.Info.Is(Info.Request);
        }
        
        public static bool Report(Message message)
        {
            return message.Info.Is(Info.Report);
        }
        
        public static bool Update(Message message)
        {
            return message.Info.Is(Info.Update);
        }
        
        public static bool AccountListRequest(Request message)
        {
            return message.Info.Is(Info.AccountListRequest);
        }
        
        public static bool AccountListRequest(Message message)
        {
            return message.Info.Is(Info.AccountListRequest);
        }
        
        public static bool AccountListReport_1(Report message)
        {
            return message.Info.Is(Info.AccountListReport_1);
        }
        
        public static bool AccountListReport_1(Message message)
        {
            return message.Info.Is(Info.AccountListReport_1);
        }
        
        public static bool AccountListReport(Report message)
        {
            return message.Info.Is(Info.AccountListReport);
        }
        
        public static bool AccountListReport(Message message)
        {
            return message.Info.Is(Info.AccountListReport);
        }
        
        public static bool BotListRequest(Request message)
        {
            return message.Info.Is(Info.BotListRequest);
        }
        
        public static bool BotListRequest(Message message)
        {
            return message.Info.Is(Info.BotListRequest);
        }
        
        public static bool BotListReport(Report message)
        {
            return message.Info.Is(Info.BotListReport);
        }
        
        public static bool BotListReport(Message message)
        {
            return message.Info.Is(Info.BotListReport);
        }
        
        public static bool PackageListRequest(Request message)
        {
            return message.Info.Is(Info.PackageListRequest);
        }
        
        public static bool PackageListRequest(Message message)
        {
            return message.Info.Is(Info.PackageListRequest);
        }
        
        public static bool PackageListReport(Report message)
        {
            return message.Info.Is(Info.PackageListReport);
        }
        
        public static bool PackageListReport(Message message)
        {
            return message.Info.Is(Info.PackageListReport);
        }
        
        public static bool SubscribeRequest(Request message)
        {
            return message.Info.Is(Info.SubscribeRequest);
        }
        
        public static bool SubscribeRequest(Message message)
        {
            return message.Info.Is(Info.SubscribeRequest);
        }
        
        public static bool SubscribeReport(Report message)
        {
            return message.Info.Is(Info.SubscribeReport);
        }
        
        public static bool SubscribeReport(Message message)
        {
            return message.Info.Is(Info.SubscribeReport);
        }
        
        public static bool AccountModelUpdate_1(Update message)
        {
            return message.Info.Is(Info.AccountModelUpdate_1);
        }
        
        public static bool AccountModelUpdate_1(Message message)
        {
            return message.Info.Is(Info.AccountModelUpdate_1);
        }
        
        public static bool AccountModelUpdate(Update message)
        {
            return message.Info.Is(Info.AccountModelUpdate);
        }
        
        public static bool AccountModelUpdate(Message message)
        {
            return message.Info.Is(Info.AccountModelUpdate);
        }
        
        public static bool BotModelUpdate(Update message)
        {
            return message.Info.Is(Info.BotModelUpdate);
        }
        
        public static bool BotModelUpdate(Message message)
        {
            return message.Info.Is(Info.BotModelUpdate);
        }
        
        public static bool PackageModelUpdate(Update message)
        {
            return message.Info.Is(Info.PackageModelUpdate);
        }
        
        public static bool PackageModelUpdate(Message message)
        {
            return message.Info.Is(Info.PackageModelUpdate);
        }
        
        public static bool BotStateUpdate(Update message)
        {
            return message.Info.Is(Info.BotStateUpdate);
        }
        
        public static bool BotStateUpdate(Message message)
        {
            return message.Info.Is(Info.BotStateUpdate);
        }
        
        public static bool AccountStateUpdate(Update message)
        {
            return message.Info.Is(Info.AccountStateUpdate);
        }
        
        public static bool AccountStateUpdate(Message message)
        {
            return message.Info.Is(Info.AccountStateUpdate);
        }
    }
    
    class Cast
    {
        public static Message Message(LoginRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static LoginRequest LoginRequest(Message message)
        {
            return new LoginRequest(message.Info, message.Data);
        }
        
        public static Message Message(LoginReport_1 message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static LoginReport_1 LoginReport_1(Message message)
        {
            return new LoginReport_1(message.Info, message.Data);
        }
        
        public static Message Message(LoginReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static LoginReport LoginReport(Message message)
        {
            return new LoginReport(message.Info, message.Data);
        }
        
        public static Message Message(LoginReject message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static LoginReject LoginReject(Message message)
        {
            return new LoginReject(message.Info, message.Data);
        }
        
        public static Message Message(LogoutRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static LogoutRequest LogoutRequest(Message message)
        {
            return new LogoutRequest(message.Info, message.Data);
        }
        
        public static Message Message(LogoutReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static LogoutReport LogoutReport(Message message)
        {
            return new LogoutReport(message.Info, message.Data);
        }
        
        public static Message Message(Request message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static Request Request(Message message)
        {
            return new Request(message.Info, message.Data);
        }
        
        public static Message Message(Report message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static Report Report(Message message)
        {
            return new Report(message.Info, message.Data);
        }
        
        public static Message Message(Update message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static Update Update(Message message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static Request Request(AccountListRequest message)
        {
            return new Request(message.Info, message.Data);
        }
        
        public static AccountListRequest AccountListRequest(Request message)
        {
            return new AccountListRequest(message.Info, message.Data);
        }
        
        public static Message Message(AccountListRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static AccountListRequest AccountListRequest(Message message)
        {
            return new AccountListRequest(message.Info, message.Data);
        }
        
        public static Report Report(AccountListReport_1 message)
        {
            return new Report(message.Info, message.Data);
        }
        
        public static AccountListReport_1 AccountListReport_1(Report message)
        {
            return new AccountListReport_1(message.Info, message.Data);
        }
        
        public static Message Message(AccountListReport_1 message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static AccountListReport_1 AccountListReport_1(Message message)
        {
            return new AccountListReport_1(message.Info, message.Data);
        }
        
        public static Report Report(AccountListReport message)
        {
            return new Report(message.Info, message.Data);
        }
        
        public static AccountListReport AccountListReport(Report message)
        {
            return new AccountListReport(message.Info, message.Data);
        }
        
        public static Message Message(AccountListReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static AccountListReport AccountListReport(Message message)
        {
            return new AccountListReport(message.Info, message.Data);
        }
        
        public static Request Request(BotListRequest message)
        {
            return new Request(message.Info, message.Data);
        }
        
        public static BotListRequest BotListRequest(Request message)
        {
            return new BotListRequest(message.Info, message.Data);
        }
        
        public static Message Message(BotListRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static BotListRequest BotListRequest(Message message)
        {
            return new BotListRequest(message.Info, message.Data);
        }
        
        public static Report Report(BotListReport message)
        {
            return new Report(message.Info, message.Data);
        }
        
        public static BotListReport BotListReport(Report message)
        {
            return new BotListReport(message.Info, message.Data);
        }
        
        public static Message Message(BotListReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static BotListReport BotListReport(Message message)
        {
            return new BotListReport(message.Info, message.Data);
        }
        
        public static Request Request(PackageListRequest message)
        {
            return new Request(message.Info, message.Data);
        }
        
        public static PackageListRequest PackageListRequest(Request message)
        {
            return new PackageListRequest(message.Info, message.Data);
        }
        
        public static Message Message(PackageListRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static PackageListRequest PackageListRequest(Message message)
        {
            return new PackageListRequest(message.Info, message.Data);
        }
        
        public static Report Report(PackageListReport message)
        {
            return new Report(message.Info, message.Data);
        }
        
        public static PackageListReport PackageListReport(Report message)
        {
            return new PackageListReport(message.Info, message.Data);
        }
        
        public static Message Message(PackageListReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static PackageListReport PackageListReport(Message message)
        {
            return new PackageListReport(message.Info, message.Data);
        }
        
        public static Request Request(SubscribeRequest message)
        {
            return new Request(message.Info, message.Data);
        }
        
        public static SubscribeRequest SubscribeRequest(Request message)
        {
            return new SubscribeRequest(message.Info, message.Data);
        }
        
        public static Message Message(SubscribeRequest message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static SubscribeRequest SubscribeRequest(Message message)
        {
            return new SubscribeRequest(message.Info, message.Data);
        }
        
        public static Report Report(SubscribeReport message)
        {
            return new Report(message.Info, message.Data);
        }
        
        public static SubscribeReport SubscribeReport(Report message)
        {
            return new SubscribeReport(message.Info, message.Data);
        }
        
        public static Message Message(SubscribeReport message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static SubscribeReport SubscribeReport(Message message)
        {
            return new SubscribeReport(message.Info, message.Data);
        }
        
        public static Update Update(AccountModelUpdate_1 message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static AccountModelUpdate_1 AccountModelUpdate_1(Update message)
        {
            return new AccountModelUpdate_1(message.Info, message.Data);
        }
        
        public static Message Message(AccountModelUpdate_1 message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static AccountModelUpdate_1 AccountModelUpdate_1(Message message)
        {
            return new AccountModelUpdate_1(message.Info, message.Data);
        }
        
        public static Update Update(AccountModelUpdate message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static AccountModelUpdate AccountModelUpdate(Update message)
        {
            return new AccountModelUpdate(message.Info, message.Data);
        }
        
        public static Message Message(AccountModelUpdate message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static AccountModelUpdate AccountModelUpdate(Message message)
        {
            return new AccountModelUpdate(message.Info, message.Data);
        }
        
        public static Update Update(BotModelUpdate message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static BotModelUpdate BotModelUpdate(Update message)
        {
            return new BotModelUpdate(message.Info, message.Data);
        }
        
        public static Message Message(BotModelUpdate message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static BotModelUpdate BotModelUpdate(Message message)
        {
            return new BotModelUpdate(message.Info, message.Data);
        }
        
        public static Update Update(PackageModelUpdate message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static PackageModelUpdate PackageModelUpdate(Update message)
        {
            return new PackageModelUpdate(message.Info, message.Data);
        }
        
        public static Message Message(PackageModelUpdate message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static PackageModelUpdate PackageModelUpdate(Message message)
        {
            return new PackageModelUpdate(message.Info, message.Data);
        }
        
        public static Update Update(BotStateUpdate message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static BotStateUpdate BotStateUpdate(Update message)
        {
            return new BotStateUpdate(message.Info, message.Data);
        }
        
        public static Message Message(BotStateUpdate message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static BotStateUpdate BotStateUpdate(Message message)
        {
            return new BotStateUpdate(message.Info, message.Data);
        }
        
        public static Update Update(AccountStateUpdate message)
        {
            return new Update(message.Info, message.Data);
        }
        
        public static AccountStateUpdate AccountStateUpdate(Update message)
        {
            return new AccountStateUpdate(message.Info, message.Data);
        }
        
        public static Message Message(AccountStateUpdate message)
        {
            return new Message(message.Info, message.Data);
        }
        
        public static AccountStateUpdate AccountStateUpdate(Message message)
        {
            return new AccountStateUpdate(message.Info, message.Data);
        }
    }
    
    class Info
    {
        static Info()
        {
            ConstructLoginRequest();
            ConstructLoginReport_1();
            ConstructLoginReport();
            ConstructLoginRejectReason();
            ConstructLoginReject();
            ConstructLogoutRequest();
            ConstructLogoutReason();
            ConstructLogoutReport();
            ConstructRequest();
            ConstructRequestExecState();
            ConstructReport();
            ConstructUpdateType();
            ConstructUpdate();
            ConstructPluginKey();
            ConstructPluginType();
            ConstructPluginDescriptor();
            ConstructPluginInfo();
            ConstructPackageModel();
            ConstructAccountKey();
            ConstructPluginPermissions();
            ConstructBotState();
            ConstructBotModel();
            ConstructConnectionState();
            ConstructConnectionErrorCode();
            ConstructConnectionErrorModel();
            ConstructAccountModel_1();
            ConstructAccountModel();
            ConstructAccountListRequest();
            ConstructAccountListReport_1();
            ConstructAccountListReport();
            ConstructBotListRequest();
            ConstructBotListReport();
            ConstructPackageListRequest();
            ConstructPackageListReport();
            ConstructSubscribeRequest();
            ConstructSubscribeReport();
            ConstructAccountModelUpdate_1();
            ConstructAccountModelUpdate();
            ConstructBotModelUpdate();
            ConstructPackageModelUpdate();
            ConstructBotStateUpdate();
            ConstructAccountStateUpdate();
            ConstructBotAgent();
        }
        
        static void ConstructLoginRequest()
        {
            FieldInfo Username = new FieldInfo();
            Username.Name = "Username";
            Username.Offset = 8;
            Username.Type = FieldType.UString;
            Username.Optional = false;
            Username.Repeatable = false;
            
            FieldInfo Password = new FieldInfo();
            Password.Name = "Password";
            Password.Offset = 16;
            Password.Type = FieldType.UString;
            Password.Optional = false;
            Password.Repeatable = false;
            
            LoginRequest = new MessageInfo();
            LoginRequest.Name = "LoginRequest";
            LoginRequest.Id = 0;
            LoginRequest.MinSize = 24;
            LoginRequest.Fields = new FieldInfo[2];
            LoginRequest.Fields[0] = Username;
            LoginRequest.Fields[1] = Password;
        }
        
        static void ConstructLoginReport_1()
        {
            FieldInfo CurrentVersion = new FieldInfo();
            CurrentVersion.Name = "CurrentVersion";
            CurrentVersion.Offset = 8;
            CurrentVersion.Type = FieldType.Int;
            CurrentVersion.Optional = false;
            CurrentVersion.Repeatable = false;
            
            LoginReport_1 = new MessageInfo();
            LoginReport_1.Name = "LoginReport_1";
            LoginReport_1.Id = 1;
            LoginReport_1.MinSize = 12;
            LoginReport_1.Fields = new FieldInfo[1];
            LoginReport_1.Fields[0] = CurrentVersion;
        }
        
        static void ConstructLoginReport()
        {
            
            LoginReport = new MessageInfo();
            LoginReport.Name = "LoginReport";
            LoginReport.Id = 20;
            LoginReport.MinSize = 8;
            LoginReport.Fields = new FieldInfo[0];
        }
        
        static void ConstructLoginRejectReason()
        {
            EnumMemberInfo InvalidCredentials = new EnumMemberInfo();
            InvalidCredentials.Name = "InvalidCredentials";
            InvalidCredentials.Value = 0;
            
            EnumMemberInfo VersionMismatch = new EnumMemberInfo();
            VersionMismatch.Name = "VersionMismatch";
            VersionMismatch.Value = 1;
            
            EnumMemberInfo InternalServerError = new EnumMemberInfo();
            InternalServerError.Name = "InternalServerError";
            InternalServerError.Value = 2;
            
            LoginRejectReason = new EnumInfo();
            LoginRejectReason.Name = "LoginRejectReason";
            LoginRejectReason.MinSize = 4;
            LoginRejectReason.Members = new EnumMemberInfo[3];
            LoginRejectReason.Members[0] = InvalidCredentials;
            LoginRejectReason.Members[1] = VersionMismatch;
            LoginRejectReason.Members[2] = InternalServerError;
        }
        
        static void ConstructLoginReject()
        {
            FieldInfo Reason = new FieldInfo();
            Reason.Name = "Reason";
            Reason.Offset = 8;
            Reason.Type = FieldType.Enum;
            Reason.EnumInfo = Info.LoginRejectReason;
            Reason.Optional = false;
            Reason.Repeatable = false;
            
            FieldInfo Text = new FieldInfo();
            Text.Name = "Text";
            Text.Offset = 12;
            Text.Type = FieldType.UString;
            Text.Optional = true;
            Text.Repeatable = false;
            
            LoginReject = new MessageInfo();
            LoginReject.Name = "LoginReject";
            LoginReject.Id = 2;
            LoginReject.MinSize = 20;
            LoginReject.Fields = new FieldInfo[2];
            LoginReject.Fields[0] = Reason;
            LoginReject.Fields[1] = Text;
        }
        
        static void ConstructLogoutRequest()
        {
            
            LogoutRequest = new MessageInfo();
            LogoutRequest.Name = "LogoutRequest";
            LogoutRequest.Id = 3;
            LogoutRequest.MinSize = 8;
            LogoutRequest.Fields = new FieldInfo[0];
        }
        
        static void ConstructLogoutReason()
        {
            EnumMemberInfo ClientRequest = new EnumMemberInfo();
            ClientRequest.Name = "ClientRequest";
            ClientRequest.Value = 0;
            
            EnumMemberInfo ServerLogout = new EnumMemberInfo();
            ServerLogout.Name = "ServerLogout";
            ServerLogout.Value = 1;
            
            EnumMemberInfo InternalServerError = new EnumMemberInfo();
            InternalServerError.Name = "InternalServerError";
            InternalServerError.Value = 2;
            
            LogoutReason = new EnumInfo();
            LogoutReason.Name = "LogoutReason";
            LogoutReason.MinSize = 4;
            LogoutReason.Members = new EnumMemberInfo[3];
            LogoutReason.Members[0] = ClientRequest;
            LogoutReason.Members[1] = ServerLogout;
            LogoutReason.Members[2] = InternalServerError;
        }
        
        static void ConstructLogoutReport()
        {
            FieldInfo Reason = new FieldInfo();
            Reason.Name = "Reason";
            Reason.Offset = 8;
            Reason.Type = FieldType.Enum;
            Reason.EnumInfo = Info.LogoutReason;
            Reason.Optional = false;
            Reason.Repeatable = false;
            
            FieldInfo Text = new FieldInfo();
            Text.Name = "Text";
            Text.Offset = 12;
            Text.Type = FieldType.UString;
            Text.Optional = true;
            Text.Repeatable = false;
            
            LogoutReport = new MessageInfo();
            LogoutReport.Name = "LogoutReport";
            LogoutReport.Id = 4;
            LogoutReport.MinSize = 20;
            LogoutReport.Fields = new FieldInfo[2];
            LogoutReport.Fields[0] = Reason;
            LogoutReport.Fields[1] = Text;
        }
        
        static void ConstructRequest()
        {
            FieldInfo Id = new FieldInfo();
            Id.Name = "Id";
            Id.Offset = 8;
            Id.Type = FieldType.String;
            Id.Optional = false;
            Id.Repeatable = false;
            
            Request = new MessageInfo();
            Request.Name = "Request";
            Request.Id = 5;
            Request.MinSize = 16;
            Request.Fields = new FieldInfo[1];
            Request.Fields[0] = Id;
        }
        
        static void ConstructRequestExecState()
        {
            EnumMemberInfo Completed = new EnumMemberInfo();
            Completed.Name = "Completed";
            Completed.Value = 0;
            
            EnumMemberInfo InternalServerError = new EnumMemberInfo();
            InternalServerError.Name = "InternalServerError";
            InternalServerError.Value = 1;
            
            RequestExecState = new EnumInfo();
            RequestExecState.Name = "RequestExecState";
            RequestExecState.MinSize = 4;
            RequestExecState.Members = new EnumMemberInfo[2];
            RequestExecState.Members[0] = Completed;
            RequestExecState.Members[1] = InternalServerError;
        }
        
        static void ConstructReport()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.Name = "RequestId";
            RequestId.Offset = 8;
            RequestId.Type = FieldType.String;
            RequestId.Optional = false;
            RequestId.Repeatable = false;
            
            FieldInfo RequestState = new FieldInfo();
            RequestState.Name = "RequestState";
            RequestState.Offset = 16;
            RequestState.Type = FieldType.Enum;
            RequestState.EnumInfo = Info.RequestExecState;
            RequestState.Optional = false;
            RequestState.Repeatable = false;
            
            FieldInfo Text = new FieldInfo();
            Text.Name = "Text";
            Text.Offset = 20;
            Text.Type = FieldType.UString;
            Text.Optional = true;
            Text.Repeatable = false;
            
            Report = new MessageInfo();
            Report.Name = "Report";
            Report.Id = 6;
            Report.MinSize = 28;
            Report.Fields = new FieldInfo[3];
            Report.Fields[0] = RequestId;
            Report.Fields[1] = RequestState;
            Report.Fields[2] = Text;
        }
        
        static void ConstructUpdateType()
        {
            EnumMemberInfo Added = new EnumMemberInfo();
            Added.Name = "Added";
            Added.Value = 0;
            
            EnumMemberInfo Updated = new EnumMemberInfo();
            Updated.Name = "Updated";
            Updated.Value = 1;
            
            EnumMemberInfo Removed = new EnumMemberInfo();
            Removed.Name = "Removed";
            Removed.Value = 2;
            
            UpdateType = new EnumInfo();
            UpdateType.Name = "UpdateType";
            UpdateType.MinSize = 4;
            UpdateType.Members = new EnumMemberInfo[3];
            UpdateType.Members[0] = Added;
            UpdateType.Members[1] = Updated;
            UpdateType.Members[2] = Removed;
        }
        
        static void ConstructUpdate()
        {
            FieldInfo Id = new FieldInfo();
            Id.Name = "Id";
            Id.Offset = 8;
            Id.Type = FieldType.String;
            Id.Optional = false;
            Id.Repeatable = false;
            
            FieldInfo Type = new FieldInfo();
            Type.Name = "Type";
            Type.Offset = 16;
            Type.Type = FieldType.Enum;
            Type.EnumInfo = Info.UpdateType;
            Type.Optional = false;
            Type.Repeatable = false;
            
            Update = new MessageInfo();
            Update.Name = "Update";
            Update.Id = 7;
            Update.MinSize = 20;
            Update.Fields = new FieldInfo[2];
            Update.Fields[0] = Id;
            Update.Fields[1] = Type;
        }
        
        static void ConstructPluginKey()
        {
            FieldInfo PackageName = new FieldInfo();
            PackageName.Name = "PackageName";
            PackageName.Offset = 0;
            PackageName.Type = FieldType.UString;
            PackageName.Optional = false;
            PackageName.Repeatable = false;
            
            FieldInfo DescriptorId = new FieldInfo();
            DescriptorId.Name = "DescriptorId";
            DescriptorId.Offset = 8;
            DescriptorId.Type = FieldType.UString;
            DescriptorId.Optional = false;
            DescriptorId.Repeatable = false;
            
            PluginKey = new GroupInfo();
            PluginKey.Name = "PluginKey";
            PluginKey.MinSize = 16;
            PluginKey.Fields = new FieldInfo[2];
            PluginKey.Fields[0] = PackageName;
            PluginKey.Fields[1] = DescriptorId;
        }
        
        static void ConstructPluginType()
        {
            EnumMemberInfo Indicator = new EnumMemberInfo();
            Indicator.Name = "Indicator";
            Indicator.Value = 0;
            
            EnumMemberInfo Robot = new EnumMemberInfo();
            Robot.Name = "Robot";
            Robot.Value = 1;
            
            EnumMemberInfo Unknown = new EnumMemberInfo();
            Unknown.Name = "Unknown";
            Unknown.Value = 2;
            
            PluginType = new EnumInfo();
            PluginType.Name = "PluginType";
            PluginType.MinSize = 4;
            PluginType.Members = new EnumMemberInfo[3];
            PluginType.Members[0] = Indicator;
            PluginType.Members[1] = Robot;
            PluginType.Members[2] = Unknown;
        }
        
        static void ConstructPluginDescriptor()
        {
            FieldInfo ApiVersion = new FieldInfo();
            ApiVersion.Name = "ApiVersion";
            ApiVersion.Offset = 0;
            ApiVersion.Type = FieldType.UString;
            ApiVersion.Optional = true;
            ApiVersion.Repeatable = false;
            
            FieldInfo Id = new FieldInfo();
            Id.Name = "Id";
            Id.Offset = 8;
            Id.Type = FieldType.UString;
            Id.Optional = false;
            Id.Repeatable = false;
            
            FieldInfo DisplayName = new FieldInfo();
            DisplayName.Name = "DisplayName";
            DisplayName.Offset = 16;
            DisplayName.Type = FieldType.UString;
            DisplayName.Optional = false;
            DisplayName.Repeatable = false;
            
            FieldInfo UserDisplayName = new FieldInfo();
            UserDisplayName.Name = "UserDisplayName";
            UserDisplayName.Offset = 24;
            UserDisplayName.Type = FieldType.UString;
            UserDisplayName.Optional = false;
            UserDisplayName.Repeatable = false;
            
            FieldInfo Category = new FieldInfo();
            Category.Name = "Category";
            Category.Offset = 32;
            Category.Type = FieldType.UString;
            Category.Optional = true;
            Category.Repeatable = false;
            
            FieldInfo Version = new FieldInfo();
            Version.Name = "Version";
            Version.Offset = 40;
            Version.Type = FieldType.UString;
            Version.Optional = true;
            Version.Repeatable = false;
            
            FieldInfo Description = new FieldInfo();
            Description.Name = "Description";
            Description.Offset = 48;
            Description.Type = FieldType.UString;
            Description.Optional = true;
            Description.Repeatable = false;
            
            FieldInfo Copyright = new FieldInfo();
            Copyright.Name = "Copyright";
            Copyright.Offset = 56;
            Copyright.Type = FieldType.UString;
            Copyright.Optional = true;
            Copyright.Repeatable = false;
            
            FieldInfo Type = new FieldInfo();
            Type.Name = "Type";
            Type.Offset = 64;
            Type.Type = FieldType.Enum;
            Type.EnumInfo = Info.PluginType;
            Type.Optional = false;
            Type.Repeatable = false;
            
            PluginDescriptor = new GroupInfo();
            PluginDescriptor.Name = "PluginDescriptor";
            PluginDescriptor.MinSize = 68;
            PluginDescriptor.Fields = new FieldInfo[9];
            PluginDescriptor.Fields[0] = ApiVersion;
            PluginDescriptor.Fields[1] = Id;
            PluginDescriptor.Fields[2] = DisplayName;
            PluginDescriptor.Fields[3] = UserDisplayName;
            PluginDescriptor.Fields[4] = Category;
            PluginDescriptor.Fields[5] = Version;
            PluginDescriptor.Fields[6] = Description;
            PluginDescriptor.Fields[7] = Copyright;
            PluginDescriptor.Fields[8] = Type;
        }
        
        static void ConstructPluginInfo()
        {
            FieldInfo Key = new FieldInfo();
            Key.Name = "Key";
            Key.Offset = 0;
            Key.Type = FieldType.Group;
            Key.GroupInfo = Info.PluginKey;
            Key.Optional = false;
            Key.Repeatable = false;
            
            FieldInfo Descriptor = new FieldInfo();
            Descriptor.Name = "Descriptor";
            Descriptor.Offset = 16;
            Descriptor.Type = FieldType.Group;
            Descriptor.GroupInfo = Info.PluginDescriptor;
            Descriptor.Optional = false;
            Descriptor.Repeatable = false;
            
            PluginInfo = new GroupInfo();
            PluginInfo.Name = "PluginInfo";
            PluginInfo.MinSize = 84;
            PluginInfo.Fields = new FieldInfo[2];
            PluginInfo.Fields[0] = Key;
            PluginInfo.Fields[1] = Descriptor;
        }
        
        static void ConstructPackageModel()
        {
            FieldInfo Name = new FieldInfo();
            Name.Name = "Name";
            Name.Offset = 0;
            Name.Type = FieldType.UString;
            Name.Optional = false;
            Name.Repeatable = false;
            
            FieldInfo Created = new FieldInfo();
            Created.Name = "Created";
            Created.Offset = 8;
            Created.Type = FieldType.DateTime;
            Created.Optional = false;
            Created.Repeatable = false;
            
            FieldInfo Plugins = new FieldInfo();
            Plugins.Name = "Plugins";
            Plugins.Offset = 16;
            Plugins.Type = FieldType.Group;
            Plugins.GroupInfo = Info.PluginInfo;
            Plugins.Optional = false;
            Plugins.Repeatable = true;
            
            PackageModel = new GroupInfo();
            PackageModel.Name = "PackageModel";
            PackageModel.MinSize = 24;
            PackageModel.Fields = new FieldInfo[3];
            PackageModel.Fields[0] = Name;
            PackageModel.Fields[1] = Created;
            PackageModel.Fields[2] = Plugins;
        }
        
        static void ConstructAccountKey()
        {
            FieldInfo Login = new FieldInfo();
            Login.Name = "Login";
            Login.Offset = 0;
            Login.Type = FieldType.UString;
            Login.Optional = false;
            Login.Repeatable = false;
            
            FieldInfo Server = new FieldInfo();
            Server.Name = "Server";
            Server.Offset = 8;
            Server.Type = FieldType.UString;
            Server.Optional = false;
            Server.Repeatable = false;
            
            AccountKey = new GroupInfo();
            AccountKey.Name = "AccountKey";
            AccountKey.MinSize = 16;
            AccountKey.Fields = new FieldInfo[2];
            AccountKey.Fields[0] = Login;
            AccountKey.Fields[1] = Server;
        }
        
        static void ConstructPluginPermissions()
        {
            FieldInfo TradeAllowed = new FieldInfo();
            TradeAllowed.Name = "TradeAllowed";
            TradeAllowed.Offset = 0;
            TradeAllowed.Type = FieldType.Bool;
            TradeAllowed.Optional = false;
            TradeAllowed.Repeatable = false;
            
            PluginPermissions = new GroupInfo();
            PluginPermissions.Name = "PluginPermissions";
            PluginPermissions.MinSize = 1;
            PluginPermissions.Fields = new FieldInfo[1];
            PluginPermissions.Fields[0] = TradeAllowed;
        }
        
        static void ConstructBotState()
        {
            EnumMemberInfo Offline = new EnumMemberInfo();
            Offline.Name = "Offline";
            Offline.Value = 0;
            
            EnumMemberInfo Starting = new EnumMemberInfo();
            Starting.Name = "Starting";
            Starting.Value = 1;
            
            EnumMemberInfo Faulted = new EnumMemberInfo();
            Faulted.Name = "Faulted";
            Faulted.Value = 2;
            
            EnumMemberInfo Online = new EnumMemberInfo();
            Online.Name = "Online";
            Online.Value = 3;
            
            EnumMemberInfo Stopping = new EnumMemberInfo();
            Stopping.Name = "Stopping";
            Stopping.Value = 4;
            
            EnumMemberInfo Broken = new EnumMemberInfo();
            Broken.Name = "Broken";
            Broken.Value = 5;
            
            EnumMemberInfo Reconnecting = new EnumMemberInfo();
            Reconnecting.Name = "Reconnecting";
            Reconnecting.Value = 6;
            
            BotState = new EnumInfo();
            BotState.Name = "BotState";
            BotState.MinSize = 4;
            BotState.Members = new EnumMemberInfo[7];
            BotState.Members[0] = Offline;
            BotState.Members[1] = Starting;
            BotState.Members[2] = Faulted;
            BotState.Members[3] = Online;
            BotState.Members[4] = Stopping;
            BotState.Members[5] = Broken;
            BotState.Members[6] = Reconnecting;
        }
        
        static void ConstructBotModel()
        {
            FieldInfo InstanceId = new FieldInfo();
            InstanceId.Name = "InstanceId";
            InstanceId.Offset = 0;
            InstanceId.Type = FieldType.UString;
            InstanceId.Optional = false;
            InstanceId.Repeatable = false;
            
            FieldInfo Isolated = new FieldInfo();
            Isolated.Name = "Isolated";
            Isolated.Offset = 8;
            Isolated.Type = FieldType.Bool;
            Isolated.Optional = false;
            Isolated.Repeatable = false;
            
            FieldInfo State = new FieldInfo();
            State.Name = "State";
            State.Offset = 9;
            State.Type = FieldType.Enum;
            State.EnumInfo = Info.BotState;
            State.Optional = false;
            State.Repeatable = false;
            
            FieldInfo Permissions = new FieldInfo();
            Permissions.Name = "Permissions";
            Permissions.Offset = 13;
            Permissions.Type = FieldType.Group;
            Permissions.GroupInfo = Info.PluginPermissions;
            Permissions.Optional = false;
            Permissions.Repeatable = false;
            
            FieldInfo Account = new FieldInfo();
            Account.Name = "Account";
            Account.Offset = 14;
            Account.Type = FieldType.Group;
            Account.GroupInfo = Info.AccountKey;
            Account.Optional = false;
            Account.Repeatable = false;
            
            FieldInfo Plugin = new FieldInfo();
            Plugin.Name = "Plugin";
            Plugin.Offset = 30;
            Plugin.Type = FieldType.Group;
            Plugin.GroupInfo = Info.PluginKey;
            Plugin.Optional = false;
            Plugin.Repeatable = false;
            
            BotModel = new GroupInfo();
            BotModel.Name = "BotModel";
            BotModel.MinSize = 46;
            BotModel.Fields = new FieldInfo[6];
            BotModel.Fields[0] = InstanceId;
            BotModel.Fields[1] = Isolated;
            BotModel.Fields[2] = State;
            BotModel.Fields[3] = Permissions;
            BotModel.Fields[4] = Account;
            BotModel.Fields[5] = Plugin;
        }
        
        static void ConstructConnectionState()
        {
            EnumMemberInfo Offline = new EnumMemberInfo();
            Offline.Name = "Offline";
            Offline.Value = 0;
            
            EnumMemberInfo Connecting = new EnumMemberInfo();
            Connecting.Name = "Connecting";
            Connecting.Value = 1;
            
            EnumMemberInfo Online = new EnumMemberInfo();
            Online.Name = "Online";
            Online.Value = 2;
            
            EnumMemberInfo Disconnecting = new EnumMemberInfo();
            Disconnecting.Name = "Disconnecting";
            Disconnecting.Value = 3;
            
            ConnectionState = new EnumInfo();
            ConnectionState.Name = "ConnectionState";
            ConnectionState.MinSize = 4;
            ConnectionState.Members = new EnumMemberInfo[4];
            ConnectionState.Members[0] = Offline;
            ConnectionState.Members[1] = Connecting;
            ConnectionState.Members[2] = Online;
            ConnectionState.Members[3] = Disconnecting;
        }
        
        static void ConstructConnectionErrorCode()
        {
            EnumMemberInfo None = new EnumMemberInfo();
            None.Name = "None";
            None.Value = 0;
            
            EnumMemberInfo Unknown = new EnumMemberInfo();
            Unknown.Name = "Unknown";
            Unknown.Value = 1;
            
            EnumMemberInfo NetworkError = new EnumMemberInfo();
            NetworkError.Name = "NetworkError";
            NetworkError.Value = 2;
            
            EnumMemberInfo Timeout = new EnumMemberInfo();
            Timeout.Name = "Timeout";
            Timeout.Value = 3;
            
            EnumMemberInfo BlockedAccount = new EnumMemberInfo();
            BlockedAccount.Name = "BlockedAccount";
            BlockedAccount.Value = 4;
            
            EnumMemberInfo ClientInitiated = new EnumMemberInfo();
            ClientInitiated.Name = "ClientInitiated";
            ClientInitiated.Value = 5;
            
            EnumMemberInfo InvalidCredentials = new EnumMemberInfo();
            InvalidCredentials.Name = "InvalidCredentials";
            InvalidCredentials.Value = 6;
            
            EnumMemberInfo SlowConnection = new EnumMemberInfo();
            SlowConnection.Name = "SlowConnection";
            SlowConnection.Value = 7;
            
            EnumMemberInfo ServerError = new EnumMemberInfo();
            ServerError.Name = "ServerError";
            ServerError.Value = 8;
            
            EnumMemberInfo LoginDeleted = new EnumMemberInfo();
            LoginDeleted.Name = "LoginDeleted";
            LoginDeleted.Value = 9;
            
            EnumMemberInfo ServerLogout = new EnumMemberInfo();
            ServerLogout.Name = "ServerLogout";
            ServerLogout.Value = 10;
            
            EnumMemberInfo Canceled = new EnumMemberInfo();
            Canceled.Name = "Canceled";
            Canceled.Value = 11;
            
            ConnectionErrorCode = new EnumInfo();
            ConnectionErrorCode.Name = "ConnectionErrorCode";
            ConnectionErrorCode.MinSize = 4;
            ConnectionErrorCode.Members = new EnumMemberInfo[12];
            ConnectionErrorCode.Members[0] = None;
            ConnectionErrorCode.Members[1] = Unknown;
            ConnectionErrorCode.Members[2] = NetworkError;
            ConnectionErrorCode.Members[3] = Timeout;
            ConnectionErrorCode.Members[4] = BlockedAccount;
            ConnectionErrorCode.Members[5] = ClientInitiated;
            ConnectionErrorCode.Members[6] = InvalidCredentials;
            ConnectionErrorCode.Members[7] = SlowConnection;
            ConnectionErrorCode.Members[8] = ServerError;
            ConnectionErrorCode.Members[9] = LoginDeleted;
            ConnectionErrorCode.Members[10] = ServerLogout;
            ConnectionErrorCode.Members[11] = Canceled;
        }
        
        static void ConstructConnectionErrorModel()
        {
            FieldInfo Code = new FieldInfo();
            Code.Name = "Code";
            Code.Offset = 0;
            Code.Type = FieldType.Enum;
            Code.EnumInfo = Info.ConnectionErrorCode;
            Code.Optional = false;
            Code.Repeatable = false;
            
            FieldInfo Text = new FieldInfo();
            Text.Name = "Text";
            Text.Offset = 4;
            Text.Type = FieldType.UString;
            Text.Optional = true;
            Text.Repeatable = false;
            
            ConnectionErrorModel = new GroupInfo();
            ConnectionErrorModel.Name = "ConnectionErrorModel";
            ConnectionErrorModel.MinSize = 12;
            ConnectionErrorModel.Fields = new FieldInfo[2];
            ConnectionErrorModel.Fields[0] = Code;
            ConnectionErrorModel.Fields[1] = Text;
        }
        
        static void ConstructAccountModel_1()
        {
            FieldInfo Login = new FieldInfo();
            Login.Name = "Login";
            Login.Offset = 0;
            Login.Type = FieldType.UString;
            Login.Optional = false;
            Login.Repeatable = false;
            
            FieldInfo Server = new FieldInfo();
            Server.Name = "Server";
            Server.Offset = 8;
            Server.Type = FieldType.UString;
            Server.Optional = false;
            Server.Repeatable = false;
            
            AccountModel_1 = new GroupInfo();
            AccountModel_1.Name = "AccountModel_1";
            AccountModel_1.MinSize = 16;
            AccountModel_1.Fields = new FieldInfo[2];
            AccountModel_1.Fields[0] = Login;
            AccountModel_1.Fields[1] = Server;
        }
        
        static void ConstructAccountModel()
        {
            FieldInfo Login = new FieldInfo();
            Login.Name = "Login";
            Login.Offset = 0;
            Login.Type = FieldType.UString;
            Login.Optional = false;
            Login.Repeatable = false;
            
            FieldInfo Server = new FieldInfo();
            Server.Name = "Server";
            Server.Offset = 8;
            Server.Type = FieldType.UString;
            Server.Optional = false;
            Server.Repeatable = false;
            
            FieldInfo UseNewProtocol = new FieldInfo();
            UseNewProtocol.Name = "UseNewProtocol";
            UseNewProtocol.Offset = 16;
            UseNewProtocol.Type = FieldType.Bool;
            UseNewProtocol.Optional = false;
            UseNewProtocol.Repeatable = false;
            
            FieldInfo ConnectionState = new FieldInfo();
            ConnectionState.Name = "ConnectionState";
            ConnectionState.Offset = 17;
            ConnectionState.Type = FieldType.Enum;
            ConnectionState.EnumInfo = Info.ConnectionState;
            ConnectionState.Optional = false;
            ConnectionState.Repeatable = false;
            
            FieldInfo LastError = new FieldInfo();
            LastError.Name = "LastError";
            LastError.Offset = 21;
            LastError.Type = FieldType.Group;
            LastError.GroupInfo = Info.ConnectionErrorModel;
            LastError.Optional = false;
            LastError.Repeatable = false;
            
            AccountModel = new GroupInfo();
            AccountModel.Name = "AccountModel";
            AccountModel.MinSize = 33;
            AccountModel.Fields = new FieldInfo[5];
            AccountModel.Fields[0] = Login;
            AccountModel.Fields[1] = Server;
            AccountModel.Fields[2] = UseNewProtocol;
            AccountModel.Fields[3] = ConnectionState;
            AccountModel.Fields[4] = LastError;
        }
        
        static void ConstructAccountListRequest()
        {
            FieldInfo Id = new FieldInfo();
            Id.Name = "Id";
            Id.Offset = 8;
            Id.Type = FieldType.String;
            Id.Optional = false;
            Id.Repeatable = false;
            
            AccountListRequest = new MessageInfo();
            AccountListRequest.ParentInfo = Request;
            AccountListRequest.Name = "AccountListRequest";
            AccountListRequest.Id = 8;
            AccountListRequest.MinSize = 16;
            AccountListRequest.Fields = new FieldInfo[1];
            AccountListRequest.Fields[0] = Id;
        }
        
        static void ConstructAccountListReport_1()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.Name = "RequestId";
            RequestId.Offset = 8;
            RequestId.Type = FieldType.String;
            RequestId.Optional = false;
            RequestId.Repeatable = false;
            
            FieldInfo RequestState = new FieldInfo();
            RequestState.Name = "RequestState";
            RequestState.Offset = 16;
            RequestState.Type = FieldType.Enum;
            RequestState.EnumInfo = Info.RequestExecState;
            RequestState.Optional = false;
            RequestState.Repeatable = false;
            
            FieldInfo Text = new FieldInfo();
            Text.Name = "Text";
            Text.Offset = 20;
            Text.Type = FieldType.UString;
            Text.Optional = true;
            Text.Repeatable = false;
            
            FieldInfo Accounts = new FieldInfo();
            Accounts.Name = "Accounts";
            Accounts.Offset = 28;
            Accounts.Type = FieldType.Group;
            Accounts.GroupInfo = Info.AccountModel_1;
            Accounts.Optional = false;
            Accounts.Repeatable = true;
            
            AccountListReport_1 = new MessageInfo();
            AccountListReport_1.ParentInfo = Report;
            AccountListReport_1.Name = "AccountListReport_1";
            AccountListReport_1.Id = 9;
            AccountListReport_1.MinSize = 36;
            AccountListReport_1.Fields = new FieldInfo[4];
            AccountListReport_1.Fields[0] = RequestId;
            AccountListReport_1.Fields[1] = RequestState;
            AccountListReport_1.Fields[2] = Text;
            AccountListReport_1.Fields[3] = Accounts;
        }
        
        static void ConstructAccountListReport()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.Name = "RequestId";
            RequestId.Offset = 8;
            RequestId.Type = FieldType.String;
            RequestId.Optional = false;
            RequestId.Repeatable = false;
            
            FieldInfo RequestState = new FieldInfo();
            RequestState.Name = "RequestState";
            RequestState.Offset = 16;
            RequestState.Type = FieldType.Enum;
            RequestState.EnumInfo = Info.RequestExecState;
            RequestState.Optional = false;
            RequestState.Repeatable = false;
            
            FieldInfo Text = new FieldInfo();
            Text.Name = "Text";
            Text.Offset = 20;
            Text.Type = FieldType.UString;
            Text.Optional = true;
            Text.Repeatable = false;
            
            FieldInfo Accounts = new FieldInfo();
            Accounts.Name = "Accounts";
            Accounts.Offset = 28;
            Accounts.Type = FieldType.Group;
            Accounts.GroupInfo = Info.AccountModel;
            Accounts.Optional = false;
            Accounts.Repeatable = true;
            
            AccountListReport = new MessageInfo();
            AccountListReport.ParentInfo = Report;
            AccountListReport.Name = "AccountListReport";
            AccountListReport.Id = 21;
            AccountListReport.MinSize = 36;
            AccountListReport.Fields = new FieldInfo[4];
            AccountListReport.Fields[0] = RequestId;
            AccountListReport.Fields[1] = RequestState;
            AccountListReport.Fields[2] = Text;
            AccountListReport.Fields[3] = Accounts;
        }
        
        static void ConstructBotListRequest()
        {
            FieldInfo Id = new FieldInfo();
            Id.Name = "Id";
            Id.Offset = 8;
            Id.Type = FieldType.String;
            Id.Optional = false;
            Id.Repeatable = false;
            
            BotListRequest = new MessageInfo();
            BotListRequest.ParentInfo = Request;
            BotListRequest.Name = "BotListRequest";
            BotListRequest.Id = 10;
            BotListRequest.MinSize = 16;
            BotListRequest.Fields = new FieldInfo[1];
            BotListRequest.Fields[0] = Id;
        }
        
        static void ConstructBotListReport()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.Name = "RequestId";
            RequestId.Offset = 8;
            RequestId.Type = FieldType.String;
            RequestId.Optional = false;
            RequestId.Repeatable = false;
            
            FieldInfo RequestState = new FieldInfo();
            RequestState.Name = "RequestState";
            RequestState.Offset = 16;
            RequestState.Type = FieldType.Enum;
            RequestState.EnumInfo = Info.RequestExecState;
            RequestState.Optional = false;
            RequestState.Repeatable = false;
            
            FieldInfo Text = new FieldInfo();
            Text.Name = "Text";
            Text.Offset = 20;
            Text.Type = FieldType.UString;
            Text.Optional = true;
            Text.Repeatable = false;
            
            FieldInfo Bots = new FieldInfo();
            Bots.Name = "Bots";
            Bots.Offset = 28;
            Bots.Type = FieldType.Group;
            Bots.GroupInfo = Info.BotModel;
            Bots.Optional = false;
            Bots.Repeatable = true;
            
            BotListReport = new MessageInfo();
            BotListReport.ParentInfo = Report;
            BotListReport.Name = "BotListReport";
            BotListReport.Id = 11;
            BotListReport.MinSize = 36;
            BotListReport.Fields = new FieldInfo[4];
            BotListReport.Fields[0] = RequestId;
            BotListReport.Fields[1] = RequestState;
            BotListReport.Fields[2] = Text;
            BotListReport.Fields[3] = Bots;
        }
        
        static void ConstructPackageListRequest()
        {
            FieldInfo Id = new FieldInfo();
            Id.Name = "Id";
            Id.Offset = 8;
            Id.Type = FieldType.String;
            Id.Optional = false;
            Id.Repeatable = false;
            
            PackageListRequest = new MessageInfo();
            PackageListRequest.ParentInfo = Request;
            PackageListRequest.Name = "PackageListRequest";
            PackageListRequest.Id = 12;
            PackageListRequest.MinSize = 16;
            PackageListRequest.Fields = new FieldInfo[1];
            PackageListRequest.Fields[0] = Id;
        }
        
        static void ConstructPackageListReport()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.Name = "RequestId";
            RequestId.Offset = 8;
            RequestId.Type = FieldType.String;
            RequestId.Optional = false;
            RequestId.Repeatable = false;
            
            FieldInfo RequestState = new FieldInfo();
            RequestState.Name = "RequestState";
            RequestState.Offset = 16;
            RequestState.Type = FieldType.Enum;
            RequestState.EnumInfo = Info.RequestExecState;
            RequestState.Optional = false;
            RequestState.Repeatable = false;
            
            FieldInfo Text = new FieldInfo();
            Text.Name = "Text";
            Text.Offset = 20;
            Text.Type = FieldType.UString;
            Text.Optional = true;
            Text.Repeatable = false;
            
            FieldInfo Packages = new FieldInfo();
            Packages.Name = "Packages";
            Packages.Offset = 28;
            Packages.Type = FieldType.Group;
            Packages.GroupInfo = Info.PackageModel;
            Packages.Optional = false;
            Packages.Repeatable = true;
            
            PackageListReport = new MessageInfo();
            PackageListReport.ParentInfo = Report;
            PackageListReport.Name = "PackageListReport";
            PackageListReport.Id = 13;
            PackageListReport.MinSize = 36;
            PackageListReport.Fields = new FieldInfo[4];
            PackageListReport.Fields[0] = RequestId;
            PackageListReport.Fields[1] = RequestState;
            PackageListReport.Fields[2] = Text;
            PackageListReport.Fields[3] = Packages;
        }
        
        static void ConstructSubscribeRequest()
        {
            FieldInfo Id = new FieldInfo();
            Id.Name = "Id";
            Id.Offset = 8;
            Id.Type = FieldType.String;
            Id.Optional = false;
            Id.Repeatable = false;
            
            SubscribeRequest = new MessageInfo();
            SubscribeRequest.ParentInfo = Request;
            SubscribeRequest.Name = "SubscribeRequest";
            SubscribeRequest.Id = 14;
            SubscribeRequest.MinSize = 16;
            SubscribeRequest.Fields = new FieldInfo[1];
            SubscribeRequest.Fields[0] = Id;
        }
        
        static void ConstructSubscribeReport()
        {
            FieldInfo RequestId = new FieldInfo();
            RequestId.Name = "RequestId";
            RequestId.Offset = 8;
            RequestId.Type = FieldType.String;
            RequestId.Optional = false;
            RequestId.Repeatable = false;
            
            FieldInfo RequestState = new FieldInfo();
            RequestState.Name = "RequestState";
            RequestState.Offset = 16;
            RequestState.Type = FieldType.Enum;
            RequestState.EnumInfo = Info.RequestExecState;
            RequestState.Optional = false;
            RequestState.Repeatable = false;
            
            FieldInfo Text = new FieldInfo();
            Text.Name = "Text";
            Text.Offset = 20;
            Text.Type = FieldType.UString;
            Text.Optional = true;
            Text.Repeatable = false;
            
            SubscribeReport = new MessageInfo();
            SubscribeReport.ParentInfo = Report;
            SubscribeReport.Name = "SubscribeReport";
            SubscribeReport.Id = 15;
            SubscribeReport.MinSize = 28;
            SubscribeReport.Fields = new FieldInfo[3];
            SubscribeReport.Fields[0] = RequestId;
            SubscribeReport.Fields[1] = RequestState;
            SubscribeReport.Fields[2] = Text;
        }
        
        static void ConstructAccountModelUpdate_1()
        {
            FieldInfo Id = new FieldInfo();
            Id.Name = "Id";
            Id.Offset = 8;
            Id.Type = FieldType.String;
            Id.Optional = false;
            Id.Repeatable = false;
            
            FieldInfo Type = new FieldInfo();
            Type.Name = "Type";
            Type.Offset = 16;
            Type.Type = FieldType.Enum;
            Type.EnumInfo = Info.UpdateType;
            Type.Optional = false;
            Type.Repeatable = false;
            
            FieldInfo Item = new FieldInfo();
            Item.Name = "Item";
            Item.Offset = 20;
            Item.Type = FieldType.Group;
            Item.GroupInfo = Info.AccountModel_1;
            Item.Optional = false;
            Item.Repeatable = false;
            
            AccountModelUpdate_1 = new MessageInfo();
            AccountModelUpdate_1.ParentInfo = Update;
            AccountModelUpdate_1.Name = "AccountModelUpdate_1";
            AccountModelUpdate_1.Id = 16;
            AccountModelUpdate_1.MinSize = 36;
            AccountModelUpdate_1.Fields = new FieldInfo[3];
            AccountModelUpdate_1.Fields[0] = Id;
            AccountModelUpdate_1.Fields[1] = Type;
            AccountModelUpdate_1.Fields[2] = Item;
        }
        
        static void ConstructAccountModelUpdate()
        {
            FieldInfo Id = new FieldInfo();
            Id.Name = "Id";
            Id.Offset = 8;
            Id.Type = FieldType.String;
            Id.Optional = false;
            Id.Repeatable = false;
            
            FieldInfo Type = new FieldInfo();
            Type.Name = "Type";
            Type.Offset = 16;
            Type.Type = FieldType.Enum;
            Type.EnumInfo = Info.UpdateType;
            Type.Optional = false;
            Type.Repeatable = false;
            
            FieldInfo Item = new FieldInfo();
            Item.Name = "Item";
            Item.Offset = 20;
            Item.Type = FieldType.Group;
            Item.GroupInfo = Info.AccountModel;
            Item.Optional = false;
            Item.Repeatable = false;
            
            AccountModelUpdate = new MessageInfo();
            AccountModelUpdate.ParentInfo = Update;
            AccountModelUpdate.Name = "AccountModelUpdate";
            AccountModelUpdate.Id = 22;
            AccountModelUpdate.MinSize = 53;
            AccountModelUpdate.Fields = new FieldInfo[3];
            AccountModelUpdate.Fields[0] = Id;
            AccountModelUpdate.Fields[1] = Type;
            AccountModelUpdate.Fields[2] = Item;
        }
        
        static void ConstructBotModelUpdate()
        {
            FieldInfo Id = new FieldInfo();
            Id.Name = "Id";
            Id.Offset = 8;
            Id.Type = FieldType.String;
            Id.Optional = false;
            Id.Repeatable = false;
            
            FieldInfo Type = new FieldInfo();
            Type.Name = "Type";
            Type.Offset = 16;
            Type.Type = FieldType.Enum;
            Type.EnumInfo = Info.UpdateType;
            Type.Optional = false;
            Type.Repeatable = false;
            
            FieldInfo Item = new FieldInfo();
            Item.Name = "Item";
            Item.Offset = 20;
            Item.Type = FieldType.Group;
            Item.GroupInfo = Info.BotModel;
            Item.Optional = false;
            Item.Repeatable = false;
            
            BotModelUpdate = new MessageInfo();
            BotModelUpdate.ParentInfo = Update;
            BotModelUpdate.Name = "BotModelUpdate";
            BotModelUpdate.Id = 17;
            BotModelUpdate.MinSize = 66;
            BotModelUpdate.Fields = new FieldInfo[3];
            BotModelUpdate.Fields[0] = Id;
            BotModelUpdate.Fields[1] = Type;
            BotModelUpdate.Fields[2] = Item;
        }
        
        static void ConstructPackageModelUpdate()
        {
            FieldInfo Id = new FieldInfo();
            Id.Name = "Id";
            Id.Offset = 8;
            Id.Type = FieldType.String;
            Id.Optional = false;
            Id.Repeatable = false;
            
            FieldInfo Type = new FieldInfo();
            Type.Name = "Type";
            Type.Offset = 16;
            Type.Type = FieldType.Enum;
            Type.EnumInfo = Info.UpdateType;
            Type.Optional = false;
            Type.Repeatable = false;
            
            FieldInfo Item = new FieldInfo();
            Item.Name = "Item";
            Item.Offset = 20;
            Item.Type = FieldType.Group;
            Item.GroupInfo = Info.PackageModel;
            Item.Optional = false;
            Item.Repeatable = false;
            
            PackageModelUpdate = new MessageInfo();
            PackageModelUpdate.ParentInfo = Update;
            PackageModelUpdate.Name = "PackageModelUpdate";
            PackageModelUpdate.Id = 18;
            PackageModelUpdate.MinSize = 44;
            PackageModelUpdate.Fields = new FieldInfo[3];
            PackageModelUpdate.Fields[0] = Id;
            PackageModelUpdate.Fields[1] = Type;
            PackageModelUpdate.Fields[2] = Item;
        }
        
        static void ConstructBotStateUpdate()
        {
            FieldInfo Id = new FieldInfo();
            Id.Name = "Id";
            Id.Offset = 8;
            Id.Type = FieldType.String;
            Id.Optional = false;
            Id.Repeatable = false;
            
            FieldInfo Type = new FieldInfo();
            Type.Name = "Type";
            Type.Offset = 16;
            Type.Type = FieldType.Enum;
            Type.EnumInfo = Info.UpdateType;
            Type.Optional = false;
            Type.Repeatable = false;
            
            FieldInfo BotId = new FieldInfo();
            BotId.Name = "BotId";
            BotId.Offset = 20;
            BotId.Type = FieldType.UString;
            BotId.Optional = false;
            BotId.Repeatable = false;
            
            FieldInfo State = new FieldInfo();
            State.Name = "State";
            State.Offset = 28;
            State.Type = FieldType.Enum;
            State.EnumInfo = Info.BotState;
            State.Optional = false;
            State.Repeatable = false;
            
            BotStateUpdate = new MessageInfo();
            BotStateUpdate.ParentInfo = Update;
            BotStateUpdate.Name = "BotStateUpdate";
            BotStateUpdate.Id = 19;
            BotStateUpdate.MinSize = 32;
            BotStateUpdate.Fields = new FieldInfo[4];
            BotStateUpdate.Fields[0] = Id;
            BotStateUpdate.Fields[1] = Type;
            BotStateUpdate.Fields[2] = BotId;
            BotStateUpdate.Fields[3] = State;
        }
        
        static void ConstructAccountStateUpdate()
        {
            FieldInfo Id = new FieldInfo();
            Id.Name = "Id";
            Id.Offset = 8;
            Id.Type = FieldType.String;
            Id.Optional = false;
            Id.Repeatable = false;
            
            FieldInfo Type = new FieldInfo();
            Type.Name = "Type";
            Type.Offset = 16;
            Type.Type = FieldType.Enum;
            Type.EnumInfo = Info.UpdateType;
            Type.Optional = false;
            Type.Repeatable = false;
            
            FieldInfo Account = new FieldInfo();
            Account.Name = "Account";
            Account.Offset = 20;
            Account.Type = FieldType.Group;
            Account.GroupInfo = Info.AccountKey;
            Account.Optional = false;
            Account.Repeatable = false;
            
            FieldInfo ConnectionState = new FieldInfo();
            ConnectionState.Name = "ConnectionState";
            ConnectionState.Offset = 36;
            ConnectionState.Type = FieldType.Enum;
            ConnectionState.EnumInfo = Info.ConnectionState;
            ConnectionState.Optional = false;
            ConnectionState.Repeatable = false;
            
            FieldInfo LastError = new FieldInfo();
            LastError.Name = "LastError";
            LastError.Offset = 40;
            LastError.Type = FieldType.Group;
            LastError.GroupInfo = Info.ConnectionErrorModel;
            LastError.Optional = false;
            LastError.Repeatable = false;
            
            AccountStateUpdate = new MessageInfo();
            AccountStateUpdate.ParentInfo = Update;
            AccountStateUpdate.Name = "AccountStateUpdate";
            AccountStateUpdate.Id = 23;
            AccountStateUpdate.MinSize = 52;
            AccountStateUpdate.Fields = new FieldInfo[5];
            AccountStateUpdate.Fields[0] = Id;
            AccountStateUpdate.Fields[1] = Type;
            AccountStateUpdate.Fields[2] = Account;
            AccountStateUpdate.Fields[3] = ConnectionState;
            AccountStateUpdate.Fields[4] = LastError;
        }
        
        static void ConstructBotAgent()
        {
            BotAgent = new ProtocolInfo();
            BotAgent.Name = "BotAgent";
            BotAgent.MajorVersion = 1;
            BotAgent.MinorVersion = 2;
            BotAgent.AddMessageInfo(LoginRequest);
            BotAgent.AddMessageInfo(LoginReport_1);
            BotAgent.AddMessageInfo(LoginReport);
            BotAgent.AddMessageInfo(LoginReject);
            BotAgent.AddMessageInfo(LogoutRequest);
            BotAgent.AddMessageInfo(LogoutReport);
            BotAgent.AddMessageInfo(Request);
            BotAgent.AddMessageInfo(Report);
            BotAgent.AddMessageInfo(Update);
            BotAgent.AddMessageInfo(AccountListRequest);
            BotAgent.AddMessageInfo(AccountListReport_1);
            BotAgent.AddMessageInfo(AccountListReport);
            BotAgent.AddMessageInfo(BotListRequest);
            BotAgent.AddMessageInfo(BotListReport);
            BotAgent.AddMessageInfo(PackageListRequest);
            BotAgent.AddMessageInfo(PackageListReport);
            BotAgent.AddMessageInfo(SubscribeRequest);
            BotAgent.AddMessageInfo(SubscribeReport);
            BotAgent.AddMessageInfo(AccountModelUpdate_1);
            BotAgent.AddMessageInfo(AccountModelUpdate);
            BotAgent.AddMessageInfo(BotModelUpdate);
            BotAgent.AddMessageInfo(PackageModelUpdate);
            BotAgent.AddMessageInfo(BotStateUpdate);
            BotAgent.AddMessageInfo(AccountStateUpdate);
        }
        
        public static MessageInfo LoginRequest;
        public static MessageInfo LoginReport_1;
        public static MessageInfo LoginReport;
        public static EnumInfo LoginRejectReason;
        public static MessageInfo LoginReject;
        public static MessageInfo LogoutRequest;
        public static EnumInfo LogoutReason;
        public static MessageInfo LogoutReport;
        public static MessageInfo Request;
        public static EnumInfo RequestExecState;
        public static MessageInfo Report;
        public static EnumInfo UpdateType;
        public static MessageInfo Update;
        public static GroupInfo PluginKey;
        public static EnumInfo PluginType;
        public static GroupInfo PluginDescriptor;
        public static GroupInfo PluginInfo;
        public static GroupInfo PackageModel;
        public static GroupInfo AccountKey;
        public static GroupInfo PluginPermissions;
        public static EnumInfo BotState;
        public static GroupInfo BotModel;
        public static EnumInfo ConnectionState;
        public static EnumInfo ConnectionErrorCode;
        public static GroupInfo ConnectionErrorModel;
        public static GroupInfo AccountModel_1;
        public static GroupInfo AccountModel;
        public static MessageInfo AccountListRequest;
        public static MessageInfo AccountListReport_1;
        public static MessageInfo AccountListReport;
        public static MessageInfo BotListRequest;
        public static MessageInfo BotListReport;
        public static MessageInfo PackageListRequest;
        public static MessageInfo PackageListReport;
        public static MessageInfo SubscribeRequest;
        public static MessageInfo SubscribeReport;
        public static MessageInfo AccountModelUpdate_1;
        public static MessageInfo AccountModelUpdate;
        public static MessageInfo BotModelUpdate;
        public static MessageInfo PackageModelUpdate;
        public static MessageInfo BotStateUpdate;
        public static MessageInfo AccountStateUpdate;
        public static ProtocolInfo BotAgent;
    }
    
    class ClientContext : Context
    {
        public ClientContext(bool waitable) : base(waitable)
        {
        }
    }
    
    class ConnectClientContext : ClientContext
    {
        public ConnectClientContext(bool waitable) : base(waitable)
        {
        }
    }
    
    class DisconnectClientContext : ClientContext
    {
        public DisconnectClientContext(bool waitable) : base(waitable)
        {
        }
    }
    
    class LoginRequestClientContext : ClientContext
    {
        public LoginRequestClientContext(bool waitable) : base(waitable)
        {
        }
    }
    
    class LogoutRequestClientContext : ClientContext
    {
        public LogoutRequestClientContext(bool waitable) : base(waitable)
        {
        }
    }
    
    class AccountListRequestClientContext : ClientContext
    {
        public AccountListRequestClientContext(bool waitable) : base(waitable)
        {
        }
    }
    
    class BotListRequestClientContext : ClientContext
    {
        public BotListRequestClientContext(bool waitable) : base(waitable)
        {
        }
    }
    
    class PackageListRequestClientContext : ClientContext
    {
        public PackageListRequestClientContext(bool waitable) : base(waitable)
        {
        }
    }
    
    class SubscribeRequestClientContext : ClientContext
    {
        public SubscribeRequestClientContext(bool waitable) : base(waitable)
        {
        }
    }
    
    struct ClientSessionLogOptions
    {
        public ClientSessionLogOptions(string directory)
        {
            Directory = directory;
#if DEBUG
            Events = true;
            States = false;
            Messages = true;
#else
            Events = false;
            States = false;
            Messages = false;
#endif
        }
        
        public string Directory;
        
        public bool Events;
        
        public bool States;
        
        public bool Messages;
    }
    
    struct ClientSessionOptions
    {
        public ClientSessionOptions(int connectPort)
        {
            ConnectPort = connectPort;
            ConnectionType = ConnectionType.Socket;
            ServerCertificateName = null;
            Certificates = null;
            ConnectMaxCount = -1;
            ReconnectMaxCount = -1;
            ConnectInterval = 10000;
            HeartbeatInterval = 10000;
            SendBufferSize = 1024 * 1024;
            OptimizationType = OptimizationType.Latency;
            Log = new ClientSessionLogOptions("Logs");
        }
        
        public int ConnectPort;
        
        public ConnectionType ConnectionType;
        
        public string ServerCertificateName;
        
        public X509Certificate2Collection Certificates;
        
        public int ConnectMaxCount;
        
        public int ReconnectMaxCount;
        
        public int ConnectInterval;
        
        public int HeartbeatInterval;
        
        public int SendBufferSize;
        
        public OptimizationType OptimizationType;
        
        public ClientSessionLogOptions Log;
    }
    
    class ClientSession
    {
        public ClientSession(string name, ClientSessionOptions options)
        {
            Core.ClientSessionOptions coreOptions = new Core.ClientSessionOptions(options.ConnectPort);
            coreOptions.ConnectionType = options.ConnectionType;
            coreOptions.ServerCertificateName = options.ServerCertificateName;
            coreOptions.Certificates = options.Certificates;
            coreOptions.WaitEventMask = WaitEventMask.Send;
            coreOptions.ConnectMaxCount = options.ConnectMaxCount;
            coreOptions.ReconnectMaxCount = options.ReconnectMaxCount;
            coreOptions.ConnectInterval = options.ConnectInterval;
            coreOptions.HeartbeatInterval = options.HeartbeatInterval;
            coreOptions.SendBufferSize = options.SendBufferSize;
            coreOptions.OptimizationType = options.OptimizationType;
            coreOptions.Log.Directory = options.Log.Directory;
            coreOptions.Log.Events = options.Log.Events;
            coreOptions.Log.States = options.Log.States;
            coreOptions.Log.Messages = options.Log.Messages;
            
            coreSession_ = new Core.ClientSession(name, Info.BotAgent, coreOptions);
            coreSessionListener_ = new CoreClientSessionListener(this);
            coreSession_.Listener = coreSessionListener_;
            
            stateMutex_ = new object();
            connected_ = false;
            
            Event_LoginReport_262_14_LoginReport_ = new Event_LoginReport_262_14_LoginReport(this);
            Event_LoginReport_1_262_14_LoginReport_1_ = new Event_LoginReport_1_262_14_LoginReport_1(this);
            Event_LoginReject_262_14_LoginReject_ = new Event_LoginReject_262_14_LoginReject(this);
            Event_LogoutReport_274_10_LogoutReport_ = new Event_LogoutReport_274_10_LogoutReport(this);
            Event_LogoutReport_292_14_LogoutReport_ = new Event_LogoutReport_292_14_LogoutReport(this);
            Event_AccountModelUpdate_365_9_AccountModelUpdate_ = new Event_AccountModelUpdate_365_9_AccountModelUpdate(this);
            Event_AccountModelUpdate_1_365_9_AccountModelUpdate_1_ = new Event_AccountModelUpdate_1_365_9_AccountModelUpdate_1(this);
            Event_BotModelUpdate_365_9_BotModelUpdate_ = new Event_BotModelUpdate_365_9_BotModelUpdate(this);
            Event_PackageModelUpdate_365_9_PackageModelUpdate_ = new Event_PackageModelUpdate_365_9_PackageModelUpdate(this);
            Event_BotStateUpdate_365_9_BotStateUpdate_ = new Event_BotStateUpdate_365_9_BotStateUpdate(this);
            Event_AccountStateUpdate_365_9_AccountStateUpdate_ = new Event_AccountStateUpdate_365_9_AccountStateUpdate(this);
            Event_AccountListReport_418_13_AccountListReport_ = new Event_AccountListReport_418_13_AccountListReport(this);
            Event_AccountListReport_1_418_13_AccountListReport_1_ = new Event_AccountListReport_1_418_13_AccountListReport_1(this);
            Event_BotListReport_427_13_BotListReport_ = new Event_BotListReport_427_13_BotListReport(this);
            Event_PackageListReport_433_13_PackageListReport_ = new Event_PackageListReport_433_13_PackageListReport(this);
            Event_SubscribeReport_439_13_SubscribeReport_ = new Event_SubscribeReport_439_13_SubscribeReport(this);
            
            event_ = null;
        }
        
        public string Name
        {
            get { return coreSession_.Name; }
        }
        
        public Guid Guid
        {
            get { return coreSession_.Guid; }
        }
        
        public ClientSessionListener Listener
        {
            set
            {
                lock (stateMutex_)
                {
                    if (connected_)
                        throw new Exception(string.Format("Session is connected : {0}({1})", coreSession_.Name, coreSession_.Guid));
                    
                    listener_ = value;
                }
            }
            
            get
            {
                lock (stateMutex_)
                {
                    return listener_;
                }
            }
        }
        
        public object Data
        {
            set { coreSession_.Data = value; }
            
            get { return coreSession_.Data; }
        }
        
        public int ServerMajorVersion
        {
            get { return coreSession_.ServerMajorVersion; }
        }
        
        public int ServerMinorVersion
        {
            get { return coreSession_.ServerMinorVersion; }
        }
        
        public Guid ServerGuid
        {
            get { return coreSession_.ServerGuid; }
        }
        
        public void Connect(ConnectClientContext context, string address)
        {
            lock (stateMutex_)
            {
                if (connected_)
                    throw new Exception(string.Format("Session is not inactive : {0}({1})", coreSession_.Name, coreSession_.Guid));
                
                connectContext_ = context;
                coreSession_.Connect(address);
                
                connected_ = true;
            }
        }
        
        public bool Disconnect(DisconnectClientContext context, string text)
        {
            bool result;
            
            lock (stateMutex_)
            {
                if (connected_)
                {
                    connected_ = false;
                    
                    if (coreSession_.Disconnect(text))
                    {
                        disconnectContext_ = context;
                        result = true;
                    }
                    else
                        result = false;
                }
                else
                    result = false;
            }
            
            return result;
        }
        
        public void Join()
        {
            coreSession_.Join();
        }
        
        public bool Reconnect
        {
            set
            {
                coreSession_.Reconnect = value;
            }
            
            get
            {
                return coreSession_.Reconnect;
            }
        }
        
        public void SendLoginRequest(LoginRequestClientContext context, LoginRequest message)
        {
            lock (stateMutex_)
            {
                if (ClientProcessor_ == null)
                    throw new Exception(string.Format("Session is not active : {0}({1})", coreSession_.Name, coreSession_.Guid));
                
                if (coreSession_.LogEvents)
                    coreSession_.LogEvent("SendLoginRequest({0})", message.ToString());
                
                ClientProcessor_.PreprocessSendLoginRequest(context, message);
                
                if (context != null)
                    context.Reset();
                
                coreSession_.Send(message);
                
                Client:
                
                ClientProcessor_.PostprocessSendLoginRequest(context, message);
            }
        }
        
        public void SendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
        {
            lock (stateMutex_)
            {
                if (ClientProcessor_ == null)
                    throw new Exception(string.Format("Session is not active : {0}({1})", coreSession_.Name, coreSession_.Guid));
                
                if (coreSession_.LogEvents)
                    coreSession_.LogEvent("SendLogoutRequest({0})", message.ToString());
                
                ClientProcessor_.PreprocessSendLogoutRequest(context, message);
                
                if (context != null)
                    context.Reset();
                
                coreSession_.Send(message);
                
                Client:
                
                ClientProcessor_.PostprocessSendLogoutRequest(context, message);
            }
        }
        
        public void SendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
        {
            lock (stateMutex_)
            {
                if (ClientProcessor_ == null)
                    throw new Exception(string.Format("Session is not active : {0}({1})", coreSession_.Name, coreSession_.Guid));
                
                if (coreSession_.LogEvents)
                    coreSession_.LogEvent("SendAccountListRequest({0})", message.ToString());
                
                ClientProcessor_.PreprocessSend(message);
                
                string key = message.Id;
                ClientRequestProcessor ClientRequestProcessor;
                
                if (! ClientRequestProcessorDictionary_.TryGetValue(key, out ClientRequestProcessor))
                {
                    ClientRequestProcessor = new ClientRequestProcessor(this, key);
                    ClientRequestProcessorDictionary_.Add(key, ClientRequestProcessor);
                }
                
                ClientRequestProcessor.PreprocessSendAccountListRequest(context, message);
                
                if (context != null)
                    context.Reset();
                
                coreSession_.Send(message);
                
                ClientRequest:
                
                ClientRequestProcessor.PostprocessSendAccountListRequest(context, message);
                
                if (ClientRequestProcessor.Completed)
                    ClientRequestProcessorDictionary_.Remove(key);
                
                Client:
                
                ClientProcessor_.PostprocessSend(message);
            }
        }
        
        public void SendBotListRequest(BotListRequestClientContext context, BotListRequest message)
        {
            lock (stateMutex_)
            {
                if (ClientProcessor_ == null)
                    throw new Exception(string.Format("Session is not active : {0}({1})", coreSession_.Name, coreSession_.Guid));
                
                if (coreSession_.LogEvents)
                    coreSession_.LogEvent("SendBotListRequest({0})", message.ToString());
                
                ClientProcessor_.PreprocessSend(message);
                
                string key = message.Id;
                ClientRequestProcessor ClientRequestProcessor;
                
                if (! ClientRequestProcessorDictionary_.TryGetValue(key, out ClientRequestProcessor))
                {
                    ClientRequestProcessor = new ClientRequestProcessor(this, key);
                    ClientRequestProcessorDictionary_.Add(key, ClientRequestProcessor);
                }
                
                ClientRequestProcessor.PreprocessSendBotListRequest(context, message);
                
                if (context != null)
                    context.Reset();
                
                coreSession_.Send(message);
                
                ClientRequest:
                
                ClientRequestProcessor.PostprocessSendBotListRequest(context, message);
                
                if (ClientRequestProcessor.Completed)
                    ClientRequestProcessorDictionary_.Remove(key);
                
                Client:
                
                ClientProcessor_.PostprocessSend(message);
            }
        }
        
        public void SendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
        {
            lock (stateMutex_)
            {
                if (ClientProcessor_ == null)
                    throw new Exception(string.Format("Session is not active : {0}({1})", coreSession_.Name, coreSession_.Guid));
                
                if (coreSession_.LogEvents)
                    coreSession_.LogEvent("SendPackageListRequest({0})", message.ToString());
                
                ClientProcessor_.PreprocessSend(message);
                
                string key = message.Id;
                ClientRequestProcessor ClientRequestProcessor;
                
                if (! ClientRequestProcessorDictionary_.TryGetValue(key, out ClientRequestProcessor))
                {
                    ClientRequestProcessor = new ClientRequestProcessor(this, key);
                    ClientRequestProcessorDictionary_.Add(key, ClientRequestProcessor);
                }
                
                ClientRequestProcessor.PreprocessSendPackageListRequest(context, message);
                
                if (context != null)
                    context.Reset();
                
                coreSession_.Send(message);
                
                ClientRequest:
                
                ClientRequestProcessor.PostprocessSendPackageListRequest(context, message);
                
                if (ClientRequestProcessor.Completed)
                    ClientRequestProcessorDictionary_.Remove(key);
                
                Client:
                
                ClientProcessor_.PostprocessSend(message);
            }
        }
        
        public void SendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
        {
            lock (stateMutex_)
            {
                if (ClientProcessor_ == null)
                    throw new Exception(string.Format("Session is not active : {0}({1})", coreSession_.Name, coreSession_.Guid));
                
                if (coreSession_.LogEvents)
                    coreSession_.LogEvent("SendSubscribeRequest({0})", message.ToString());
                
                ClientProcessor_.PreprocessSend(message);
                
                string key = message.Id;
                ClientRequestProcessor ClientRequestProcessor;
                
                if (! ClientRequestProcessorDictionary_.TryGetValue(key, out ClientRequestProcessor))
                {
                    ClientRequestProcessor = new ClientRequestProcessor(this, key);
                    ClientRequestProcessorDictionary_.Add(key, ClientRequestProcessor);
                }
                
                ClientRequestProcessor.PreprocessSendSubscribeRequest(context, message);
                
                if (context != null)
                    context.Reset();
                
                coreSession_.Send(message);
                
                ClientRequest:
                
                ClientRequestProcessor.PostprocessSendSubscribeRequest(context, message);
                
                if (ClientRequestProcessor.Completed)
                    ClientRequestProcessorDictionary_.Remove(key);
                
                Client:
                
                ClientProcessor_.PostprocessSend(message);
            }
        }
        
        public void Send(Message message)
        {
            lock (stateMutex_)
            {
                if (ClientProcessor_ == null)
                    throw new Exception(string.Format("Session is not active : {0}({1})", coreSession_.Name, coreSession_.Guid));
                
                if (coreSession_.LogEvents)
                    coreSession_.LogEvent("Send({0})", message.ToString());
                
                ClientProcessor_.PreprocessSend(message);
                
                if (Is.Update(message))
                {
                    Update Update = Cast.Update(message);
                    
                    string key = Update.Id;
                    ClientUpdateProcessor ClientUpdateProcessor;
                    
                    if (! ClientUpdateProcessorDictionary_.TryGetValue(key, out ClientUpdateProcessor))
                    {
                        ClientUpdateProcessor = new ClientUpdateProcessor(this, key);
                        ClientUpdateProcessorDictionary_.Add(key, ClientUpdateProcessor);
                    }
                    
                    ClientUpdateProcessor.PreprocessSend(message);
                    
                    coreSession_.Send(message);
                    
                    ClientUpdate:
                    
                    ClientUpdateProcessor.PostprocessSend(message);
                    
                    if (ClientUpdateProcessor.Completed)
                        ClientUpdateProcessorDictionary_.Remove(key);
                    
                    goto Client;
                }
                
                if (Is.Request(message))
                {
                    Request Request = Cast.Request(message);
                    
                    string key = Request.Id;
                    ClientRequestProcessor ClientRequestProcessor;
                    
                    if (! ClientRequestProcessorDictionary_.TryGetValue(key, out ClientRequestProcessor))
                    {
                        ClientRequestProcessor = new ClientRequestProcessor(this, key);
                        ClientRequestProcessorDictionary_.Add(key, ClientRequestProcessor);
                    }
                    
                    ClientRequestProcessor.PreprocessSend(message);
                    
                    coreSession_.Send(message);
                    
                    ClientRequest:
                    
                    ClientRequestProcessor.PostprocessSend(message);
                    
                    if (ClientRequestProcessor.Completed)
                        ClientRequestProcessorDictionary_.Remove(key);
                    
                    goto Client;
                }
                
                if (Is.Report(message))
                {
                    Report Report = Cast.Report(message);
                    
                    string key = Report.RequestId;
                    ClientRequestProcessor ClientRequestProcessor;
                    
                    if (! ClientRequestProcessorDictionary_.TryGetValue(key, out ClientRequestProcessor))
                    {
                        ClientRequestProcessor = new ClientRequestProcessor(this, key);
                        ClientRequestProcessorDictionary_.Add(key, ClientRequestProcessor);
                    }
                    
                    ClientRequestProcessor.PreprocessSend(message);
                    
                    coreSession_.Send(message);
                    
                    ClientRequest:
                    
                    ClientRequestProcessor.PostprocessSend(message);
                    
                    if (ClientRequestProcessor.Completed)
                        ClientRequestProcessorDictionary_.Remove(key);
                    
                    goto Client;
                }
                
                coreSession_.Send(message);
                
                Client:
                
                ClientProcessor_.PostprocessSend(message);
            }
        }
        
        public bool WaitSend(Message message, int timeout)
        {
            return coreSession_.WaitSend(message, timeout);
        }
        
        public bool LogEvents
        {
            set { coreSession_.LogEvents = value; }
            
            get { return coreSession_.LogEvents; }
        }
        
        public bool LogStatets
        {
            set { coreSession_.LogStates = value; }
            
            get { return coreSession_.LogStates; }
        }
        
        public bool LogMessages
        {
            set { coreSession_.LogMessages = value; }
            
            get { return coreSession_.LogMessages; }
        }
        
        public void LogError(string format, params object[] args)
        {
            coreSession_.LogError(format, args);
        }
        
        public void LogWarning(string format, params object[] args)
        {
            coreSession_.LogWarning(format, args);
        }
        
        public void LogInfo(string format, params object[] args)
        {
            coreSession_.LogWarning(format, args);
        }
        
        public void LogEvent(string format, params object[] args)
        {
            coreSession_.LogError(format, args);
        }
        
        public void LogState(string format, params object[] args)
        {
            coreSession_.LogState(format, args);
        }
        
        class ClientProcessor
        {
            public ClientProcessor(ClientSession session)
            {
                session_ = session;
                
                State_260_10_ = new State_260_10(this);
                State_262_14_ = new State_262_14(this);
                State_274_10_ = new State_274_10(this);
                State_292_14_ = new State_292_14(this);
                State_0_ = new State_0(this);
                
                state_ = State_260_10_;
                
                if (session_.coreSession_.LogStates)
                    session_.coreSession_.LogState("Client : 260_10");
            }
            
            public bool Completed
            {
                get { return state_ == State_0_; }
            }
            
            public void PreprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
            {
                state_.PreprocessSendLoginRequest(context, message);
            }
            
            public void PostprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
            {
                state_.PostprocessSendLoginRequest(context, message);
            }
            
            public void PreprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
            {
                state_.PreprocessSendLogoutRequest(context, message);
            }
            
            public void PostprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
            {
                state_.PostprocessSendLogoutRequest(context, message);
            }
            
            public void PreprocessSend(Message message)
            {
                state_.PreprocessSend(message);
            }
            
            public void PostprocessSend(Message message)
            {
                state_.PostprocessSend(message);
            }
            
            public void ProcessReceive(Message message)
            {
                state_.ProcessReceive(message);
            }
            
            public void ProcessDisconnect(List<ClientContext> contextList)
            {
                state_.ProcessDisconnect(contextList);
            }
            
            abstract class State
            {
                public State(ClientProcessor processor)
                {
                    processor_ = processor;
                }
                
                public abstract void PreprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message);
                
                public abstract void PostprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message);
                
                public abstract void PreprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message);
                
                public abstract void PostprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message);
                
                public abstract void PreprocessSend(Message message);
                
                public abstract void PostprocessSend(Message message);
                
                public abstract void ProcessReceive(Message message);
                
                public abstract void ProcessDisconnect(List<ClientContext> contextList);
                
                protected ClientProcessor processor_;
            }
            
            class State_260_10 : State
            {
                public State_260_10(ClientProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                }
                
                public override void PostprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                    processor_.State_262_14_.LoginRequestClientContext_ = context;
                    
                    processor_.state_ = processor_.State_262_14_;
                    
                    if (processor_.session_.coreSession_.LogStates)
                        processor_.session_.coreSession_.LogState("Client : 262_14");
                }
                
                public override void PreprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 260_10 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.Name));
                }
                
                public override void PostprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    if (Is.LoginRequest(message))
                        return;
                    
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 260_10 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.Name));
                }
                
                public override void PostprocessSend(Message message)
                {
                    if (Is.LoginRequest(message))
                    {
                        processor_.state_ = processor_.State_262_14_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 262_14");
                        
                        return;
                    }
                }
                
                public override void ProcessReceive(Message message)
                {
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Client() : 260_10 : {0}", message.Info.Name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                }
            }
            
            class State_262_14 : State
            {
                public State_262_14(ClientProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 262_14 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.Name));
                }
                
                public override void PostprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                }
                
                public override void PreprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 262_14 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.Name));
                }
                
                public override void PostprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 262_14 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.Name));
                }
                
                public override void PostprocessSend(Message message)
                {
                }
                
                public override void ProcessReceive(Message message)
                {
                    if (Is.LoginReport(message))
                    {
                        LoginReport LoginReport = Cast.LoginReport(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_LoginReport_262_14_LoginReport_.LoginRequestClientContext_ = LoginRequestClientContext_;
                            processor_.session_.Event_LoginReport_262_14_LoginReport_.LoginReport_ = LoginReport;
                            
                            processor_.session_.event_ = processor_.session_.Event_LoginReport_262_14_LoginReport_;
                        }
                        
                        LoginRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_274_10_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 274_10");
                        
                        return;
                    }
                    
                    if (Is.LoginReport_1(message))
                    {
                        LoginReport_1 LoginReport_1 = Cast.LoginReport_1(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_LoginReport_1_262_14_LoginReport_1_.LoginRequestClientContext_ = LoginRequestClientContext_;
                            processor_.session_.Event_LoginReport_1_262_14_LoginReport_1_.LoginReport_1_ = LoginReport_1;
                            
                            processor_.session_.event_ = processor_.session_.Event_LoginReport_1_262_14_LoginReport_1_;
                        }
                        
                        LoginRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_274_10_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 274_10");
                        
                        return;
                    }
                    
                    if (Is.LoginReject(message))
                    {
                        LoginReject LoginReject = Cast.LoginReject(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_LoginReject_262_14_LoginReject_.LoginRequestClientContext_ = LoginRequestClientContext_;
                            processor_.session_.Event_LoginReject_262_14_LoginReject_.LoginReject_ = LoginReject;
                            
                            processor_.session_.event_ = processor_.session_.Event_LoginReject_262_14_LoginReject_;
                        }
                        
                        LoginRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 0");
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Client() : 262_14 : {0}", message.Info.Name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                    if (LoginRequestClientContext_ != null)
                        contextList.Add(LoginRequestClientContext_);
                }
                
                public LoginRequestClientContext LoginRequestClientContext_;
            }
            
            class State_274_10 : State
            {
                public State_274_10(ClientProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 274_10 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.Name));
                }
                
                public override void PostprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                }
                
                public override void PreprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                }
                
                public override void PostprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                    processor_.State_292_14_.LogoutRequestClientContext_ = context;
                    
                    processor_.state_ = processor_.State_292_14_;
                    
                    if (processor_.session_.coreSession_.LogStates)
                        processor_.session_.coreSession_.LogState("Client : 292_14");
                }
                
                public override void PreprocessSend(Message message)
                {
                    if (Is.Request(message))
                        return;
                    
                    if (Is.LogoutRequest(message))
                        return;
                    
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 274_10 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.Name));
                }
                
                public override void PostprocessSend(Message message)
                {
                    if (Is.Request(message))
                    {
                        processor_.state_ = processor_.State_274_10_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 274_10");
                        
                        return;
                    }
                    
                    if (Is.LogoutRequest(message))
                    {
                        processor_.state_ = processor_.State_292_14_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 292_14");
                        
                        return;
                    }
                }
                
                public override void ProcessReceive(Message message)
                {
                    if (Is.Report(message))
                    {
                        Report Report = Cast.Report(message);
                        
                        processor_.state_ = processor_.State_274_10_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 274_10");
                        
                        return;
                    }
                    
                    if (Is.Update(message))
                    {
                        Update Update = Cast.Update(message);
                        
                        processor_.state_ = processor_.State_274_10_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 274_10");
                        
                        return;
                    }
                    
                    if (Is.LogoutReport(message))
                    {
                        LogoutReport LogoutReport = Cast.LogoutReport(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_LogoutReport_274_10_LogoutReport_.LogoutReport_ = LogoutReport;
                            
                            processor_.session_.event_ = processor_.session_.Event_LogoutReport_274_10_LogoutReport_;
                        }
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 0");
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Client() : 274_10 : {0}", message.Info.Name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                }
            }
            
            class State_292_14 : State
            {
                public State_292_14(ClientProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 292_14 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.Name));
                }
                
                public override void PostprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                }
                
                public override void PreprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 292_14 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.Name));
                }
                
                public override void PostprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 292_14 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.Name));
                }
                
                public override void PostprocessSend(Message message)
                {
                }
                
                public override void ProcessReceive(Message message)
                {
                    if (Is.Report(message))
                    {
                        Report Report = Cast.Report(message);
                        
                        processor_.state_ = processor_.State_292_14_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 292_14");
                        
                        return;
                    }
                    
                    if (Is.Update(message))
                    {
                        Update Update = Cast.Update(message);
                        
                        processor_.state_ = processor_.State_292_14_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 292_14");
                        
                        return;
                    }
                    
                    if (Is.LogoutReport(message))
                    {
                        LogoutReport LogoutReport = Cast.LogoutReport(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_LogoutReport_292_14_LogoutReport_.LogoutRequestClientContext_ = LogoutRequestClientContext_;
                            processor_.session_.Event_LogoutReport_292_14_LogoutReport_.LogoutReport_ = LogoutReport;
                            
                            processor_.session_.event_ = processor_.session_.Event_LogoutReport_292_14_LogoutReport_;
                        }
                        
                        LogoutRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("Client : 0");
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Client() : 292_14 : {0}", message.Info.Name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                    if (LogoutRequestClientContext_ != null)
                        contextList.Add(LogoutRequestClientContext_);
                }
                
                public LogoutRequestClientContext LogoutRequestClientContext_;
            }
            
            class State_0 : State
            {
                public State_0(ClientProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 0 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.Name));
                }
                
                public override void PostprocessSendLoginRequest(LoginRequestClientContext context, LoginRequest message)
                {
                }
                
                public override void PreprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 0 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.Name));
                }
                
                public override void PostprocessSendLogoutRequest(LogoutRequestClientContext context, LogoutRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Client() : 0 : {2}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, message.Info.Name));
                }
                
                public override void PostprocessSend(Message message)
                {
                }
                
                public override void ProcessReceive(Message message)
                {
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Client() : 0 : {0}", message.Info.Name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                }
            }
            
            ClientSession session_;
            
            State_260_10 State_260_10_;
            State_262_14 State_262_14_;
            State_274_10 State_274_10_;
            State_292_14 State_292_14_;
            State_0 State_0_;
            
            State state_;
        }
        
        class ClientUpdateProcessor
        {
            public ClientUpdateProcessor(ClientSession session, string id)
            {
                session_ = session;
                id_ = id;
                
                State_365_9_ = new State_365_9(this);
                State_0_ = new State_0(this);
                
                state_ = State_365_9_;
                
                if (session_.coreSession_.LogStates)
                    session_.coreSession_.LogState("ClientUpdate({0}) : 365_9", id_);
            }
            
            public bool Completed
            {
                get { return state_ == State_0_; }
            }
            
            public void PreprocessSend(Message message)
            {
                state_.PreprocessSend(message);
            }
            
            public void PostprocessSend(Message message)
            {
                state_.PostprocessSend(message);
            }
            
            public void ProcessReceive(Message message)
            {
                state_.ProcessReceive(message);
            }
            
            public void ProcessDisconnect(List<ClientContext> contextList)
            {
                state_.ProcessDisconnect(contextList);
            }
            
            abstract class State
            {
                public State(ClientUpdateProcessor processor)
                {
                    processor_ = processor;
                }
                
                public abstract void PreprocessSend(Message message);
                
                public abstract void PostprocessSend(Message message);
                
                public abstract void ProcessReceive(Message message);
                
                public abstract void ProcessDisconnect(List<ClientContext> contextList);
                
                protected ClientUpdateProcessor processor_;
            }
            
            class State_365_9 : State
            {
                public State_365_9(ClientUpdateProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientUpdate({2}) : 365_9 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSend(Message message)
                {
                }
                
                public override void ProcessReceive(Message message)
                {
                    if (Is.AccountModelUpdate(message))
                    {
                        AccountModelUpdate AccountModelUpdate = Cast.AccountModelUpdate(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_AccountModelUpdate_365_9_AccountModelUpdate_.AccountModelUpdate_ = AccountModelUpdate;
                            
                            processor_.session_.event_ = processor_.session_.Event_AccountModelUpdate_365_9_AccountModelUpdate_;
                        }
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientUpdate({0}) : 0", processor_.id_);
                        
                        return;
                    }
                    
                    if (Is.AccountModelUpdate_1(message))
                    {
                        AccountModelUpdate_1 AccountModelUpdate_1 = Cast.AccountModelUpdate_1(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_AccountModelUpdate_1_365_9_AccountModelUpdate_1_.AccountModelUpdate_1_ = AccountModelUpdate_1;
                            
                            processor_.session_.event_ = processor_.session_.Event_AccountModelUpdate_1_365_9_AccountModelUpdate_1_;
                        }
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientUpdate({0}) : 0", processor_.id_);
                        
                        return;
                    }
                    
                    if (Is.BotModelUpdate(message))
                    {
                        BotModelUpdate BotModelUpdate = Cast.BotModelUpdate(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_BotModelUpdate_365_9_BotModelUpdate_.BotModelUpdate_ = BotModelUpdate;
                            
                            processor_.session_.event_ = processor_.session_.Event_BotModelUpdate_365_9_BotModelUpdate_;
                        }
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientUpdate({0}) : 0", processor_.id_);
                        
                        return;
                    }
                    
                    if (Is.PackageModelUpdate(message))
                    {
                        PackageModelUpdate PackageModelUpdate = Cast.PackageModelUpdate(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_PackageModelUpdate_365_9_PackageModelUpdate_.PackageModelUpdate_ = PackageModelUpdate;
                            
                            processor_.session_.event_ = processor_.session_.Event_PackageModelUpdate_365_9_PackageModelUpdate_;
                        }
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientUpdate({0}) : 0", processor_.id_);
                        
                        return;
                    }
                    
                    if (Is.BotStateUpdate(message))
                    {
                        BotStateUpdate BotStateUpdate = Cast.BotStateUpdate(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_BotStateUpdate_365_9_BotStateUpdate_.BotStateUpdate_ = BotStateUpdate;
                            
                            processor_.session_.event_ = processor_.session_.Event_BotStateUpdate_365_9_BotStateUpdate_;
                        }
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientUpdate({0}) : 0", processor_.id_);
                        
                        return;
                    }
                    
                    if (Is.AccountStateUpdate(message))
                    {
                        AccountStateUpdate AccountStateUpdate = Cast.AccountStateUpdate(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_AccountStateUpdate_365_9_AccountStateUpdate_.AccountStateUpdate_ = AccountStateUpdate;
                            
                            processor_.session_.event_ = processor_.session_.Event_AccountStateUpdate_365_9_AccountStateUpdate_;
                        }
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientUpdate({0}) : 0", processor_.id_);
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ClientUpdate({0}) : 365_9 : {1}", processor_.id_, message.Info.Name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                }
            }
            
            class State_0 : State
            {
                public State_0(ClientUpdateProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientUpdate({2}) : 0 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSend(Message message)
                {
                }
                
                public override void ProcessReceive(Message message)
                {
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ClientUpdate({0}) : 0 : {1}", processor_.id_, message.Info.Name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                }
            }
            
            ClientSession session_;
            string id_;
            
            State_365_9 State_365_9_;
            State_0 State_0_;
            
            State state_;
        }
        
        class ClientRequestProcessor
        {
            public ClientRequestProcessor(ClientSession session, string id)
            {
                session_ = session;
                id_ = id;
                
                State_416_9_ = new State_416_9(this);
                State_418_13_ = new State_418_13(this);
                State_427_13_ = new State_427_13(this);
                State_433_13_ = new State_433_13(this);
                State_439_13_ = new State_439_13(this);
                State_0_ = new State_0(this);
                
                state_ = State_416_9_;
                
                if (session_.coreSession_.LogStates)
                    session_.coreSession_.LogState("ClientRequest({0}) : 416_9", id_);
            }
            
            public bool Completed
            {
                get { return state_ == State_0_; }
            }
            
            public void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
            {
                state_.PreprocessSendAccountListRequest(context, message);
            }
            
            public void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
            {
                state_.PostprocessSendAccountListRequest(context, message);
            }
            
            public void PreprocessSendBotListRequest(BotListRequestClientContext context, BotListRequest message)
            {
                state_.PreprocessSendBotListRequest(context, message);
            }
            
            public void PostprocessSendBotListRequest(BotListRequestClientContext context, BotListRequest message)
            {
                state_.PostprocessSendBotListRequest(context, message);
            }
            
            public void PreprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
            {
                state_.PreprocessSendPackageListRequest(context, message);
            }
            
            public void PostprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
            {
                state_.PostprocessSendPackageListRequest(context, message);
            }
            
            public void PreprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
            {
                state_.PreprocessSendSubscribeRequest(context, message);
            }
            
            public void PostprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
            {
                state_.PostprocessSendSubscribeRequest(context, message);
            }
            
            public void PreprocessSend(Message message)
            {
                state_.PreprocessSend(message);
            }
            
            public void PostprocessSend(Message message)
            {
                state_.PostprocessSend(message);
            }
            
            public void ProcessReceive(Message message)
            {
                state_.ProcessReceive(message);
            }
            
            public void ProcessDisconnect(List<ClientContext> contextList)
            {
                state_.ProcessDisconnect(contextList);
            }
            
            abstract class State
            {
                public State(ClientRequestProcessor processor)
                {
                    processor_ = processor;
                }
                
                public abstract void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message);
                
                public abstract void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message);
                
                public abstract void PreprocessSendBotListRequest(BotListRequestClientContext context, BotListRequest message);
                
                public abstract void PostprocessSendBotListRequest(BotListRequestClientContext context, BotListRequest message);
                
                public abstract void PreprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message);
                
                public abstract void PostprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message);
                
                public abstract void PreprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message);
                
                public abstract void PostprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message);
                
                public abstract void PreprocessSend(Message message);
                
                public abstract void PostprocessSend(Message message);
                
                public abstract void ProcessReceive(Message message);
                
                public abstract void ProcessDisconnect(List<ClientContext> contextList);
                
                protected ClientRequestProcessor processor_;
            }
            
            class State_416_9 : State
            {
                public State_416_9(ClientRequestProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                }
                
                public override void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                    processor_.State_418_13_.AccountListRequestClientContext_ = context;
                    
                    processor_.state_ = processor_.State_418_13_;
                    
                    if (processor_.session_.coreSession_.LogStates)
                        processor_.session_.coreSession_.LogState("ClientRequest({0}) : 418_13", processor_.id_);
                }
                
                public override void PreprocessSendBotListRequest(BotListRequestClientContext context, BotListRequest message)
                {
                }
                
                public override void PostprocessSendBotListRequest(BotListRequestClientContext context, BotListRequest message)
                {
                    processor_.State_427_13_.BotListRequestClientContext_ = context;
                    
                    processor_.state_ = processor_.State_427_13_;
                    
                    if (processor_.session_.coreSession_.LogStates)
                        processor_.session_.coreSession_.LogState("ClientRequest({0}) : 427_13", processor_.id_);
                }
                
                public override void PreprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                }
                
                public override void PostprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                    processor_.State_433_13_.PackageListRequestClientContext_ = context;
                    
                    processor_.state_ = processor_.State_433_13_;
                    
                    if (processor_.session_.coreSession_.LogStates)
                        processor_.session_.coreSession_.LogState("ClientRequest({0}) : 433_13", processor_.id_);
                }
                
                public override void PreprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                }
                
                public override void PostprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                    processor_.State_439_13_.SubscribeRequestClientContext_ = context;
                    
                    processor_.state_ = processor_.State_439_13_;
                    
                    if (processor_.session_.coreSession_.LogStates)
                        processor_.session_.coreSession_.LogState("ClientRequest({0}) : 439_13", processor_.id_);
                }
                
                public override void PreprocessSend(Message message)
                {
                    if (Is.AccountListRequest(message))
                        return;
                    
                    if (Is.BotListRequest(message))
                        return;
                    
                    if (Is.PackageListRequest(message))
                        return;
                    
                    if (Is.SubscribeRequest(message))
                        return;
                    
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 416_9 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSend(Message message)
                {
                    if (Is.AccountListRequest(message))
                    {
                        processor_.state_ = processor_.State_418_13_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientRequest({0}) : 418_13", processor_.id_);
                        
                        return;
                    }
                    
                    if (Is.BotListRequest(message))
                    {
                        processor_.state_ = processor_.State_427_13_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientRequest({0}) : 427_13", processor_.id_);
                        
                        return;
                    }
                    
                    if (Is.PackageListRequest(message))
                    {
                        processor_.state_ = processor_.State_433_13_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientRequest({0}) : 433_13", processor_.id_);
                        
                        return;
                    }
                    
                    if (Is.SubscribeRequest(message))
                    {
                        processor_.state_ = processor_.State_439_13_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientRequest({0}) : 439_13", processor_.id_);
                        
                        return;
                    }
                }
                
                public override void ProcessReceive(Message message)
                {
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ClientRequest({0}) : 416_9 : {1}", processor_.id_, message.Info.Name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                }
            }
            
            class State_418_13 : State
            {
                public State_418_13(ClientRequestProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 418_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                }
                
                public override void PreprocessSendBotListRequest(BotListRequestClientContext context, BotListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 418_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendBotListRequest(BotListRequestClientContext context, BotListRequest message)
                {
                }
                
                public override void PreprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 418_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                }
                
                public override void PreprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 418_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 418_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSend(Message message)
                {
                }
                
                public override void ProcessReceive(Message message)
                {
                    if (Is.AccountListReport(message))
                    {
                        AccountListReport AccountListReport = Cast.AccountListReport(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_AccountListReport_418_13_AccountListReport_.AccountListRequestClientContext_ = AccountListRequestClientContext_;
                            processor_.session_.Event_AccountListReport_418_13_AccountListReport_.AccountListReport_ = AccountListReport;
                            
                            processor_.session_.event_ = processor_.session_.Event_AccountListReport_418_13_AccountListReport_;
                        }
                        
                        AccountListRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientRequest({0}) : 0", processor_.id_);
                        
                        return;
                    }
                    
                    if (Is.AccountListReport_1(message))
                    {
                        AccountListReport_1 AccountListReport_1 = Cast.AccountListReport_1(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_AccountListReport_1_418_13_AccountListReport_1_.AccountListRequestClientContext_ = AccountListRequestClientContext_;
                            processor_.session_.Event_AccountListReport_1_418_13_AccountListReport_1_.AccountListReport_1_ = AccountListReport_1;
                            
                            processor_.session_.event_ = processor_.session_.Event_AccountListReport_1_418_13_AccountListReport_1_;
                        }
                        
                        AccountListRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientRequest({0}) : 0", processor_.id_);
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ClientRequest({0}) : 418_13 : {1}", processor_.id_, message.Info.Name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                    if (AccountListRequestClientContext_ != null)
                        contextList.Add(AccountListRequestClientContext_);
                }
                
                public AccountListRequestClientContext AccountListRequestClientContext_;
            }
            
            class State_427_13 : State
            {
                public State_427_13(ClientRequestProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 427_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                }
                
                public override void PreprocessSendBotListRequest(BotListRequestClientContext context, BotListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 427_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendBotListRequest(BotListRequestClientContext context, BotListRequest message)
                {
                }
                
                public override void PreprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 427_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                }
                
                public override void PreprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 427_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 427_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSend(Message message)
                {
                }
                
                public override void ProcessReceive(Message message)
                {
                    if (Is.BotListReport(message))
                    {
                        BotListReport BotListReport = Cast.BotListReport(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_BotListReport_427_13_BotListReport_.BotListRequestClientContext_ = BotListRequestClientContext_;
                            processor_.session_.Event_BotListReport_427_13_BotListReport_.BotListReport_ = BotListReport;
                            
                            processor_.session_.event_ = processor_.session_.Event_BotListReport_427_13_BotListReport_;
                        }
                        
                        BotListRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientRequest({0}) : 0", processor_.id_);
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ClientRequest({0}) : 427_13 : {1}", processor_.id_, message.Info.Name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                    if (BotListRequestClientContext_ != null)
                        contextList.Add(BotListRequestClientContext_);
                }
                
                public BotListRequestClientContext BotListRequestClientContext_;
            }
            
            class State_433_13 : State
            {
                public State_433_13(ClientRequestProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 433_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                }
                
                public override void PreprocessSendBotListRequest(BotListRequestClientContext context, BotListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 433_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendBotListRequest(BotListRequestClientContext context, BotListRequest message)
                {
                }
                
                public override void PreprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 433_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                }
                
                public override void PreprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 433_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 433_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSend(Message message)
                {
                }
                
                public override void ProcessReceive(Message message)
                {
                    if (Is.PackageListReport(message))
                    {
                        PackageListReport PackageListReport = Cast.PackageListReport(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_PackageListReport_433_13_PackageListReport_.PackageListRequestClientContext_ = PackageListRequestClientContext_;
                            processor_.session_.Event_PackageListReport_433_13_PackageListReport_.PackageListReport_ = PackageListReport;
                            
                            processor_.session_.event_ = processor_.session_.Event_PackageListReport_433_13_PackageListReport_;
                        }
                        
                        PackageListRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientRequest({0}) : 0", processor_.id_);
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ClientRequest({0}) : 433_13 : {1}", processor_.id_, message.Info.Name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                    if (PackageListRequestClientContext_ != null)
                        contextList.Add(PackageListRequestClientContext_);
                }
                
                public PackageListRequestClientContext PackageListRequestClientContext_;
            }
            
            class State_439_13 : State
            {
                public State_439_13(ClientRequestProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 439_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                }
                
                public override void PreprocessSendBotListRequest(BotListRequestClientContext context, BotListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 439_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendBotListRequest(BotListRequestClientContext context, BotListRequest message)
                {
                }
                
                public override void PreprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 439_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                }
                
                public override void PreprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 439_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 439_13 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSend(Message message)
                {
                }
                
                public override void ProcessReceive(Message message)
                {
                    if (Is.SubscribeReport(message))
                    {
                        SubscribeReport SubscribeReport = Cast.SubscribeReport(message);
                        
                        if (processor_.session_.event_ == null)
                        {
                            processor_.session_.Event_SubscribeReport_439_13_SubscribeReport_.SubscribeRequestClientContext_ = SubscribeRequestClientContext_;
                            processor_.session_.Event_SubscribeReport_439_13_SubscribeReport_.SubscribeReport_ = SubscribeReport;
                            
                            processor_.session_.event_ = processor_.session_.Event_SubscribeReport_439_13_SubscribeReport_;
                        }
                        
                        SubscribeRequestClientContext_ = null;
                        
                        processor_.state_ = processor_.State_0_;
                        
                        if (processor_.session_.coreSession_.LogStates)
                            processor_.session_.coreSession_.LogState("ClientRequest({0}) : 0", processor_.id_);
                        
                        return;
                    }
                    
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ClientRequest({0}) : 439_13 : {1}", processor_.id_, message.Info.Name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                    if (SubscribeRequestClientContext_ != null)
                        contextList.Add(SubscribeRequestClientContext_);
                }
                
                public SubscribeRequestClientContext SubscribeRequestClientContext_;
            }
            
            class State_0 : State
            {
                public State_0(ClientRequestProcessor processor) : base(processor)
                {
                }
                
                public override void PreprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 0 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendAccountListRequest(AccountListRequestClientContext context, AccountListRequest message)
                {
                }
                
                public override void PreprocessSendBotListRequest(BotListRequestClientContext context, BotListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 0 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendBotListRequest(BotListRequestClientContext context, BotListRequest message)
                {
                }
                
                public override void PreprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 0 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendPackageListRequest(PackageListRequestClientContext context, PackageListRequest message)
                {
                }
                
                public override void PreprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 0 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSendSubscribeRequest(SubscribeRequestClientContext context, SubscribeRequest message)
                {
                }
                
                public override void PreprocessSend(Message message)
                {
                    throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ClientRequest({2}) : 0 : {3}", processor_.session_.coreSession_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                }
                
                public override void PostprocessSend(Message message)
                {
                }
                
                public override void ProcessReceive(Message message)
                {
                    processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ClientRequest({0}) : 0 : {1}", processor_.id_, message.Info.Name));
                }
                
                public override void ProcessDisconnect(List<ClientContext> contextList)
                {
                }
            }
            
            ClientSession session_;
            string id_;
            
            State_416_9 State_416_9_;
            State_418_13 State_418_13_;
            State_427_13 State_427_13_;
            State_433_13 State_433_13_;
            State_439_13 State_439_13_;
            State_0 State_0_;
            
            State state_;
        }
        
        abstract class Event
        {
            public Event(ClientSession session)
            {
                session_ = session;
            }
            
            public abstract void Dispatch();
            
            protected ClientSession session_;
        }
        
        class Event_LoginReport_262_14_LoginReport : Event
        {
            public Event_LoginReport_262_14_LoginReport(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnLoginReport({0})", LoginReport_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnLoginReport(session_, LoginRequestClientContext_, LoginReport_);
                    }
                    catch
                    {
                    }
                }
                
                if (LoginRequestClientContext_ != null)
                    LoginRequestClientContext_.SetCompleted();
                
                LoginReport_ = new LoginReport();
                
                LoginRequestClientContext_ = null;
            }
            
            public LoginRequestClientContext LoginRequestClientContext_;
            
            public LoginReport LoginReport_;
        }
        
        class Event_LoginReport_1_262_14_LoginReport_1 : Event
        {
            public Event_LoginReport_1_262_14_LoginReport_1(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnLoginReport_1({0})", LoginReport_1_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnLoginReport_1(session_, LoginRequestClientContext_, LoginReport_1_);
                    }
                    catch
                    {
                    }
                }
                
                if (LoginRequestClientContext_ != null)
                    LoginRequestClientContext_.SetCompleted();
                
                LoginReport_1_ = new LoginReport_1();
                
                LoginRequestClientContext_ = null;
            }
            
            public LoginRequestClientContext LoginRequestClientContext_;
            
            public LoginReport_1 LoginReport_1_;
        }
        
        class Event_LoginReject_262_14_LoginReject : Event
        {
            public Event_LoginReject_262_14_LoginReject(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnLoginReject({0})", LoginReject_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnLoginReject(session_, LoginRequestClientContext_, LoginReject_);
                    }
                    catch
                    {
                    }
                }
                
                if (LoginRequestClientContext_ != null)
                    LoginRequestClientContext_.SetCompleted();
                
                LoginReject_ = new LoginReject();
                
                LoginRequestClientContext_ = null;
            }
            
            public LoginRequestClientContext LoginRequestClientContext_;
            
            public LoginReject LoginReject_;
        }
        
        class Event_LogoutReport_274_10_LogoutReport : Event
        {
            public Event_LogoutReport_274_10_LogoutReport(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnLogoutReport({0})", LogoutReport_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnLogoutReport(session_, LogoutReport_);
                    }
                    catch
                    {
                    }
                }
                
                LogoutReport_ = new LogoutReport();
            }
            
            public LogoutReport LogoutReport_;
        }
        
        class Event_LogoutReport_292_14_LogoutReport : Event
        {
            public Event_LogoutReport_292_14_LogoutReport(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnLogoutReport({0})", LogoutReport_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnLogoutReport(session_, LogoutRequestClientContext_, LogoutReport_);
                    }
                    catch
                    {
                    }
                }
                
                if (LogoutRequestClientContext_ != null)
                    LogoutRequestClientContext_.SetCompleted();
                
                LogoutReport_ = new LogoutReport();
                
                LogoutRequestClientContext_ = null;
            }
            
            public LogoutRequestClientContext LogoutRequestClientContext_;
            
            public LogoutReport LogoutReport_;
        }
        
        class Event_AccountModelUpdate_365_9_AccountModelUpdate : Event
        {
            public Event_AccountModelUpdate_365_9_AccountModelUpdate(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnAccountModelUpdate({0})", AccountModelUpdate_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnAccountModelUpdate(session_, AccountModelUpdate_);
                    }
                    catch
                    {
                    }
                }
                
                AccountModelUpdate_ = new AccountModelUpdate();
            }
            
            public AccountModelUpdate AccountModelUpdate_;
        }
        
        class Event_AccountModelUpdate_1_365_9_AccountModelUpdate_1 : Event
        {
            public Event_AccountModelUpdate_1_365_9_AccountModelUpdate_1(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnAccountModelUpdate_1({0})", AccountModelUpdate_1_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnAccountModelUpdate_1(session_, AccountModelUpdate_1_);
                    }
                    catch
                    {
                    }
                }
                
                AccountModelUpdate_1_ = new AccountModelUpdate_1();
            }
            
            public AccountModelUpdate_1 AccountModelUpdate_1_;
        }
        
        class Event_BotModelUpdate_365_9_BotModelUpdate : Event
        {
            public Event_BotModelUpdate_365_9_BotModelUpdate(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnBotModelUpdate({0})", BotModelUpdate_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnBotModelUpdate(session_, BotModelUpdate_);
                    }
                    catch
                    {
                    }
                }
                
                BotModelUpdate_ = new BotModelUpdate();
            }
            
            public BotModelUpdate BotModelUpdate_;
        }
        
        class Event_PackageModelUpdate_365_9_PackageModelUpdate : Event
        {
            public Event_PackageModelUpdate_365_9_PackageModelUpdate(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnPackageModelUpdate({0})", PackageModelUpdate_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnPackageModelUpdate(session_, PackageModelUpdate_);
                    }
                    catch
                    {
                    }
                }
                
                PackageModelUpdate_ = new PackageModelUpdate();
            }
            
            public PackageModelUpdate PackageModelUpdate_;
        }
        
        class Event_BotStateUpdate_365_9_BotStateUpdate : Event
        {
            public Event_BotStateUpdate_365_9_BotStateUpdate(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnBotStateUpdate({0})", BotStateUpdate_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnBotStateUpdate(session_, BotStateUpdate_);
                    }
                    catch
                    {
                    }
                }
                
                BotStateUpdate_ = new BotStateUpdate();
            }
            
            public BotStateUpdate BotStateUpdate_;
        }
        
        class Event_AccountStateUpdate_365_9_AccountStateUpdate : Event
        {
            public Event_AccountStateUpdate_365_9_AccountStateUpdate(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnAccountStateUpdate({0})", AccountStateUpdate_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnAccountStateUpdate(session_, AccountStateUpdate_);
                    }
                    catch
                    {
                    }
                }
                
                AccountStateUpdate_ = new AccountStateUpdate();
            }
            
            public AccountStateUpdate AccountStateUpdate_;
        }
        
        class Event_AccountListReport_418_13_AccountListReport : Event
        {
            public Event_AccountListReport_418_13_AccountListReport(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnAccountListReport({0})", AccountListReport_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnAccountListReport(session_, AccountListRequestClientContext_, AccountListReport_);
                    }
                    catch
                    {
                    }
                }
                
                if (AccountListRequestClientContext_ != null)
                    AccountListRequestClientContext_.SetCompleted();
                
                AccountListReport_ = new AccountListReport();
                
                AccountListRequestClientContext_ = null;
            }
            
            public AccountListRequestClientContext AccountListRequestClientContext_;
            
            public AccountListReport AccountListReport_;
        }
        
        class Event_AccountListReport_1_418_13_AccountListReport_1 : Event
        {
            public Event_AccountListReport_1_418_13_AccountListReport_1(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnAccountListReport_1({0})", AccountListReport_1_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnAccountListReport_1(session_, AccountListRequestClientContext_, AccountListReport_1_);
                    }
                    catch
                    {
                    }
                }
                
                if (AccountListRequestClientContext_ != null)
                    AccountListRequestClientContext_.SetCompleted();
                
                AccountListReport_1_ = new AccountListReport_1();
                
                AccountListRequestClientContext_ = null;
            }
            
            public AccountListRequestClientContext AccountListRequestClientContext_;
            
            public AccountListReport_1 AccountListReport_1_;
        }
        
        class Event_BotListReport_427_13_BotListReport : Event
        {
            public Event_BotListReport_427_13_BotListReport(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnBotListReport({0})", BotListReport_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnBotListReport(session_, BotListRequestClientContext_, BotListReport_);
                    }
                    catch
                    {
                    }
                }
                
                if (BotListRequestClientContext_ != null)
                    BotListRequestClientContext_.SetCompleted();
                
                BotListReport_ = new BotListReport();
                
                BotListRequestClientContext_ = null;
            }
            
            public BotListRequestClientContext BotListRequestClientContext_;
            
            public BotListReport BotListReport_;
        }
        
        class Event_PackageListReport_433_13_PackageListReport : Event
        {
            public Event_PackageListReport_433_13_PackageListReport(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnPackageListReport({0})", PackageListReport_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnPackageListReport(session_, PackageListRequestClientContext_, PackageListReport_);
                    }
                    catch
                    {
                    }
                }
                
                if (PackageListRequestClientContext_ != null)
                    PackageListRequestClientContext_.SetCompleted();
                
                PackageListReport_ = new PackageListReport();
                
                PackageListRequestClientContext_ = null;
            }
            
            public PackageListRequestClientContext PackageListRequestClientContext_;
            
            public PackageListReport PackageListReport_;
        }
        
        class Event_SubscribeReport_439_13_SubscribeReport : Event
        {
            public Event_SubscribeReport_439_13_SubscribeReport(ClientSession clientSession) : base(clientSession)
            {
            }
            
            public override void Dispatch()
            {
                if (session_.coreSession_.LogEvents)
                    session_.coreSession_.LogEvent("OnSubscribeReport({0})", SubscribeReport_.ToString());
                
                if (session_.listener_ != null)
                {
                    try
                    {
                        session_.listener_.OnSubscribeReport(session_, SubscribeRequestClientContext_, SubscribeReport_);
                    }
                    catch
                    {
                    }
                }
                
                if (SubscribeRequestClientContext_ != null)
                    SubscribeRequestClientContext_.SetCompleted();
                
                SubscribeReport_ = new SubscribeReport();
                
                SubscribeRequestClientContext_ = null;
            }
            
            public SubscribeRequestClientContext SubscribeRequestClientContext_;
            
            public SubscribeReport SubscribeReport_;
        }
        
        class CoreClientSessionListener : Core.ClientSessionListener
        {
            public CoreClientSessionListener(ClientSession session)
            {
                session_ = session;
            }
            
            public override void OnConnect(Core.ClientSession clientSession)
            {
                session_.OnCoreConnect();
            }
            
            public override void OnConnectError(Core.ClientSession clientSession, string text)
            {
                session_.OnCoreConnectError(text);
            }
            
            public override void OnDisconnect(Core.ClientSession clientSession, string text)
            {
                session_.OnCoreDisconnect(text);
            }
            
            public override void OnReceive(Core.ClientSession clientSession, Message message)
            {
                session_.OnCoreReceive(message);
            }
            
            public override void OnSend(Core.ClientSession clientSession)
            {
                session_.OnCoreSend();
            }
            
            ClientSession session_;
        }
        
        void OnCoreConnect()
        {
            try
            {
                ConnectClientContext connectContext;
                
                lock (stateMutex_)
                {
                    ClientProcessor_ = new ClientProcessor(this);
                    
                    ClientUpdateProcessorDictionary_ = new SortedDictionary<string, ClientUpdateProcessor>();
                    
                    ClientRequestProcessorDictionary_ = new SortedDictionary<string, ClientRequestProcessor>();
                    
                    connectContext = connectContext_;
                    connectContext_ = null;
                }
                
                if (connectContext != null)
                {
                    if (listener_ != null)
                    {
                        try
                        {
                            listener_.OnConnect(this, connectContext);
                        }
                        catch
                        {
                        }
                    }
                    
                    connectContext.SetCompleted();
                }
                else
                {
                    if (listener_ != null)
                    {
                        try
                        {
                            listener_.OnConnect(this);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                coreSession_.LogError(exception.Message);
            }
        }
        
        void OnCoreConnectError(string text)
        {
            try
            {
                ConnectClientContext connectContext;
                
                lock (stateMutex_)
                {
                    connectContext = connectContext_;
                    connectContext_ = null;
                }
                
                if (connectContext != null)
                {
                    if (listener_ != null)
                    {
                        try
                        {
                            listener_.OnConnectError(this, connectContext, text);
                        }
                        catch
                        {
                        }
                    }
                    
                    connectContext.SetCompleted();
                }
                else
                {
                    if (listener_ != null)
                    {
                        try
                        {
                            listener_.OnConnectError(this, text);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                coreSession_.LogError(exception.Message);
            }
        }
        
        void OnCoreDisconnect(string text)
        {
            try
            {
                DisconnectClientContext disconnectContext;
                List<ClientContext> contexList = new List<ClientContext>();
                
                lock (stateMutex_)
                {
                    disconnectContext = disconnectContext_;
                    disconnectContext_ = null;
                    
                    foreach(var processor in ClientUpdateProcessorDictionary_)
                        processor.Value.ProcessDisconnect(contexList);
                    
                    ClientUpdateProcessorDictionary_ = null;
                    
                    foreach(var processor in ClientRequestProcessorDictionary_)
                        processor.Value.ProcessDisconnect(contexList);
                    
                    ClientRequestProcessorDictionary_ = null;
                    
                    ClientProcessor_.ProcessDisconnect(contexList);
                    ClientProcessor_ = null;
                }
                
                if (disconnectContext != null)
                {
                    if (listener_ != null)
                    {
                        try
                        {
                            listener_.OnDisconnect(this, disconnectContext, contexList.ToArray(), text);
                        }
                        catch
                        {
                        }
                    }
                    
                    foreach (ClientContext context in contexList)
                        context.SetCompleted();
                    
                    disconnectContext.SetCompleted();
                }
                else
                {
                    if (listener_ != null)
                    {
                        try
                        {
                            listener_.OnDisconnect(this, contexList.ToArray(), text);
                        }
                        catch
                        {
                        }
                    }
                    
                    foreach (ClientContext context in contexList)
                        context.SetCompleted();
                }
            }
            catch (Exception exception)
            {
                coreSession_.LogError(exception.Message);
            }
        }
        
        void OnCoreReceive(Message message)
        {
            try
            {
                lock (stateMutex_)
                {
                    ClientProcessor_.ProcessReceive(message);
                    
                    if (Is.Update(message))
                    {
                        Update Update = Cast.Update(message);
                        
                        string key = Update.Id;
                        ClientUpdateProcessor ClientUpdateProcessor;
                        
                        if (! ClientUpdateProcessorDictionary_.TryGetValue(key, out ClientUpdateProcessor))
                        {
                            ClientUpdateProcessor = new ClientUpdateProcessor(this, key);
                            ClientUpdateProcessorDictionary_.Add(key, ClientUpdateProcessor);
                        }
                        
                        ClientUpdateProcessor.ProcessReceive(message);
                        
                        ClientUpdate:
                        
                        if (ClientUpdateProcessor.Completed)
                            ClientUpdateProcessorDictionary_.Remove(key);
                        
                        goto Client;
                    }
                    
                    if (Is.Request(message))
                    {
                        Request Request = Cast.Request(message);
                        
                        string key = Request.Id;
                        ClientRequestProcessor ClientRequestProcessor;
                        
                        if (! ClientRequestProcessorDictionary_.TryGetValue(key, out ClientRequestProcessor))
                        {
                            ClientRequestProcessor = new ClientRequestProcessor(this, key);
                            ClientRequestProcessorDictionary_.Add(key, ClientRequestProcessor);
                        }
                        
                        ClientRequestProcessor.ProcessReceive(message);
                        
                        ClientRequest:
                        
                        if (ClientRequestProcessor.Completed)
                            ClientRequestProcessorDictionary_.Remove(key);
                        
                        goto Client;
                    }
                    
                    if (Is.Report(message))
                    {
                        Report Report = Cast.Report(message);
                        
                        string key = Report.RequestId;
                        ClientRequestProcessor ClientRequestProcessor;
                        
                        if (! ClientRequestProcessorDictionary_.TryGetValue(key, out ClientRequestProcessor))
                        {
                            ClientRequestProcessor = new ClientRequestProcessor(this, key);
                            ClientRequestProcessorDictionary_.Add(key, ClientRequestProcessor);
                        }
                        
                        ClientRequestProcessor.ProcessReceive(message);
                        
                        ClientRequest:
                        
                        if (ClientRequestProcessor.Completed)
                            ClientRequestProcessorDictionary_.Remove(key);
                        
                        goto Client;
                    }
                    
                    Client:
                    ;
                }
                
                if (event_ != null)
                {
                    event_.Dispatch();
                    event_ = null;
                }
                else
                {
                    if (coreSession_.LogEvents)
                        coreSession_.LogEvent("OnReceive({0})", message.ToString());
                    
                    if (listener_ != null)
                    {
                        try
                        {
                            listener_.OnReceive(this, message);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                coreSession_.LogError(exception.Message);
            }
        }
        
        void OnCoreSend()
        {
            try
            {
                if (listener_ != null)
                {
                    try
                    {
                        listener_.OnSend(this);
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception exception)
            {
                coreSession_.LogError(exception.Message);
            }
        }
        
        Core.ClientSession coreSession_;
        CoreClientSessionListener coreSessionListener_;
        
        ClientSessionListener listener_;
        
        object stateMutex_;
        ConnectClientContext connectContext_;
        DisconnectClientContext disconnectContext_;
        bool connected_;
        
        ClientProcessor ClientProcessor_;
        SortedDictionary<string, ClientUpdateProcessor> ClientUpdateProcessorDictionary_;
        SortedDictionary<string, ClientRequestProcessor> ClientRequestProcessorDictionary_;
        
        Event_LoginReport_262_14_LoginReport Event_LoginReport_262_14_LoginReport_;
        Event_LoginReport_1_262_14_LoginReport_1 Event_LoginReport_1_262_14_LoginReport_1_;
        Event_LoginReject_262_14_LoginReject Event_LoginReject_262_14_LoginReject_;
        Event_LogoutReport_274_10_LogoutReport Event_LogoutReport_274_10_LogoutReport_;
        Event_LogoutReport_292_14_LogoutReport Event_LogoutReport_292_14_LogoutReport_;
        Event_AccountModelUpdate_365_9_AccountModelUpdate Event_AccountModelUpdate_365_9_AccountModelUpdate_;
        Event_AccountModelUpdate_1_365_9_AccountModelUpdate_1 Event_AccountModelUpdate_1_365_9_AccountModelUpdate_1_;
        Event_BotModelUpdate_365_9_BotModelUpdate Event_BotModelUpdate_365_9_BotModelUpdate_;
        Event_PackageModelUpdate_365_9_PackageModelUpdate Event_PackageModelUpdate_365_9_PackageModelUpdate_;
        Event_BotStateUpdate_365_9_BotStateUpdate Event_BotStateUpdate_365_9_BotStateUpdate_;
        Event_AccountStateUpdate_365_9_AccountStateUpdate Event_AccountStateUpdate_365_9_AccountStateUpdate_;
        Event_AccountListReport_418_13_AccountListReport Event_AccountListReport_418_13_AccountListReport_;
        Event_AccountListReport_1_418_13_AccountListReport_1 Event_AccountListReport_1_418_13_AccountListReport_1_;
        Event_BotListReport_427_13_BotListReport Event_BotListReport_427_13_BotListReport_;
        Event_PackageListReport_433_13_PackageListReport Event_PackageListReport_433_13_PackageListReport_;
        Event_SubscribeReport_439_13_SubscribeReport Event_SubscribeReport_439_13_SubscribeReport_;
        
        Event event_;
    }
    
    class ClientSessionListener
    {
        public virtual void OnConnect(ClientSession clientSession, ConnectClientContext connectContext)
        {
        }
        
        public virtual void OnConnect(ClientSession clientSession)
        {
        }
        
        public virtual void OnConnectError(ClientSession clientSession, ConnectClientContext connectContext, string text)
        {
        }
        
        public virtual void OnConnectError(ClientSession clientSession, string text)
        {
        }
        
        public virtual void OnDisconnect(ClientSession clientSession, DisconnectClientContext disconnectContext, ClientContext[] contexts, string text)
        {
        }
        
        public virtual void OnDisconnect(ClientSession clientSession, ClientContext[] contexts, string text)
        {
        }
        
        public virtual void OnLoginReport(ClientSession session, LoginRequestClientContext LoginRequestClientContext, LoginReport message)
        {
        }
        
        public virtual void OnLoginReport_1(ClientSession session, LoginRequestClientContext LoginRequestClientContext, LoginReport_1 message)
        {
        }
        
        public virtual void OnLoginReject(ClientSession session, LoginRequestClientContext LoginRequestClientContext, LoginReject message)
        {
        }
        
        public virtual void OnLogoutReport(ClientSession session, LogoutReport message)
        {
        }
        
        public virtual void OnLogoutReport(ClientSession session, LogoutRequestClientContext LogoutRequestClientContext, LogoutReport message)
        {
        }
        
        public virtual void OnAccountModelUpdate(ClientSession session, AccountModelUpdate message)
        {
        }
        
        public virtual void OnAccountModelUpdate_1(ClientSession session, AccountModelUpdate_1 message)
        {
        }
        
        public virtual void OnBotModelUpdate(ClientSession session, BotModelUpdate message)
        {
        }
        
        public virtual void OnPackageModelUpdate(ClientSession session, PackageModelUpdate message)
        {
        }
        
        public virtual void OnBotStateUpdate(ClientSession session, BotStateUpdate message)
        {
        }
        
        public virtual void OnAccountStateUpdate(ClientSession session, AccountStateUpdate message)
        {
        }
        
        public virtual void OnAccountListReport(ClientSession session, AccountListRequestClientContext AccountListRequestClientContext, AccountListReport message)
        {
        }
        
        public virtual void OnAccountListReport_1(ClientSession session, AccountListRequestClientContext AccountListRequestClientContext, AccountListReport_1 message)
        {
        }
        
        public virtual void OnBotListReport(ClientSession session, BotListRequestClientContext BotListRequestClientContext, BotListReport message)
        {
        }
        
        public virtual void OnPackageListReport(ClientSession session, PackageListRequestClientContext PackageListRequestClientContext, PackageListReport message)
        {
        }
        
        public virtual void OnSubscribeReport(ClientSession session, SubscribeRequestClientContext SubscribeRequestClientContext, SubscribeReport message)
        {
        }
        
        public virtual void OnReceive(ClientSession clientSession, Message message)
        {
        }
        
        public virtual void OnSend(ClientSession clientSession)
        {
        }
    }
    
    class ServerContext : Context
    {
        public ServerContext(bool waitable) : base(waitable)
        {
        }
    }
    
    struct ServerLogOptions
    {
        public ServerLogOptions(string directory)
        {
            Directory = directory;
#if DEBUG
            Events = true;
            States = false;
            Messages = true;
#else
            Events = false;
            States = false;
            Messages = false;
#endif
        }
        
        public string Directory;
        
        public bool Events;
        
        public bool States;
        
        public bool Messages;
    }
    
    struct ServerOptions
    {
        public ServerOptions(int listeningPort)
        {
            ListeningPort = listeningPort;
            ConnectionType = ConnectionType.Socket;
            Certificate = null;
            RequireClientCertificate = false;
            SessionMaxCount = 1000;
            SessionThreadCount = 1;
            HeartbeatInterval = 10000;
            SendBufferSize = 128 * 1024;
            OptimizationType = OptimizationType.Latency;
            Log = new ServerLogOptions("Logs");
        }
        
        public int ListeningPort;
        
        public ConnectionType ConnectionType;
        
        public X509Certificate2 Certificate;
        
        public bool RequireClientCertificate;
        
        public int SessionMaxCount;
        
        public int SessionThreadCount;
        
        public int HeartbeatInterval;
        
        public int SendBufferSize;
        
        public OptimizationType OptimizationType;
        
        public ServerLogOptions Log;
    }
    
    class Server
    {
        public Server(string name, ServerOptions options)
        {
            Core.ServerOptions coreOptions = new Core.ServerOptions(options.ListeningPort);
            coreOptions.ConnectionType = options.ConnectionType;
            coreOptions.Certificate = options.Certificate;
            coreOptions.RequireClientCertificate = options.RequireClientCertificate;
            coreOptions.SessionMaxCount = options.SessionMaxCount;
            coreOptions.SessionThreadCount = options.SessionThreadCount;
            coreOptions.HeartbeatInterval = options.HeartbeatInterval;
            coreOptions.SendBufferSize = options.SendBufferSize;
            coreOptions.OptimizationType = options.OptimizationType;
            coreOptions.Log.Directory = options.Log.Directory;
            coreOptions.Log.Events = options.Log.Events;
            coreOptions.Log.States = options.Log.States;
            coreOptions.Log.Messages = options.Log.Messages;
            
            coreServer_ = new Core.Server(name, Info.BotAgent, coreOptions);
            coreServerListener_ = new CoreServerListener(this);
            coreServer_.Listener = coreServerListener_;
            
            stateMutex_ = new object();
            started_ = false;
            
            sessionDictionary_ = new Dictionary<int, Session>();
        }
        
        public string Name
        {
            get { return coreServer_.Name; }
        }
        
        public ServerListener Listener
        {
            set
            {
                lock (stateMutex_)
                {
                    if (started_)
                        throw new Exception(string.Format("Server is started : {0}", coreServer_.Name));
                    
                    listener_ = value;
                }
            }
            
            get
            {
                lock (stateMutex_)
                {
                    return listener_;
                }
            }
        }
        
        public void Start()
        {
            lock (stateMutex_)
            {
                coreServer_.Start();
                started_ = true;
            }
        }
        
        public void Stop(string text)
        {
            lock (stateMutex_)
            {
                if (started_)
                {
                    started_ = false;
                    coreServer_.Stop(text);
                }
            }
        }
        
        public void Join()
        {
            coreServer_.Join();
        }
        
        public int SessionCount
        {
            get { return coreServer_.SessionCount; }
        }
        
        public void Broadcast(Message message)
        {
            lock (stateMutex_)
            {
                if (! started_)
                    throw new Exception(string.Format("Server is not active : {0}", coreServer_.Name));
                
                foreach (var item in sessionDictionary_)
                {
                    try
                    {
                        item.Value.Send(message);
                    }
                    catch (Exception exception)
                    {
                        coreServer_.LogError(exception.Message);
                    }
                }
            }
        }
        
        public void Send(int sessionId, Message message)
        {
            lock (stateMutex_)
            {
                if (! started_)
                    throw new Exception(string.Format("Server is not active : {0}", coreServer_.Name));
                
                Session session;
                if (! sessionDictionary_.TryGetValue(sessionId, out session))
                    throw new Exception(string.Format("Session does not exist : {0}({1})", coreServer_.Name, sessionId));
                
                session.Send(message);
            }
        }
        
        public void Disconnect(string text)
        {
            coreServer_.Disconnect(text);
        }
        
        public void Disconnect(int sessionId, string text)
        {
            coreServer_.Disconnect(sessionId, text);
        }
        
        public bool LogEvents
        {
            set { coreServer_.LogEvents = value; }
            
            get { return coreServer_.LogEvents; }
        }
        
        public bool LogStatets
        {
            set { coreServer_.LogStates = value; }
            
            get { return coreServer_.LogStates; }
        }
        
        public bool LogMessages
        {
            set { coreServer_.LogMessages = value; }
            
            get { return coreServer_.LogMessages; }
        }
        
        public void LogError(string format, params object[] args)
        {
            coreServer_.LogError(format, args);
        }
        
        public void LogWarning(string format, params object[] args)
        {
            coreServer_.LogWarning(format, args);
        }
        
        public void LogInfo(string format, params object[] args)
        {
            coreServer_.LogWarning(format, args);
        }
        
        public void LogEvent(string format, params object[] args)
        {
            coreServer_.LogError(format, args);
        }
        
        public void LogState(string format, params object[] args)
        {
            coreServer_.LogState(format, args);
        }
        
        public class Session
        {
            internal Session(Server server, Core.Server.Session coreSession)
            {
                server_ = server;
                coreSession_ = coreSession;
                
                data_ = null;
                
                stateMutex_ = new object();
                
                Event_LoginRequest_312_10_LoginRequest_ = new Event_LoginRequest_312_10_LoginRequest(this);
                Event_LogoutRequest_326_10_LogoutRequest_ = new Event_LogoutRequest_326_10_LogoutRequest(this);
                Event_AccountListRequest_451_9_AccountListRequest_ = new Event_AccountListRequest_451_9_AccountListRequest(this);
                Event_BotListRequest_451_9_BotListRequest_ = new Event_BotListRequest_451_9_BotListRequest(this);
                Event_PackageListRequest_451_9_PackageListRequest_ = new Event_PackageListRequest_451_9_PackageListRequest(this);
                Event_SubscribeRequest_451_9_SubscribeRequest_ = new Event_SubscribeRequest_451_9_SubscribeRequest(this);
                
                event_ = null;
            }
            
            public int Id
            {
                get { return coreSession_.Id; }
            }
            
            public Guid Guid
            {
                get { return coreSession_.Guid; }
            }
            
            public object Data
            {
                set { data_ = value; }
                
                get { return data_; }
            }
            
            public int ClientMajorVersion
            {
                get { return coreSession_.ClientMajorVersion; }
            }
            
            public int ClientMinorVersion
            {
                get { return coreSession_.ClientMinorVersion; }
            }
            
            public Guid ClientGuid
            {
                get { return coreSession_.ClientGuid; }
            }
            
            public void Disconnect(string text)
            {
                lock (stateMutex_)
                {
                    coreSession_.Disconnect(text);
                }
            }
            
            public void Send(Message message)
            {
                lock (stateMutex_)
                {
                    if (ServerProcessor_ == null)
                        throw new Exception(string.Format("Session is not active : {0}({1})", server_.coreServer_.Name, coreSession_.Guid));
                    
                    if (coreSession_.LogEvents)
                        coreSession_.LogEvent("Send({0})", message.ToString());
                    
                    ServerProcessor_.PreprocessSend(message);
                    
                    if (Is.Update(message))
                    {
                        Update Update = Cast.Update(message);
                        
                        string key = Update.Id;
                        ServerUpdateProcessor ServerUpdateProcessor;
                        
                        if (! ServerUpdateProcessorDictionary_.TryGetValue(key, out ServerUpdateProcessor))
                        {
                            ServerUpdateProcessor = new ServerUpdateProcessor(this, key);
                            ServerUpdateProcessorDictionary_.Add(key, ServerUpdateProcessor);
                        }
                        
                        ServerUpdateProcessor.PreprocessSend(message);
                        
                        coreSession_.Send(message);
                        
                        ServerUpdate:
                        
                        ServerUpdateProcessor.PostprocessSend(message);
                        
                        if (ServerUpdateProcessor.Completed)
                            ServerUpdateProcessorDictionary_.Remove(key);
                        
                        goto Server;
                    }
                    
                    if (Is.Request(message))
                    {
                        Request Request = Cast.Request(message);
                        
                        string key = Request.Id;
                        ServerRequestProcessor ServerRequestProcessor;
                        
                        if (! ServerRequestProcessorDictionary_.TryGetValue(key, out ServerRequestProcessor))
                        {
                            ServerRequestProcessor = new ServerRequestProcessor(this, key);
                            ServerRequestProcessorDictionary_.Add(key, ServerRequestProcessor);
                        }
                        
                        ServerRequestProcessor.PreprocessSend(message);
                        
                        coreSession_.Send(message);
                        
                        ServerRequest:
                        
                        ServerRequestProcessor.PostprocessSend(message);
                        
                        if (ServerRequestProcessor.Completed)
                            ServerRequestProcessorDictionary_.Remove(key);
                        
                        goto Server;
                    }
                    
                    if (Is.Report(message))
                    {
                        Report Report = Cast.Report(message);
                        
                        string key = Report.RequestId;
                        ServerRequestProcessor ServerRequestProcessor;
                        
                        if (! ServerRequestProcessorDictionary_.TryGetValue(key, out ServerRequestProcessor))
                        {
                            ServerRequestProcessor = new ServerRequestProcessor(this, key);
                            ServerRequestProcessorDictionary_.Add(key, ServerRequestProcessor);
                        }
                        
                        ServerRequestProcessor.PreprocessSend(message);
                        
                        coreSession_.Send(message);
                        
                        ServerRequest:
                        
                        ServerRequestProcessor.PostprocessSend(message);
                        
                        if (ServerRequestProcessor.Completed)
                            ServerRequestProcessorDictionary_.Remove(key);
                        
                        goto Server;
                    }
                    
                    coreSession_.Send(message);
                    
                    Server:
                    
                    ServerProcessor_.PostprocessSend(message);
                }
            }
            
            public void LogError(string format, params object[] args)
            {
                coreSession_.LogError(format, args);
            }
            
            public void LogWarning(string format, params object[] args)
            {
                coreSession_.LogWarning(format, args);
            }
            
            public void LogInfo(string format, params object[] args)
            {
                coreSession_.LogWarning(format, args);
            }
            
            public void LogEvent(string format, params object[] args)
            {
                coreSession_.LogError(format, args);
            }
            
            public void LogState(string format, params object[] args)
            {
                coreSession_.LogState(format, args);
            }
            
            class ServerProcessor
            {
                public ServerProcessor(Session session)
                {
                    session_ = session;
                    
                    State_312_10_ = new State_312_10(this);
                    State_314_14_ = new State_314_14(this);
                    State_326_10_ = new State_326_10(this);
                    State_344_14_ = new State_344_14(this);
                    State_0_ = new State_0(this);
                    
                    state_ = State_312_10_;
                    
                    if (session_.coreSession_.LogStates)
                        session_.coreSession_.LogState("Server : 312_10");
                }
                
                public bool Completed
                {
                    get { return state_ == State_0_; }
                }
                
                public void PreprocessSend(Message message)
                {
                    state_.PreprocessSend(message);
                }
                
                public void PostprocessSend(Message message)
                {
                    state_.PostprocessSend(message);
                }
                
                public void ProcessReceive(Message message)
                {
                    state_.ProcessReceive(message);
                }
                
                public void ProcessDisconnect(List<ServerContext> contextList)
                {
                    state_.ProcessDisconnect(contextList);
                }
                
                abstract class State
                {
                    public State(ServerProcessor processor)
                    {
                        processor_ = processor;
                    }
                    
                    public abstract void PreprocessSend(Message message);
                    
                    public abstract void PostprocessSend(Message message);
                    
                    public abstract void ProcessReceive(Message message);
                    
                    public abstract void ProcessDisconnect(List<ServerContext> contextList);
                    
                    protected ServerProcessor processor_;
                }
                
                class State_312_10 : State
                {
                    public State_312_10(ServerProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Server() : 312_10 : {2}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, message.Info.Name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        if (Is.LoginRequest(message))
                        {
                            LoginRequest LoginRequest = Cast.LoginRequest(message);
                            
                            if (processor_.session_.event_ == null)
                            {
                                processor_.session_.Event_LoginRequest_312_10_LoginRequest_.LoginRequest_ = LoginRequest;
                                
                                processor_.session_.event_ = processor_.session_.Event_LoginRequest_312_10_LoginRequest_;
                            }
                            
                            processor_.state_ = processor_.State_314_14_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 314_14");
                            
                            return;
                        }
                        
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Server() : 312_10 : {0}", message.Info.Name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_314_14 : State
                {
                    public State_314_14(ServerProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.LoginReport(message))
                            return;
                        
                        if (Is.LoginReport_1(message))
                            return;
                        
                        if (Is.LoginReject(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Server() : 314_14 : {2}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, message.Info.Name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.LoginReport(message))
                        {
                            processor_.state_ = processor_.State_326_10_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 326_10");
                            
                            return;
                        }
                        
                        if (Is.LoginReport_1(message))
                        {
                            processor_.state_ = processor_.State_326_10_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 326_10");
                            
                            return;
                        }
                        
                        if (Is.LoginReject(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 0");
                            
                            return;
                        }
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Server() : 314_14 : {0}", message.Info.Name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_326_10 : State
                {
                    public State_326_10(ServerProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.Report(message))
                            return;
                        
                        if (Is.Update(message))
                            return;
                        
                        if (Is.LogoutReport(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Server() : 326_10 : {2}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, message.Info.Name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.Report(message))
                        {
                            processor_.state_ = processor_.State_326_10_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 326_10");
                            
                            return;
                        }
                        
                        if (Is.Update(message))
                        {
                            processor_.state_ = processor_.State_326_10_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 326_10");
                            
                            return;
                        }
                        
                        if (Is.LogoutReport(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 0");
                            
                            return;
                        }
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        if (Is.Request(message))
                        {
                            Request Request = Cast.Request(message);
                            
                            processor_.state_ = processor_.State_326_10_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 326_10");
                            
                            return;
                        }
                        
                        if (Is.LogoutRequest(message))
                        {
                            LogoutRequest LogoutRequest = Cast.LogoutRequest(message);
                            
                            if (processor_.session_.event_ == null)
                            {
                                processor_.session_.Event_LogoutRequest_326_10_LogoutRequest_.LogoutRequest_ = LogoutRequest;
                                
                                processor_.session_.event_ = processor_.session_.Event_LogoutRequest_326_10_LogoutRequest_;
                            }
                            
                            processor_.state_ = processor_.State_344_14_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 344_14");
                            
                            return;
                        }
                        
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Server() : 326_10 : {0}", message.Info.Name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_344_14 : State
                {
                    public State_344_14(ServerProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.Report(message))
                            return;
                        
                        if (Is.Update(message))
                            return;
                        
                        if (Is.LogoutReport(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Server() : 344_14 : {2}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, message.Info.Name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.Report(message))
                        {
                            processor_.state_ = processor_.State_344_14_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 344_14");
                            
                            return;
                        }
                        
                        if (Is.Update(message))
                        {
                            processor_.state_ = processor_.State_344_14_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 344_14");
                            
                            return;
                        }
                        
                        if (Is.LogoutReport(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("Server : 0");
                            
                            return;
                        }
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Server() : 344_14 : {0}", message.Info.Name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_0 : State
                {
                    public State_0(ServerProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : Server() : 0 : {2}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, message.Info.Name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : Server() : 0 : {0}", message.Info.Name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                Session session_;
                
                State_312_10 State_312_10_;
                State_314_14 State_314_14_;
                State_326_10 State_326_10_;
                State_344_14 State_344_14_;
                State_0 State_0_;
                
                State state_;
            }
            
            class ServerUpdateProcessor
            {
                public ServerUpdateProcessor(Session session, string id)
                {
                    session_ = session;
                    id_ = id;
                    
                    State_390_9_ = new State_390_9(this);
                    State_0_ = new State_0(this);
                    
                    state_ = State_390_9_;
                    
                    if (session_.coreSession_.LogStates)
                        session_.coreSession_.LogState("ServerUpdate({0}) : 390_9", id_);
                }
                
                public bool Completed
                {
                    get { return state_ == State_0_; }
                }
                
                public void PreprocessSend(Message message)
                {
                    state_.PreprocessSend(message);
                }
                
                public void PostprocessSend(Message message)
                {
                    state_.PostprocessSend(message);
                }
                
                public void ProcessReceive(Message message)
                {
                    state_.ProcessReceive(message);
                }
                
                public void ProcessDisconnect(List<ServerContext> contextList)
                {
                    state_.ProcessDisconnect(contextList);
                }
                
                abstract class State
                {
                    public State(ServerUpdateProcessor processor)
                    {
                        processor_ = processor;
                    }
                    
                    public abstract void PreprocessSend(Message message);
                    
                    public abstract void PostprocessSend(Message message);
                    
                    public abstract void ProcessReceive(Message message);
                    
                    public abstract void ProcessDisconnect(List<ServerContext> contextList);
                    
                    protected ServerUpdateProcessor processor_;
                }
                
                class State_390_9 : State
                {
                    public State_390_9(ServerUpdateProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.AccountModelUpdate(message))
                            return;
                        
                        if (Is.AccountModelUpdate_1(message))
                            return;
                        
                        if (Is.BotModelUpdate(message))
                            return;
                        
                        if (Is.PackageModelUpdate(message))
                            return;
                        
                        if (Is.BotStateUpdate(message))
                            return;
                        
                        if (Is.AccountStateUpdate(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ServerUpdate({2}) : 390_9 : {3}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.AccountModelUpdate(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerUpdate({0}) : 0", processor_.id_);
                            
                            return;
                        }
                        
                        if (Is.AccountModelUpdate_1(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerUpdate({0}) : 0", processor_.id_);
                            
                            return;
                        }
                        
                        if (Is.BotModelUpdate(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerUpdate({0}) : 0", processor_.id_);
                            
                            return;
                        }
                        
                        if (Is.PackageModelUpdate(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerUpdate({0}) : 0", processor_.id_);
                            
                            return;
                        }
                        
                        if (Is.BotStateUpdate(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerUpdate({0}) : 0", processor_.id_);
                            
                            return;
                        }
                        
                        if (Is.AccountStateUpdate(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerUpdate({0}) : 0", processor_.id_);
                            
                            return;
                        }
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ServerUpdate({0}) : 390_9 : {1}", processor_.id_, message.Info.Name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_0 : State
                {
                    public State_0(ServerUpdateProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ServerUpdate({2}) : 0 : {3}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ServerUpdate({0}) : 0 : {1}", processor_.id_, message.Info.Name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                Session session_;
                string id_;
                
                State_390_9 State_390_9_;
                State_0 State_0_;
                
                State state_;
            }
            
            class ServerRequestProcessor
            {
                public ServerRequestProcessor(Session session, string id)
                {
                    session_ = session;
                    id_ = id;
                    
                    State_451_9_ = new State_451_9(this);
                    State_453_13_ = new State_453_13(this);
                    State_462_13_ = new State_462_13(this);
                    State_468_13_ = new State_468_13(this);
                    State_474_13_ = new State_474_13(this);
                    State_0_ = new State_0(this);
                    
                    state_ = State_451_9_;
                    
                    if (session_.coreSession_.LogStates)
                        session_.coreSession_.LogState("ServerRequest({0}) : 451_9", id_);
                }
                
                public bool Completed
                {
                    get { return state_ == State_0_; }
                }
                
                public void PreprocessSend(Message message)
                {
                    state_.PreprocessSend(message);
                }
                
                public void PostprocessSend(Message message)
                {
                    state_.PostprocessSend(message);
                }
                
                public void ProcessReceive(Message message)
                {
                    state_.ProcessReceive(message);
                }
                
                public void ProcessDisconnect(List<ServerContext> contextList)
                {
                    state_.ProcessDisconnect(contextList);
                }
                
                abstract class State
                {
                    public State(ServerRequestProcessor processor)
                    {
                        processor_ = processor;
                    }
                    
                    public abstract void PreprocessSend(Message message);
                    
                    public abstract void PostprocessSend(Message message);
                    
                    public abstract void ProcessReceive(Message message);
                    
                    public abstract void ProcessDisconnect(List<ServerContext> contextList);
                    
                    protected ServerRequestProcessor processor_;
                }
                
                class State_451_9 : State
                {
                    public State_451_9(ServerRequestProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ServerRequest({2}) : 451_9 : {3}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        if (Is.AccountListRequest(message))
                        {
                            AccountListRequest AccountListRequest = Cast.AccountListRequest(message);
                            
                            if (processor_.session_.event_ == null)
                            {
                                processor_.session_.Event_AccountListRequest_451_9_AccountListRequest_.AccountListRequest_ = AccountListRequest;
                                
                                processor_.session_.event_ = processor_.session_.Event_AccountListRequest_451_9_AccountListRequest_;
                            }
                            
                            processor_.state_ = processor_.State_453_13_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerRequest({0}) : 453_13", processor_.id_);
                            
                            return;
                        }
                        
                        if (Is.BotListRequest(message))
                        {
                            BotListRequest BotListRequest = Cast.BotListRequest(message);
                            
                            if (processor_.session_.event_ == null)
                            {
                                processor_.session_.Event_BotListRequest_451_9_BotListRequest_.BotListRequest_ = BotListRequest;
                                
                                processor_.session_.event_ = processor_.session_.Event_BotListRequest_451_9_BotListRequest_;
                            }
                            
                            processor_.state_ = processor_.State_462_13_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerRequest({0}) : 462_13", processor_.id_);
                            
                            return;
                        }
                        
                        if (Is.PackageListRequest(message))
                        {
                            PackageListRequest PackageListRequest = Cast.PackageListRequest(message);
                            
                            if (processor_.session_.event_ == null)
                            {
                                processor_.session_.Event_PackageListRequest_451_9_PackageListRequest_.PackageListRequest_ = PackageListRequest;
                                
                                processor_.session_.event_ = processor_.session_.Event_PackageListRequest_451_9_PackageListRequest_;
                            }
                            
                            processor_.state_ = processor_.State_468_13_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerRequest({0}) : 468_13", processor_.id_);
                            
                            return;
                        }
                        
                        if (Is.SubscribeRequest(message))
                        {
                            SubscribeRequest SubscribeRequest = Cast.SubscribeRequest(message);
                            
                            if (processor_.session_.event_ == null)
                            {
                                processor_.session_.Event_SubscribeRequest_451_9_SubscribeRequest_.SubscribeRequest_ = SubscribeRequest;
                                
                                processor_.session_.event_ = processor_.session_.Event_SubscribeRequest_451_9_SubscribeRequest_;
                            }
                            
                            processor_.state_ = processor_.State_474_13_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerRequest({0}) : 474_13", processor_.id_);
                            
                            return;
                        }
                        
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ServerRequest({0}) : 451_9 : {1}", processor_.id_, message.Info.Name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_453_13 : State
                {
                    public State_453_13(ServerRequestProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.AccountListReport(message))
                            return;
                        
                        if (Is.AccountListReport_1(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ServerRequest({2}) : 453_13 : {3}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.AccountListReport(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerRequest({0}) : 0", processor_.id_);
                            
                            return;
                        }
                        
                        if (Is.AccountListReport_1(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerRequest({0}) : 0", processor_.id_);
                            
                            return;
                        }
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ServerRequest({0}) : 453_13 : {1}", processor_.id_, message.Info.Name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_462_13 : State
                {
                    public State_462_13(ServerRequestProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.BotListReport(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ServerRequest({2}) : 462_13 : {3}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.BotListReport(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerRequest({0}) : 0", processor_.id_);
                            
                            return;
                        }
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ServerRequest({0}) : 462_13 : {1}", processor_.id_, message.Info.Name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_468_13 : State
                {
                    public State_468_13(ServerRequestProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.PackageListReport(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ServerRequest({2}) : 468_13 : {3}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.PackageListReport(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerRequest({0}) : 0", processor_.id_);
                            
                            return;
                        }
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ServerRequest({0}) : 468_13 : {1}", processor_.id_, message.Info.Name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_474_13 : State
                {
                    public State_474_13(ServerRequestProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        if (Is.SubscribeReport(message))
                            return;
                        
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ServerRequest({2}) : 474_13 : {3}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                        if (Is.SubscribeReport(message))
                        {
                            processor_.state_ = processor_.State_0_;
                            
                            if (processor_.session_.coreSession_.LogStates)
                                processor_.session_.coreSession_.LogState("ServerRequest({0}) : 0", processor_.id_);
                            
                            return;
                        }
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ServerRequest({0}) : 474_13 : {1}", processor_.id_, message.Info.Name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                class State_0 : State
                {
                    public State_0(ServerRequestProcessor processor) : base(processor)
                    {
                    }
                    
                    public override void PreprocessSend(Message message)
                    {
                        throw new UnexpectedMessageException(string.Format("Session unexpected message : {0}({1}) : ServerRequest({2}) : 0 : {3}", processor_.session_.server_.coreServer_.Name, processor_.session_.coreSession_.Guid, processor_.id_, message.Info.Name));
                    }
                    
                    public override void PostprocessSend(Message message)
                    {
                    }
                    
                    public override void ProcessReceive(Message message)
                    {
                        processor_.session_.coreSession_.Disconnect(string.Format("Unexpected message : ServerRequest({0}) : 0 : {1}", processor_.id_, message.Info.Name));
                    }
                    
                    public override void ProcessDisconnect(List<ServerContext> contextList)
                    {
                    }
                }
                
                Session session_;
                string id_;
                
                State_451_9 State_451_9_;
                State_453_13 State_453_13_;
                State_462_13 State_462_13_;
                State_468_13 State_468_13_;
                State_474_13 State_474_13_;
                State_0 State_0_;
                
                State state_;
            }
            
            abstract class Event
            {
                public Event(Session session)
                {
                    session_ = session;
                }
                
                public abstract void Dispatch();
                
                protected Session session_;
            }
            
            class Event_LoginRequest_312_10_LoginRequest : Event
            {
                public Event_LoginRequest_312_10_LoginRequest(Session session) : base(session)
                {
                }
                
                public override void Dispatch()
                {
                    if (session_.coreSession_.LogEvents)
                        session_.coreSession_.LogEvent("OnLoginRequest({0})", LoginRequest_.ToString());
                    
                    if (session_.server_.listener_ != null)
                    {
                        try
                        {
                            session_.server_.listener_.OnLoginRequest(session_.server_, session_, LoginRequest_);
                        }
                        catch
                        {
                        }
                    }
                    
                    LoginRequest_ = new LoginRequest();
                }
                
                public LoginRequest LoginRequest_;
            }
            
            class Event_LogoutRequest_326_10_LogoutRequest : Event
            {
                public Event_LogoutRequest_326_10_LogoutRequest(Session session) : base(session)
                {
                }
                
                public override void Dispatch()
                {
                    if (session_.coreSession_.LogEvents)
                        session_.coreSession_.LogEvent("OnLogoutRequest({0})", LogoutRequest_.ToString());
                    
                    if (session_.server_.listener_ != null)
                    {
                        try
                        {
                            session_.server_.listener_.OnLogoutRequest(session_.server_, session_, LogoutRequest_);
                        }
                        catch
                        {
                        }
                    }
                    
                    LogoutRequest_ = new LogoutRequest();
                }
                
                public LogoutRequest LogoutRequest_;
            }
            
            class Event_AccountListRequest_451_9_AccountListRequest : Event
            {
                public Event_AccountListRequest_451_9_AccountListRequest(Session session) : base(session)
                {
                }
                
                public override void Dispatch()
                {
                    if (session_.coreSession_.LogEvents)
                        session_.coreSession_.LogEvent("OnAccountListRequest({0})", AccountListRequest_.ToString());
                    
                    if (session_.server_.listener_ != null)
                    {
                        try
                        {
                            session_.server_.listener_.OnAccountListRequest(session_.server_, session_, AccountListRequest_);
                        }
                        catch
                        {
                        }
                    }
                    
                    AccountListRequest_ = new AccountListRequest();
                }
                
                public AccountListRequest AccountListRequest_;
            }
            
            class Event_BotListRequest_451_9_BotListRequest : Event
            {
                public Event_BotListRequest_451_9_BotListRequest(Session session) : base(session)
                {
                }
                
                public override void Dispatch()
                {
                    if (session_.coreSession_.LogEvents)
                        session_.coreSession_.LogEvent("OnBotListRequest({0})", BotListRequest_.ToString());
                    
                    if (session_.server_.listener_ != null)
                    {
                        try
                        {
                            session_.server_.listener_.OnBotListRequest(session_.server_, session_, BotListRequest_);
                        }
                        catch
                        {
                        }
                    }
                    
                    BotListRequest_ = new BotListRequest();
                }
                
                public BotListRequest BotListRequest_;
            }
            
            class Event_PackageListRequest_451_9_PackageListRequest : Event
            {
                public Event_PackageListRequest_451_9_PackageListRequest(Session session) : base(session)
                {
                }
                
                public override void Dispatch()
                {
                    if (session_.coreSession_.LogEvents)
                        session_.coreSession_.LogEvent("OnPackageListRequest({0})", PackageListRequest_.ToString());
                    
                    if (session_.server_.listener_ != null)
                    {
                        try
                        {
                            session_.server_.listener_.OnPackageListRequest(session_.server_, session_, PackageListRequest_);
                        }
                        catch
                        {
                        }
                    }
                    
                    PackageListRequest_ = new PackageListRequest();
                }
                
                public PackageListRequest PackageListRequest_;
            }
            
            class Event_SubscribeRequest_451_9_SubscribeRequest : Event
            {
                public Event_SubscribeRequest_451_9_SubscribeRequest(Session session) : base(session)
                {
                }
                
                public override void Dispatch()
                {
                    if (session_.coreSession_.LogEvents)
                        session_.coreSession_.LogEvent("OnSubscribeRequest({0})", SubscribeRequest_.ToString());
                    
                    if (session_.server_.listener_ != null)
                    {
                        try
                        {
                            session_.server_.listener_.OnSubscribeRequest(session_.server_, session_, SubscribeRequest_);
                        }
                        catch
                        {
                        }
                    }
                    
                    SubscribeRequest_ = new SubscribeRequest();
                }
                
                public SubscribeRequest SubscribeRequest_;
            }
            
            internal void OnCoreConnect()
            {
                try
                {
                    lock (stateMutex_)
                    {
                        ServerProcessor_ = new ServerProcessor(this);
                        
                        ServerUpdateProcessorDictionary_ = new SortedDictionary<string, ServerUpdateProcessor>();
                        
                        ServerRequestProcessorDictionary_ = new SortedDictionary<string, ServerRequestProcessor>();
                    }
                    
                    if (server_.listener_ != null)
                    {
                        try
                        {
                            server_.listener_.OnConnect(server_, this);
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }
            }
            
            internal void OnCoreDisconnect(string text)
            {
                try
                {
                    List<ServerContext> contexList = new List<ServerContext>();
                    
                    lock (stateMutex_)
                    {
                        foreach(var processor in ServerUpdateProcessorDictionary_)
                            processor.Value.ProcessDisconnect(contexList);
                        
                        ServerUpdateProcessorDictionary_ = null;
                        
                        foreach(var processor in ServerRequestProcessorDictionary_)
                            processor.Value.ProcessDisconnect(contexList);
                        
                        ServerRequestProcessorDictionary_ = null;
                        
                        ServerProcessor_.ProcessDisconnect(contexList);
                        ServerProcessor_ = null;
                    }
                    
                    if (server_.listener_ != null)
                    {
                        try
                        {
                            server_.listener_.OnDisconnect(server_, this, contexList.ToArray(), text);
                        }
                        catch
                        {
                        }
                    }
                    
                    foreach (ServerContext context in contexList)
                        context.SetCompleted();
                }
                catch (Exception exception)
                {
                    coreSession_.LogError(exception.Message);
                }
            }
            
            internal void OnCoreReceive(Message message)
            {
                try
                {
                    lock (stateMutex_)
                    {
                        ServerProcessor_.ProcessReceive(message);
                        
                        if (Is.Update(message))
                        {
                            Update Update = Cast.Update(message);
                            
                            string key = Update.Id;
                            ServerUpdateProcessor ServerUpdateProcessor;
                            
                            if (! ServerUpdateProcessorDictionary_.TryGetValue(key, out ServerUpdateProcessor))
                            {
                                ServerUpdateProcessor = new ServerUpdateProcessor(this, key);
                                ServerUpdateProcessorDictionary_.Add(key, ServerUpdateProcessor);
                            }
                            
                            ServerUpdateProcessor.ProcessReceive(message);
                            
                            ServerUpdate:
                            
                            if (ServerUpdateProcessor.Completed)
                                ServerUpdateProcessorDictionary_.Remove(key);
                            
                            goto Server;
                        }
                        
                        if (Is.Request(message))
                        {
                            Request Request = Cast.Request(message);
                            
                            string key = Request.Id;
                            ServerRequestProcessor ServerRequestProcessor;
                            
                            if (! ServerRequestProcessorDictionary_.TryGetValue(key, out ServerRequestProcessor))
                            {
                                ServerRequestProcessor = new ServerRequestProcessor(this, key);
                                ServerRequestProcessorDictionary_.Add(key, ServerRequestProcessor);
                            }
                            
                            ServerRequestProcessor.ProcessReceive(message);
                            
                            ServerRequest:
                            
                            if (ServerRequestProcessor.Completed)
                                ServerRequestProcessorDictionary_.Remove(key);
                            
                            goto Server;
                        }
                        
                        if (Is.Report(message))
                        {
                            Report Report = Cast.Report(message);
                            
                            string key = Report.RequestId;
                            ServerRequestProcessor ServerRequestProcessor;
                            
                            if (! ServerRequestProcessorDictionary_.TryGetValue(key, out ServerRequestProcessor))
                            {
                                ServerRequestProcessor = new ServerRequestProcessor(this, key);
                                ServerRequestProcessorDictionary_.Add(key, ServerRequestProcessor);
                            }
                            
                            ServerRequestProcessor.ProcessReceive(message);
                            
                            ServerRequest:
                            
                            if (ServerRequestProcessor.Completed)
                                ServerRequestProcessorDictionary_.Remove(key);
                            
                            goto Server;
                        }
                        
                        Server:
                        ;
                    }
                    
                    if (event_ != null)
                    {
                        event_.Dispatch();
                        event_ = null;
                    }
                    else
                    {
                        if (coreSession_.LogEvents)
                            coreSession_.LogEvent("OnReceive({0})", message.ToString());
                        
                        if (server_.listener_ != null)
                        {
                            try
                            {
                                server_.listener_.OnReceive(server_, this, message);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    coreSession_.LogError(exception.Message);
                }
            }
            
            internal void OnCoreSend()
            {
                try
                {
                    if (server_.listener_ != null)
                    {
                        try
                        {
                            server_.listener_.OnSend(server_, this);
                        }
                        catch
                        {
                        }
                    }
                }
                catch (Exception exception)
                {
                    coreSession_.LogError(exception.Message);
                }
            }
            
            Server server_;
            Core.Server.Session coreSession_;
            
            object data_;
            
            object stateMutex_;
            
            ServerProcessor ServerProcessor_;
            SortedDictionary<string, ServerUpdateProcessor> ServerUpdateProcessorDictionary_;
            SortedDictionary<string, ServerRequestProcessor> ServerRequestProcessorDictionary_;
            
            Event_LoginRequest_312_10_LoginRequest Event_LoginRequest_312_10_LoginRequest_;
            Event_LogoutRequest_326_10_LogoutRequest Event_LogoutRequest_326_10_LogoutRequest_;
            Event_AccountListRequest_451_9_AccountListRequest Event_AccountListRequest_451_9_AccountListRequest_;
            Event_BotListRequest_451_9_BotListRequest Event_BotListRequest_451_9_BotListRequest_;
            Event_PackageListRequest_451_9_PackageListRequest Event_PackageListRequest_451_9_PackageListRequest_;
            Event_SubscribeRequest_451_9_SubscribeRequest Event_SubscribeRequest_451_9_SubscribeRequest_;
            
            Event event_;
        }
        
        class CoreServerListener : Core.ServerListener
        {
            public CoreServerListener(Server server)
            {
                server_ = server;
            }
            
            public override void OnConnect(Core.Server coreServer, Core.Server.Session coreSession)
            {
                Session session = new Session(server_, coreSession);
                
                lock (server_.stateMutex_)
                {
                    server_.sessionDictionary_.Add(session.Id, session);
                }
                
                coreSession.Data = session;
                
                session.OnCoreConnect();
            }
            
            public override void OnDisconnect(Core.Server coreServer, Core.Server.Session coreSession, string text)
            {
                Session session = ((Session) coreSession.Data);
                
                session.OnCoreDisconnect(text);
                
                coreSession.Data = null;
                
                lock (server_.stateMutex_)
                {
                    server_.sessionDictionary_.Remove(session.Id);
                }
            }
            
            public override void OnReceive(Core.Server coreServer, Core.Server.Session coreSession, Message message)
            {
                ((Session) coreSession.Data).OnCoreReceive(message);
            }
            
            public override void OnSend(Core.Server coreServer, Core.Server.Session coreSession)
            {
                ((Session) coreSession.Data).OnCoreSend();
            }
            
            Server server_;
        }
        
        Core.Server coreServer_;
        CoreServerListener coreServerListener_;
        
        ServerListener listener_;
        
        object stateMutex_;
        bool started_;
        
        Dictionary<int, Session> sessionDictionary_;
    }
    
    class ServerListener
    {
        public virtual void OnConnect(Server server, Server.Session session)
        {
        }
        
        public virtual void OnDisconnect(Server server, Server.Session session, ServerContext[] contexts, string text)
        {
        }
        
        public virtual void OnLoginRequest(Server server, Server.Session session, LoginRequest message)
        {
        }
        
        public virtual void OnLogoutRequest(Server server, Server.Session session, LogoutRequest message)
        {
        }
        
        public virtual void OnAccountListRequest(Server server, Server.Session session, AccountListRequest message)
        {
        }
        
        public virtual void OnBotListRequest(Server server, Server.Session session, BotListRequest message)
        {
        }
        
        public virtual void OnPackageListRequest(Server server, Server.Session session, PackageListRequest message)
        {
        }
        
        public virtual void OnSubscribeRequest(Server server, Server.Session session, SubscribeRequest message)
        {
        }
        
        public virtual void OnReceive(Server server, Server.Session session, Message message)
        {
        }
        
        public virtual void OnSend(Server server, Server.Session session)
        {
        }
    }
    
    #pragma warning restore 164
}

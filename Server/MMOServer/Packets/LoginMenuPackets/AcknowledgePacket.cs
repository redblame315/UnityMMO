﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MMOServer
{
    public class AcknowledgePacket
    {
        private uint characterId;
        private string clientAddress;
        private bool ackSuccessful;
        private uint sessionId;

        public bool AckSuccessful
        {
            get
            {
                return ackSuccessful;
            }

            set
            {
                ackSuccessful = value;
            }
        }

        public uint CharacterId
        {
            get
            {
                return characterId;
            }

            set
            {
                characterId = value;
            }
        }

        public string ClientAddress
        {
            get
            {
                return clientAddress;
            }

            set
            {
                clientAddress = value;
            }
        }

        public uint SessionId { get => sessionId; set => sessionId = value; }

        public AcknowledgePacket(bool ackSuccessful, string clientAddress, uint characterId)
        {
            this.ackSuccessful = ackSuccessful;
            this.clientAddress = clientAddress;
            this.characterId = characterId;
        }

        public AcknowledgePacket(bool ackSuccessful, uint sessionId)
        {
            this.ackSuccessful = ackSuccessful;
            this.sessionId = sessionId;
        }

        public AcknowledgePacket() { }

        public AcknowledgePacket(byte[] received)
          {
              MemoryStream mem = new MemoryStream(received);
              BinaryReader br = new BinaryReader(mem);
              try
              {
                  ackSuccessful = BitConverter.ToBoolean(br.ReadBytes(sizeof(bool)), 0);
                  var lengthAddress = BitConverter.ToUInt16(br.ReadBytes(sizeof(ushort)), 0);
                  clientAddress = Encoding.Unicode.GetString(br.ReadBytes(lengthAddress));
                  characterId = BitConverter.ToUInt32(br.ReadBytes(sizeof(uint)), 0);
              }
              catch (Exception e)
              {
                  Console.WriteLine("Error in reading ack packet: " + e.Message);

              }
          }

        /// <summary>
        /// Currently a bit of a hack. Too lazy to fix this up properly right now.
        /// This will only populate fields ackSuccessful and sessionId (only ones required for client anyway)
        /// </summary>
        /// <param name="received"></param>
        public void GetWorldResponse(byte[] received)
        {
            MemoryStream mem = new MemoryStream(received);
            BinaryReader br = new BinaryReader(mem);
            try
            {
                ackSuccessful = BitConverter.ToBoolean(br.ReadBytes(sizeof(bool)), 0);
                sessionId = BitConverter.ToUInt16(br.ReadBytes(sizeof(ushort)), 0);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in reading ack packet: " + e.Message);

            }
        }

        public byte[] GetBytes()
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(mem);
            byte[] successBytes = BitConverter.GetBytes(ackSuccessful);
            byte[] addressBytes = Encoding.Unicode.GetBytes(clientAddress);
            byte[] addressLengthBytes = BitConverter.GetBytes((ushort)addressBytes.Length);
            byte[] characterIdBytes = BitConverter.GetBytes(characterId);

            byte[] data = new byte[successBytes.Length + addressLengthBytes.Length + addressBytes.Length + characterIdBytes.Length];

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(successBytes);
                Array.Reverse(addressLengthBytes);
                Array.Reverse(characterIdBytes);
            }

            try
            {
                bw.Write(successBytes);
                bw.Write(addressLengthBytes);
                bw.Write(addressBytes);
                bw.Write(characterIdBytes);
                data = mem.GetBuffer();
                mem.Dispose();
                mem.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong with AckPacket");
                Console.WriteLine(e.ToString());
            }
            return data;
        }

        public byte[] GetResponseFromWorldServerBytes()
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(mem);
            byte[] successBytes = BitConverter.GetBytes(ackSuccessful);
            byte[] sessionIdBytes = BitConverter.GetBytes(sessionId);

            byte[] data = new byte[successBytes.Length + sessionIdBytes.Length];

            try
            {
                bw.Write(successBytes);
                bw.Write(sessionIdBytes);
                data = mem.GetBuffer();
                mem.Dispose();
                mem.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong with response from world server AckPacket");
                Console.WriteLine(e.ToString());
            }
            return data;



        }
    }
}

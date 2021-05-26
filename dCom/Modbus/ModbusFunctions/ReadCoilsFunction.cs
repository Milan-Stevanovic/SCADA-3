﻿using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
        public override byte[] PackRequest()
        {
            //           READ COILS -> Citanje DIGITALNOG IZLAZA (DIGITAL OUT)
            // READ DISCRETE INPUTS -> Citanje DIGITALNOG ULAZA  (DIGITAL IN )
            ModbusReadCommandParameters mrcp = this.CommandParameters as ModbusReadCommandParameters;
            byte[] niz = new byte[12];

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mrcp.TransactionId)), 0, niz, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mrcp.ProtocolId)), 0, niz, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mrcp.Length)), 0, niz, 4, 2);
            niz[6] = mrcp.UnitId;
            niz[7] = mrcp.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mrcp.StartAddress)), 0, niz, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mrcp.Quantity)), 0, niz, 10, 2);

            return niz;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> dictionary = new Dictionary<Tuple<PointType, ushort>, ushort>();
            if(response[7] == CommandParameters.FunctionCode + 0x80)
            {
                HandeException(response[8]);
            }
            else
            {
                ModbusReadCommandParameters mrcp = this.CommandParameters as ModbusReadCommandParameters;

                ushort kolicina = response[8];
                int brojac = 0;
                for (int i = 0; i < kolicina; i++)
                {
                    byte tempByte = response[9 + i];
                    for (int j = 0; j < 8; j++)
                    {
                        ushort value = (ushort)(tempByte & 1);
                        tempByte >>= 1;
                        brojac++;
                        if (mrcp.Quantity == brojac)
                            break;

                        Console.WriteLine("\nTEST DIGITAL OUT ADRESA {0}, VREDNOST {1}\n", mrcp.StartAddress + brojac, value);
                        dictionary.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, (ushort)(mrcp.StartAddress + brojac)), value);
                    }
                }
            }
            return dictionary;
        }
    }
}
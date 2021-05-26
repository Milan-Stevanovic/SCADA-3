﻿using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read holding registers functions/requests.
    /// </summary>
    public class ReadHoldingRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadHoldingRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            // READ HOLDING REGISTER -> Citanje ANALOGNOG IZLAZA (ANALOG OUT)
            //   READ INPUT REGISTER -> Citanje ANALOGNOG ULAZA  (ANALOG IN )
            ModbusReadCommandParameters mrcp = this.CommandParameters as ModbusReadCommandParameters;
            byte[] niz = new byte[12];

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mrcp.TransactionId)), 0, niz, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mrcp.ProtocolId)), 0, niz, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mrcp.Length)), 0, niz, 4, 2);
            // Do sada je popunjeno vec 6 bajtova tako da je sledeci 7mi tj. krecemo od indexa 6
            niz[6] = mrcp.UnitId; // Nema potrebe za konvertovanjem zato sto su UnitID i FunctionCode vec tipa 'byte'
            niz[7] = mrcp.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mrcp.StartAddress)), 0, niz, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mrcp.Quantity)), 0, niz, 10, 2);

            return niz;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> dictionary = new Dictionary<Tuple<PointType, ushort>, ushort>();
            if (response[7] == CommandParameters.FunctionCode + 0x80)
            {
                HandeException(response[8]);
            }
            else
            {
                ModbusReadCommandParameters mrcp = this.CommandParameters as ModbusReadCommandParameters;

                ushort q = response[8];
                ushort value;

                int start1 = 7;
                int start2 = 8;

                for (int i = 0; i < q / 2; i++)
                {
                    byte p1 = response[start1 += 2];
                    byte p2 = response[start2 += 2];

                    value = (ushort)(p2 + (p1 << 8));

                    dictionary.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, (ushort)(mrcp.StartAddress + i)), value);
                }

                // KOD ISPOD VAZI KADA IMAMO SAMO JEDNU VREDNOST
                /*
                byte port1 = response[10];
                byte port2 = response[9];

                ushort value = (ushort)(port1 + (port2 << 8));
                dictionary.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, mrcp.StartAddress), value);
                */
            }
            return dictionary;
        }
    }
}
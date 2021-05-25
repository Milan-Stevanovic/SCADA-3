﻿using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write single register functions/requests.
    /// </summary>
    public class WriteSingleRegisterFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleRegisterFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleRegisterFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters mwcp = this.CommandParameters as ModbusWriteCommandParameters;
            byte[] niz = new byte[12];

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mwcp.TransactionId)), 0, niz, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mwcp.ProtocolId)), 0, niz, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mwcp.Length)), 0, niz, 4, 2);
            niz[6] = mwcp.UnitId;
            niz[7] = mwcp.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mwcp.OutputAddress)), 0, niz, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mwcp.Value)), 0, niz, 10, 2);

            return niz;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> dictionary = new Dictionary<Tuple<PointType, ushort>, ushort>();

            ushort address1 = response[8];
            ushort address2 = response[9];
            ushort value1 = response[10];
            ushort value2 = response[11];

            ushort address = (ushort)(address2 + (address1 << 8));
            ushort value = (ushort)(value2 + (value1 << 8));

            dictionary.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, address), value);

            return dictionary;
        }
    }
}
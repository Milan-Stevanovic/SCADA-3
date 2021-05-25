using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read discrete inputs functions/requests.
    /// </summary>
    public class ReadDiscreteInputsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadDiscreteInputsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadDiscreteInputsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
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
            ModbusReadCommandParameters mrcp = this.CommandParameters as ModbusReadCommandParameters;

            // U ovom zadatku se radi samo sa jednim bitom tj. potrebno je proveriti da li je poslednji bit 0 ili 1
            // Zadatak se mora resiti pomocu bit maske ili shiftovanja
            // Ako uzmemo citav bajt u kome se nalazi podatak i uradimo logicko '&' na njega dobicemo vrednost ili 0 ili 1
            ushort value = (ushort)(response[9] & 1);

            dictionary.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_INPUT, mrcp.StartAddress), value);

            return dictionary;
        }
    }
}
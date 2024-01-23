﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace GitHub.JPMikkers.DHCP
{
    public abstract class DHCPOptionServerListBase : DHCPOptionBase
    {
        private List<IPAddress> _IPAddresses = new List<IPAddress>();

        public IEnumerable<IPAddress> IPAddresses
        {
            get
            {
                return _IPAddresses;
            }
            set
            {
                _IPAddresses = value.ToList();
            }
        }

        public abstract DHCPOptionServerListBase Create();

        #region IDHCPOption Members

        public override IDHCPOption FromStream(Stream s)
        {
            if(s.Length % 4 != 0) throw new IOException("Invalid DHCP option length");

            var result = Create();

            for(int t = 0; t < s.Length; t += 4)
            {
                result._IPAddresses.Add(ParseHelper.ReadIPAddress(s));
            }

            return result;
        }

        public override void ToStream(Stream s)
        {
            foreach(var ipAddress in _IPAddresses)
            {
                ParseHelper.WriteIPAddress(s, ipAddress);
            }
        }

        #endregion

        protected DHCPOptionServerListBase(TDHCPOption optionType)
            : base(optionType)
        {
        }

        public override string ToString()
        {
            return $"Option(name=[{OptionType}],value=[{string.Join(",", _IPAddresses.Select(x => x.ToString()))}])";
        }
    }
}

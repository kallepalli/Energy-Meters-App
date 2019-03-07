using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MCR_EMApp
{
    class FetchData
    {
        private int _currentMeterType, _start, _end, _meterTypes;
        private string _modelNo, _jsonData;
        private JArray _jarrayReader;
        private JObject _jObject;
        private List<ushort> _currentReadings = new List<ushort>();
        private AppDBContext _context;
        //private List<ushort> _previousReadings = new List<ushort>();
        private List<ushort[]> _memAddress = new List<ushort[]>();
        private int _ptRatio, _ctRatio;
        private float _MW, _MVAR, _KWH;
        private SerialPort _comport;
     
        public FetchData(string pathToConfiguration, SerialPort comport)
        {
            _jsonData = File.ReadAllText(pathToConfiguration);
            _jarrayReader = JArray.Parse(_jsonData);
            _jObject = JObject.Parse(_jarrayReader[0].ToString());
            _comport = comport;
            _context = new AppDBContext();

        }
        //Getdata data from settings.json file
        private void loadSerialPortDetails()
        {
            _comport.BaudRate = int.Parse(_jObject["SerialPort"]["BaudRate"].ToString());
            int parity = int.Parse(_jObject["SerialPort"]["Parity"].ToString());
            if (parity == 2)
            {
                _comport.Parity = Parity.Even;
            }
            else if (parity == 1)
            {
                _comport.Parity = Parity.Odd;
            }
            else
            {
                _comport.Parity = Parity.None;
            }
        }
        private void loadNoOfDifferentMeterTypes()
        {
            _meterTypes = int.Parse(_jObject["MeterTypes"].ToString());
        }
        private string getDeviceModel()
        {
            return _jObject["MeterDetails"][_currentMeterType.ToString()]["Model"].ToString();

        }
        private void loadMeterRange()
        {
            _start = int.Parse(_jObject["MeterDetails"][_currentMeterType.ToString()]["SlaveIdStart"].ToString());
            _end = int.Parse(_jObject["MeterDetails"][_currentMeterType.ToString()]["SlaveIdEnd"].ToString());
        }
        private void loadMemAddress()
        {
            _memAddress.Add(JArray.Parse(_jObject["MeterDetails"][_currentMeterType.ToString()]["MemoryMap"]["IB"].ToString()).Select(s => ushort.Parse(s.ToString())).ToArray());
            _memAddress.Add(JArray.Parse(_jObject["MeterDetails"][_currentMeterType.ToString()]["MemoryMap"]["IR"].ToString()).Select(s => ushort.Parse(s.ToString())).ToArray());
            _memAddress.Add(JArray.Parse(_jObject["MeterDetails"][_currentMeterType.ToString()]["MemoryMap"]["IY"].ToString()).Select(s => ushort.Parse(s.ToString())).ToArray());
            _memAddress.Add(JArray.Parse(_jObject["MeterDetails"][_currentMeterType.ToString()]["MemoryMap"]["KWH"].ToString()).Select(s => ushort.Parse(s.ToString())).ToArray());
            _memAddress.Add(JArray.Parse(_jObject["MeterDetails"][_currentMeterType.ToString()]["MemoryMap"]["MVAR"].ToString()).Select(s => ushort.Parse(s.ToString())).ToArray());
            _memAddress.Add(JArray.Parse(_jObject["MeterDetails"][_currentMeterType.ToString()]["MemoryMap"]["MW"].ToString()).Select(s => ushort.Parse(s.ToString())).ToArray());
            _memAddress.Add(JArray.Parse(_jObject["MeterDetails"][_currentMeterType.ToString()]["MemoryMap"]["VB"].ToString()).Select(s => ushort.Parse(s.ToString())).ToArray());
            _memAddress.Add(JArray.Parse(_jObject["MeterDetails"][_currentMeterType.ToString()]["MemoryMap"]["VR"].ToString()).Select(s => ushort.Parse(s.ToString())).ToArray());
            _memAddress.Add(JArray.Parse(_jObject["MeterDetails"][_currentMeterType.ToString()]["MemoryMap"]["VY"].ToString()).Select(s => ushort.Parse(s.ToString())).ToArray());
        }
        private void LoadMeterRatios(int MeterNo)
        {
            _ptRatio = int.Parse(_jObject["MeterRatios"][MeterNo.ToString()]["PT"].ToString());
            _ctRatio = int.Parse(_jObject["MeterRatios"][MeterNo.ToString()]["CT"].ToString());
            _MW = float.Parse(_jObject["MeterRatios"][MeterNo.ToString()]["MW"].ToString());
            _MVAR = float.Parse(_jObject["MeterRatios"][MeterNo.ToString()]["MVAR"].ToString());
            _KWH = float.Parse(_jObject["MeterRatios"][MeterNo.ToString()]["KWH"].ToString());
        }

        public void GetDataFromMeters()
        {

            try
            {
                //Load Serial port data
                loadSerialPortDetails();
                //Get No of type of meters
                loadNoOfDifferentMeterTypes();
                //Get Meter Details from different type of meters
                for (int i = 1; i <= _meterTypes; i++)
                {
                    _currentMeterType = i;
                    //Load device model
                    _modelNo = getDeviceModel();
                    //Load slave id ranges of the meter
                    loadMeterRange();
                    //Load Memory address of modbus registers for Ir,Iy,Ib,Vr,Vy,Vb,KWH,MVAR,MW
                    loadMemAddress();
                    //Load Meter Ratio of individual meter and fetch data from meters
                    for (int j = _start; j <= _end; j++)
                    {
                        //Load meter Ratios
                        LoadMeterRatios(j);
                        //Get data from meters
                                                
                        foreach (var address in _memAddress)
                        {
                            //Constructing request header 
                            byte[] requestwithoutcrc = new byte[6];
                            byte[] requestwithcrc = new byte[8];
                            //first byte is the id of the meter 
                            //in this scenario j variable holds meter id                            
                            requestwithoutcrc[0] = Convert.ToByte(j);
                            //second byte is the function code of modbus
                            //As we are trying to read input registers function code is 4
                            requestwithoutcrc[1] = 4;
                            //The consecutive two bytes stores the register start address
                            //address[0] holds the register start address in int we are converting this to 16bit binary using convert2bitarray function and this is further converted to two 8 bit integers
                            int[] registerstartaddress = convert2int8(convert2bitarray(address[0], 16), 2);
                            for ( int k=0;k<registerstartaddress.Length;k++ )
                            {
                                requestwithoutcrc[k+2] = Convert.ToByte(registerstartaddress[k]);
                            }
                            //The consecutive two bytes stores the registers length
                            //address[1] holds the value ie., no registers needed from starting address this is also converted to two 8 bit integers
                            //response length variable holds no of bytes the output response will be
                            int responselength = address[1];
                            byte[] response = new byte[responselength];
                            int[] length = convert2int8(convert2bitarray(address[1], 16), 2);
                            for (int k = 0; k < registerstartaddress.Length; k++)
                            {
                                requestwithoutcrc[k + 4] = Convert.ToByte(registerstartaddress[k]);
                            }
                            //Till this point we have constructed the modbus request modbus request as crc is little indian in modbus
                            //The loop 
                            int[] crcbytes = calculatecrc(requestwithoutcrc);
                            requestwithcrc.CopyTo(requestwithoutcrc, 0);
                            for (int k = crcbytes.Length,l=0;k>=0 ; k--,l++)
                            {
                                requestwithcrc[l + 6] = Convert.ToByte(crcbytes[l]);
                            }
                            _comport.Write(requestwithcrc.ToString());
                            _comport.Read(response, 0, responselength);
                            _currentReadings.Add(data1[1]);
                        }
                        var model = new MeterModel()
                        {
                            MeterID=j,
                            IB   = _currentReadings[0],
                            IR   = _currentReadings[1],
                            IY   = _currentReadings[2],
                            KWH  = _currentReadings[3],
                            MVAR = _currentReadings[4],
                            MW   = _currentReadings[5],
                            VB   = _currentReadings[6],
                            VR   = _currentReadings[7],
                            VY   = _currentReadings[8],
                            Timestamp = DateTime.Now
                        };
                        _context.MeterModel.Add(model);
                        _context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
            }

        }

        private int[] convert2int8(BitArray bitArray, int v)
        {
            int[] array = new int[v];
            int pivotpoint = bitArray.Length / v;
            for (int i = 0; i < v; i++) {
                int[] j = new int[1];
                bitArray.CopyTo(j, i*pivotpoint);
                array[i] = j[0];
            }
            return array;
        }

        private int[] calculatecrc(byte[] data)
        {
            BitArray crc = convert2bitarray(65535, 16);
            BitArray constMultiple = convert2bitarray(40961, 16);
            foreach (byte by in data)
            {
                int j = 1;
                BitArray ba = convert2bitarray(Convert.ToInt16(by), 16);
                crc.Xor(ba);
                int intcrca = ConvertToInt16(crc);
                for (int i = 0; i < 8; i++)
                {
                    int intcrc = 0;
                    if (crc[0] == false)
                    {
                        intcrc = ConvertToInt16(crc);
                        intcrc = intcrc >> 1;
                        crc = convert2bitarray(intcrc, 16);
                    }
                    else
                    {
                        intcrc = ConvertToInt16(crc);
                        intcrc = intcrc >> 1;
                        crc = convert2bitarray(intcrc, 16);
                        crc = crc.Xor(constMultiple);
                    }
                    tbOut.Text += j.ToString() + "st xor of byte results " + ConvertToInt16(crc).ToString("X") + System.Environment.NewLine;

                }
            }
            int[] crcdata = convert2int8(crc, 2);
            return crcdata;
        }

        private int ConvertToInt16(BitArray crc)
        {
            int[] array = new int[1];
            crc.CopyTo(array, 0);
            return array[0];
        }

        private BitArray convert2bitarray(int intcrc, int length)
        {
            string s = Convert.ToString(intcrc, 2);
            BitArray temp = new BitArray(length);
            int padding = length - s.Length;
            for (int i = s.Length - 1, j = 0; i >= 0; i--, j++)
            {
                if (s[j].ToString() == "1")
                    temp[i] = true;
                else
                    temp[i] = false;
            }
            for (int i = s.Length; i < s.Length + padding; i++)
            {
                temp[i] = false;
            }
            return temp;
        }
    }
}





